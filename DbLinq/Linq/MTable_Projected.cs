////////////////////////////////////////////////////////////////////
// MIT license:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//
// Authors:
//        Jiri George Moudry
////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using DBLinq.Linq.clause;
using DBLinq.util;

namespace DBLinq.Linq
{
    /// <summary>
    /// Used in queries where we project from a row type into a new type.
    /// Unlike MTable, it does not have SaveAll() and Insert(), 
    /// since objects returned from here are not 'live'.
    /// </summary>
    /// <typeparam name="T">eg. 'int' in query 'from e in db.Employees select e.ID'</typeparam>
    class MTable_Projected<T> 
        : IQueryable<T>
        , IGetModifiedEnumerator<T>
        , IQueryText
        , IQueryProvider //new as of Beta2
    {
        public SessionVars _vars;

        public MTable_Projected(SessionVars vars)
        {
            _vars = vars;
        }

        public IQueryable<S> CreateQuery<S>(Expression expression)
        {
            //this occurs in GroupBy followed by Where, eg. F6B_OrdersByCity()
            string msg1 = "MTable_Proj.CreateQuery: T=<"+typeof(T)+"> -> S=<"+typeof(S)+">";
            string msg2 = "MTable_Proj.CreateQuery: "+expression;
            //Log1.Info(msg1);
            //Log1.Info(msg2);
            if (_vars.log != null)
                _vars.log.WriteLine("MTable_Proj.CreateQuery: " + expression);

            //todo: Clone SessionVars, call StoreQuery, and return secondary MTable_Proj?
            SessionVars vars2 = _vars.Clone();
            vars2.StoreQuery(expression);
            MTable_Projected<S> projectedQ2 = new MTable_Projected<S>(vars2);
            return projectedQ2;
            //throw new ApplicationException("L61: Not prepared for double projection");
        }

        public S Execute<S>(Expression expression)
        {
            if (_vars.log != null)
                _vars.log.WriteLine("MTable_Proj.Execute<" + typeof(S) + ">: " + expression);

            SessionVars vars = _vars.Clone();
            return new RowScalar<T>(vars, this).GetScalar<S>(expression);
        }

        /// <summary>
        /// entry point for 'foreach' statement.
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            //we don't keep projections in cache, pass cache=null
            if(_vars.log!=null)
                _vars.log.WriteLine("MTable_Proj.GetEnumerator <"+typeof(T)+">");

            SessionVars vars = _vars.Clone();
            QueryProcessor.ProcessLambdas(vars, typeof(T)); //for test D7, already done in MTable.CreateQ?

            //RowEnumerator<T> rowEnumerator = new RowEnumerator<T>(vars,null);
            RowEnumerator<T> rowEnumerator = RowEnumFactory<T>.Create(vars,null);

            //rowEnumerator.ExecuteSqlCommand();
            return rowEnumerator.GetEnumerator();
        }

        /// <summary>
        /// GetEnumerator where you can inject an extra clause, eg. "LIMIT 2" or an extra where
        /// </summary>
        /// <param name="fct"></param>
        public RowEnumerator<T> GetModifiedEnumerator(CustomExpressionHandler fct)
        {
            SessionVars vars2 = _vars.Clone();
            fct(vars2);
            QueryProcessor.ProcessLambdas(vars2, typeof(T));
            RowEnumerator<T> rowEnumerator = new RowEnumerator<T>(vars2, null);
            //rowEnumerator.ExecuteSqlCommand();
            return rowEnumerator;
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new ApplicationException("Not implemented");
        }

        public Type ElementType { 
            get {
                throw new ApplicationException("Not implemented");
            }
        }
        public Expression Expression { 
            //copied from RdfProvider
            get { return Expression.Constant(this); }
        }
 

        public IQueryable CreateQuery(Expression expression)
        {
            throw new ApplicationException("Not implemented");
        }

        public object Execute(Expression expression)
        {
            throw new ApplicationException("Not implemented");
        }

        public string GetQueryText()
        {
            QueryProcessor.ProcessLambdas(_vars, typeof(T));
            return _vars.sqlString;
        }

        public void Dispose()
        {
            Console.WriteLine("Dispose2");
        }

        //New as of Orcas Beta2 - what does it do?
        public IQueryProvider Provider 
        {
            get { return this; }
        }
    }
}

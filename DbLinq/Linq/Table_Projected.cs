#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion
using System;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using DbLinq.Data.Linq;
using DbLinq.Linq.Clause;
using DbLinq.Logging;
using DbLinq.Util;

namespace DbLinq.Linq
{
    /// <summary>
    /// Used in queries where we project from a row type into a new type.
    /// Unlike MTable, it does not have SaveAll() and Insert(), 
    /// since objects returned from here are not 'live'.
    /// </summary>
    /// <typeparam name="T">eg. 'int' in query 'from e in db.Employees select e.ID'</typeparam>
    class MTable_Projected<T>
        : IOrderedQueryable<T> //projections and joins can still be ordered
        , IQueryText
        , IQueryProvider //new as of Beta2
        , IGetSessionVars
    {
        public SessionVars _vars;

        public MTable_Projected(SessionVars vars)
        {
            _vars = vars;
        }

        public IQueryable<S> CreateQuery<S>(Expression expression)
        {
            //this occurs in GroupBy followed by Where, eg. F6B_OrdersByCity()
            //string msg1 = "MTable_Proj.CreateQuery: T=<"+typeof(T)+"> -> S=<"+typeof(S)+">";
            //string msg2 = "MTable_Proj.CreateQuery: "+expression;
            //Log1.Info(msg1);
            //Log1.Info(msg2);
            _vars.Context.Logger.Write(Level.Debug, "MTable_Proj.CreateQuery: " + expression);

            //todo: Clone SessionVars, call StoreQuery, and return secondary MTable_Proj?
            SessionVars vars2 = new SessionVars(_vars).Add(expression);
            MTable_Projected<S> projectedQ2 = new MTable_Projected<S>(vars2);
            return projectedQ2;
            //throw new ApplicationException("L61: Not prepared for double projection");
        }

        /// <summary>
        /// this is only called during Dynamic Linq
        /// </summary>
        [Obsolete("COMPLETELY UNTESTED")]
        public IQueryable CreateQuery(Expression expression)
        {
            SessionVars vars2 = new SessionVars(_vars).Add(expression);
            Type S = expression.Type;
            //MTable_Projected<S> projectedQ2 = new MTable_Projected<S>(vars2);
            Type mtableProjectedType2 = typeof(MTable_Projected<>).MakeGenericType(S);
            object projectedQ = Activator.CreateInstance(mtableProjectedType2, vars2);
            return projectedQ as IQueryable;
        }


        public S Execute<S>(Expression expression)
        {
            _vars.Context.Logger.Write(Level.Debug, "MTable_Proj.Execute<{0}>: {1}", typeof(S), expression);

            SessionVars vars = new SessionVars(_vars).AddScalar(expression); //clone and append Expr
            SessionVarsParsed varsFin = _vars.Context.QueryGenerator.GenerateQuery(vars, null); //parse all
            return new RowScalar<T>(varsFin, this).GetScalar<S>(expression);
        }

        /// <summary>
        /// entry point for 'foreach' statement.
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            //we don't keep projections in cache, pass cache=null
            _vars.Context.Logger.Write(Level.Debug, "MTable_Proj.GetEnumerator <{0}>", typeof (T));

            //SessionVars vars = _vars.Clone();
            SessionVarsParsed varsFin = _vars.Context.QueryGenerator.GenerateQuery(_vars, typeof(T)); //for test D7, already done in MTable.CreateQ?

            RowEnumerator<T> rowEnumerator = RowEnumFactory<T>.Create(varsFin);

            return rowEnumerator.GetEnumerator();
        }

        [Obsolete("COMPLETELY UNTESTED")]
        IEnumerator IEnumerable.GetEnumerator()
        {
            Type dynamicType;
            if (!TypeExtensions.IsDynamicClass(typeof(T), out dynamicType))
            {
                IEnumerator<T> enumT = GetEnumerator();
                return enumT;
            }
            else
            {
                Type mtableProjectedType2 = typeof(MTable_Projected<>).MakeGenericType(dynamicType);
                object projectedQ = Activator.CreateInstance(mtableProjectedType2, _vars);
                System.Reflection.MethodInfo[] methods = mtableProjectedType2.GetMethods();
                System.Reflection.MethodInfo getEnumMethod = methods.Where(m => m.Name == "GetEnumerator").Single();
                object enumeratorObj = getEnumMethod.Invoke(projectedQ, new object[0]);
                return enumeratorObj as IEnumerator;
            }

        }

        [Obsolete("COMPLETELY UNTESTED")]
        public Type ElementType
        {
            //this is a wild guess
            get { return typeof(T); }
        }

        public Expression Expression
        {
            //copied from RdfProvider
            get { return Expression.Constant(this); }
        }


        [Obsolete("NOT IMPLEMENTED")]
        public object Execute(Expression expression)
        {
            throw new ApplicationException("L142 Not implemented");
        }

        public string GetQueryText()
        {
            SessionVarsParsed varsFin = _vars.Context.QueryGenerator.GenerateQuery(_vars, typeof(T));
            return varsFin.SqlString;
        }

        public void Dispose()
        {
            //Logger.Write("Dispose2");
        }

        //New as of Orcas Beta2 - what does it do?
        public IQueryProvider Provider
        {
            get { return this; }
        }

        /// <summary>
        /// during a Union clause, we need to retrieve sessionVars from the outside
        /// </summary>
        public SessionVars SessionVars { get { return _vars; } }

    }

    public interface IGetSessionVars
    {
        SessionVars SessionVars { get; }
    }
}

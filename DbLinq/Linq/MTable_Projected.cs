////////////////////////////////////////////////////////////////////
//Initial author: Jiri George Moudry, 2006.
//License: LGPL. (Visit http://www.gnu.org)
//Commercial code may call into this library, if it's in a different module (DLL)
////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Query;
using System.Data.DLinq;
using System.Expressions;
using DBLinq.Linq.clause;
using DBLinq.util;
//using MySql.Data.MySqlClient;

namespace DBLinq.Linq
{
    class MTable_Projected<T> 
        : IQueryable<T>
        , IGetModifiedEnumerator<T>
    {
        public SessionVars _vars;
        //public WhereClauses _whereClauses;
        public SqlExpressionParts _sqlParts;
        public readonly LambdaExpression _selectExpr;
        Expression _expression;
        ProjectionData _projectionData;

        //eg. when calling "SELECT e.Name FROM Employee", _sourceType=Employee and T=String
        Type _sourceType; 
        //public MTable_Projected(SessionVars vars, Expression expr, LambdaExpression whereExpr,LambdaExpression selectExpr)

        public MTable_Projected(SessionVars vars)
        {
            _vars = vars;
            //this._whereExpr = vars.whereExpr;
            this._selectExpr = vars.selectExpr;
            this._sqlParts = vars._sqlParts;
            //WhereClauseBuilder whereBuilder = new WhereClauseBuilder();
            string methodName;
            LambdaExpression lambda = WhereClauseBuilder.FindLambda(vars.createQueryExpr,out methodName);
            switch(methodName)
            {
                case "Where":
                    //WhereClauses whereClauses = whereBuilder.Main_AnalyzeLambda(lambda);
                    //this._sqlParts.whereList.Add( whereClauses.sb.ToString());
                    vars.whereExpr.Add(lambda);
                    break;
                case "Select":
                    _selectExpr = lambda;
                    _projectionData = ProjectionData.FromSelectExpr(_selectExpr);
                    vars.projectionData = _projectionData;
                    break;
                case "SelectMany":
                    _selectExpr = lambda;
                    _projectionData = ProjectionData.FromSelectManyExpr(_selectExpr);
                    vars.projectionData = _projectionData;
                    break;
                default:
                    Console.WriteLine("MTable_Proj ERROR L54 - unprepared for method "+methodName);
                    break;
            }
            //Console.WriteLine("WHERE "+_whereClauses.sb);
            ParameterExpression paramExpr0 = lambda.Parameters[0];
            _sourceType = paramExpr0.Type;


            _expression = vars.createQueryExpr;
        }

        public IQueryable<S> CreateQuery<S>(Expression expression)
        {
            throw new ApplicationException("L61: Not prepared for double projection");
        }
        public S Execute<S>(Expression expression)
        {
            Console.WriteLine("MTable_Proj.Execute<"+typeof(S)+">: "+expression);
            return new RowScalar<T>(_vars, this).GetScalar<S>(expression);
        }

        /// <summary>
        /// entry point for 'foreach' statement.
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            //we don't keep projections in cache, pass cache=null
            QueryProcessor.ProcessLambdas(_vars);
            RowEnumerator<T> rowEnumerator = new RowEnumerator<T>(_vars,null);
            rowEnumerator.ExecuteSqlCommand();
            return rowEnumerator;
        }

        /// <summary>
        /// GetEnumerator where you can inject an extra clause, eg. "LIMIT 2" or an extra where
        /// </summary>
        /// <param name="fct"></param>
        public RowEnumerator<T> GetModifiedEnumerator(CustomExpressionHandler fct)
        {
            SessionVars vars2 = _vars.Clone();
            fct(vars2);
            QueryProcessor.ProcessLambdas(vars2);
            RowEnumerator<T> rowEnumerator = new RowEnumerator<T>(vars2, null);
            rowEnumerator.ExecuteSqlCommand();
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

    }
}

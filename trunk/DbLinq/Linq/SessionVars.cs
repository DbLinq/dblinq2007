////////////////////////////////////////////////////////////////////
//Initial author: Jiri George Moudry, 2006.
//License: LGPL. (Visit http://www.gnu.org)
////////////////////////////////////////////////////////////////////
using System;
using System.Expressions;
using System.Collections.Generic;
using System.Text;
using DBLinq.Linq.clause;
using DBLinq.util;

namespace DBLinq.Linq
{
    /// <summary>
    /// holds variables that are generated 
    /// during expression parsing and querying.
    /// They are passed from MTable to MTable_Projected to RowEnumerator.
    /// </summary>
    public class SessionVars
    {
        public Expression       createQueryExpr;
        public MContext         context;

        /// <summary>
        /// type of Table from which this query originated, eg. typeof(Customer).
        /// </summary>
        public Type             sourceType;

        //public WhereClauses _whereClauses;
        public SqlExpressionParts _sqlParts;

        /// <summary>
        /// todo: add all top-level expressions into this collection.
        /// </summary>
        public readonly List<LambdaExpression> lambdasInOrder = new List<LambdaExpression>();
        
        /// <summary>
        /// there can be more than one Where clause
        /// </summary>
        public readonly List<LambdaExpression> whereExpr = new List<LambdaExpression>();

        /// <summary>
        /// there can be more than one select
        /// </summary>
        public LambdaExpression selectExpr;

        public LambdaExpression selectManyExpr;


        public LambdaExpression orderByExpr;
        public ProjectionData   projectionData;
        public string           limitClause;

        /// <summary>
        /// every time the framework calls CreateQuery to further project our query, 
        /// record it in this list
        /// </summary>
        public List<Type>       createQueryList = new List<Type>();


        /// <summary>
        /// Look at selectExpr or whereExpr, return e.g. '$c'
        /// </summary>
        public string GetDefaultVarName()
        {
            if(selectExpr!=null)
                return VarName.GetSqlName(selectExpr.Parameters[0].Name);
            if(whereExpr.Count>0)
                return VarName.GetSqlName(whereExpr[0].Parameters[0].Name);
            return VarName.GetSqlName("x"); //if no expressions, provide fallback
        }

        /// <summary>
        /// We don't want subsequent queries (e.g. Count()) to modify early one (eg. Where)
        /// </summary>
        public SessionVars Clone()
        {
            SessionVars clone = (SessionVars) base.MemberwiseClone();
            clone._sqlParts = _sqlParts.Clone();
            clone.createQueryList = new List<Type>(this.createQueryList);
            clone.lambdasInOrder.AddRange(this.lambdasInOrder);
            return clone;
        }

        public void StoreLambda(string methodName, LambdaExpression lambda)
        {
            //From the C# spec: Specifically, query expressions are translated 
            //into invocations of methods named 
            //  Where, Select, SelectMany, Join, GroupJoin, OrderBy, OrderByDescending, 
            //  ThenBy, ThenByDescending, GroupBy, and Cast
            switch(methodName)
            {
                case "Where":  
                        whereExpr.Add(lambda); break;
                case "Select": 
                        selectExpr = lambda; break;
                case "SelectMany":
                        selectExpr = lambda; 
                    LambdaExpression lambda2 = WhereClauseBuilder.FindSelectManyLambda(lambda,out methodName);
                    if(methodName=="Where")
                    {
                        whereExpr.Add(lambda2);
                    }
                    break;
                case "OrderBy": 
                    orderByExpr = lambda; break;
                default: 
                    throw new ApplicationException("L109: Unprepared for method "+methodName);
            }
        }
    }
}

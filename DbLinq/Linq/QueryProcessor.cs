using System;
using System.Expressions;
using System.Collections.Generic;
using System.Text;
using System.Data.DLinq;
using DBLinq.Linq.clause;
using DBLinq.util;

namespace DBLinq.Linq
{
    /// <summary>
    /// after all Lambdas are collected: 
    /// 1) build SQL expression from parts, 
    /// and 2) compile row enum reader
    /// </summary>
    class QueryProcessor
    {
        readonly SessionVars _vars;
        readonly WhereClauseBuilder _whereBuilder; // = new WhereClauseBuilder();

        private QueryProcessor(SessionVars vars)
        {
            _vars = vars;
            //TODO - pass in either vars or a delegate which allows asking for nickname for 'o.Customer'
            _whereBuilder = new WhereClauseBuilder(vars._sqlParts);
            _whereBuilder.NicknameRequest +=new AskNicknameHandler(_whereBuilder_NicknameRequest);
        }

        string _whereBuilder_NicknameRequest(Expression ex, AssociationAttribute assoc)
        {
            foreach(ExprPair pair in ExprEnum.EnumExpressions(_vars.selectExpr))
            {
                if(pair.child.NodeType==ExpressionType.MemberAccess)
                {
                    MemberExpression member2 = (MemberExpression)pair.child;
                    if(AttribHelper.IsAssociation(member2,out assoc))
                    {
                        return pair.name;
                    }
                }                
            }
            return null; //not found
        }

        public static void ProcessLambdas(SessionVars vars)
        {
            new QueryProcessor(vars).go();
        }

        void go()
        {
            foreach(LambdaExpression lambda in _vars.whereExpr)
            {
                WhereClauses whereClauses = _whereBuilder.Main_AnalyzeLambda(lambda);
                whereClauses.CopyInto(_vars._sqlParts);
            }
            if(_vars.selectExpr!=null)
            {
                FromClauseBuilder.Main_AnalyzeLambda(_vars._sqlParts, _vars.selectExpr);
            }


            /*
            switch(methodName)
            {
                case "Where":  
                    {
                        this._vars.whereExpr.Add(lambda);
                    }
                    break;
                case "Select": 
                    {
                    _vars.selectExpr = lambda;
                    FromClauseBuilder.Main_AnalyzeLambda(_sqlParts,lambda);
                    }
                    break;
                case "SelectMany":
                    //dig deeper to find the where clause
                    _vars.selectExpr = lambda; 
                    lambda = whereBuilder.FindSelectManyLambda(lambda,out methodName);
                    if(methodName=="Where"){
                        WhereClauses whereClauses = whereBuilder.Main_AnalyzeLambda(lambda);
                        whereClauses.CopyInto(_sqlParts);
                        _vars.whereExpr.Add(lambda);
                    }
                    break;
                case "OrderBy": 
                    _vars.orderByExpr = lambda; 
                    break;
                default: 
                    throw new ApplicationException("L45: Unprepared for method "+methodName);
            }
             * */
        }
    }
}

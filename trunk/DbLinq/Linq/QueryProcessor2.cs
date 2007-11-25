#region MIT license
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
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using DBLinq.Linq.clause;
using DBLinq.util;

namespace DBLinq.Linq
{
    /// <summary>
    /// after all Lambdas are collected, and GetEnumerator() is called:
    /// QueryProcessor calls ExpressionTreeParser to build SQL expression from parts
    /// </summary>
    partial class QueryProcessor
    {
        void processWhereClause(LambdaExpression lambda)
        {
            ParseResult result = null;
            ParseInputs inputs = new ParseInputs(result);
            result = ExpressionTreeParser.Parse(this, lambda.Body, inputs);

            if (GroupHelper.IsGrouping(lambda.Parameters[0].Type))
            {
                _vars._sqlParts.AddHaving(result.columns);
            }
            else
            {
                _vars._sqlParts.AddWhere(result.columns);
            }

            result.CopyInto(_vars._sqlParts); //transfer params and tablesUsed
        }

        void processSelectClause(LambdaExpression selectExpr)
        {
            this.selectExpr = selectExpr; //store for GroupBy & friends

            //necesary for projections?
            if (_vars.groupByExpr == null)
            {
                _vars.projectionData = ProjectionData.FromSelectExpr(selectExpr);
            }
            else
            {
                _vars.projectionData = ProjectionData.FromSelectGroupByExpr(selectExpr, _vars.groupByExpr, _vars._sqlParts);
            }

            ParseResult result = null;
            ParseInputs inputs = new ParseInputs(result);
            inputs.groupByExpr = _vars.groupByExpr;
            if (selectExpr.Body.NodeType == ExpressionType.Parameter)
            {
                //'from p in Products select p' - do nothing, will result in SelectAllFields() later
            }
            else
            {
                result = ExpressionTreeParser.Parse(this, selectExpr.Body, inputs);
                _vars._sqlParts.AddSelect(result.columns);
                result.CopyInto(_vars._sqlParts); //transfer params and tablesUsed

                //support for subsequent Count() - see F2_ProductCount_Clause
                if (result.columns.Count > 0)
                {
                    // currentVarNames[int] = "p$.ProductID";
                    this.currentVarNames[selectExpr.Body.Type] = result.columns[0];
                }
            }
        }

        void processOrderByClause(LambdaExpression orderByExpr, string orderBy_desc)
        {
            ParseResult result = null;
            ParseInputs inputs = new ParseInputs(result);
            result = ExpressionTreeParser.Parse(this, orderByExpr.Body, inputs);
            string orderByFields = string.Join(",", result.columns.ToArray());
            _vars._sqlParts.orderByList.Add(orderByFields);
            _vars._sqlParts.orderBy_desc = orderBy_desc; //copy 'DESC' specifier
        }

        void processJoinClause(MethodCallExpression joinExpr)
        {
            ParseResult result = null;
            ParseInputs inputs = new ParseInputs(result);

            if (joinExpr == null || joinExpr.Arguments.Count != 5)
                throw new ArgumentOutOfRangeException("L112 Currently only handling 5-arg Joins");

            Expression arg2 = joinExpr.Arguments[2]; //{p => p.ProductID}
            Expression arg3 = joinExpr.Arguments[3]; //{o => o.OrderID}
            Expression arg4 = joinExpr.Arguments[4]; //{(p, o) => new <>f__AnonymousType9`2(ProductName = p.ProductName, CustomerID = o.CustomerID)}

            string joinField1, joinField2;
            //processSelectClause(arg2.XLambda());
            {
                result = ExpressionTreeParser.Parse(this, arg2.XLambda().Body, inputs);
                joinField1 = result.columns[0]; // "p$.ProductID"
                result.CopyInto(_vars._sqlParts); //transfer params and tablesUsed
            }
            {
                result = ExpressionTreeParser.Parse(this, arg3.XLambda().Body, inputs);
                joinField2 = result.columns[0]; // "p$.ProductID"
                result.CopyInto(_vars._sqlParts); //transfer params and tablesUsed
            }
            processSelectClause(arg4.XLambda());
            _vars._sqlParts.joinList.Add(joinField1 + "=" + joinField2);
            //throw new ArgumentOutOfRangeException("L118 TODO: Join");

            //result = ExpressionTreeParser.Parse(this, joinExpr.Body, inputs);
            //string orderByFields = string.Join(",", result.columns.ToArray());
            //_vars._sqlParts.orderByList.Add(orderByFields);
            //_vars._sqlParts.orderBy_desc = orderBy_desc; //copy 'DESC' specifier
        }

        private void processSelectMany(MethodCallExpression exprCall)
        {
            //special case: SelectMany can come with 2 or 3 params
            switch (exprCall.Arguments.Count)
            {
                case 2: //???
                    processSelectManyLambda_simple(exprCall.Arguments[1].XLambda());
                    return;
                case 3: //'from c in db.Customers from o in c.Orders where c.City == "London" select new { c, o }'
                    //ignore arg[0]: MTable<>
                    //ignore arg[1]: c=>c.Orders

                    LambdaExpression lambda1 = exprCall.Arguments[1].XLambda();
                    LambdaExpression lambda2 = exprCall.Arguments[2].XLambda();
                    {
                        MemberExpression memberExpression = lambda1.Body as MemberExpression;
                        ParameterExpression paramExpression = lambda2.Parameters[1];
                        ParseResult result = new ParseResult(null);
                        JoinBuilder.AddJoin1(memberExpression, paramExpression, result);
                        result.CopyInto(_vars._sqlParts);
                    }

                    //StoreLambda("Select", lambda2);
                    processSelectClause(lambda2);
                    return;
                default:
                    throw new ApplicationException("StoreQuery L117: Prepared only for 2 or 3 param GroupBys");
            }
        }

        private void processSelectManyLambda_simple(LambdaExpression selectExpr)
        {
            //there is an inner Where and inner Select
            Expression exInner = selectExpr.Body.XCastOperand();
            //exInner={c.Orders.Where(o => op_Equality(c.City, "London")).Select(o => new {c = c, o = o})}
            MethodCallExpression exInnFct = exInner.XMethodCall();
            if (exInnFct == null)
                throw new ArgumentException("StoreSelMany L257 bad args");
            //each parameter is a lambda
            foreach (Expression innerLambda in exInnFct.Arguments)
            {
                if (innerLambda.NodeType == ExpressionType.Lambda)
                {
                    //eg. {o => new {c = c, o = o}}'
                    selectExpr = innerLambda as LambdaExpression;
                }
                else
                {
                    //eg. '{c.Orders.Where(o => op_Equality(c.City, "London"))}'
                    MethodCallExpression whereCall = innerLambda.XMethodCall();
                    processQuery(whereCall);
#if OBSO
                    string methodName2 = whereCall.Method.Name;

                    MemberExpression memberExpressionCache = null; //'c.Orders', needed below for nicknames
                    ParameterExpression paramExpressionCache = null; //'o', for nicknames

                    foreach (Expression whereCallParam in whereCall.Arguments)
                    {
                        if (whereCallParam.NodeType == ExpressionType.Convert)
                        {
                            //eg. '{c.Orders}'
                            //CHANGED: - join now handled below
                            Expression memberEx = whereCallParam.XCastOperand();
                            memberExpressionCache = memberEx.XMember();
                            //ParameterExpression memExParm = memberExpressionCache.Expression.XParam(); //'c'

                            ////cook up delgType: c=>MSet<Order>
                            //Type funcType1 = typeof(System.Query.Func<int,int>);
                            //Type funcType2 = funcType1.GetGenericTypeDefinition();
                            //Type funcType3 = funcType2.MakeGenericType(memExParm.Type, memberEx.Type);
                            //Type delgType = funcType3;

                            //List<ParameterExpression> fakeParam = new List<ParameterExpression>
                            //{ memExParm };
                            //LambdaExpression fakeJoinLambda = Expression.Lambda(delgType, memberEx, fakeParam);
                            //this.whereExpr.Add(fakeJoinLambda);
                        }
                        else if (whereCallParam.NodeType == ExpressionType.Lambda)
                        {
                            //{o => op_Equality(c.City, "London")}
                            LambdaExpression innerWhereLambda = whereCallParam as LambdaExpression;
                            //StoreLambda(methodName2, innerWhereLambda);
                            processWhereClause(methodName2, innerWhereLambda);
                            paramExpressionCache = innerWhereLambda.Parameters[0];
                        }
                        else
                        {
                            //StoreLambda(methodName2, null);
                        }
                    }

                    //assign nikname mapping c.Orders=o
                    if (memberExpressionCache != null && paramExpressionCache != null)
                    {
                        //memberExprNickames[memberExpressionCache] = paramExpressionCache.Name;
                        ParseResult result = new ParseResult(null);
                        JoinBuilder.AddJoin1(memberExpressionCache, paramExpressionCache, result);
                        result.CopyInto(_vars._sqlParts);
                    }
#endif

                }
            }
        }

        void processGroupByCall(MethodCallExpression exprCall)
        {
            //special case: GroupBy can come with 2 or 3 params
            switch (exprCall.Arguments.Count)
            {
                case 2: //'group o by o.CustomerID into g'
                    processGroupByLambda(exprCall.Arguments[1].XLambda());
                    return;
                case 3: //'group new {c.PostalCode, c.ContactName} by c.City into g'
                    processGroupByLambda(exprCall.Arguments[1].XLambda());
                    _vars.groupByNewExpr = exprCall.Arguments[2].XLambda();
                    return;
                default:
                    throw new ApplicationException("StoreQuery L117: Prepared only for 2 or 3 param GroupBys");
            }
        }

        void processGroupByLambda(LambdaExpression groupBy)
        {
            ParseResult result = null;
            ParseInputs inputs = new ParseInputs(result);
            _vars.groupByExpr = groupBy;
            //inputs.groupByExpr = _vars.groupByExpr;
            result = ExpressionTreeParser.Parse(this, groupBy.Body, inputs);
            string groupByFields = string.Join(",", result.columns.ToArray());
            _vars._sqlParts.groupByList.Add(groupByFields);

            if (selectExpr == null //&& _vars.groupByNewExpr==null
                )
            {
                //manually add "SELECT c.City"
                //_vars._sqlParts.AddSelect(result.columns);

                result.CopyInto(_vars._sqlParts); //transfer params and tablesUsed
            }

#if OBSO
            if (_vars.groupByNewExpr == null && selectExpr == null)
            {
                //eg. 'db.Customers.GroupBy( c=>c.City )' - select entire Customer
                ParameterExpression paramEx = groupBy.Parameters[0];
                FromClauseBuilder.SelectAllFields(_vars, _vars._sqlParts, paramEx.Type, VarName.GetSqlName(paramEx.Name));
            }
            else 
#endif
                if (_vars.groupByNewExpr != null)
            {
                inputs = new ParseInputs(result);
                //inputs.groupByExpr = _vars.groupByExpr;
                result = ExpressionTreeParser.Parse(this, _vars.groupByNewExpr.Body, inputs);
                _vars._sqlParts.AddSelect(result.columns);
                result.CopyInto(_vars._sqlParts); //transfer params and tablesUsed
            }
        }

        void processGroupJoin(MethodCallExpression exprCall)
        {
            //occurs in LinqToSqlJoin10()
            Console.WriteLine("TODO L299 Support GroupJoin()");
        }

    }
}

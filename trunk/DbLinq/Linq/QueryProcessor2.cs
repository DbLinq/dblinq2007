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
using DBLinq.Linq.Clause;
using DBLinq.Util;

namespace DBLinq.Linq
{
    /// <summary>
    /// after all Lambdas are collected, and GetEnumerator() is called:
    /// QueryProcessor calls ExpressionTreeParser to build SQL expression from parts
    /// </summary>
    partial class QueryProcessor
    {
        void ProcessWhereClause(LambdaExpression lambda)
        {
            ParseResult result = ExpressionTreeParser.Parse(_vars.Context.Vendor, this, lambda.Body);

            if (GroupHelper.IsGrouping(lambda.Parameters[0].Type))
            {
                _vars._sqlParts.AddHaving(result.columns);
            }
            else
            {
                _vars._sqlParts.AddWhere(result.columns);
            }

            result.CopyInto(this, _vars._sqlParts); //transfer params and tablesUsed
        }

        void ProcessSelectClause(LambdaExpression selectExpr)
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

            Expression body = selectExpr.Body;

            body = body.StripTransparentID(); //only does something in Joins - see LinqToSqlJoin01()

            if (body.NodeType == ExpressionType.Parameter)
            {
                //'from p in Products select p' - do nothing, will result in SelectAllFields() later
            }
            else
            {
                ParseResult result = ExpressionTreeParser.Parse(_vars.Context.Vendor, this, body);
                _vars._sqlParts.AddSelect(result.columns);
                result.CopyInto(this, _vars._sqlParts); //transfer params and tablesUsed

                //support for subsequent Count() - see F2_ProductCount_Clause
                if (result.columns.Count > 0)
                {
                    // currentVarNames[int] = "p$.ProductID";
                    this.currentVarNames[body.Type] = result.columns[0];
                }
            }
        }

        void ProcessOrderByClause(LambdaExpression orderByExpr, string orderBy_desc)
        {
            ParseResult result = ExpressionTreeParser.Parse(_vars.Context.Vendor, this, orderByExpr.Body);
            string orderByFields = string.Join(",", result.columns.ToArray());
            _vars._sqlParts.orderByList.Add(orderByFields);
            _vars._sqlParts.orderBy_desc = orderBy_desc; //copy 'DESC' specifier
        }

        void ProcessJoinClause(MethodCallExpression joinExpr)
        {
            ParseResult result;

            if (joinExpr == null || joinExpr.Arguments.Count != 5)
                throw new ArgumentOutOfRangeException("L112 Currently only handling 5-arg Joins");

            Expression arg2 = joinExpr.Arguments[2]; //{p => p.ProductID}
            Expression arg3 = joinExpr.Arguments[3]; //{o => o.OrderID}
            Expression arg4 = joinExpr.Arguments[4]; //{(p, o) => new <>f__AnonymousType9`2(ProductName = p.ProductName, CustomerID = o.CustomerID)}

            string joinField1, joinField2;
            //processSelectClause(arg2.XLambda());
            {
                result = ExpressionTreeParser.Parse(_vars.Context.Vendor, this, arg2.XLambda().Body);
                joinField1 = result.columns[0]; // "p$.ProductID"
                result.CopyInto(this, _vars._sqlParts); //transfer params and tablesUsed
            }
            {
                result = ExpressionTreeParser.Parse(_vars.Context.Vendor, this, arg3.XLambda().Body);
                joinField2 = result.columns[0]; // "p$.ProductID"
                result.CopyInto(this, _vars._sqlParts); //transfer params and tablesUsed
            }

            bool doPermFields = DoesJoinAssignPermanentFields(arg4.XLambda());
            if (doPermFields)
            {
                ProcessSelectClause(arg4.XLambda());
            }
            else
            {
                //skip processing - only mentions temp objects
                //e.g. '{(m, u) => new <>f__AnonymousType0`2(m = m, u = u)}'
            }

            _vars._sqlParts.joinList.Add(joinField1 + "=" + joinField2);
        }

        /// <summary>
        /// return true for '{(p, o) => new <>f__AnonymousType9`2(ProductName = p.ProductName, CustomerID = o.CustomerID)}'
        ///  because it assigns user-visible field ProductName.
        ///  
        /// return false for '{(m, u) => new <>f__AnonymousType0`2(m = m, u = u)}',
        ///  because it only works with temp variables m,u.
        /// </summary>
        /// <returns></returns>
        static bool DoesJoinAssignPermanentFields(LambdaExpression arg4)
        {
            //warning: nasty string handling here
            string arg4str = arg4.ToString();
            int indxAnon = arg4str.IndexOf("<>f_AnonymousType");
            if (indxAnon < 0)
                return false;
            int indxBracket = arg4str.IndexOf("(", indxAnon);
            if (indxBracket < 0)
                return false;
            string tail = arg4str.Substring(indxBracket + 1);
            bool hasDot = tail.Contains(".");
            return hasDot;
        }

        private void ProcessSelectMany(MethodCallExpression exprCall)
        {
            //see ReadTest.D07_OrdersFromLondon_Alt(), or Join.LinqToSqlJoin01() for example
            //special case: SelectMany can come with 2 or 3 params
            switch (exprCall.Arguments.Count)
            {
                case 2: //???
                    ProcessSelectManyLambdaSimple(exprCall.Arguments[1].XLambda());
                    return;
                case 3: //'from c in db.Customers from o in c.Orders where c.City == "London" select new { c, o }'
                    //arg[0]: MTable<> - type info, ignore
                    //arg[1]: c=>c.Orders - provides join information
                    //arg[2]: (c, o) => new <>f__AnonymousType0`2(c = c, o = o)) --ignore

                    LambdaExpression lambda1 = exprCall.Arguments[1].XLambda();
                    LambdaExpression lambda2 = exprCall.Arguments[2].XLambda();
                    {
                        MemberExpression memberExpression = lambda1.Body as MemberExpression;
                        ParameterExpression paramExpression = lambda2.Parameters[1];
                        ParseResult result = new ParseResult(_vars.Context.Vendor);
                        JoinBuilder.AddJoin1(memberExpression, paramExpression, result);
                        result.CopyInto(this, _vars._sqlParts);  //transfer params and tablesUsed
                    }

                    //processSelectClause(lambda2);
                    return;
                default:
                    throw new ApplicationException("processSelectMany: Prepared only for 2 or 3 param GroupBys");
            }
        }

        private void ProcessSelectManyLambdaSimple(LambdaExpression selectExpr)
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

        private void ProcessGroupByCall(MethodCallExpression exprCall)
        {
            //special case: GroupBy can come with 2 or 3 params
            switch (exprCall.Arguments.Count)
            {
                case 2: //'group o by o.CustomerID into g'
                    ProcessGroupByLambda(exprCall.Arguments[1].XLambda());
                    return;
                case 3: //'group new {c.PostalCode, c.ContactName} by c.City into g'
                    ProcessGroupByLambda(exprCall.Arguments[1].XLambda());
                    _vars.groupByNewExpr = exprCall.Arguments[2].XLambda();
                    return;
                default:
                    throw new ApplicationException("processGroupBy: Prepared only for 2 or 3 param GroupBys");
            }
        }

        private void ProcessGroupByLambda(LambdaExpression groupBy)
        {
            _vars.groupByExpr = groupBy;
            ParseResult result = ExpressionTreeParser.Parse(_vars.Context.Vendor, this, groupBy.Body);
            string groupByFields = string.Join(",", result.columns.ToArray());
            _vars._sqlParts.groupByList.Add(groupByFields);

            if (selectExpr == null //&& _vars.groupByNewExpr==null
                )
            {
                //manually add "SELECT c.City"
                //_vars._sqlParts.AddSelect(result.columns);

                result.CopyInto(this, _vars._sqlParts); //transfer params and tablesUsed
            }

            if (_vars.groupByNewExpr != null)
            {
                result = ExpressionTreeParser.Parse(_vars.Context.Vendor, this, _vars.groupByNewExpr.Body);
                _vars._sqlParts.AddSelect(result.columns);
                result.CopyInto(this, _vars._sqlParts); //transfer params and tablesUsed
            }
        }

        void ProcessGroupJoin(MethodCallExpression exprCall)
        {
            //occurs in LinqToSqlJoin10()
            switch (exprCall.Arguments.Count)
            {
                case 5:
                    LambdaExpression l1 = exprCall.Arguments[1].XLambda();
                    LambdaExpression l2 = exprCall.Arguments[2].XLambda();
                    ProcessJoinClause(exprCall);
                    break;
                default:
                    throw new ApplicationException("processGroupJoin: Prepared only for 5 param GroupBys");
            }
            Console.WriteLine("TODO L299 Support GroupJoin()");
        }

    }
}

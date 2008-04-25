#region MIT license
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
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using DbLinq.Linq.Clause;
using DbLinq.Logging;
using DbLinq.Util;
using DbLinq.Util.ExprVisitor;

namespace DbLinq.Linq
{
    /// <summary>
    /// after all Lambdas are collected, and GetEnumerator() is called:
    /// QueryProcessor calls ExpressionTreeParser to build SQL expression from parts
    /// </summary>
    partial class QueryProcessor
    {
        void ProcessWhereClause(LambdaExpression lambda)
        {
            MethodCallExpression call = lambda.Body.XMethodCall();
            if (call != null && call.Method.Name == "All")
            {
                ProcessWhereClause_All(call);
                return;
            }

            ParseResult result = ExpressionTreeParser.Parse(_vars.Context.Vendor, this, lambda.Body);

            if (GroupHelper.IsGrouping(lambda.Parameters[0].Type))
            {
                _vars.SqlParts.AddHaving(result.columns);
            }
            else
            {
                _vars.SqlParts.AddWhere(result.columns);
            }

            result.CopyInto(this, _vars.SqlParts); //transfer params and tablesUsed
        }

        /// <summary>
        /// process 'All' subclause 
        /// </summary>
        /// <param name="call"></param>
        void ProcessWhereClause_All(MethodCallExpression call)
        {
            //WARNING: this is still a hack
            //TODO: All and Any are similar and should be handled by one function
            //at the moment, they are handled by ProcessWhereClause_All and AnalyzeMethodCall_Queryable

            Expression arg0 = call.Arguments[0]; //{c.Orders.Select(o => o)}
            Expression arg1 = call.Arguments[1]; //{o => (o.ShipCity = c.City)}
            MethodCallExpression sel0 = arg0.XMethodCall(); //it's a Select()
            Expression sel0arg0 = sel0.Arguments[0]; //Member
            //Expression sel0arg1 = sel0.Arguments[1]; //Quote of Lambda - ignore
            Expression arg1B = arg1.XLambda().Body;
            //Console.WriteLine("" + arg0);

            ParseResult result3 = ExpressionTreeParser.Parse(_vars.Context.Vendor, this, arg1B);

            ExpressionTreeParser.RecurData recurData = new ExpressionTreeParser.RecurData { allowSelectAllFields = false };
            ParseResult result1 = ExpressionTreeParser.Parse(recurData, _vars.Context.Vendor, this, sel0arg0);
            //ParseResult result2 = ExpressionTreeParser.Parse(_vars.Context.Vendor, this, sel0arg1);

            //WHERE 
            //(    SELECT  COUNT(*) 
            //    FROM Orders AS t1
            //    WHERE ((t1.CustomerID = t0.CustomerID) AND  NOT  ((t1.ShipCity = t0.City)))
            //) = 0
            string whereClauseFmt = " ( SELECT COUNT(*) FROM {0} AS {1} WHERE {2} AND NOT ({3}) )=0  ";

            var tablesUsed3 = result3.tablesUsed.First();
            string tableName = AttribHelper.GetTableAttrib(tablesUsed3.Key).Name;
            string nicknameOk = tablesUsed3.Value;
            string nicknameBad = result1.tablesUsed.First().Value;

            string join1 = result1.joins[0];
            string join2 = join1.Replace(nicknameBad, nicknameOk);

            string whereClause = string.Format(whereClauseFmt, tableName, nicknameOk
                                        , join2
                                        , result3.columns[0]);
            _vars.SqlParts.AddWhere(whereClause);
        }



        void ProcessSelectClause(LambdaExpression selectExpr)
        {
            this.selectExpr = selectExpr; //store for GroupBy & friends

            //necesary for projections?
            if (_vars.GroupByExpression == null)
            {
                _vars.ProjectionData = ProjectionData.FromSelectExpr(selectExpr);
            }
            else
            {
                _vars.ProjectionData = ProjectionData.FromSelectGroupByExpr(selectExpr, _vars.GroupByExpression, _vars.SqlParts);
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
                _vars.SqlParts.AddSelect(result.columns);
                result.CopyInto(this, _vars.SqlParts); //transfer params and tablesUsed
            }
        }

        void ProcessOrderByClause(LambdaExpression orderByExpr, string orderDirection)
        {
            ParseResult result = ExpressionTreeParser.Parse(_vars.Context.Vendor, this, orderByExpr.Body);
            string orderByFields = string.Join(",", result.columns.ToArray());
            _vars.SqlParts.OrderByList.Add(orderByFields);
            _vars.SqlParts.OrderDirection = orderDirection; //copy 'DESC' specifier
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
                result.CopyInto(this, _vars.SqlParts); //transfer params and tablesUsed
            }
            {
                result = ExpressionTreeParser.Parse(_vars.Context.Vendor, this, arg3.XLambda().Body);
                joinField2 = result.columns[0]; // "p$.ProductID"
                result.CopyInto(this, _vars.SqlParts); //transfer params and tablesUsed
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

            _vars.SqlParts.JoinList.Add(joinField1 + "=" + joinField2);
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
            int indxAnon = arg4str.IndexOf("<>f__AnonymousType");
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
            if (_didGroupJoin)
            {
                ProcessSelectMany_PostJoin(exprCall);
                return;
            }

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
                        result.CopyInto(this, _vars.SqlParts);  //transfer params and tablesUsed
                    }

                    //processSelectClause(lambda2);
                    return;
                default:
                    throw new ApplicationException("processSelectMany: Prepared only for 2 or 3 param GroupBys");
            }
        }

        private void ProcessSelectMany_PostJoin(MethodCallExpression exprCall)
        {
            //see ReadTest.D07_OrdersFromLondon_Alt(), or Join.LinqToSqlJoin01() for example
            //special case: SelectMany can come with 2 or 3 params
            switch (exprCall.Arguments.Count)
            {
                case 2: //???
                    ProcessSelectManyLambdaSimple(exprCall.Arguments[1].XLambda());
                    return;
                case 3:
                    //(A) subsequent SelectMany contains:
                    //[0]{value(DbLinq.Linq.MTable_Projected`1[<>f__AnonymousType1a`2[nwind.Order,IEnumerable`1[nwind.Employee]]])}
                    //[1]{<>h__TransparentIdentifier4 => <>h__TransparentIdentifier4.emps}
                    //[2]{(<>h__TransparentIdentifier4, e) 
                    //=> new <>f__AnonymousType1b`2(OrderID = <>h__TransparentIdentifier4.o.OrderID, FirstName = e.FirstName)}

                    //(B) subsequent SelectMany for LeftOuterJoin_DefaultIfEmpty
                    //[0]{value(DbLinq.Linq.MTable_Projected`1[<>f__AnonymousType1c`2[nwind.Customer,System.Collections.Generic.IEnumerable`1[nwind.Order]]])}
                    //[1]{<>h__TransparentIdentifier6 => <>h__TransparentIdentifier6.oc.DefaultIfEmpty()}
                    //[2]{(<>h__TransparentIdentifier6, x) => new <>f__AnonymousType1d`2(<>h__TransparentIdentifier6 = <>h__TransparentIdentifier6, x = x)}

                    LambdaExpression lambda1 = exprCall.Arguments[1].XLambda();
                    LambdaExpression lambda2 = exprCall.Arguments[2].XLambda();
                    {
                        //ParameterExpression paramExpression = lambda2.Parameters[1];
                        ParseResult result = ExpressionTreeParser.Parse(_vars.Context.Vendor, this, lambda2.Body);
                        _vars.SqlParts.AddSelect(result.columns);
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
                    ProcessQuery(whereCall);
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
                    _vars.GroupByNewExpression = exprCall.Arguments[2].XLambda();
                    return;
                default:
                    throw new ApplicationException("processGroupBy: Prepared only for 2 or 3 param GroupBys");
            }
        }

        private void ProcessGroupByLambda(LambdaExpression groupBy)
        {
            _vars.GroupByExpression = groupBy;
            ParseResult result = ExpressionTreeParser.Parse(_vars.Context.Vendor, this, groupBy.Body);
            string groupByFields = string.Join(",", result.columns.ToArray());
            _vars.SqlParts.GroupByList.Add(groupByFields);

            if (selectExpr == null //&& _vars.groupByNewExpr==null
                )
            {
                //manually add "SELECT c.City"
                //_vars.SqlParts.AddSelect(result.columns);

                result.CopyInto(this, _vars.SqlParts); //transfer params and tablesUsed
            }

            if (_vars.GroupByNewExpression != null)
            {
                result = ExpressionTreeParser.Parse(_vars.Context.Vendor, this, _vars.GroupByNewExpression.Body);
                _vars.SqlParts.AddSelect(result.columns);
                result.CopyInto(this, _vars.SqlParts); //transfer params and tablesUsed
            }
        }

        void ProcessGroupJoin(MethodCallExpression exprCall)
        {
            _didGroupJoin = true;
            //our test case: occurs in LinqToSqlJoin10()

            //reading materials for GroupJoin:
            //http://blogs.msdn.com/vbteam/rss_tag_LINQ_2F00_VB9.xml by Bill Horst
            //http://www.developer.com/db/article.php/3739391 by Paul Kimmel
            //http://weblogs.asp.net/fbouma/archive/2007/11/23/developing-linq-to-llblgen-pro-part-9.aspx by Frans Brouma

            //Bart-De-Smet's Linq-To-SQO (to learn how operators work internally)
            //http://community.bartdesmet.net/blogs/bart/archive/2007/07/28/linq-sqo-v0-9-2-for-orcas-beta-2-rtw.aspx

            switch (exprCall.Arguments.Count)
            {
                case 5:
                    //arg[0]: {value(DbLinq.Linq.Table`1[nwind.Order])}
                    //arg[1]: {value(DbLinq.Linq.Table`1[nwind.Employee])}
                    //arg[2]: {o => o.EmployeeID}
                    //arg[3]: {e => Convert(e.EmployeeID)}
                    //arg[4]: {(o, emps) => new <>f__AnonymousType1a`2(o = o, emps = emps)}

                    //(A) subsequent SelectMany contains:
                    //[0]{value(DbLinq.Linq.MTable_Projected`1[<>f__AnonymousType1a`2[nwind.Order,IEnumerable`1[nwind.Employee]]])}
                    //[1]{<>h__TransparentIdentifier4 => <>h__TransparentIdentifier4.emps}
                    //[2]{(<>h__TransparentIdentifier4, e) 
                    //=> new <>f__AnonymousType1b`2(OrderID = <>h__TransparentIdentifier4.o.OrderID, FirstName = e.FirstName)}

                    //(B) subsequent SelectMany for LeftOuterJoin_DefaultIfEmpty
                    //[0]{value(DbLinq.Linq.MTable_Projected`1[<>f__AnonymousType1c`2[nwind.Customer,System.Collections.Generic.IEnumerable`1[nwind.Order]]])}
                    //[1]{<>h__TransparentIdentifier6 => <>h__TransparentIdentifier6.oc.DefaultIfEmpty()}
                    //[2]{(<>h__TransparentIdentifier6, x) => new <>f__AnonymousType1d`2(<>h__TransparentIdentifier6 = <>h__TransparentIdentifier6, x = x)}

                    LambdaExpression l1 = exprCall.Arguments[1].XLambda();
                    LambdaExpression l2 = exprCall.Arguments[2].XLambda();
                    LambdaExpression l4 = exprCall.Arguments[4].XUnary().Operand.XLambda();
                    ProcessJoinClause(exprCall);
                    break;
                default:
                    throw new ApplicationException("processGroupJoin: Prepared only for 5 param GroupBys");
            }
            Logger.Write(Level.Error, "TODO L299 Support GroupJoin()");
        }

        void ProcessUnionClause(MethodCallExpression unionExpr)
        {
            //ConstantExpression c0 = unionExpr.Arguments[0].XConstant();
            ConstantExpression c1 = unionExpr.Arguments[1].XConstant();
            //IGetSessionVars mtableProjected0 = c0.Value as IGetSessionVars;
            IGetSessionVars mtableProjected1 = c1.Value as IGetSessionVars;
            //SessionVars vars0 = mtableProjected0.SessionVars;
            SessionVars vars1 = mtableProjected1.SessionVars;

            SqlExpressionParts sqlPart1 = _vars.SqlParts;
            SqlExpressionParts sqlPart2 = new SqlExpressionParts(_vars.Context.Vendor);
            sqlPart1.UnionPart2 = sqlPart2;

            //start populating part2 in calls to ProcessQuery:
            _vars.SqlParts = sqlPart2;

            foreach (MethodCallExpression expr in vars1.ExpressionChain)
            {
                this.ProcessQuery(expr);
            }

            //restore part1, it knows to include part2 during ToString()
            _vars.SqlParts = sqlPart1;
        }


    }
}

#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne
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
using DbLinq.Vendor;

namespace DbLinq.Linq
{
    internal partial class QueryProcessor
    {
        void ProcessGroupJoin(MethodCallExpression exprCall)
        {
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

                    //(B) subsequent SelectMany for LeftJoin_DefaultIfEmpty test:
                    //[0]{value(DbLinq.Linq.MTable_Projected`1[<>f__AnonymousType1c`2[nwind.Customer,System.Collections.Generic.IEnumerable`1[nwind.Order]]])}
                    //[1]{<>h__TransparentIdentifier6 => <>h__TransparentIdentifier6.oc.DefaultIfEmpty()}
                    //[2]{(<>h__TransparentIdentifier6, x) => new <>f__AnonymousType1d`2(<>h__TransparentIdentifier6 = <>h__TransparentIdentifier6, x = x)}

                    LambdaExpression l1 = exprCall.Arguments[1].XLambda();
                    LambdaExpression l2 = exprCall.Arguments[2].XLambda();
                    LambdaExpression l3 = exprCall.Arguments[3].XLambda();
                    LambdaExpression l4 = exprCall.Arguments[4].XUnary().Operand.XLambda();
                    _prevGroupJoinExpression = l3;
                    ProcessJoinClause(exprCall);
                    break;
                default:
                    throw new ApplicationException("processGroupJoin: Prepared only for 5 param GroupBys");
            }
            Logger.Write(Level.Error, "TODO L299 Support GroupJoin()");
        }

        private void ProcessSelectMany_PostJoin(MethodCallExpression exprCall)
        {
            //see ReadTest.D07_OrdersFromLondon_Alt(), or Join.LinqToSqlJoin01() for example
            //special case: SelectMany can come with 2 or 3 params
            if (exprCall.Arguments.Count != 3)
                throw new ArgumentException("L273: Expected SelectMany with 3 args - got " + exprCall);

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
            Expression selectExpr = lambda2.Body;

            MethodCallExpression defaultIfEmpty = lambda1.Body.XMethodCall();
            if (defaultIfEmpty != null && defaultIfEmpty.Method.Name == "DefaultIfEmpty")
            {
                //it's not a JOIN, it's a LEFT JOIN
                int joinCount = _vars.SqlParts.JoinList.Count;
                if (joinCount == 0)
                    throw new InvalidOperationException("L287 PoistJoin called with empty JoinList");
                _vars.SqlParts.JoinList.Last().JoinType = JoinSpec.JoinTypeEnum.Left;

                //MemberExpression matchExpr = defaultIfEmpty.Arguments[0].XMember(); //{<>h__TransparentIdentifier8.temp}
                Type joinRightSide = lambda2.Body.Type;
                string rightSideVarname = lambda2.Parameters[1].Name;
                System.Reflection.PropertyInfo propInfo = joinRightSide.GetProperty(rightSideVarname);
                Expression pattern = null;
                if (propInfo == null)
                {
                    //replace param for param
                    pattern = lambda2.Parameters[1];
                }
                else
                {
                    ParameterExpression prevParam = _prevGroupJoinExpression.Parameters[0];
                    _expressionModifiers.Add(new ParentExpressionRemover(propInfo, prevParam));
                }

                if (HasAnotherSelect(exprCall))
                    return;

                if (pattern != null)
                {
                    //throw new ApplicationException("Hardcoded OOO");
                    ParameterExpression replacementExpr = Expression.Parameter(pattern.Type, "name_not_used");
                    Expression selectExpr2 = ExpressionRegex.Replace(selectExpr, pattern, replacementExpr);
                    selectExpr = selectExpr2;
                }
            }
            {
                ExpressionTreeParser.RecurData recurData = new ExpressionTreeParser.RecurData { allowSelectAllFields = false };
                ParseResult result = ExpressionTreeParser.Parse(recurData, _vars.Context.Vendor, this, selectExpr);
                _vars.SqlParts.AddSelect(result.columns);
            }

            //processSelectClause(lambda2);
        }

        bool HasAnotherSelect(MethodCallExpression callExpr)
        {
            int currPos = _vars.ExpressionChain.IndexOf(callExpr);

            for (int i = currPos + 1; i < _vars.ExpressionChain.Count; i++)
            {
                MethodCallExpression expr = _vars.ExpressionChain[i];
                if (expr.Method.Name == "Select")
                    return true;
            }
            return false;

            //foreach (MethodCallExpression expr in this._vars.ExpressionChain)
            //{
            //    queryProcessor.ProcessQuery(expr);
            //}
        }

    }
}

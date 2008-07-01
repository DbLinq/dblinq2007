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
using System.Data.Linq;
using System.Data.Linq.Mapping;

#if MONO_STRICT
using System.Data.Linq;
#else
using DbLinq.Data.Linq;
#endif

using DbLinq.Util;
using DbLinq.Vendor;
using DbLinq.Util.ExprVisitor;

namespace DbLinq.Linq.Clause
{
    public class JoinBuilder
    {
        /// <summary>
        /// Given memberEx={c.Orders}, and paramExpr={o}, 
        /// add sql JOIN clause: 'c$.CustomerID=o$.CustomerID'.
        /// This is used from SelectMany.
        /// </summary>
        public static void AddJoin1(MemberExpression memberExpr, QueryProcessor qp, ParameterExpression paramExpr, ParseResult result)
        {
            AttribAndProp attribAndProp;
            bool isAssoc1 = AttribHelper.IsAssociation(memberExpr, out attribAndProp);
            if (!isAssoc1)
                return; //no join
            AssociationAttribute assoc1 = attribAndProp.assoc;

            //user passed in part of SelectMany, eg. 'c.Orders'
            string nick1 = VarName.GetSqlName(memberExpr.Expression.XParam().Name); // c$
            string nick2 = VarName.GetSqlName(paramExpr.Name); //o$
            AssociationAttribute assoc2 = AttribHelper.FindReverseAssociation(attribAndProp);
            if (assoc2 == null)
                throw new ApplicationException("Failed to find reverse assoc for " + assoc1.Name);

            var vendor = qp._vars.Context.Vendor;
            var type1 = memberExpr.Expression.Type;
            var type2 = paramExpr.Type;
            var otherKeyColumn = vendor.GetSqlFieldSafeName(AttribHelper.GetColumnAttribute(type1, assoc1.OtherKey).Name);
            var thisKeyColumn = vendor.GetSqlFieldSafeName(AttribHelper.GetColumnAttribute(type2, assoc2.ThisKey).Name);
            //string joinString = nick1 + "." + otherKeyColumn + "=" + nick2 + "." + thisKeyColumn;
            string joinLeft = nick1 + "." + otherKeyColumn;
            string joinRight = nick2 + "." + thisKeyColumn;

            //Type childType = AttribHelper.ExtractTypeFromMSet(memberExpr.Type);
            //result.addJoin(joinString);
            TableSpec tblLeft = vendor.FormatTableSpec(memberExpr.Expression.XParam());
            TableSpec tblRight = vendor.FormatTableSpec(paramExpr);
            JoinSpec js = new JoinSpec() { LeftSpec = tblLeft, LeftField = joinLeft, RightSpec = tblRight, RightField = joinRight };
            result.addJoin(js);

            result.tablesUsed[memberExpr.Expression.Type] = nick1;//tablesUsed[Customer] = c$
            //result.tablesUsed[childType] = nick2;           //tablesUsed[Order] = join$
            result.tablesUsed[paramExpr.Type] = nick2;           //tablesUsed[Order] = join$

            //result.memberExprNickames[memberExpr] = nick1; //memberExprNickames[c.Orders] = "o$"
        }

        /// <summary>
        /// process 'o.Customer.City':
        /// a) insert join $o.CustomerId=$c.CustomerID
        /// b) insert into our StringBuilder '$c.City'
        /// c) insert into tablesUsed[Customer]='$c' and tablesUsed[Order]='$o'
        /// </summary>
        public static void AddJoin2(ExpressionTreeParser.RecurData recurData, QueryProcessor qp, MemberExpression exprOuter, ParseResult result)
        {
            string nick1, nick2;
            AttribAndProp attribAndProp1;
            AssociationAttribute assoc1, assoc2;
            Type type2;

            MemberExpression exprOuterOrig = exprOuter;
            Expression exprInner = exprOuter.Expression;
            var vendor = qp._vars.Context.Vendor;

            if (exprOuter.Expression.NodeType == ExpressionType.MemberAccess)
            {
                //as of Beta2, the former 'p.ProductID' now appears here as '<>h__TransparentIdentifier.p.ProductID'
                //the 'p' used to be a ParameterExpression - not anymore
                MemberExpression member1 = exprOuter.Expression.XMember(); //'<>h__TransparentIdentifier.p
                string member1Name = member1.Expression.ToString();
                if (member1Name.StartsWith("<>h__TransparentIdentifier"))
                {
                    //turn '<>h__TransparentIdentifier.p.ProductID' into 'p.ProductID'
                    string nameP = member1.Member.Name; //'p'
                    System.Reflection.PropertyInfo propInfoP = member1.Member as System.Reflection.PropertyInfo;
                    Type typeP = propInfoP.PropertyType; //typeof(Product)
                    ParameterExpression fakeParam = Expression.Parameter(typeP, nameP);
                    exprInner = fakeParam;
                    exprOuter = Expression.MakeMemberAccess(fakeParam, exprOuterOrig.Member);
                }

            }

            switch (exprInner.NodeType)
            {
                case ExpressionType.MemberAccess:
                    {
                        //eg. "SELECT order.Product.ProductID"
                        MemberExpression member = exprInner.XMember();
                        bool isAssoc = AttribHelper.IsAssociation(member, out attribAndProp1);
                        if (!isAssoc)
                        {
                            throw new Exception("L113 AddJoin2: member1.member2.member3 only allowed for associations, not for: " + member);
                        }
                        assoc1 = attribAndProp1.assoc;
                        nick1 = qp.NicknameRequest(exprOuter, assoc1);
                        //store this nickname for subsequent selects:
                        //result.memberExprNickames[member] = nick1;
                        qp.memberExprNickames[member] = nick1;
                        nick1 = VarName.GetSqlName(nick1);

                        nick2 = member.Expression.XParam().Name;
                        nick2 = VarName.GetSqlName(nick2);

                        type2 = member.Expression.Type;

                        //System.Reflection.PropertyInfo propInfo = exprOuter.Member as System.Reflection.PropertyInfo;
                        string sqlColumnName = AttribHelper.GetSQLColumnName(exprOuter.Member)
                            ?? exprOuter.Member.Name; //'City' or 'Content'

                        result.AppendString(nick1 + "." + sqlColumnName); //where clause: '$c.City'
                        break;
                    }
                case ExpressionType.Parameter:
                    {
                        //eg. "SELECT order.Product"
                        ParameterExpression paramExpr = exprInner.XParam();

                        bool isAssoc = AttribHelper.IsAssociation(exprOuter, out attribAndProp1);
                        if (!isAssoc)
                        {
                            throw new Exception("L143 AddJoin2: member1.member2 only allowed for associations, not for: " + exprOuter.Expression);
                        }

                        assoc1 = attribAndProp1.assoc;

                        Type outerType = exprOuter.Type; //eg. EntityMSet<Order>
                        if (outerType.IsGenericType && outerType.GetGenericTypeDefinition() == typeof(EntityMSet<>))
                        {
                            outerType = outerType.GetGenericArguments()[0]; //extract Order from EntityMSet<Order>
                        }
                        type2 = outerType;

                        //nickname for parent table (not available in Expr tree) - eg. "p94$" for Products
                        string nick2Inner;
                        if (qp.memberExprNickames.TryGetValueEx(exprOuter, out nick2Inner))
                        {
                            //this join was processed previously
                        }
                        else
                        {
                            string parentTypeName = outerType.Name;
                            nick2Inner = Char.ToLower(parentTypeName[0]) + "94";
                            qp.memberExprNickames[exprOuterOrig] = nick2Inner;

                            //replace subsequent occurences of {o.Customer} with {c94}
                            ParameterExpression replacementParam = Expression.Parameter(outerType, nick2Inner);
                            qp._expressionModifiers.Add(new ExpressionRegex(exprOuter, replacementParam));

                        }
                        //eg. nick2="s94$" when doing "select p.Supplier from Products"
                        nick2 = VarName.GetSqlName(nick2Inner);
                        nick1 = VarName.GetSqlName(paramExpr.Name);

                        if (recurData.allowSelectAllFields)
                        {
                            SqlExpressionParts sqlParts = new SqlExpressionParts(vendor);
                            FromClauseBuilder.SelectAllFields(null, sqlParts, type2, nick2);
                            result.AppendString(sqlParts.GetSelect());
                        }
                        break;
                    }
                default:
                    throw new Exception("L49 AddJoin2: member1.member2.member3 only allowed for associations, not for: " + exprOuter.Expression);
            }

            Type type1 = exprOuter.Expression.Type;
            assoc2 = AttribHelper.FindReverseAssociation(attribAndProp1);

            // TODO here: ThisKey and OtherKey are property names, not column names
            //            --> get column name from ColumnAttribute

            // picrap: this test is probably inaccurate, it should rely on other properties (... to be determined)
            if (assoc1.OtherKey != null && assoc2.ThisKey != null)
            {
                string otherKeyColumn, thisKeyColumn;
                otherKeyColumn = vendor.GetSqlFieldSafeName(AttribHelper.GetColumnAttribute(type1, assoc1.OtherKey).Name);
                JoinSpec js = new JoinSpec();

                if (assoc2.OtherKey == null)
                    // O2_OperatorAny() and O1_OperatorAll() tests.
                    thisKeyColumn = vendor.GetSqlFieldSafeName(AttribHelper.GetColumnAttribute(type2, assoc2.ThisKey).Name);
                else
                {
                    // used to retrieve associated propery if column names are different in both sides of association
                    thisKeyColumn = vendor.GetSqlFieldSafeName(AttribHelper.GetColumnAttribute(type2, assoc2.OtherKey).Name);
                    js.JoinType = JoinSpec.JoinTypeEnum.Left;
                }

                //string joinString = "$c.CustomerID=$o.CustomerID"
                string joinLeft = nick1 + "." + otherKeyColumn;
                string joinRight = nick2 + "." + thisKeyColumn;
                TableSpec tblLeft = vendor.FormatTableSpec(type2, nick2);
                TableSpec tblRight = vendor.FormatTableSpec(type1, nick1);
                js.LeftSpec = tblLeft;
                js.LeftField = joinLeft;
                js.RightSpec = tblRight;
                js.RightField = joinRight;
                result.addJoin(js);

            }
            else
            {
                JoinSpec.JoinTypeEnum joinType = JoinSpec.JoinTypeEnum.Plain;
                if(attribAndProp1!=null && attribAndProp1.columnAttribute!=null && attribAndProp1.columnAttribute.CanBeNull)
                    joinType = JoinSpec.JoinTypeEnum.Left;

                var thisKeyColumn = vendor.GetSqlFieldSafeName(AttribHelper.GetColumnAttribute(type2, assoc1.ThisKey).Name);
                var otherKeyColumn = vendor.GetSqlFieldSafeName(AttribHelper.GetColumnAttribute(type1, assoc2.OtherKey).Name);
                //string joinString = "$c.CustomerID=$o.CustomerID"
                //string joinString = nick1 + "." + thisKeyColumn + "=" + nick2 + "." + otherKeyColumn;
                //result.addJoin(nick1 + "." + thisKeyColumn, nick2 + "." + otherKeyColumn);

                string joinLeft = nick1 + "." + thisKeyColumn;
                string joinRight = nick2 + "." + otherKeyColumn;
                TableSpec tblLeft = vendor.FormatTableSpec(type2, nick2);
                TableSpec tblRight = vendor.FormatTableSpec(type1, nick1);

                JoinSpec js = new JoinSpec() { LeftSpec = tblLeft, LeftField = joinLeft, RightSpec = tblRight, RightField = joinRight, JoinType = joinType };
                //js.JoinType = JoinSpec.JoinTypeEnum.Left;
                result.addJoin(js);
            }

#if FailsIfAssociatonSideColumnNamesAreDifferent
            if (assoc1.OtherKey != null && assoc2.ThisKey != null)
                {
                var otherKeyColumn = vendor.GetSqlFieldSafeName(AttribHelper.GetColumnAttribute(type1, assoc1.OtherKey).Name);
                var thisKeyColumn = vendor.GetSqlFieldSafeName(AttribHelper.GetColumnAttribute(type2, assoc2.ThisKey).Name);
                
                //string joinString = "$c.CustomerID=$o.CustomerID"
                string joinLeft = nick1 + "." + otherKeyColumn;
                string joinRight = nick2 + "." + thisKeyColumn;
                TableSpec tblLeft = vendor.FormatTableSpec(type2, nick2);
                TableSpec tblRight = vendor.FormatTableSpec(type1, nick1);
                JoinSpec js = new JoinSpec() { LeftSpec = tblLeft, LeftField = joinLeft, RightSpec = tblRight, RightField = joinRight };
                //js.JoinType = JoinSpec.JoinTypeEnum.Left;
                result.addJoin(js);

            }
            else
            {
                var thisKeyColumn = vendor.GetSqlFieldSafeName(AttribHelper.GetColumnAttribute(type2, assoc1.ThisKey).Name);
                var otherKeyColumn = vendor.GetSqlFieldSafeName(AttribHelper.GetColumnAttribute(type1, assoc2.OtherKey).Name);
                //string joinString = "$c.CustomerID=$o.CustomerID"
                //string joinString = nick1 + "." + thisKeyColumn + "=" + nick2 + "." + otherKeyColumn;
                //result.addJoin(nick1 + "." + thisKeyColumn, nick2 + "." + otherKeyColumn);

                string joinLeft = nick1 + "." + thisKeyColumn;
                string joinRight = nick2 + "." + otherKeyColumn;
                TableSpec tblLeft = vendor.FormatTableSpec(type2, nick2);
                TableSpec tblRight = vendor.FormatTableSpec(type1, nick1);
                JoinSpec js = new JoinSpec() { LeftSpec = tblLeft, LeftField = joinLeft, RightSpec = tblRight, RightField = joinRight };
                //js.JoinType = JoinSpec.JoinTypeEnum.Left;
                result.addJoin(js);
            }
#endif

            //order matters: 
            //for self join, this order of statements loses tablesUsed[Employee]='join$' but preserves tablesUsed[Employee]='e$'
            result.tablesUsed[type1] = nick1;    //tablesUsed[Customer] = $join
            result.tablesUsed[type2] = nick2;    //tablesUsed[Order] = $o

        }
    }
}

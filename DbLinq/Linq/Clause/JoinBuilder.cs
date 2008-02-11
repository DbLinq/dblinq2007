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
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using DBLinq.util;
using DBLinq.vendor;

namespace DBLinq.Linq.clause
{
    public class JoinBuilder
    {
        /// <summary>
        /// Given memberEx={c.Orders}, and paramExpr={o}, 
        /// add sql JOIN clause: 'c$.CustomerID=o$.CustomerID'.
        /// This is used from SelectMany.
        /// </summary>
        public static void AddJoin1(MemberExpression memberExpr, ParameterExpression paramExpr, ParseResult result)
        {
            AttribAndProp attribAndProp;
            AssociationAttribute assoc1, assoc2;
            bool isAssoc1 = AttribHelper.IsAssociation(memberExpr, out attribAndProp);
            if( ! isAssoc1)
                return; //no join
            assoc1 = attribAndProp.assoc;

            //user passed in part of SelectMany, eg. 'c.Orders'
            string nick1 = VarName.GetSqlName(memberExpr.Expression.XParam().Name); // c$
            string nick2 = VarName.GetSqlName(paramExpr.Name); //o$
            assoc2 = AttribHelper.FindReverseAssociation(attribAndProp);
            if (assoc2 == null)
                throw new ApplicationException("Failed to find reverse assoc for " + assoc1.Name);
            string joinString = nick1+"."+assoc1.OtherKey+"="+nick2+"."+assoc2.ThisKey;

            //Type childType = AttribHelper.ExtractTypeFromMSet(memberExpr.Type);
            result.addJoin(joinString);
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
        public static void AddJoin2(QueryProcessor qp, MemberExpression exprOuter, ParseResult result)
        {
            string nick1, nick2;
            AttribAndProp attribAndProp1;
            AssociationAttribute assoc1, assoc2;
            Type type2;

            MemberExpression exprOuterOrig = exprOuter;
            Expression exprInner = exprOuter.Expression;

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
                            throw new Exception("L55 AddJoin2: member1.member2.member3 only allowed for associations, not for: " + member);
                        }
                        assoc1 = attribAndProp1.assoc;
                        nick1 = qp.NicknameRequest(exprOuter, assoc1);
                        nick1 = VarName.GetSqlName(nick1);

                        //store this nickname for subsequent selects:
                        //result.memberExprNickames[member] = nick1;
                        qp.memberExprNickames[member] = nick1;

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
                            throw new Exception("L55 AddJoin2: member1.member2 only allowed for associations, not for: " + exprOuter.Expression);
                        }

                        assoc1 = attribAndProp1.assoc;
                        //nickname for parent table (not available in Expr tree) - eg. "p94$" for Products
                        string parentTypeName = exprOuter.Type.Name;
                        nick2 = VarName.GetSqlName(Char.ToLower(parentTypeName[0]) + "94"); 
                        nick1 = VarName.GetSqlName(paramExpr.Name);
                        type2 = exprOuter.Type;
                        SqlExpressionParts sqlParts = new SqlExpressionParts(qp._vars.Context.Vendor);
                        FromClauseBuilder.SelectAllFields(null, sqlParts,type2, nick2);
                        result.AppendString(sqlParts.GetSelect());
                        break;
                    }
                default:
                    throw new Exception("L49 AddJoin2: member1.member2.member3 only allowed for associations, not for: " + exprOuter.Expression);
            }

            Type type1 = exprOuter.Expression.Type;
            assoc2 = AttribHelper.FindReverseAssociation(attribAndProp1);

            //string joinString = "$c.CustomerID=$o.CustomerID"
            string joinString = nick1+"."+assoc1.ThisKey+"="+nick2+"."+assoc2.OtherKey;

            //_parts.joinList.Add(joinString);
            result.addJoin(joinString);
            result.tablesUsed[type2] = nick2;    //tablesUsed[Order] = $o
            result.tablesUsed[type1] = nick1;    //tablesUsed[Customer] = $join

        }
    }
}

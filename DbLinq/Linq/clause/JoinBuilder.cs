////////////////////////////////////////////////////////////////////
//Initial author: Jiri George Moudry, 2006.
//License: LGPL. (Visit http://www.gnu.org)
//Commercial code may call into this library, if it's in a different module (DLL)
////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using System.Query;
using System.Expressions;
using System.Data.DLinq;
using DBLinq.util;

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
            AssociationAttribute assoc1, assoc2;
            bool isAssoc1 = AttribHelper.IsAssociation(memberExpr,out assoc1);
            if( ! isAssoc1)
                return; //no join

            //user passed in part of SelectMany, eg. 'c.Orders'
            string nick1 = VarName.GetSqlName(memberExpr.Expression.XParam().Name); // c$
            string nick2 = VarName.GetSqlName(paramExpr.Name); //o$
            assoc2 = AttribHelper.FindReverseAssociation(assoc1);
            string joinString = nick1+"."+assoc1.OtherKey+"="+nick2+"."+assoc2.ThisKey;

            //Type childType = AttribHelper.ExtractTypeFromMSet(memberExpr.Type);
            result.addJoin(joinString);
            result.tablesUsed[memberExpr.Expression.Type] = nick1;//tablesUsed[Customer] = c$
            //result.tablesUsed[childType] = nick2;           //tablesUsed[Order] = join$
            result.tablesUsed[paramExpr.Type] = nick2;           //tablesUsed[Order] = join$
        }

        /// <summary>
        /// process 'o.Customer.City':
        /// a) insert join $o.CustomerId=$c.CustomerID
        /// b) insert into our StringBuilder '$c.City'
        /// c) insert into tablesUsed[Customer]='$c' and tablesUsed[Order]='$o'
        /// </summary>
        public static void AddJoin2(MemberExpression exprOuter, ParseInputs inputs, ParseResult result)
        {
            string nick1, nick2;
            AssociationAttribute assoc1, assoc2;
            Type type2;

            switch (exprOuter.Expression.NodeType)
            {
                case ExpressionType.MemberAccess:
                    {
                        //eg. "SELECT order.Product.ProductID"
                        MemberExpression member = exprOuter.Expression.XMember();
                        bool isAssoc = AttribHelper.IsAssociation(member, out assoc1);
                        if (!isAssoc)
                        {
                            throw new Exception("L55 AddJoin2: member1.member2.member3 only allowed for associations, not for: " + member);
                        }

                        nick1 = inputs.NicknameRequest(exprOuter, assoc1);
                        nick1 = VarName.GetSqlName(nick1);

                        //store this nickname for subsequent selects:
                        result.memberExprNickames[member] = nick1;

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
                        ParameterExpression paramExpr = exprOuter.Expression.XParam();

                        bool isAssoc = AttribHelper.IsAssociation(exprOuter, out assoc1);
                        if (!isAssoc)
                        {
                            throw new Exception("L55 AddJoin2: member1.member2 only allowed for associations, not for: " + exprOuter.Expression);
                        }

                        //nickname for parent table (not available in Expr tree) - eg. "p94$" for Products
                        string parentTypeName = exprOuter.Type.Name;
                        nick2 = VarName.GetSqlName(Char.ToLower(parentTypeName[0]) + "94"); 
                        nick1 = VarName.GetSqlName(paramExpr.Name);
                        type2 = exprOuter.Type;
                        SqlExpressionParts sqlParts = new SqlExpressionParts();
                        FromClauseBuilder.SelectAllFields(new SessionVars(), sqlParts,type2, nick2);
                        result.AppendString(sqlParts.GetSelect());
                        break;
                    }
                default:
                    throw new Exception("L49 AddJoin2: member1.member2.member3 only allowed for associations, not for: " + exprOuter.Expression);
            }

            Type type1 = exprOuter.Expression.Type;
            assoc2 = AttribHelper.FindReverseAssociation(assoc1);

            //string joinString = "$c.CustomerID=$o.CustomerID"
            string joinString = nick1+"."+assoc1.ThisKey+"="+nick2+"."+assoc2.OtherKey;

            //_parts.joinList.Add(joinString);
            result.addJoin(joinString);
            result.tablesUsed[type2] = nick2;    //tablesUsed[Order] = $o
            result.tablesUsed[type1] = nick1;               //tablesUsed[Customer] = $join
            //result.AppendString(nick1+"."+exprOuter.Member.Name); //where clause: '$c.City' //moved up
            //TODO - replace expr.Member.Name with SQL column name (use attribs)
            //Console.WriteLine("TODO: handle o.Customer.City !!!");

        }
    }
}

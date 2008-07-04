using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace DbLinq.Util.ExprVisitor
{
    internal class StripTransparentId : ExpressionVisitor
    {
        public Expression Modify(Expression expression)
        {
            Expression modified = Visit(expression);
            if (modified.ToString().Contains("<>h__TransparentIdentifier"))
                modified = Visit(modified); //nested two deep, occurs in LeftJoin_DefaultIfEmpty
            return modified;
        }

        protected override Expression VisitMemberAccess(MemberExpression m)
        {
            if (m.Expression.NodeType == ExpressionType.Parameter)
            {
                //match {<>h__TransparentIdentifier.p}
                ParameterExpression paramExpr = (ParameterExpression)m.Expression;
                if (paramExpr.Name.StartsWith("<>h__TransparentIdentifier"))
                {
                    //return {p}
                    ParameterExpression paramExpr2 = Expression.Parameter(m.Type, m.Member.Name);
                    return paramExpr2;
                }
            }
            return base.VisitMemberAccess(m);
        }

        protected override Expression VisitParameter(ParameterExpression p)
        {
            return base.VisitParameter(p);
        }
    }
}

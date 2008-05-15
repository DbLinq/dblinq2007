using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace DbLinq.Util.ExprVisitor
{
    /// <summary>
    /// given param 'c' and input 'H_TransparentIdentifier.x.CustomerID', 
    /// return modified 'c.CustomerID'
    /// </summary>
    public class ParentExpressionRemover : ExpressionVisitor, IExpressionModifier
    {
        PropertyInfo _matchingProperty;
        ParameterExpression _replace;

        public ParentExpressionRemover(PropertyInfo matchingProperty, ParameterExpression replace)
        {
            _matchingProperty = matchingProperty;
            _replace = replace;
        }

        public Expression Modify(Expression expression)
        {
            return Visit(expression);
        }

        protected override Expression VisitLambda(LambdaExpression lambda)
        {
            Expression body = this.Visit(lambda.Body);
            return Expression.Lambda(body, _replace);
            //return Expression.Lambda(lambda.Type, body, lambda.Parameters);
            //return base.VisitLambda(lambda);
        }

        protected override Expression VisitMemberAccess(MemberExpression m)
        {
            bool isSame = m.Expression.NodeType == ExpressionType.Parameter
                && m.Member == _matchingProperty;
            if (isSame)
                return _replace;
            else
                return base.VisitMemberAccess(m);
        }
    }
}

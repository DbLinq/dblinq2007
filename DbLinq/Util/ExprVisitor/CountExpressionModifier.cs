using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace DbLinq.Util.ExprVisitor
{
    /// <summary>
    /// when the user calls 'Count(i =&gt; 2)', we lost the information about what 'i' was referring to.
    /// That info lives in the preceding Select.
    /// This helper class edits our expression to put the info back in:
    /// Given 'i=>2', we return 'p=>ProductID>2'
    /// </summary>
    public class CountExpressionModifier : ExpressionVisitor
    {
        #region CountExpressionModifier
        LambdaExpression _substituteExpr;
        public CountExpressionModifier(LambdaExpression substituteExpr)
        {
            _substituteExpr = substituteExpr;
        }
        public Expression Modify(Expression expression)
        {
            return Visit(expression);
        }
        protected override Expression VisitLambda(LambdaExpression lambda)
        {
            Expression body2 = base.Visit(lambda.Body);
            Expression lambda2 = Expression.Lambda(body2, _substituteExpr.Parameters[0]);
            return lambda2;
        }
        protected override Expression VisitParameter(ParameterExpression p)
        {
            return _substituteExpr.Body;
        }
        #endregion
    }
}

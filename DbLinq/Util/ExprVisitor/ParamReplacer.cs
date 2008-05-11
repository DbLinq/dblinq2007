using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace DbLinq.Util.ExprVisitor
{
    /// <summary>
    /// Replaces parameters in a LambdaExpression.
    /// Given ParameterExpression of type Customer - {c} - in ctor, 
    /// Modify({c2=>c2.CustomerID}) will return replace each c2 with c, 
    /// and return {c=>c.CustomerID}.
    /// </summary>
    public class ParamReplacer : ExpressionVisitor, IExpressionModifier
    {
        readonly ParameterExpression _paramExpr;
        ParameterExpression _lambdaParamToMatch;

        public ParamReplacer(ParameterExpression paramExpr)
        {
            _paramExpr = paramExpr;
        }

        public Expression Modify(Expression expression)
        {
            if (expression.NodeType != ExpressionType.Lambda)
                return expression; //we only process lambdas

            LambdaExpression lambda = (LambdaExpression)expression;

            var modifiedParamList = new List<ParameterExpression>();
            foreach (var p in lambda.Parameters)
            {
                if (p.Type == _paramExpr.Type)
                {
                    _lambdaParamToMatch = p;
                    modifiedParamList.Add(_paramExpr);
                }
                else
                {
                    modifiedParamList.Add(p);
                }
            }

            if (_lambdaParamToMatch == null)
                return expression; //haven't found matching param type

            Expression modifiedBody = Visit(lambda.Body);
            LambdaExpression modifiedLambda = Expression.Lambda(modifiedBody, modifiedParamList.ToArray());
            return modifiedLambda;
        }

        protected override Expression VisitParameter(ParameterExpression p)
        {
            if (p == _lambdaParamToMatch)
                return _paramExpr; //replace {c2} with {c}
            else
                return p;
        }
        public override string ToString()
        {
            return "ParamReplacer " + _lambdaParamToMatch + " -> " + _paramExpr;
        }

    }
}

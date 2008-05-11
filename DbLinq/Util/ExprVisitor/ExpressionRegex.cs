using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace DbLinq.Util.ExprVisitor
{
    public class ExpressionRegex : IExpressionModifier
    {
        Expression _pattern;
        ParameterExpression _replacement;
        readonly IExpressionModifier _modifier;

        public ExpressionRegex(Expression pattern, ParameterExpression replacement)
        {
            if (pattern.NodeType == ExpressionType.Parameter)
            {
                _modifier = new ParameterReplacer((ParameterExpression)pattern, replacement);
                return;
            }
            else if (pattern.NodeType == ExpressionType.MemberAccess)
            {
                _modifier = new MemberReplacer((MemberExpression)pattern, replacement);
                return;
            }
            throw new ArgumentOutOfRangeException("L26 Unprepared for pattern " + pattern.NodeType);
        }

        public static Expression Replace(Expression input, Expression pattern, ParameterExpression replacement)
        {
            if (input == null || pattern == null || replacement == null)
                return input;

            //if (pattern.NodeType == ExpressionType.MemberAccess)
            //{
            //    ParentExpressionRemover modifier = new ParentExpressionRemover((MemberExpression)pattern, replacement);
            //    return modifier.Modify(input);
            //}
            //else 
            if (pattern.NodeType == ExpressionType.Parameter)
            {
                ParameterReplacer modifier = new ParameterReplacer((ParameterExpression)pattern, replacement);
                return modifier.Modify(input);
            }
            throw new ArgumentOutOfRangeException("L26 Unprepared for pattern " + pattern.NodeType);


        }

        public Expression Modify(Expression expr)
        {
            Expression modified = _modifier.Modify(expr);
            return modified;
        }

        public class MemberReplacer : ExpressionVisitor, IExpressionModifier
        {
            MemberExpression _match;
            ParameterExpression _replace;

            public MemberReplacer(MemberExpression match, ParameterExpression replace)
            {
                _match = match;
                _replace = replace;
            }

            public Expression Modify(Expression expression)
            {
                return Visit(expression);
            }
            protected override Expression VisitMemberAccess(MemberExpression m)
            {
                if (ExpressionTreeComparer.Equals(_match, m))
                    return _replace;
                return base.VisitMemberAccess(m);
            }
        }

        public class ParameterReplacer : ExpressionVisitor, IExpressionModifier
        {
            ParameterExpression _match;
            ParameterExpression _replace;

            public ParameterReplacer(ParameterExpression match, ParameterExpression replace)
            {
                _match = match;
                _replace = replace;
            }

            public Expression Modify(Expression expression)
            {
                return Visit(expression);
            }
            protected override Expression VisitParameter(ParameterExpression p)
            {
                if (p == _match)
                    return _replace;
                return base.VisitParameter(p);
            }
        }

    }
}

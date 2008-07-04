using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace DbLinq.Util.ExprVisitor
{
    internal static class ExpressionTreeComparer
    {
        public static bool TryGetValueEx(this Dictionary<MemberExpression, string> exprNicknames, Expression exprToFind, out string nickname)
        {
            foreach (var kv in exprNicknames)
            {
                bool same = Equals(exprToFind, kv.Key);
                if (same)
                {
                    nickname = kv.Value;
                    return true;
                }
            }
            nickname = null;
            return false;
        }

        /// <summary>
        /// recursively compare an expression tree.
        /// TODO: decide if it's better to compare 
        /// </summary>
        public static bool Equals(Expression e1, Expression e2)
        {
            if (e1.NodeType != e2.NodeType || e1.Type != e2.Type)
                return false;

            switch (e1.NodeType)
            {
                case ExpressionType.Parameter:
                    return CompareParameter((ParameterExpression)e1, (ParameterExpression)e2);
                case ExpressionType.MemberAccess:
                    return CompareMember((MemberExpression)e1, (MemberExpression)e2);
                default:
                    throw new ApplicationException("Unprepared for " + e1.NodeType);
            }
        }

        static bool CompareParameter(ParameterExpression p1, ParameterExpression p2)
        {
            return p1.Name == p2.Name;
        }

        static bool CompareMember(MemberExpression e1, MemberExpression e2)
        {
            if (e1.Member != e2.Member)
                return false;
            return Equals(e1.Expression, e2.Expression);
        }
    }
}

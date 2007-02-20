using System;
using System.Expressions;
using System.Collections.Generic;
using System.Text;

namespace DBLinq.util
{
    /// <summary>
    /// extension methods for Expressions.
    /// Allow casting of 'ex' as 'ex.Lambda' etc.
    /// </summary>
    public static class ExprExtensions
    {
        public static MethodCallExpression XMethodCall(this Expression ex)
        {
            if(ex==null || ex.NodeType!=ExpressionType.MethodCall)
                return null;
            return (MethodCallExpression)ex;
        }

        public static Expression XParam(this MethodCallExpression call,int index)
        {
            if(call==null)
                return null;
            if(call.Parameters.Count<index)
                return null;
            return call.Parameters[index];
        }

        public static UnaryExpression XCast(this Expression ex)
        {
            if(ex==null || ex.NodeType!=ExpressionType.Cast)
                return null;
            return (UnaryExpression)ex;
        }

        public static LambdaExpression XLambda(this Expression ex)
        {
            if(ex==null || ex.NodeType!=ExpressionType.Lambda)
                return null;
            return (LambdaExpression)ex;
        }

        public static Expression XOp(this UnaryExpression ex)
        {
            if(ex==null)
                return null;
            return ex.Operand;
        }

        public static ParameterExpression XParam(this Expression ex)
        {
            if(ex==null || ex.NodeType!=ExpressionType.Parameter)
                return null;
            return (ParameterExpression)ex;
        }

        public static MemberExpression XMember(this Expression ex)
        {
            if(ex==null || ex.NodeType!=ExpressionType.MemberAccess)
                return null;
            return (MemberExpression)ex;
        }
        public static MemberInitExpression XMemberInit(this Expression ex)
        {
            if(ex==null || ex.NodeType!=ExpressionType.MemberInit)
                return null;
            return (MemberInitExpression)ex;
        }
        

    }
}

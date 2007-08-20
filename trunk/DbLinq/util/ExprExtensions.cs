////////////////////////////////////////////////////////////////////
//Initial author: Jiri George Moudry, 2006.
//License: LGPL. (Visit http://www.gnu.org)
//Commercial code may call into this library, if it's in a different module (DLL)
////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;

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
            if(ex==null || ex.NodeType!=ExpressionType.Call)
                return null;
            return (MethodCallExpression)ex;
        }

        public static Expression XParam(this MethodCallExpression call,int index)
        {
            if(call==null)
                return null;
            if(call.Arguments.Count<index)
                return null;
            return call.Arguments[index];
        }

        public static UnaryExpression XCast(this Expression ex)
        {
            //Cast disappeared in Bet2?!
            return null;
            //if(ex==null || ex.NodeType!=ExpressionType.Cast)
            //    return null;
            //return (UnaryExpression)ex;
        }

        [DebuggerStepThrough]
        public static LambdaExpression XLambda(this Expression ex)
        {
            if (ex == null)
                return null;
            
            if (ex.NodeType == ExpressionType.Quote)
            {
                ex = ex.XUnary().Operand;
            }

            if (ex.NodeType != ExpressionType.Lambda)
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
        public static UnaryExpression XUnary(this Expression ex)
        {
            if(ex==null)
                return null;
            if(ex.NodeType==ExpressionType.Convert
                //|| ex.NodeType == ExpressionType.Cast //Cast disappeared in Beta2?!
                || ex.NodeType == ExpressionType.Quote
                )
            {
                return (UnaryExpression)ex;
            }
            return null;
        }

        public static Expression XCastOperand(this Expression ex)
        {
            if(ex==null)
                return null;
            //Cast disappeared in Beta2?!
            //if(ex.NodeType==ExpressionType.Cast)
            //{
            //    return ((UnaryExpression)ex).Operand;
            //}
            return null;
        }
        

    }
}

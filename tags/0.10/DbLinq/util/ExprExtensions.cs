////////////////////////////////////////////////////////////////////
//Initial author: Jiri George Moudry, 2006.
//License: LGPL. (Visit http://www.gnu.org)
//Commercial code may call into this library, if it's in a different module (DLL)
////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
#if LINQ_PREVIEW_2006
//Visual Studio 2005 with Linq Preview May 2006 - can run on Win2000
using System.Expressions;
#else
//Visual Studio Orcas - requires WinXP
using System.Linq.Expressions;
#endif

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
            if(ex==null || ex.NodeType!=ExpressionType.Cast)
                return null;
            return (UnaryExpression)ex;
        }

        [DebuggerStepThrough]
        public static LambdaExpression XLambda(this Expression ex)
        {
            if (ex == null)
                return null;
            
#if LINQ_PREVIEW_2006
            //Quote does not exist
#else
            if (ex.NodeType == ExpressionType.Quote)
            {
                ex = ex.XUnary().Operand;
            }
#endif

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
                || ex.NodeType == ExpressionType.Cast
#if LINQ_PREVIEW_2006
              //in 2006, NodeType.Quote did not exist
#else
                || ex.NodeType == ExpressionType.Quote
#endif
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
            if(ex.NodeType==ExpressionType.Cast)
            {
                return ((UnaryExpression)ex).Operand;
            }
            return null;
        }
        

    }
}

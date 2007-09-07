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

#if DEAD_CODE
        public static Expression XOp(this UnaryExpression ex)
        {
            if(ex==null)
                return null;
            return ex.Operand;
        }
#endif

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

        /// <summary>
        /// given '<>h__TransparentIdentifier.p.ProductID', return 'p.ProductID'
        /// given '<>h__TransparentIdentifier.p', return 'p'
        /// (or null)
        /// </summary>
        public static Expression StripTransparentID(this Expression ex)
        {
            if (ex == null || ex.NodeType != ExpressionType.MemberAccess)
                return ex;

            MemberExpression exprOuter = ex as MemberExpression;
            Expression exprLeft = exprOuter.Expression;
            string leftName = exprLeft.ToString();
            if (!leftName.StartsWith("<>h__TransparentIdentifier"))
                return ex;

            switch (exprLeft.NodeType)
            {
                case ExpressionType.MemberAccess:
                    //as of Beta2, the former 'p.ProductID' now appears here as '<>h__TransparentIdentifier.p.ProductID'
                    //the 'p' used to be a ParameterExpression - not anymore
                    {
                        MemberExpression member1 = exprOuter.Expression.XMember(); //'<>h__TransparentIdentifier.p

                        //turn '<>h__TransparentIdentifier.p.ProductID' into 'p.ProductID'
                        string nameP = member1.Member.Name; //'p'
                        System.Reflection.PropertyInfo propInfoP = member1.Member as System.Reflection.PropertyInfo;
                        Type typeP = propInfoP.PropertyType; //typeof(Product)
                        ParameterExpression fakeParam = Expression.Parameter(typeP, nameP);
                        MemberExpression expr2 = Expression.MakeMemberAccess(fakeParam, exprOuter.Member);
                        return expr2;
                    }

                case ExpressionType.Parameter:
                    {
                        ParameterExpression param1 = exprOuter.Expression as ParameterExpression; //'<>h__TransparentIdentifier.p

                        //turn '<>h__TransparentIdentifier.p.ProductID' into 'p.ProductID'
                        string nameP = exprOuter.Member.Name; //'p'
                        System.Reflection.PropertyInfo propInfoP = exprOuter.Member as System.Reflection.PropertyInfo;
                        Type typeP = propInfoP.PropertyType; //typeof(Product)
                        ParameterExpression fakeParam = Expression.Parameter(typeP, nameP);
                        return fakeParam;
                    }

                default:
                    return exprLeft;
            }

        }

#if DEAD_CODE
        public static MemberInitExpression XMemberInit(this Expression ex)
        {
            if(ex==null || ex.NodeType!=ExpressionType.MemberInit)
                return null;
            return (MemberInitExpression)ex;
        }
#endif

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

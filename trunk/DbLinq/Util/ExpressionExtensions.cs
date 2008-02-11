#region MIT license
////////////////////////////////////////////////////////////////////
// MIT license:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//
// Authors:
//        Jiri George Moudry
////////////////////////////////////////////////////////////////////
#endregion

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;

namespace DBLinq.Util
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
            if(call.Arguments.Count<=index)
                return null;
            return call.Arguments[index];
        }

        public static NewExpression XNew(this Expression expr)
        {
            if (expr == null)
                return null;
            if (expr.NodeType!=ExpressionType.New)
                return null;
            return expr as NewExpression;
        }

        public static UnaryExpression XCast(this Expression ex)
        {
            //Cast disappeared in Bet2?!
            return null;
            //if(ex==null || ex.NodeType!=ExpressionType.Cast)
            //    return null;
            //return (UnaryExpression)ex;
        }

        /// <summary>
        /// given Lambda Where, return 'Where'
        /// </summary>
        [DebuggerStepThrough]
        public static string XLambdaName(this Expression ex)
        {
            if (ex == null)
                return null;
            MethodCallExpression mcall = ex.XMethodCall();
            if (mcall == null)
                return null;
            return mcall.Method.Name;
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

        [DebuggerStepThrough]
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
            if (ex == null)
                return null;
            //Cast disappeared in Beta2?!
            //if(ex.NodeType==ExpressionType.Cast)
            //{
            //    return ((UnaryExpression)ex).Operand;
            //}
            return null;
        }

        public static ConstantExpression XConstant(this Expression ex)
        {
            if (ex == null)
                return null;
            if (ex.NodeType == ExpressionType.Constant)
            {
                return ((ConstantExpression)ex);
            }
            return null;
        }

        /// <summary>
        /// return true for expression 'e.ReportsTo==null' or 'null==e.ReportsTo'
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        public static bool IsNullTest(BinaryExpression expr, out Expression exprBeingComparedWithNull)
        {
            bool foundNullConst = false;
            Expression exprOther = null;
            if (expr.Left.NodeType == ExpressionType.Constant && expr.Left.XConstant().Value == null)
            {
                foundNullConst = true;
                exprOther = expr.Right;
            }
            else if (expr.Right.NodeType == ExpressionType.Constant && expr.Right.XConstant().Value == null)
            {
                foundNullConst = true;
                exprOther = expr.Left;
            }

            if (!foundNullConst)
            {
                exprBeingComparedWithNull = null;
                return false;
            }

            Type opType = exprOther.Type;
            bool isNullableExpr = opType == typeof(string)
                            || IsTypeNullable(exprOther);
            exprBeingComparedWithNull = exprOther;
            return isNullableExpr;
        }

        public static bool IsTypeNullable(Expression expr)
        {
            if (expr == null)
                return false;
            Type type = expr.Type;
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
        

    }
}

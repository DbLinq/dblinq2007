#region MIT license
// 
// Copyright (c) 2007-2008 Jiri Moudry
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion

using System;
using System.Linq.Expressions;
using DbLinq.Linq.Data.Sugar.Expressions;

namespace DbLinq.Linq.Data.Sugar
{
    public class ExpressionPrecedence
    {
        public OperatorPrecedence Get(Expression expression)
        {
            if (expression is SpecialExpression)
            {
                var specialNodeType = ((SpecialExpression)expression).SpecialNodeType;
                switch (specialNodeType)
                {
                case SpecialExpressionType.IsNull:
                case SpecialExpressionType.IsNotNull:
                    return OperatorPrecedence.Equality;
                case SpecialExpressionType.Concat:
                    return OperatorPrecedence.Additive;
                case SpecialExpressionType.Count:
                    return OperatorPrecedence.Primary;
                case SpecialExpressionType.Like:
                    return OperatorPrecedence.Equality;
                default:
                    throw Error.BadArgument("S0050: Unhandled SpecialExpressionType {0}", specialNodeType);
                }
            }
            switch (expression.NodeType)
            {
            case ExpressionType.Add:
            case ExpressionType.AddChecked:
                return OperatorPrecedence.Additive;
            case ExpressionType.And:
            case ExpressionType.AndAlso:
                return OperatorPrecedence.ConditionalAnd;
            case ExpressionType.ArrayLength:
            case ExpressionType.ArrayIndex:
            case ExpressionType.Call:
                return OperatorPrecedence.Primary;
            case ExpressionType.Coalesce:
                return OperatorPrecedence.NullCoalescing;
            case ExpressionType.Conditional:
                return OperatorPrecedence.Conditional;
            case ExpressionType.Constant:
                return OperatorPrecedence.Primary;
            case ExpressionType.Convert:
            case ExpressionType.ConvertChecked:
                return OperatorPrecedence.Primary;
            case ExpressionType.Divide:
                return OperatorPrecedence.Multiplicative;
            case ExpressionType.Equal:
                return OperatorPrecedence.Equality;
            case ExpressionType.ExclusiveOr:
                return OperatorPrecedence.LogicalXor;
            case ExpressionType.GreaterThan:
            case ExpressionType.GreaterThanOrEqual:
                return OperatorPrecedence.RelationalAndTypeTest;
            case ExpressionType.Invoke:
                return OperatorPrecedence.Primary;
            case ExpressionType.Lambda:
                return OperatorPrecedence.Primary;
            case ExpressionType.LeftShift:
                return OperatorPrecedence.Shift;
            case ExpressionType.LessThan:
            case ExpressionType.LessThanOrEqual:
                return OperatorPrecedence.RelationalAndTypeTest;
            case ExpressionType.ListInit:
            case ExpressionType.MemberAccess:
            case ExpressionType.MemberInit:
                return OperatorPrecedence.Primary;
            case ExpressionType.Modulo:
            case ExpressionType.Multiply:
            case ExpressionType.MultiplyChecked:
                return OperatorPrecedence.Multiplicative;
            case ExpressionType.Negate:
            case ExpressionType.UnaryPlus:
            case ExpressionType.NegateChecked:
                return OperatorPrecedence.Unary;
            case ExpressionType.New:
            case ExpressionType.NewArrayInit:
            case ExpressionType.NewArrayBounds:
                return OperatorPrecedence.Primary;
            case ExpressionType.Not:
                return OperatorPrecedence.Unary;
            case ExpressionType.NotEqual:
                return OperatorPrecedence.Equality;
            case ExpressionType.Or:
            case ExpressionType.OrElse:
                return OperatorPrecedence.ConditionalOr;
            case ExpressionType.Parameter:
                return OperatorPrecedence.Primary;
            case ExpressionType.Power:
                return OperatorPrecedence.Primary;
            case ExpressionType.Quote:
                return OperatorPrecedence.Primary;
            case ExpressionType.RightShift:
                return OperatorPrecedence.Shift;
            case ExpressionType.Subtract:
            case ExpressionType.SubtractChecked:
                return OperatorPrecedence.Additive;
            case ExpressionType.TypeAs:
            case ExpressionType.TypeIs:
                return OperatorPrecedence.RelationalAndTypeTest;
            }
            return OperatorPrecedence.Primary;
        }
    }
}

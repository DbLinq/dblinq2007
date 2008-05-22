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
using System.Globalization;
using System.Linq.Expressions;
using DbLinq.Linq.Data.Sugar.Expressions;

namespace DbLinq.Vendor.Implementation
{
    public class SqlBuilder : ISqlBuilder
    {
        /// <summary>
        /// Converts a constant value to a literal representation
        /// </summary>
        /// <param name="literal"></param>
        /// <returns></returns>
        public virtual string GetLiteral(object literal)
        {
            if (literal == null)
                return GetNullLiteral();
            if (literal is string)
                return GetLiteral((string)literal);
            return Convert.ToString(literal, CultureInfo.InvariantCulture);
        }

        public virtual string GetLiteral(ExpressionType operationType, params string[] p)
        {
            switch (operationType)
            {
            case ExpressionType.Add:
                return GetLiteralAdd(p[0], p[1]);
            case ExpressionType.AddChecked:
                return GetLiteralAddChecked(p[0], p[1]);
            case ExpressionType.And:
                return GetLiteralAnd(p[0], p[1]);
            case ExpressionType.AndAlso:
                return GetLiteralAndAlso(p[0], p[1]);
            case ExpressionType.ArrayLength:
                return GetLiteralArrayLength(p[0], p[1]);
            case ExpressionType.ArrayIndex:
                return GetLiteralArrayIndex(p[0], p[1]);
            case ExpressionType.Call:
                return GetLiteralCall(p[0]);
            case ExpressionType.Coalesce:
                return GetLiteralCoalesce(p[0], p[1]);
            case ExpressionType.Conditional:
                return GetLiteralConditional(p[0], p[1], p[2]);
            //case ExpressionType.Constant:
            //break;
            case ExpressionType.Convert:
                return GetLiteralConvert(p[0], p[1]);
            case ExpressionType.ConvertChecked:
                return GetLiteralConvertChecked(p[0], p[1]);
            case ExpressionType.Divide:
                return GetLiteralDivide(p[0], p[1]);
            case ExpressionType.Equal:
                return GetLiteralEqual(p[0], p[1]);
            case ExpressionType.ExclusiveOr:
                return GetLiteralExclusiveOr(p[0], p[1]);
            case ExpressionType.GreaterThan:
                return GetLiteralGreaterThan(p[0], p[1]);
            case ExpressionType.GreaterThanOrEqual:
                return GetLiteralGreaterThanOrEqual(p[0], p[1]);
            //case ExpressionType.Invoke:
            //break;
            //case ExpressionType.Lambda:
            //break;
            case ExpressionType.LeftShift:
                return GetLiteralLeftShift(p[0], p[1]);
            case ExpressionType.LessThan:
                return GetLiteralLessThan(p[0], p[1]);
            case ExpressionType.LessThanOrEqual:
                return GetLiteralLessThanOrEqual(p[0], p[1]);
            //case ExpressionType.ListInit:
            //break;
            //case ExpressionType.MemberAccess:
            //    break;
            //case ExpressionType.MemberInit:
            //    break;
            case ExpressionType.Modulo:
                return GetLiteralModulo(p[0], p[1]);
            case ExpressionType.Multiply:
                return GetLiteralMultiply(p[0], p[1]);
            case ExpressionType.MultiplyChecked:
                return GetLiteralMultiplyChecked(p[0], p[1]);
            case ExpressionType.Negate:
                return GetLiteralNegate(p[0]);
            case ExpressionType.UnaryPlus:
                return GetLiteralUnaryPlus(p[0]);
            case ExpressionType.NegateChecked:
                return GetLiteralNegateChecked(p[0]);
            //case ExpressionType.New:
            //    break;
            //case ExpressionType.NewArrayInit:
            //    break;
            //case ExpressionType.NewArrayBounds:
            //    break;
            case ExpressionType.Not:
                return GetLiteralNot(p[0]);
            case ExpressionType.NotEqual:
                return GetLiteralNotEqual(p[0], p[1]);
            case ExpressionType.Or:
                return GetLiteralOr(p[0], p[1]);
            case ExpressionType.OrElse:
                return GetLiteralOrElse(p[0], p[1]);
            //case ExpressionType.Parameter:
            //    break;
            case ExpressionType.Power:
                return GetLiteralPower(p[0], p[1]);
            //case ExpressionType.Quote:
            //    break;
            case ExpressionType.RightShift:
                return GetLiteralRightShift(p[0], p[1]);
            case ExpressionType.Subtract:
                return GetLiteralSubtract(p[0], p[1]);
            case ExpressionType.SubtractChecked:
                return GetLiteralSubtractChecked(p[0], p[1]);
            //case ExpressionType.TypeAs:
            //    break;
            //case ExpressionType.TypeIs:
            //    break;
            }
            throw new ArgumentException(operationType.ToString());
        }
        public virtual string GetLiteral(SpecialExpressionType operationType, params string[] p)
        {
            switch (operationType)
            {

            case SpecialExpressionType.IsNull:
                return GetLiteralIsNull(p[0]);
            case SpecialExpressionType.IsNotNull:
                return GetLiteralIsNotNull(p[0]);
            case SpecialExpressionType.Concat:
                return GetLiteralConcat(p[0], p[1]);
            }
            throw new ArgumentException(operationType.ToString());
        }

        protected virtual string GetLiteralAdd(string a, string b)
        {
            throw new NotImplementedException();
        }

        protected virtual string GetLiteralAddChecked(string a, string b)
        {
            throw new NotImplementedException();
        }

        protected virtual string GetLiteralAnd(string a, string b)
        {
            throw new NotImplementedException();
        }

        protected virtual string GetLiteralAndAlso(string a, string b)
        {
            throw new NotImplementedException();
        }

        protected virtual string GetLiteralArrayLength(string a, string b)
        {
            throw new NotImplementedException();
        }

        protected virtual string GetLiteralArrayIndex(string a, string b)
        {
            throw new NotImplementedException();
        }

        protected virtual string GetLiteralCall(string a)
        {
            throw new NotImplementedException();
        }

        protected virtual string GetLiteralCoalesce(string a, string b)
        {
            throw new NotImplementedException();
        }

        protected virtual string GetLiteralConditional(string a, string b, string c)
        {
            throw new NotImplementedException();
        }

        protected virtual string GetLiteralConvert(string a, string b)
        {
            throw new NotImplementedException();
        }

        protected virtual string GetLiteralConvertChecked(string a, string b)
        {
            throw new NotImplementedException();
        }

        protected virtual string GetLiteralDivide(string a, string b)
        {
            throw new NotImplementedException();
        }

        protected virtual string GetLiteralEqual(string a, string b)
        {
            throw new NotImplementedException();
        }

        protected virtual string GetLiteralExclusiveOr(string a, string b)
        {
            throw new NotImplementedException();
        }

        protected virtual string GetLiteralGreaterThan(string a, string b)
        {
            throw new NotImplementedException();
        }

        protected virtual string GetLiteralGreaterThanOrEqual(string a, string b)
        {
            throw new NotImplementedException();
        }

        protected virtual string GetLiteralLeftShift(string a, string b)
        {
            throw new NotImplementedException();
        }

        protected virtual string GetLiteralLessThan(string a, string b)
        {
            throw new NotImplementedException();
        }

        protected virtual string GetLiteralLessThanOrEqual(string a, string b)
        {
            throw new NotImplementedException();
        }

        protected virtual string GetLiteralModulo(string a, string b)
        {
            throw new NotImplementedException();
        }

        protected virtual string GetLiteralMultiply(string a, string b)
        {
            throw new NotImplementedException();
        }

        protected virtual string GetLiteralMultiplyChecked(string a, string b)
        {
            throw new NotImplementedException();
        }

        protected virtual string GetLiteralNegate(string a)
        {
            throw new NotImplementedException();
        }

        protected virtual string GetLiteralUnaryPlus(string a)
        {
            throw new NotImplementedException();
        }

        protected virtual string GetLiteralNegateChecked(string a)
        {
            throw new NotImplementedException();
        }

        protected virtual string GetLiteralNot(string a)
        {
            throw new NotImplementedException();
        }

        protected virtual string GetLiteralNotEqual(string a, string b)
        {
            throw new NotImplementedException();
        }

        protected virtual string GetLiteralOr(string a, string b)
        {
            throw new NotImplementedException();
        }

        protected virtual string GetLiteralOrElse(string a, string b)
        {
            throw new NotImplementedException();
        }

        protected virtual string GetLiteralPower(string a, string b)
        {
            throw new NotImplementedException();
        }

        protected virtual string GetLiteralRightShift(string a, string b)
        {
            throw new NotImplementedException();
        }

        protected virtual string GetLiteralSubtract(string a, string b)
        {
            throw new NotImplementedException();
        }

        protected virtual string GetLiteralSubtractChecked(string a, string b)
        {
            throw new NotImplementedException();
        }

        protected virtual string GetLiteralIsNull(string a)
        {
            throw new NotImplementedException();
        }

        protected virtual string GetLiteralIsNotNull(string a)
        {
            throw new NotImplementedException();
        }

        protected virtual string GetLiteralConcat(string a, string b)
        {
            throw new NotImplementedException();
        }

        protected virtual string GetNullLiteral()
        {
            return "null";
        }

        protected virtual string GetLiteral(string literal)
        {
            return string.Format("'{0}'", literal.Replace("'", "''"));
        }
    }
}
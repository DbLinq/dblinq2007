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
using DbLinq.Linq.Data.Sugar.Pieces;

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

        public virtual string GetLiteral(OperationType operationType, params string[] p)
        {
            switch (operationType)
            {
                case OperationType.Add:
                    return GetLiteralAdd(p[0], p[1]);
                case OperationType.AddChecked:
                    return GetLiteralAddChecked(p[0], p[1]);
                case OperationType.And:
                    return GetLiteralAnd(p[0], p[1]);
                case OperationType.AndAlso:
                    return GetLiteralAndAlso(p[0], p[1]);
                case OperationType.ArrayLength:
                    return GetLiteralArrayLength(p[0], p[1]);
                case OperationType.ArrayIndex:
                    return GetLiteralArrayIndex(p[0], p[1]);
                case OperationType.Call:
                    return GetLiteralCall(p[0]);
                case OperationType.Coalesce:
                    return GetLiteralCoalesce(p[0], p[1]);
                case OperationType.Conditional:
                    return GetLiteralConditional(p[0], p[1], p[2]);
                    //case OperationType.Constant:
                    //break;
                case OperationType.Convert:
                    return GetLiteralConvert(p[0], p[1]);
                case OperationType.ConvertChecked:
                    return GetLiteralConvertChecked(p[0], p[1]);
                case OperationType.Divide:
                    return GetLiteralDivide(p[0], p[1]);
                case OperationType.Equal:
                    return GetLiteralEqual(p[0], p[1]);
                case OperationType.ExclusiveOr:
                    return GetLiteralExclusiveOr(p[0], p[1]);
                case OperationType.GreaterThan:
                    return GetLiteralGreaterThan(p[0], p[1]);
                case OperationType.GreaterThanOrEqual:
                    return GetLiteralGreaterThanOrEqual(p[0], p[1]);
                    //case OperationType.Invoke:
                    //break;
                    //case OperationType.Lambda:
                    //break;
                case OperationType.LeftShift:
                    return GetLiteralLeftShift(p[0], p[1]);
                case OperationType.LessThan:
                    return GetLiteralLessThan(p[0], p[1]);
                case OperationType.LessThanOrEqual:
                    return GetLiteralLessThanOrEqual(p[0], p[1]);
                    //case OperationType.ListInit:
                    //break;
                    //case OperationType.MemberAccess:
                    //    break;
                    //case OperationType.MemberInit:
                    //    break;
                case OperationType.Modulo:
                    return GetLiteralModulo(p[0], p[1]);
                case OperationType.Multiply:
                    return GetLiteralMultiply(p[0], p[1]);
                case OperationType.MultiplyChecked:
                    return GetLiteralMultiplyChecked(p[0], p[1]);
                case OperationType.Negate:
                    return GetLiteralNegate(p[0]);
                case OperationType.UnaryPlus:
                    return GetLiteralUnaryPlus(p[0]);
                case OperationType.NegateChecked:
                    return GetLiteralNegateChecked(p[0]);
                    //case OperationType.New:
                    //    break;
                    //case OperationType.NewArrayInit:
                    //    break;
                    //case OperationType.NewArrayBounds:
                    //    break;
                case OperationType.Not:
                    return GetLiteralNot(p[0]);
                case OperationType.NotEqual:
                    return GetLiteralNotEqual(p[0], p[1]);
                case OperationType.Or:
                    return GetLiteralOr(p[0], p[1]);
                case OperationType.OrElse:
                    return GetLiteralOrElse(p[0], p[1]);
                    //case OperationType.Parameter:
                    //    break;
                case OperationType.Power:
                    return GetLiteralPower(p[0], p[1]);
                    //case OperationType.Quote:
                    //    break;
                case OperationType.RightShift:
                    return GetLiteralRightShift(p[0], p[1]);
                case OperationType.Subtract:
                    return GetLiteralSubtract(p[0], p[1]);
                case OperationType.SubtractChecked:
                    return GetLiteralSubtractChecked(p[0], p[1]);
                    //case OperationType.TypeAs:
                    //    break;
                    //case OperationType.TypeIs:
                    //    break;
                case OperationType.IsNull:
                    return GetLiteralIsNull(p[0]);
                case OperationType.IsNotNull:
                    return GetLiteralIsNotNull(p[0]);
                case OperationType.Concat:
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
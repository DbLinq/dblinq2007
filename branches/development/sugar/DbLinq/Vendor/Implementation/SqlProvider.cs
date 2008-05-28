﻿#region MIT license
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
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using DbLinq.Linq.Data.Sugar.Expressions;

namespace DbLinq.Vendor.Implementation
{
    public class SqlProvider : ISqlProvider
    {
        public string NewLine
        {
            get { return Environment.NewLine; }
        }
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

        /// <summary>
        /// Converts a standard operator to an expression
        /// </summary>
        /// <param name="operationType"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public virtual string GetLiteral(ExpressionType operationType, IList<string> p)
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
                return GetLiteralConvert(p[0]);
            case ExpressionType.ConvertChecked:
                return GetLiteralConvertChecked(p[0]);
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

        /// <summary>
        /// Converts a special expression type to literal
        /// </summary>
        /// <param name="operationType"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public virtual string GetLiteral(SpecialExpressionType operationType, IList<string> p)
        {
            switch (operationType) // SETuse
            {

            case SpecialExpressionType.IsNull:
                return GetLiteralIsNull(p[0]);
            case SpecialExpressionType.IsNotNull:
                return GetLiteralIsNotNull(p[0]);
            case SpecialExpressionType.Concat:
                return GetLiteralConcat(p[0], p[1]);
            case SpecialExpressionType.Count:
                return GetLiteralCount(p[0]);
            case SpecialExpressionType.Like:
                return GetLiteralLike(p[0], p[1]);
            case SpecialExpressionType.Min:
                return GetLiteralMin(p[0]);
            case SpecialExpressionType.Max:
                return GetLiteralMax(p[0]);
            case SpecialExpressionType.Sum:
                return GetLiteralSum(p[0]);
            case SpecialExpressionType.Average:
                return GetLiteralAverage(p[0]);
            case SpecialExpressionType.StringLength:
                return GetLiteralStringLength(p[0]);
            case SpecialExpressionType.ToUpper:
                return GetLiteralStringToUpper(p[0]);
            case SpecialExpressionType.ToLower:
                return GetLiteralStringToLower(p[0]);
            }
            throw new ArgumentException(operationType.ToString());
        }

        /// <summary>
        /// Places the expression into parenthesis
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public virtual string GetParenthesis(string a)
        {
            return string.Format("({0})", a);
        }

        /// <summary>
        /// Returns a column related to a table.
        /// Ensures about the right case
        /// </summary>
        /// <param name="table"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public virtual string GetColumn(string table, string column)
        {
            return string.Format("{0}.{1}", table, GetColumn(column));
        }

        /// <summary>
        /// Returns a column related to a table.
        /// Ensures about the right case
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public string GetColumn(string column)
        {
            // TODO: case check
            return column;
        }

        /// <summary>
        /// Returns a table alias
        /// Ensures about the right case
        /// </summary>
        /// <param name="table"></param>
        /// <param name="alias"></param>
        /// <returns></returns>
        public virtual string GetTableAsAlias(string table, string alias)
        {
            return string.Format("{0} {1}", table, GetTableAlias(alias));
        }

        /// <summary>
        /// Returns a table alias
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public virtual string GetTable(string table)
        {
            return table;
        }

        /// <summary>
        /// Joins a list of table selection to make a FROM clause
        /// </summary>
        /// <param name="tables"></param>
        /// <returns></returns>
        public virtual string GetFromClause(string[] tables)
        {
            if (tables.Length == 0)
                return string.Empty;
            return string.Format("FROM {0}", string.Join(", ", tables));
        }

        /// <summary>
        /// Joins a list of conditions to make a WHERE clause
        /// </summary>
        /// <param name="wheres"></param>
        /// <returns></returns>
        public virtual string GetWhereClause(string[] wheres)
        {
            if (wheres.Length == 0)
                return string.Empty;
            return string.Format("WHERE {0}", string.Join(" AND ", wheres));
        }

        /// <summary>
        /// Joins a list of operands to make a SELECT clause
        /// </summary>
        /// <param name="selects"></param>
        /// <returns></returns>
        public virtual string GetSelectClause(string[] selects)
        {
            if (selects.Length == 0)
                return string.Empty;
            return string.Format("SELECT {0}", string.Join(", ", selects));
        }

        /// <summary>
        /// Returns all table columns (*)
        /// </summary>
        /// <returns></returns>
        public virtual string GetColumns()
        {
            return "*";
        }

        /// <summary>
        /// Returns a literal parameter name
        /// </summary>
        /// <returns></returns>
        public virtual string GetParameterName(string nameBase)
        {
            return string.Format(":{0}", nameBase);
        }

        /// <summary>
        /// Returns a valid alias syntax for the given table
        /// </summary>
        /// <param name="nameBase"></param>
        /// <returns></returns>
        public virtual string GetTableAlias(string nameBase)
        {
            return string.Format("{0}$", nameBase);
        }

        protected virtual string GetLiteralAdd(string a, string b)
        {
            return string.Format("{0} + {1}", a, b);
        }

        protected virtual string GetLiteralAddChecked(string a, string b)
        {
            return GetLiteralAdd(a, b);
        }

        protected virtual string GetLiteralAnd(string a, string b)
        {
            return string.Format("{0} AND {1}", a, b);
        }

        protected virtual string GetLiteralAndAlso(string a, string b)
        {
            return GetLiteralAnd(a, b);
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
            return string.Format("COALESCE({0}, {1})", a, b);
        }

        protected virtual string GetLiteralConditional(string a, string b, string c)
        {
            throw new NotImplementedException();
        }

        protected virtual string GetLiteralConvert(string a)
        {
            return a;
        }

        protected virtual string GetLiteralConvertChecked(string a)
        {
            return GetLiteralConvert(a);
        }

        protected virtual string GetLiteralDivide(string a, string b)
        {
            return string.Format("{0} / {1}", a, b);
        }

        protected virtual string GetLiteralEqual(string a, string b)
        {
            return string.Format("{0} = {1}", a, b);
        }

        protected virtual string GetLiteralExclusiveOr(string a, string b)
        {
            return string.Format("{0} XOR {1}", a, b);
        }

        protected virtual string GetLiteralGreaterThan(string a, string b)
        {
            return string.Format("{0} > {1}", a, b);
        }

        protected virtual string GetLiteralGreaterThanOrEqual(string a, string b)
        {
            return string.Format("{0} >= {1}", a, b);
        }

        protected virtual string GetLiteralLeftShift(string a, string b)
        {
            throw new NotImplementedException();
        }

        protected virtual string GetLiteralLessThan(string a, string b)
        {
            return string.Format("{0} < {1}", a, b);
        }

        protected virtual string GetLiteralLessThanOrEqual(string a, string b)
        {
            return string.Format("{0} <= {1}", a, b);
        }

        protected virtual string GetLiteralModulo(string a, string b)
        {
            return string.Format("{0} % {1}", a, b);
        }

        protected virtual string GetLiteralMultiply(string a, string b)
        {
            return string.Format("{0} * {1}", a, b);
        }

        protected virtual string GetLiteralMultiplyChecked(string a, string b)
        {
            return GetLiteralMultiply(a, b);
        }

        protected virtual string GetLiteralNegate(string a)
        {
            return string.Format("-{0}", a);
        }

        protected virtual string GetLiteralUnaryPlus(string a)
        {
            return string.Format("+{0}", a);
        }

        protected virtual string GetLiteralNegateChecked(string a)
        {
            return GetLiteralNegate(a);
        }

        protected virtual string GetLiteralNot(string a)
        {
            return string.Format("NOT {0}", a);
        }

        protected virtual string GetLiteralNotEqual(string a, string b)
        {
            return string.Format("{0} <> {1}", a, b);
        }

        protected virtual string GetLiteralOr(string a, string b)
        {
            return string.Format("{0} OR {1}", a, b);
        }

        protected virtual string GetLiteralOrElse(string a, string b)
        {
            return GetLiteralOr(a, b);
        }

        protected virtual string GetLiteralPower(string a, string b)
        {
            return string.Format("POWER ({0}, {1})", a, b);
        }

        protected virtual string GetLiteralRightShift(string a, string b)
        {
            throw new NotImplementedException();
        }

        protected virtual string GetLiteralSubtract(string a, string b)
        {
            return string.Format("{0} - {1}", a, b);
        }

        protected virtual string GetLiteralSubtractChecked(string a, string b)
        {
            return GetLiteralSubtract(a, b);
        }

        protected virtual string GetLiteralIsNull(string a)
        {
            return string.Format("{0} IS NULL", a);
        }

        protected virtual string GetLiteralIsNotNull(string a)
        {
            return string.Format("{0} IS NOT NULL", a);
        }

        protected virtual string GetLiteralConcat(string a, string b)
        {
            // for some vendors, it is "CONCAT(a,b)"
            return string.Format("{0} || {1}", a, b);
        }

        protected virtual string GetLiteralStringLength(string a)
        {
            return string.Format("CHARACTER_LENGTH({0})", a);
        }

        protected virtual string GetLiteralStringToUpper(string a)
        {
            return string.Format("UPPER({0})", a);
            return string.Format("UCASE({0})", a);
        }

        protected virtual string GetLiteralStringToLower(string a)
        {
            return string.Format("LOWER({0})", a);
            return string.Format("LCASE({0})", a);
        }

        protected virtual string GetLiteralLike(string a, string b)
        {
            return string.Format("{0} LIKE {1}", a, b);
        }

        protected virtual string GetLiteralCount(string a)
        {
            return string.Format("COUNT({0})", a);
        }

        protected virtual string GetLiteralMin(string a)
        {
            return string.Format("MIN({0})", a);
        }

        protected virtual string GetLiteralMax(string a)
        {
            return string.Format("MAX({0})", a);
        }

        protected virtual string GetLiteralSum(string a)
        {
            return string.Format("SUM({0})", a);
        }

        protected virtual string GetLiteralAverage(string a)
        {
            return string.Format("AVG({0})", a);
        }

        protected virtual string GetNullLiteral()
        {
            return "NULL";
        }

        /// <summary>
        /// Returns a LIMIT clause around a SELECT clause
        /// </summary>
        /// <param name="select">SELECT clause</param>
        /// <param name="limit">limit value (number of columns to be returned)</param>
        /// <returns></returns>
        public virtual string GetLiteralLimit(string select, string limit)
        {
            return string.Format("{0} LIMIT {1}", select, limit);
        }

        /// <summary>
        /// Returns a LIMIT clause around a SELECT clause, with offset
        /// </summary>
        /// <param name="select">SELECT clause</param>
        /// <param name="limit">limit value (number of columns to be returned)</param>
        /// <param name="offset">first row to be returned (starting from 0)</param>
        /// <param name="offsetAndLimit">limit+offset</param>
        /// <returns></returns>
        public virtual string GetLiteralLimit(string select, string limit, string offset, string offsetAndLimit)
        {
            // default SQL syntax: LIMIT limit OFFSET offset
            return string.Format("{0} LIMIT {1} OFFSET {2}", select, limit, offset);
        }

        protected virtual string GetLiteral(string literal)
        {
            return string.Format("'{0}'", literal.Replace("'", "''"));
        }
    }
}

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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq.Expressions;
using DbLinq.Data.Linq.Sugar;
using DbLinq.Data.Linq.Sugar.ExpressionMutator;
using DbLinq.Data.Linq.Sugar.Expressions;

namespace DbLinq.Data.Linq.Sugar.Expressions
{
    /// <summary>
    /// Holds new expression types (sql related), all well as their operands
    /// </summary>
    [DebuggerDisplay("SpecialExpression {SpecialNodeType}")]
    public class SpecialExpression : OperandsMutableExpression, IExecutableExpression
    {
        public SpecialExpressionType SpecialNodeType { get { return (SpecialExpressionType)NodeType; } }

        protected static Type GetSpecialExpressionTypeType(SpecialExpressionType specialExpressionType, IList<Expression> operands)
        {
            Type defaultType;
            if (operands.Count > 0)
                defaultType = operands[0].Type;
            else
                defaultType = null;
            switch (specialExpressionType) // SETuse
            {
                case SpecialExpressionType.IsNull:
                case SpecialExpressionType.IsNotNull:
                    return typeof(bool);
                case SpecialExpressionType.Concat:
                    return typeof(string);
                case SpecialExpressionType.Count:
                    return typeof(int);
                case SpecialExpressionType.Like:
                    return typeof(bool);
                case SpecialExpressionType.Min:
                case SpecialExpressionType.Max:
                case SpecialExpressionType.Sum:
                    return defaultType; // for such methods, the type is related to the operands type
                case SpecialExpressionType.Average:
                    return typeof(double);
                case SpecialExpressionType.StringLength:
                    return typeof(int);
                case SpecialExpressionType.ToUpper:
                case SpecialExpressionType.ToLower:
                    return typeof(string);
                case SpecialExpressionType.In:
                    return typeof(bool);
                case SpecialExpressionType.SubString:
                    return defaultType;
                default:
                    throw Error.BadArgument("S0058: Unknown SpecialExpressionType value {0}", specialExpressionType);
            }
        }

        public SpecialExpression(SpecialExpressionType expressionType, params Expression[] operands)
            : base((ExpressionType)expressionType, GetSpecialExpressionTypeType(expressionType, operands), operands)
        {
        }

        public SpecialExpression(SpecialExpressionType expressionType, IList<Expression> operands)
            : base((ExpressionType)expressionType, GetSpecialExpressionTypeType(expressionType, operands), operands)
        {
        }

        protected override Expression Mutate2(IList<Expression> newOperands)
        {
            return new SpecialExpression((SpecialExpressionType)NodeType, newOperands);
        }

        public object Execute()
        {
            switch (SpecialNodeType) // SETuse
            {
                case SpecialExpressionType.IsNull:
                    return operands[0].Evaluate() == null;
                case SpecialExpressionType.IsNotNull:
                    return operands[0].Evaluate() != null;
                case SpecialExpressionType.Concat:
                    {
                        var values = new List<string>();
                        foreach (var operand in operands)
                        {
                            var value = operand.Evaluate();
                            if (value != null)
                                values.Add(System.Convert.ToString(value, CultureInfo.InvariantCulture));
                            else
                                values.Add(null);
                        }
                        return string.Concat(values.ToArray());
                    }
                case SpecialExpressionType.Count:
                    {
                        var value = operands[0].Evaluate();
                        // TODO: string is IEnumerable. See what we do here
                        if (value is IEnumerable)
                        {
                            int count = 0;
                            foreach (var dontCare in (IEnumerable)value)
                                count++;
                            return count;
                        }
                        // TODO: by default, shall we answer 1 or throw an exception?
                        return 1;
                    }
                case SpecialExpressionType.Min:
                    {
                        decimal? min = null;
                        foreach (var operand in operands)
                        {
                            var value = System.Convert.ToDecimal(operand.Evaluate());
                            if (!min.HasValue || value < min.Value)
                                min = value;
                        }
                        return System.Convert.ChangeType(min.Value, operands[0].Type);
                    }
                case SpecialExpressionType.Max:
                    {
                        decimal? max = null;
                        foreach (var operand in operands)
                        {
                            var value = System.Convert.ToDecimal(operand.Evaluate());
                            if (!max.HasValue || value > max.Value)
                                max = value;
                        }
                        return System.Convert.ChangeType(max.Value, operands[0].Type);
                    }
                case SpecialExpressionType.Sum:
                    {
                        decimal sum = 0;
                        foreach (var operand in operands)
                            sum += System.Convert.ToDecimal(operand.Evaluate());
                        return System.Convert.ChangeType(sum, operands[0].Type);
                    }
                case SpecialExpressionType.Average:
                    {
                        decimal sum = 0;
                        foreach (var operand in operands)
                            sum += System.Convert.ToDecimal(operand.Evaluate());
                        return sum / operands.Count;
                    }
                case SpecialExpressionType.StringLength:
                    return operands[0].Evaluate().ToString().Length;
                case SpecialExpressionType.ToUpper:
                    return operands[0].Evaluate().ToString().ToUpper();
                case SpecialExpressionType.ToLower:
                    return operands[0].Evaluate().ToString().ToLower();
                case SpecialExpressionType.SubString:
                    {
                        var str = operands[0].Evaluate().ToString();
                        var start = (int)operands[1].Evaluate();
                        if (operands.Count > 2)
                        {
                            var length = (int)operands[2].Evaluate();
                            return str.Substring(start, length);
                        }
                        return str.Substring(start);
                    }
                case SpecialExpressionType.In:
                    // TODO
                default:
                    throw Error.BadArgument("S0116: Unknown SpecialExpressionType ({0})", SpecialNodeType);
            }
        }
    }
}
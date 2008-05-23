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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;

namespace DbLinq.Linq.Data.Sugar.Expressions
{
    /// <summary>
    /// Holds new expression types (sql related), all well as their operands
    /// </summary>
    [DebuggerDisplay("SpecialExpression {SpecialNodeType}")]
    public class SpecialExpression : OperandsMutableExpression
    {
        public SpecialExpressionType SpecialNodeType { get { return (SpecialExpressionType)NodeType; } }

        protected static Type GetSpecialExpressionTypeType(SpecialExpressionType specialExpressionType, IList<Expression> operands)
        {
            // unused by now, may be used later
            //Type defaultType;
            //if (operands.Count > 0)
            //    defaultType = operands[0].Type;
            //else
            //    defaultType = null;
            switch (specialExpressionType)
            {
            case SpecialExpressionType.IsNull:
            case SpecialExpressionType.IsNotNull:
                return typeof(bool);
            case SpecialExpressionType.Concat:
                return typeof(string);
            case SpecialExpressionType.Count:
                return typeof(int);
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

        protected override Expression Mutate2(IList<Expression> operands)
        {
            return new SpecialExpression((SpecialExpressionType)NodeType, operands);
        }
    }
}

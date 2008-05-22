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
using System.Linq.Expressions;

namespace DbLinq.Linq.Data.Sugar.Expressions
{
    public class ScopeExpression : OperandsMutableExpression
    {
        public static ExpressionType ExpressionType { get { return (ExpressionType)1010; } }

        // Involved entities
        public IList<TableExpression> Tables { get; private set; }
        public IList<ColumnExpression> Columns { get; private set; }

        // Clauses
        public IList<Expression> Where { get; private set; }

        // Parent scope: we will climb up to find if we don't find the request table in the current scope
        public ScopeExpression ParentScopePiece { get; private set; }

        public ScopeExpression()
            : base(ExpressionType, null, null)
        {
            Tables = new List<TableExpression>();
            Columns = new List<ColumnExpression>();
            // Local clauses
            Where = new List<Expression>();
        }

        public ScopeExpression(ScopeExpression parentScopePiece)
            : base(ExpressionType, null, null)
        {
            ParentScopePiece = parentScopePiece;
            // Tables and columns are inherited from parent scope
            // TODO: are columns necessary to be copied
            Tables = new List<TableExpression>(parentScopePiece.Tables);
            Columns = new List<ColumnExpression>(parentScopePiece.Columns);
            // Local clauses
            Where = new List<Expression>();
        }

        private ScopeExpression(Type type, IList<Expression> operands)
            : base(ExpressionType, type, operands)
        {
        }

        public ScopeExpression Select(Expression expression)
        {
            return (ScopeExpression)Mutate(new[] { expression });
        }

        protected override Expression Mutate2(IList<Expression> newOperands)
        {
            Type type;
            if (newOperands.Count > 0)
                type = newOperands[0].Type;
            else
                type = Type;
            var scopeExpression = new ScopeExpression(type, newOperands);
            scopeExpression.Tables = Tables;
            scopeExpression.Columns = Columns;
            scopeExpression.Where = Where;
            scopeExpression.ParentScopePiece = ParentScopePiece;
            return scopeExpression;
        }
    }
}

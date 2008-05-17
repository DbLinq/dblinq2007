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
using DbLinq.Linq.Data.Sugar.Expressions;

namespace DbLinq.Linq.Data.Sugar
{
    public class QueryExpressionMatch
    {
        public IDictionary<string, QueryExpression> Captures { get; private set; }
        public bool IsMatch { get; private set; }

        public static implicit operator bool(QueryExpressionMatch match)
        {
            return match.IsMatch;
        }

        private Stack<QueryExpression> expressions = new Stack<QueryExpression>();

        // Match public methods
        public QueryExpressionMatch Is(ExpressionType expressionType)
        {
            Is<QueryOperationExpression>();
            IsMatch = IsMatch && ((QueryOperationExpression)expressions.Peek()).Operation == expressionType;
            return this;
        }

        public QueryExpressionMatch Is<T>()
            where T : QueryExpression
        {
            IsMatch = IsMatch && expressions.Peek() is T;
            return this;
        }

        public QueryExpressionMatch IsConstant(object value)
        {
            Is<QueryConstantExpression>();
            IsMatch = IsMatch && ((QueryConstantExpression)expressions.Peek()).Value == value;
            return this;
        }

        public QueryExpressionMatch GetConstant<T>(out T value)
        {
            value = default(T);
            Is<QueryConstantExpression>();
            if (IsMatch)
            {
                if (((QueryConstantExpression)expressions.Peek()).Value is T)
                    value = (T)((QueryConstantExpression)expressions.Peek()).Value;
                else
                    IsMatch = false;
            }
            return this;
        }

        public QueryExpressionMatch IsFunction(string functionName)
        {
            return Is<QueryConstantExpression>().LoadOperand(0, match => match.IsConstant(functionName));
        }

        public QueryExpressionMatch Or(IEnumerable<Action<QueryExpressionMatch>> evaluations)
        {
            if (IsMatch)
            {
                bool stop = true;
                foreach (var evaluation in evaluations)
                {
                    var newMatch = Clone();
                    evaluation(newMatch);
                    if (newMatch.IsMatch)
                    {
                        stop = false;
                        break;
                    }
                }
                if (stop)
                    IsMatch = false;
            }
            return this;

        }

        public QueryExpressionMatch Or(params Action<QueryExpressionMatch>[] evaluations)
        {
            return Or((IEnumerable<Action<QueryExpressionMatch>>)evaluations);
        }

        public QueryExpressionMatch LoadOperand(int index, Action<QueryExpressionMatch> evaluation)
        {
            //Is<QueryOperationExpression>();
            if (IsMatch)
            {
                var topExpression = expressions.Peek();
                if (index < 0)
                    index = topExpression.Operands.Count - index;
                var newMatch = Clone();
                evaluation(newMatch);
                IsMatch = newMatch.IsMatch;
            }
            return this;
        }

        public QueryExpressionMatch Process(Func<QueryExpression, bool> evaluationPart)
        {
            IsMatch = IsMatch && evaluationPart(expressions.Peek());
            return this;
        }

        private QueryExpressionMatch Clone()
        {
            var clone = new QueryExpressionMatch();
            clone.Captures = new Dictionary<string, QueryExpression>(Captures);
            clone.IsMatch = IsMatch; // should be true. Shall we check?
            clone.expressions = new Stack<QueryExpression>(expressions);
            return clone;
        }

        private QueryExpressionMatch()
        {
        }

        internal QueryExpressionMatch(QueryExpression queryExpression)
        {
            Captures = new Dictionary<string, QueryExpression>();
            IsMatch = true;
            expressions.Push(queryExpression);
        }
    }
}

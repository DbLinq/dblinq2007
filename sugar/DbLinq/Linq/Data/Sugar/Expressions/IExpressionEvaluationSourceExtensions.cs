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
    public static class IExpressionEvaluationSourceExtensions
    {
        // Match public methods
        public static QueryExpressionEvalution Is(this IExpressionEvaluationSource sourceEvaluation, ExpressionType expressionType)
        {
            var source = sourceEvaluation.GetEvaluationSource();
            Is<QueryOperationExpression>(source);
            source.IsEvaluationValid = source.IsEvaluationValid && ((QueryOperationExpression)source.EvaluatedExpression).Operation == expressionType;
            return source;
        }

        public static QueryExpressionEvalution Is<T>(this IExpressionEvaluationSource sourceEvaluation)
            where T : QueryExpression
        {
            var source = sourceEvaluation.GetEvaluationSource();
            source.IsEvaluationValid = source.IsEvaluationValid && source.EvaluatedExpression is T;
            return source;
        }

        public static QueryExpressionEvalution IsConstant(this IExpressionEvaluationSource sourceEvaluation, object value)
        {
            var source = sourceEvaluation.GetEvaluationSource();
            Is<QueryConstantExpression>(source);
            source.IsEvaluationValid = source.IsEvaluationValid && ((QueryConstantExpression)source.EvaluatedExpression).Value == value;
            return source;
        }

        public static QueryExpressionEvalution GetConstant<T>(this IExpressionEvaluationSource sourceEvaluation, out T value)
        {
            var source = sourceEvaluation.GetEvaluationSource();
            value = default(T);
            Is<QueryConstantExpression>(source);
            if (source.IsEvaluationValid)
            {
                if (((QueryConstantExpression)source.EvaluatedExpression).Value is T)
                    value = (T)((QueryConstantExpression)source.EvaluatedExpression).Value;
                else
                    source.IsEvaluationValid = false;
            }
            return source;
        }

        public static T GetConstantOrDefault<T>(this IExpressionEvaluationSource sourceEvaluation)
        {
            T value;
            GetConstant(sourceEvaluation, out value);
            return value;
        }

        public static QueryExpressionEvalution IsFunction(this IExpressionEvaluationSource sourceEvaluation, string functionName)
        {
            return Is<QueryConstantExpression>(sourceEvaluation).LoadOperand(0, match => match.IsConstant(functionName));
        }

        public static QueryExpressionEvalution Or(this IExpressionEvaluationSource sourceEvaluation,
            IEnumerable<Action<IExpressionEvaluationSource>> evaluations)
        {
            var source = sourceEvaluation.GetEvaluationSource();
            if (source.IsEvaluationValid)
            {
                bool stop = true;
                foreach (var evaluation in evaluations)
                {
                    var newMatch = source.CloneEvaluationSource();
                    evaluation(newMatch);
                    if (newMatch.IsEvaluationValid)
                    {
                        stop = false;
                        break;
                    }
                }
                if (stop)
                    source.IsEvaluationValid = false;
            }
            return source;
        }

        public static QueryExpressionEvalution LoadOperand(this IExpressionEvaluationSource sourceEvaluation, int index,
            Action<IExpressionEvaluationSource> evaluation)
        {
            var source = sourceEvaluation.GetEvaluationSource();
            if (source.IsEvaluationValid)
            {
                if (index < 0)
                    index = source.EvaluatedExpression.Operands.Count - index;
                var newMatch = source.CloneEvaluationSource();
                newMatch.EvaluatedExpression = source.EvaluatedExpression.Operands[index];
                evaluation(newMatch);
                source.IsEvaluationValid = newMatch.IsEvaluationValid;
            }
            return source;
        }

        public static QueryExpressionEvalution Process(this IExpressionEvaluationSource sourceEvaluation, Func<QueryExpression, bool> evaluationPart)
        {
            var source = sourceEvaluation.GetEvaluationSource();
            source.IsEvaluationValid = source.IsEvaluationValid && evaluationPart(source.EvaluatedExpression);
            return source;
        }
    }
}

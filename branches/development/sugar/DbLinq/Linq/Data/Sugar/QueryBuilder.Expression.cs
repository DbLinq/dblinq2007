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
    /*
     * System.Linq.Expression are immutable
     * since we will perform optimizations on tree, we need some mutable values
     * The first step is thus to convert an Expression tree to a QueryExpression tree
     * The second step is to identify patterns and replace them by the right QueryExpressions (table, column, etc.)
     */
    public partial class QueryBuilder
    {
        protected virtual ExpressionQuery BuildExpressionQuery(ExpressionChain expressions, QueryContext queryContext)
        {
            var builderContext = new BuilderContext(queryContext);
            BuildExpressionQuery(expressions, builderContext);
            return builderContext.ExpressionQuery;
        }

        protected virtual void BuildExpressionQuery(ExpressionChain expressions, BuilderContext builderContext)
        {
            foreach (var expression in expressions)
            {
                // Convert linq Expressions to QueryOperationExpressions and QueryConstantExpressions 
                var queryExpression = CreateQueryExpression(expression, builderContext);
                // Query expressions language identification 
                queryExpression = AnalyzeLanguagePatterns(queryExpression, builderContext);
                // Query expressions query identification 
                queryExpression = AnalyzeQueryPatterns(queryExpression, builderContext);
            }
        }

        /// <summary>
        /// Top-down pattern analysis.
        /// From here, we convert common QueryExpressions to tables, columns, parameters.
        /// </summary>
        /// <param name="queryExpression">The original expression</param>
        /// <param name="builderContext">The operation specific context</param>
        /// <returns>A new QueryExpression or the original one</returns>
        protected virtual QueryExpression AnalyzePatterns(QueryExpression queryExpression, Func<QueryExpression, BuilderContext, QueryExpression> analyzer, BuilderContext builderContext)
        {
            // we first may replace the current expression
            QueryExpression previousQueryExpression;
            do
            {
                previousQueryExpression = queryExpression;
                queryExpression = analyzer(previousQueryExpression, builderContext);
                // and then, eventually replace its children
                // important: evaluations are right to left, since parameters are pushed left to right
                // and lambda bodies are at first position
                for (int operandIndex = queryExpression.Operands.Count - 1; operandIndex >= 0; operandIndex--)
                {
                    // the new child takes the original place
                    queryExpression.Operands[operandIndex] = AnalyzePatterns(
                        queryExpression.Operands[operandIndex], analyzer, builderContext);
                }
                // the loop is repeated until there's nothing new
            } while (queryExpression != previousQueryExpression);
            return queryExpression;
        }
    }
}

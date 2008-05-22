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
using DbLinq.Linq.Data.Sugar.ExpressionMutator;

namespace DbLinq.Linq.Data.Sugar.Implementation
{
    public class ExpressionWalker
    {
        /// <summary>
        /// Down-top pattern analysis.
        /// From here, we convert common QueryExpressions to tables, columns, parameters.
        /// </summary>
        /// <param name="expression">The original expression</param>
        /// <param name="analyzer"></param>
        /// <param name="builderContext">The operation specific context</param>
        /// <returns>A new QueryExpression or the original one</returns>
        protected virtual Expression Recurse(Expression expression,
                                        Func<Expression, BuilderContext, Expression> analyzer,
                                        BuilderContext builderContext)
        {
            var newOperands = new List<Expression>();
            foreach (var operand in expression.GetOperands())
            {
                if (operand != null)
                    newOperands.Add(analyzer(operand, builderContext));
                else
                    newOperands.Add(null);
            }
            return expression.Mutate(newOperands);
        }

        /// <summary>
        /// Down-top pattern analysis, with integrated Analyze() method
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual Expression Recurse(Expression expression, BuilderContext builderContext)
        {
            return Recurse(expression, Analyze, builderContext);
        }

        protected virtual Expression Analyze(Expression expression, BuilderContext builderContext)
        {
            return expression;
        }
    }
}

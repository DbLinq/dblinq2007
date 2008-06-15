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

using System.Linq.Expressions;
using DbLinq.Data.Linq.Sugar;
using DbLinq.Data.Linq.Sugar.ExpressionMutator;

namespace DbLinq.Data.Linq.Sugar.Implementation
{
    /// <summary>
    /// Analyzes language patterns and replace them with standard expressions
    /// </summary>
    public class ExpressionLanguageParser : IExpressionLanguageParser
    {
        public virtual Expression Parse(Expression expression, BuilderContext builderContext)
        {
            return expression.Recurse(e => Analyze(e, builderContext));
        }

        protected virtual Expression Analyze(Expression expression, BuilderContext builderContext)
        {
            var methodCallExpression = expression as MethodCallExpression;
            if(methodCallExpression!=null)
            {
                if(methodCallExpression.Method.DeclaringType.Name=="Convert")
                {
                    if (methodCallExpression.Method.Name == "ToBoolean")
                        return Expression.Convert(methodCallExpression.Arguments[0], methodCallExpression.Type);
                }
            }
            return expression;
        }
    }
}
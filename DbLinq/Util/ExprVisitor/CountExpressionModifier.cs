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
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace DbLinq.Util.ExprVisitor
{
    /// <summary>
    /// when the user calls 'Count(i =&gt; 2)', we lost the information about what 'i' was referring to.
    /// That info lives in the preceding Select.
    /// This helper class edits our expression to put the info back in:
    /// Given 'i=>2', we return 'p=>ProductID>2'
    /// </summary>
    public class CountExpressionModifier : ExpressionVisitor
    {
        #region CountExpressionModifier
        LambdaExpression _substituteExpr;
        public CountExpressionModifier(LambdaExpression substituteExpr)
        {
            _substituteExpr = substituteExpr;
        }
        public Expression Modify(Expression expression)
        {
            return Visit(expression);
        }
        protected override Expression VisitLambda(LambdaExpression lambda)
        {
            Expression body2 = base.Visit(lambda.Body);
            Expression lambda2 = Expression.Lambda(body2, _substituteExpr.Parameters[0]);
            return lambda2;
        }
        protected override Expression VisitParameter(ParameterExpression p)
        {
            return _substituteExpr.Body;
        }
        #endregion
    }
}

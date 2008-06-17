#region MIT license
// 
// MIT license
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
    public delegate object FunctionReturningObject();

    public class LocalExpressionChecker : ExpressionVisitor
    {
        bool _foundParameter = false;
        bool _foundConstant = false;

        public LocalExpressionChecker()
        {
        }

        /// <summary>
        /// given 'localObject.FieldName', check if it's local, 
        /// and try to return compiled lambda expression {()->localObject.FieldName}.
        /// </summary>
        public static bool TryMatchLocalExpression(Expression expr, out FunctionReturningObject funcReturningObj)
        {
            try
            {
                LocalExpressionChecker obj = new LocalExpressionChecker();
                obj.Visit(expr);

                bool isLocal = obj._foundConstant && !obj._foundParameter;
                if (isLocal)
                {
                    //compile an access function
                    Expression expr2 = expr;
                    if (expr.Type.IsPrimitive())
                        expr2 = Expression.Convert(expr, typeof(object));

                    var empty = new ParameterExpression[] { };
                    LambdaExpression lambda = Expression.Lambda<FunctionReturningObject>(expr2, empty);
                    Delegate delg = lambda.Compile();
                    funcReturningObj = delg as FunctionReturningObject;
                }
                else
                {
                    funcReturningObj = null;
                }
                return isLocal;

            }
            catch (Exception ex)
            {
                Console.WriteLine("TryMatchLocalExpression failed: " + ex);
                throw;
            }
        }

        protected override Expression VisitConstant(ConstantExpression c)
        {
            _foundConstant = true;
            return base.VisitConstant(c);
        }
        protected override Expression VisitParameter(ParameterExpression p)
        {
            _foundParameter = true;
            return base.VisitParameter(p);
        }
    }
}

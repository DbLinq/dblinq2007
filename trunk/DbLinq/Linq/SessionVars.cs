#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne
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
using DbLinq.Data.Linq;
using DbLinq.Logging;
using DbLinq.Util;

namespace DbLinq.Linq
{
    /// <summary>
    /// holds queries as they are passed in via MTable.CreateQuery() and MTable.Execute()
    /// </summary>
    public class SessionVars
    {
        public DataContext Context { get; private set; }
        public IMTable Table { get; private set; }

        /// <summary>
        /// chain of expressions processed so far. 
        /// for db.Employees.Where(...).Select(...), we would hold 2 expressions.
        /// </summary>
        public List<MethodCallExpression> ExpressionChain { get { return expressionChain; } }
        private readonly List<MethodCallExpression> expressionChain = new List<MethodCallExpression>();
        
        /// <summary>
        /// optional scalar expression, terminating the chain
        /// </summary>
        public Expression ScalarExpression { get; set; }

        public SessionVars(DataContext context, IMTable table)
        {
            Context = context;
            Table = table;
        }

        /// <summary>
        /// clone existing SessionVars and add latest Expression.
        /// </summary>
        public SessionVars(SessionVars vars)
        { 
            Context = vars.Context;
            Table = vars.Table;
            expressionChain = new List<MethodCallExpression>(vars.ExpressionChain);
            ScalarExpression = vars.ScalarExpression;
        }

        /// <summary>
        /// there can be many calls to Add()
        /// </summary>
        public SessionVars Add(Expression expressionToAdd)
        {
            //Context.Logger.WriteExpression(Level.Debug, expressionToAdd);
            MethodCallExpression exprCall = expressionToAdd as MethodCallExpression;
            if (exprCall == null)
                throw new ArgumentException("L77 Expression must be a MethodCall");
            ExpressionChain.Add(exprCall);
            return this;
        }

        /// <summary>
        /// there can be only one call to AddScalar() - occurs eg. during "Count()"
        /// </summary>
        public SessionVars AddScalar(Expression expression)
        {
            //Context.Logger.WriteExpression(Level.Debug, expression);
            MethodCallExpression exprCall = expression as MethodCallExpression;
            if (exprCall == null)
                throw new ArgumentException("L85 Expression must be a MethodCall");
            ScalarExpression = expression;
            return this;
        }
    }

}

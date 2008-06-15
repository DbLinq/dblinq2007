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
using DbLinq.Factory;

namespace DbLinq.Linq.Data.Sugar.Implementation
{
    /// <summary>
    /// This class will be removed when fully switching to Sugar
    /// </summary>
    public class QueryGenerator : IQueryGenerator
    {
        public IQueryBuilder QueryBuilder { get; set; }

        public QueryGenerator()
        {
            QueryBuilder = ObjectFactory.Get<IQueryBuilder>();
        }

        public SessionVarsParsed GenerateQuery(SessionVars vars, Type T)
        {
            var expressionChain = new ExpressionChain(vars.ExpressionChain);
            // the forgotten one: the SessionVars have a list of expressions, and may end with a ScalarExpression
            // (that we don't differentiate in Sugar)
            if (vars.ScalarExpression != null)
                expressionChain.Expressions.Add(vars.ScalarExpression);

            var varsParsed = new SessionVarsParsed(vars);
            varsParsed.Query = QueryBuilder.GetQuery(expressionChain, new QueryContext(vars.Context));

            return varsParsed;
        }
    }
}
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
    public class BuilderContext
    {
        // global context
        public QueryContext QueryContext { get; private set; }

        // true if the columns are part of the select (or similar statement)
        public bool RequestColumns { get; set; }

        // current expression being built
        public ExpressionQuery ExpressionQuery { get; private set; }

        // context dependant values
        public IDictionary<QueryExpression, Expression> ExpressionByQueryExpression { get; private set; }

        // parameters are pushed here and pulled when required
        public Stack<QueryExpression> CallStack { get; private set; }

        public IDictionary<string, QueryExpression> Parameters { get; private set; }

        public BuilderContext(QueryContext queryContext)
        {
            QueryContext = queryContext;
            ExpressionQuery = new ExpressionQuery();
            ExpressionByQueryExpression = new Dictionary<QueryExpression, Expression>();
            CallStack = new Stack<QueryExpression>();
            Parameters = new Dictionary<string, QueryExpression>();
        }

        private BuilderContext()
        { }

        public BuilderContext Clone()
        {
            var builderContext = new BuilderContext();
            // scope independent parts
            builderContext.QueryContext = QueryContext;
            builderContext.ExpressionQuery = ExpressionQuery;
            builderContext.ExpressionByQueryExpression = ExpressionByQueryExpression;
            builderContext.CallStack = CallStack;
            // scope dependent parts
            builderContext.Parameters = new Dictionary<string, QueryExpression>(Parameters);
            return builderContext;
        }

        private class ColumnRequesterHolder : IDisposable
        {
            protected bool PreviousRequestColumns;
            protected BuilderContext BuilderContext;

            public void Dispose()
            {
                BuilderContext.RequestColumns = PreviousRequestColumns;
            }

            public ColumnRequesterHolder(BuilderContext builderContext)
            {
                BuilderContext = builderContext;
                PreviousRequestColumns = BuilderContext.RequestColumns;
                BuilderContext.RequestColumns = true;
            }
        }

        public IDisposable ColumnRequester()
        {
            return new ColumnRequesterHolder(this);
        }
    }
}

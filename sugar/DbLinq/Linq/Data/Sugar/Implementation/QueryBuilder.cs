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
using DbLinq.Logging;
using DbLinq.Util;

namespace DbLinq.Linq.Data.Sugar.Implementation
{
    public class QueryBuilder : IQueryBuilder
    {
        public IPiecesBuilder PiecesBuilder { get; set; }
        public IPiecesLanguageParser PiecesLanguageParser { get; set; }
        public IPiecesLanguageOptimizer PiecesLanguageOptimizer { get; set; }
        public IPiecesDispatcher PiecesDispatcher { get; set; }

        protected virtual PiecesQuery BuildExpressionQuery(ExpressionChain expressions, QueryContext queryContext)
        {
            var builderContext = new BuilderContext(queryContext);
            BuildExpressionQuery(expressions, builderContext);
            return builderContext.PiecesQuery;
        }

        protected virtual void BuildExpressionQuery(ExpressionChain expressions, BuilderContext builderContext)
        {
            foreach (var expression in expressions)
            {
                builderContext.QueryContext.DataContext.Logger.WriteExpression(Level.Debug, expression);
                // Convert linq Expressions to QueryOperationExpressions and QueryConstantExpressions 
                var queryExpression = PiecesBuilder.CreateQueryExpression(expression, builderContext);
                // Query expressions language identification and optimization
                queryExpression = PiecesLanguageParser.AnalyzeLanguagePatterns(queryExpression, builderContext);
                queryExpression = PiecesLanguageOptimizer.AnalyzeLanguagePatterns(queryExpression, builderContext);
                // Query expressions query identification 
                queryExpression = PiecesDispatcher.AnalyzeQueryPatterns(queryExpression, builderContext);
            }
        }

        protected virtual Query BuildSqlQuery(PiecesQuery expressionQuery, QueryContext queryContext)
        {
            var sql = "";
            var sqlQuery = new Query(sql, expressionQuery.Parameters);
            return sqlQuery;
        }

        [DbLinqToDo]
        protected virtual Query GetFromCache(ExpressionChain expressions)
        {
            return null;
        }

        [DbLinqToDo]
        protected virtual void SetInCache(ExpressionChain expressions, Query sqlQuery)
        {
        }

        public Query GetQuery(ExpressionChain expressions, QueryContext queryContext)
        {
            var sqlQuery = GetFromCache(expressions);
            if (sqlQuery == null)
            {
                var expressionsQuery = BuildExpressionQuery(expressions, queryContext);
                sqlQuery = BuildSqlQuery(expressionsQuery, queryContext);
                SetInCache(expressions, sqlQuery);
            }
            throw new Exception("Can't go further anyway...");
            return sqlQuery;
        }

        public QueryBuilder()
        {
            PiecesBuilder = ObjectFactory.Get<IPiecesBuilder>();
            PiecesLanguageParser = ObjectFactory.Get<IPiecesLanguageParser>();
            PiecesLanguageOptimizer = ObjectFactory.Get<IPiecesLanguageOptimizer>();
            PiecesDispatcher = ObjectFactory.Get<IPiecesDispatcher>();
        }
    }
}
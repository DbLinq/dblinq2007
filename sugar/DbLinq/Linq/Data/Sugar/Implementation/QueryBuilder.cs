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
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using DbLinq.Factory;
using DbLinq.Linq.Data.Sugar.ExpressionMutator;
using DbLinq.Linq.Data.Sugar.Expressions;
using DbLinq.Logging;
using DbLinq.Util;

namespace DbLinq.Linq.Data.Sugar.Implementation
{
    /// <summary>
    /// Full query builder, with cache management
    /// 1. Parses Linq Expression
    /// 2. Generates SQL
    /// </summary>
    public class QueryBuilder : IQueryBuilder
    {
        public IExpressionLanguageParser ExpressionLanguageParser { get; set; }
        public IExpressionOptimizer ExpressionOptimizer { get; set; }
        public IExpressionDispatcher ExpressionDispatcher { get; set; }
        public ISqlBuilder SqlBuilder { get; set; }

        public QueryBuilder()
        {
            ExpressionLanguageParser = ObjectFactory.Get<IExpressionLanguageParser>();
            ExpressionOptimizer = ObjectFactory.Get<IExpressionOptimizer>();
            ExpressionDispatcher = ObjectFactory.Get<IExpressionDispatcher>();
            SqlBuilder = ObjectFactory.Get<ISqlBuilder>();
        }

        /// <summary>
        /// Builds the ExpressionQuery:
        /// - parses Expressions and builds row creator
        /// - checks names unicity
        /// </summary>
        /// <param name="expressions"></param>
        /// <param name="queryContext"></param>
        /// <returns></returns>
        protected virtual ExpressionQuery BuildExpressionQuery(ExpressionChain expressions, QueryContext queryContext)
        {
            var builderContext = new BuilderContext(queryContext);
            BuildExpressionQuery(expressions, builderContext);
            CheckTablesAlias(builderContext);
            CheckParametersAlias(builderContext);
            return builderContext.ExpressionQuery;
        }

        /// <summary>
        /// Finds all registered tables or columns with the given name.
        /// We exclude parameter because they won't be prefixed/suffixed the same way (well, that's a guess, I hope it's a good one)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual IList<Expression> FindExpressionsByName(string name, BuilderContext builderContext)
        {
            var expressions = new List<Expression>();
            expressions.AddRange(from t in builderContext.EnumerateAllTables() where t.Alias == name select (Expression)t);
            expressions.AddRange(from c in builderContext.EnumerateScopeColumns() where c.Alias == name select (Expression)c);
            return expressions;
        }

        protected virtual string MakeName(string aliasBase, int index, string anonymousBase, BuilderContext builderContext)
        {
            if (string.IsNullOrEmpty(aliasBase))
                aliasBase = anonymousBase;
            return string.Format("{0}{1}", aliasBase, index);
        }

        protected virtual string MakeTableName(string aliasBase, int index, BuilderContext builderContext)
        {
            return MakeName(aliasBase, index, "t", builderContext);
        }

        protected virtual string MakeParameterName(string aliasBase, int index, BuilderContext builderContext)
        {
            return MakeName(aliasBase, index, "p", builderContext);
        }

        /// <summary>
        /// Give all non-aliased tables a name
        /// </summary>
        /// <param name="builderContext"></param>
        protected virtual void CheckTablesAlias(BuilderContext builderContext)
        {
            var tables = builderContext.EnumerateAllTables().ToList();
            // just to be nice: if we have only one table involved, there's no need to alias it
            if (tables.Count == 1)
            {
                tables[0].Alias = null;
            }
            else
            {
                foreach (var tableExpression in tables)
                {
                    // if no alias, or duplicate alias
                    if (string.IsNullOrEmpty(tableExpression.Alias) ||
                        FindExpressionsByName(tableExpression.Alias, builderContext).Count > 1)
                    {
                        int anonymousIndex = 0;
                        var aliasBase = tableExpression.Alias;
                        // we try to assign one until we have a unique alias
                        do
                        {
                            tableExpression.Alias = MakeTableName(aliasBase, ++anonymousIndex, builderContext);
                        } while (FindExpressionsByName(tableExpression.Alias, builderContext).Count != 1);
                    }
                }
            }
        }

        protected virtual IList<ExternalParameterExpression> FindParametersByName(string name, BuilderContext builderContext)
        {
            return (from p in builderContext.ExpressionQuery.Parameters where p.Alias == name select p).ToList();
        }

        /// <summary>
        /// Gives anonymous parameters a name and checks for names unicity
        /// The fact of giving a nice name just helps for readability
        /// </summary>
        /// <param name="builderContext"></param>
        protected virtual void CheckParametersAlias(BuilderContext builderContext)
        {
            foreach (var externalParameterEpxression in builderContext.ExpressionQuery.Parameters)
            {
                if (string.IsNullOrEmpty(externalParameterEpxression.Alias)
                    || FindParametersByName(externalParameterEpxression.Alias, builderContext).Count > 1)
                {
                    int anonymousIndex = 0;
                    var aliasBase = externalParameterEpxression.Alias;
                    // we try to assign one until we have a unique alias
                    do
                    {
                        externalParameterEpxression.Alias = MakeTableName(aliasBase, ++anonymousIndex, builderContext);
                    } while (FindExpressionsByName(externalParameterEpxression.Alias, builderContext).Count != 1);
                }
            }
        }

        /// <summary>
        /// Builds and chains the provided Expressions
        /// </summary>
        /// <param name="expressions"></param>
        /// <param name="builderContext"></param>
        protected virtual void BuildExpressionQuery(ExpressionChain expressions, BuilderContext builderContext)
        {
            var previousExpression = ExpressionDispatcher.CreateTableExpression(expressions.Expressions[0], builderContext);
            foreach (var expression in expressions)
            {
                builderContext.QueryContext.DataContext.Logger.WriteExpression(Level.Debug, expression);
                // Convert linq Expressions to QueryOperationExpressions and QueryConstantExpressions 
                // Query expressions language identification
                var currentExpression = ExpressionLanguageParser.Parse(expression, builderContext);
                // Query expressions query identification 
                currentExpression = ExpressionDispatcher.Analyze(currentExpression, previousExpression, builderContext);

                previousExpression = currentExpression;
            }
            ExpressionDispatcher.BuildSelect(previousExpression, builderContext);
            // now, we optimize anything we can
            OptimizeQuery(builderContext);
            // finally, compile our object creation method
            CompileRowCreator(builderContext);
        }

        /// <summary>
        /// Builds the delegate to create a row
        /// </summary>
        /// <param name="builderContext"></param>
        protected virtual void CompileRowCreator(BuilderContext builderContext)
        {
            builderContext.ExpressionQuery.RowObjectCreator = builderContext.ExpressionQuery.Select.Select.Compile();
        }

        /// <summary>
        /// Optimizes the query by optimizing subexpressions, and preparsing constant expressions
        /// </summary>
        /// <param name="builderContext"></param>
        protected virtual void OptimizeQuery(BuilderContext builderContext)
        {
            for (int scopeExpressionIndex = 0; scopeExpressionIndex < builderContext.ScopeExpressions.Count; scopeExpressionIndex++)
            {
                var scopeExpression = builderContext.ScopeExpressions[scopeExpressionIndex];

                // where clauses
                for (int whereIndex = 0; whereIndex < scopeExpression.Where.Count; whereIndex++)
                {
                    scopeExpression.Where[whereIndex] = ExpressionOptimizer.Optimize(scopeExpression.Where[whereIndex],
                                                                                     builderContext);
                }

                // select clause
                scopeExpression = (ScopeExpression)ExpressionOptimizer.Optimize(scopeExpression, builderContext);

                // limit clauses
                if (scopeExpression.Offset != null)
                    scopeExpression.Offset = ExpressionOptimizer.Optimize(scopeExpression.Offset, builderContext);
                if (scopeExpression.Limit != null)
                    scopeExpression.Limit = ExpressionOptimizer.Optimize(scopeExpression.Limit, builderContext);
                if (scopeExpression.Offset != null && scopeExpression.Limit != null)
                {
                    scopeExpression.OffsetAndLimit = ExpressionOptimizer.Optimize(
                        Expression.Add(scopeExpression.Offset, scopeExpression.Limit),
                        builderContext);
                }

                builderContext.ScopeExpressions[scopeExpressionIndex] = scopeExpression;
            }
            builderContext.ExpressionQuery.Select = (ScopeExpression)ExpressionOptimizer.Optimize(builderContext.ExpressionQuery.Select, builderContext);
        }

        protected virtual Query BuildSqlQuery(ExpressionQuery expressionQuery, QueryContext queryContext)
        {
            var sql = SqlBuilder.Build(expressionQuery, queryContext);
            var sqlQuery = new Query(queryContext.DataContext, sql, expressionQuery.Parameters, expressionQuery.RowObjectCreator, expressionQuery.Select.ExecuteMethodName);
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

        /// <summary>
        /// Main entry point for the class. Builds or retrive from cache a SQL query corresponding to given Expressions
        /// </summary>
        /// <param name="expressions"></param>
        /// <param name="queryContext"></param>
        /// <returns></returns>
        public Query GetQuery(ExpressionChain expressions, QueryContext queryContext)
        {
            var query = GetFromCache(expressions);
            if (query == null)
            {
                var expressionsQuery = BuildExpressionQuery(expressions, queryContext);
                query = BuildSqlQuery(expressionsQuery, queryContext);
                queryContext.DataContext.Logger.Write(Level.Debug, "SQL: {0}", query.Sql);
                SetInCache(expressions, query);
            }
            return query;
        }
    }
}

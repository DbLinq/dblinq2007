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
using DbLinq.Factory;
using DbLinq.Linq.Data.Sugar.Pieces;
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
        public IPiecesBuilder PiecesBuilder { get; set; }
        public IPiecesLanguageParser PiecesLanguageParser { get; set; }
        public IPiecesLanguageOptimizer PiecesLanguageOptimizer { get; set; }
        public IPiecesDispatcher PiecesDispatcher { get; set; }

        protected virtual PiecesQuery BuildExpressionQuery(ExpressionChain expressions, QueryContext queryContext)
        {
            var builderContext = new BuilderContext(queryContext);
            BuildExpressionQuery(expressions, builderContext);
            CheckTablesAlias(builderContext);
            return builderContext.PiecesQuery;
        }

        protected virtual IList<Piece> FindPiecesByName(string name, BuilderContext builderContext)
        {
            var pieces = new List<Piece>();
            pieces.AddRange(from t in builderContext.PiecesQuery.Tables where t.Alias == name select (Piece)t);
            pieces.AddRange(from c in builderContext.PiecesQuery.Columns where c.Alias == name select (Piece)c);
            return pieces;
        }

        protected virtual string GetAnonymousTableName(string aliasBase, int anonymousIndex, BuilderContext builderContext)
        {
            if (string.IsNullOrEmpty(aliasBase))
                aliasBase = "t";
            return string.Format("{0}{1}", aliasBase, anonymousIndex);
        }

        /// <summary>
        /// Give all non-aliased tables a name
        /// </summary>
        /// <param name="builderContext"></param>
        protected virtual void CheckTablesAlias(BuilderContext builderContext)
        {
            int anonymousIndex = 0;
            foreach (TablePiece tablePiece in builderContext.PiecesQuery.Tables)
            {
                // if no alias, or duplicate alias
                if (string.IsNullOrEmpty(tablePiece.Alias) || FindPiecesByName(tablePiece.Alias, builderContext).Count > 1)
                {
                    var aliasBase = tablePiece.Alias;
                    // we try to assign one until we have a unique alias
                    do
                    {
                        tablePiece.Alias = GetAnonymousTableName(aliasBase, ++anonymousIndex, builderContext);
                    } while (FindPiecesByName(tablePiece.Alias, builderContext).Count != 1);
                }
            }
            // TODO: move down this to IVendor
            foreach (TablePiece tablePiece in builderContext.PiecesQuery.Tables)
            {
                tablePiece.Alias += "$";
            }
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
                // The last request is the select, whatever the loop count is
                builderContext.PiecesQuery.Select = PiecesDispatcher.Dispatch(queryExpression, builderContext);
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
            var query = GetFromCache(expressions);
            if (query == null)
            {
                var expressionsQuery = BuildExpressionQuery(expressions, queryContext);
                query = BuildSqlQuery(expressionsQuery, queryContext);
                SetInCache(expressions, query);
            }
            throw new Exception("Can't go further anyway...");
            return query;
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
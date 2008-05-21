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
using DbLinq.Linq.Data.Sugar.Pieces;

namespace DbLinq.Linq.Data.Sugar
{
    public class BuilderContext
    {
        // Global context
        public QueryContext QueryContext { get; private set; }

        // Current expression being built
        public PiecesQuery PiecesQuery { get; private set; }

        // Build context: values here are related to current context, and can change with it
        public ScopePiece CurrentScope { get; private set; }
        public IList<ScopePiece> ScopePieces { get; private set; }
        public IDictionary<Type, MetaTablePiece> MetaTables { get; private set; }
        public IDictionary<string, Piece> Parameters { get; private set; }

        /// <summary>
        /// Helper to enumerate all registered tables
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TablePiece> EnumerateTables()
        {
            foreach (var scopePiece in ScopePieces)
            {
                foreach (var table in scopePiece.Tables)
                    yield return table;
            }
        }

        /// <summary>
        /// Helper to enumerate all registered columns
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ColumnPiece> EnumerateColumns()
        {
            foreach (var scopePiece in ScopePieces)
            {
                foreach (var column in scopePiece.Columns)
                    yield return column;
            }
        }

        public BuilderContext(QueryContext queryContext)
        {
            ScopePieces = new List<ScopePiece>();
            CurrentScope = new ScopePiece();
            ScopePieces.Add(CurrentScope);
            QueryContext = queryContext;
            PiecesQuery = new PiecesQuery();
            MetaTables = new Dictionary<Type, MetaTablePiece>();
            Parameters = new Dictionary<string, Piece>();
        }

        private BuilderContext()
        { }

        /// <summary>
        /// Creates a new BuilderContext where parameters have a local scope
        /// </summary>
        /// <returns></returns>
        public BuilderContext NewQuote()
        {
            var builderContext = new BuilderContext();

            // scope independent parts
            builderContext.QueryContext = QueryContext;
            builderContext.PiecesQuery = PiecesQuery;
            builderContext.MetaTables = MetaTables;
            builderContext.CurrentScope = CurrentScope;
            builderContext.ScopePieces = ScopePieces;

            // scope dependent parts
            builderContext.Parameters = new Dictionary<string, Piece>(Parameters);

            return builderContext;
        }

        /// <summary>
        /// Creates a new BuilderContext with a new query scope
        /// </summary>
        /// <returns></returns>
        public BuilderContext NewScope()
        {
            var builderContext = new BuilderContext();

            // we basically copy everything
            builderContext.QueryContext = QueryContext;
            builderContext.PiecesQuery = PiecesQuery;
            builderContext.MetaTables = MetaTables;
            builderContext.Parameters = Parameters;
            builderContext.ScopePieces = ScopePieces;

            // except CurrentScope, of course
            builderContext.CurrentScope = new ScopePiece(CurrentScope);

            ScopePieces.Add(builderContext.CurrentScope);

            return builderContext;
        }
    }
}

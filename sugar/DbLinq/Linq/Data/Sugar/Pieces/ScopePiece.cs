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

using System.Collections.Generic;

namespace DbLinq.Linq.Data.Sugar.Pieces
{
    /// <summary>
    /// Represents a subexpression, with its scope
    /// </summary>
    public class ScopePiece: Piece
    {
        // Involved entities
        public IList<TablePiece> Tables { get; private set; }
        public IList<ColumnPiece> Columns { get; private set; }

        // Clauses
        public IList<Piece> Where { get; private set; }
        public Piece Select { get; set; } 

        // Parent scope: we will climb up to find if we don't find the request table in the current scope
        public ScopePiece ParentScopePiece { get; private set; }

        public ScopePiece()
        {
            Tables = new List<TablePiece>();
            Columns = new List<ColumnPiece>();
            // Local clauses
            Where = new List<Piece>();
        }

        public ScopePiece(ScopePiece parentScopePiece)
        {
            ParentScopePiece = parentScopePiece;
            // Tables and columns are inherited from parent scope
            // TODO: are columns necessary to be copied
            Tables = new List<TablePiece>(parentScopePiece.Tables);
            Columns = new List<ColumnPiece>(parentScopePiece.Columns);
            // Local clauses
            Where = new List<Piece>();
        }
    }
}

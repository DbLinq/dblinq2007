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
using System.Diagnostics;
using DbLinq.Linq.Data.Sugar.Pieces;

namespace DbLinq.Linq.Data.Sugar.Pieces
{
    /// <summary>
    /// Describes a column, related to a table
    /// </summary>
    [DebuggerDisplay("ColumnPiece {Table.Name} (as {Table.Alias}).{Name}")]
    public class ColumnPiece : Piece
    {
        public TablePiece Table { get; private set; }
        public string Name { get; private set; }
        public Type Type { get; private set; }

        public string Alias { get; set; }

        public bool Request { get; set; }
        public int RequestIndex { get; set; }

        public ColumnPiece(TablePiece table, string name, Type type)
        {
            Table = table;
            Name = name;
            Type = type;
        }

        protected override bool InnerEquals(Piece other)
        {
            var columnOther = (ColumnPiece) other;
            return Name == columnOther.Name
                   && Table.Equals(columnOther.Table)
                   && Type == columnOther.Type;
        }
    }
}
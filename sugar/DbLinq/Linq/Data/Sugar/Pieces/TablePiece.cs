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
    /// A table is a default table, or a joined table
    /// Different joins specify different tables
    /// </summary>
    [DebuggerDisplay("TablePiece {Name} (as {Alias})")]
    public class TablePiece : Piece
    {
        // Table idenfitication
        public string Name { get; private set; }
        public Type Type { get; private set; }

        // Join: if this table is related to another, the following informations are filled
        public Piece JoinPiece { get; private set; } // full information is here, ReferencedTable and Join could be (in theory) extracted from this
        public TableJoinType JoinType { get; private set; }
        public TablePiece JoinedTable { get; private set; }

        public string Alias { get; set; }

        public void Join(TableJoinType joinType, TablePiece joinedTable, Piece joinPiece)
        {
            JoinPiece = joinPiece;
            JoinType = joinType;
            JoinedTable = joinedTable;
        }

        /// <summary>
        /// Ctor for associated table
        /// </summary>
        /// <param name="type">.NET type</param>
        /// <param name="name">Table base name</param>
        /// <param name="joinType"></param>
        /// <param name="joinedTable"></param>
        /// <param name="joinPiece"></param>
        public TablePiece(Type type, string name, TableJoinType joinType, TablePiece joinedTable, Piece joinPiece)
        {
            Type = type;
            Name = name;
            JoinPiece = joinPiece;
            JoinType = joinType;
            JoinedTable = joinedTable;
        }

        /// <summary>
        /// Ctor for default table
        /// </summary>
        /// <param name="type">.NET type</param>
        /// <param name="name">Table base name</param>
        public TablePiece(Type type, string name)
            : this(type, name, TableJoinType.Default, null, null)
        {
        }

        protected virtual bool EquatableEquals<T>(IEquatable<T> a, IEquatable<T> b)
        {
            if (a == null)
                return b == null;
            return a.Equals(b);
        }

        protected override bool InnerEquals(Piece other)
        {
            var tableOther = (TablePiece)other;
            return Name == tableOther.Name
                   && Type == tableOther.Type
                   && JoinType == tableOther.JoinType
                   && EquatableEquals(JoinedTable, tableOther.JoinedTable)
                   && EquatableEquals(JoinPiece, tableOther.JoinPiece);
        }
    }
}

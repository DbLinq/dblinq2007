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
using System.Reflection;

namespace DbLinq.Linq.Data.Sugar.Pieces
{
    /// <summary>
    /// A MetaTablePiece contains aliases for tables (used on joins)
    /// </summary>
    public class MetaTablePiece : Piece
    {
        protected IDictionary<MemberInfo, TablePiece> Aliases;

        public TablePiece GetTablePiece(MemberInfo memberInfo)
        {
            TablePiece tablePiece;
            Aliases.TryGetValue(memberInfo, out tablePiece);
            return tablePiece;
        }

        public MetaTablePiece(IDictionary<MemberInfo, TablePiece> aliases)
        {
            Aliases = aliases;
        }

        protected override bool InnerEquals(Piece other)
        {
            var metaTableOther = (MetaTablePiece)other;

            if (Aliases.Count != metaTableOther.Aliases.Count)
                return false;

            foreach (var memberInfo in Aliases.Keys)
            {
                if (!Aliases[memberInfo].Equals(metaTableOther.GetTablePiece(memberInfo)))
                    return false;
            }
            return true;
        }
    }
}

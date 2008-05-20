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
using System.Linq.Expressions;
using System.Reflection;
using DbLinq.Linq.Data.Sugar.Pieces;

namespace DbLinq.Linq.Data.Sugar.Implementation
{
    /// <summary>
    /// Service to get information from a Piece
    /// </summary>
    public class PiecesService
    {
        /// <summary>
        /// Returns a MethodInfo from a given expression, or null if the types are not related
        /// </summary>
        /// <param name="piece"></param>
        /// <returns></returns>
        public virtual MethodInfo GetMethodInfo(Piece piece)
        {
            return piece.GetConstantOrDefault<MethodInfo>();
        }

        /// <summary>
        /// Returns a MemberInfo from a given expression, or null on unrelated types
        /// </summary>
        /// <param name="piece"></param>
        /// <returns></returns>
        public virtual MemberInfo GetMemberInfo(Piece piece)
        {
            return piece.GetConstantOrDefault<MemberInfo>();
        }

        /// <summary>
        /// Returns a member name, from a given expression, or null if it can not be extracted
        /// </summary>
        /// <param name="piece"></param>
        /// <returns></returns>
        public virtual string GetMemberName(Piece piece)
        {
            var memberInfo = GetMemberInfo(piece);
            if (memberInfo != null)
                return memberInfo.Name;
            return piece.GetConstantOrDefault<string>();
        }

        /// <summary>
        /// Returns a queried type from a given expression, or null if no type can be found
        /// </summary>
        /// <param name="piece"></param>
        /// <returns></returns>
        public virtual Type GetQueriedType(Piece piece)
        {
            var constantExpression = piece as ConstantPiece;
            if (constantExpression != null)
                return GetQueriedType(constantExpression.Value.GetType());
            return null;
        }

        /// <summary>
        /// Extracts the type from the potentially generic type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual Type GetQueriedType(Type type)
        {
            if (typeof(IQueryable).IsAssignableFrom(type))
            {
                if (type.IsGenericType)
                    return type.GetGenericArguments()[0];
            }
            return null;
        }

        public virtual string GetParameterName(Piece piece)
        {
            string name = null;
            piece.Is(ExpressionType.Parameter).LoadOperand(0, m => m.GetConstant(out name));
            return name;
        }

        public virtual IList<Piece> MergeParameters(IList<Piece> p1, IList<Piece> p2)
        {
            var p = new List<Piece>(p1);
            p.AddRange(p2);
            return p;
        }

        public virtual IList<Piece> MergeParameters(Piece p1, IList<Piece> p2)
        {
            var p = new List<Piece>();
            p.Add(p1);
            p.AddRange(p2);
            return p;
        }

        public virtual IList<Piece> ExtractParameters(IList<Piece> pieces, int first)
        {
            return new List<Piece>((from q in pieces select q).Skip(first));
        }
    }
}

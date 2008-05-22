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

namespace DbLinq.Linq.Data.Sugar.Implementation
{
    /// <summary>
    /// Service to get information from a Piece
    /// </summary>
    public class ExpressionService
    {
        /// <summary>
        /// Returns a queried type from a given expression, or null if no type can be found
        /// </summary>
        /// <param name="piece"></param>
        /// <returns></returns>
        public virtual Type GetQueriedType(Expression piece)
        {
            return GetQueriedType(piece.Type);
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

        public virtual string GetParameterName(Expression piece)
        {
            if (piece is ParameterExpression)
                return ((ParameterExpression)piece).Name;
            return null;
        }

        public virtual IList<Expression> MergeParameters(IEnumerable<Expression> p1, IEnumerable<Expression> p2)
        {
            var p = new List<Expression>(p1);
            p.AddRange(p2);
            return p;
        }

        public virtual IList<Expression> MergeParameters(Expression p1, IEnumerable<Expression> p2)
        {
            var p = new List<Expression>();
            p.Add(p1);
            p.AddRange(p2);
            return p;
        }

        public virtual IList<Expression> ExtractParameters(IEnumerable<Expression> pieces, int first)
        {
            return new List<Expression>((from q in pieces select q).Skip(first));
        }
    }
}

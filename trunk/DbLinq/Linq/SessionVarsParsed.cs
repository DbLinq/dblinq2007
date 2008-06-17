#region MIT license
// 
// MIT license
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
using System.Linq.Expressions;
using DbLinq.Data.Linq.Sugar;

namespace DbLinq.Linq
{
    /// <summary>
    /// the 'finalized' SessionVars.
    /// (meaning expressions have been parsed, after enumeration has started).
    /// 
    /// You create an instance via QueryProcessor.GenerateQuery()
    /// </summary>
    public sealed class SessionVarsParsed : SessionVars
    {
        /// <summary>
        /// Sugar.Query: this will replace the rest
        /// </summary>
        public Query Query;

        /// <summary>
        /// components of SQL expression (where clause, order, select ...)
        /// </summary>
        public SqlExpressionParts SqlParts;

        public LambdaExpression GroupByExpression;
        public LambdaExpression GroupByNewExpression;

        /// <summary>
        /// list of reflected fields - this will be used to compile a row reader method
        /// </summary>
        public ProjectionData ProjectionData;

        /// <summary>
        /// in SelectMany, there is mapping c.Orders => o
        /// </summary>
        //public Dictionary<MemberExpression,string> memberExprNickames = new Dictionary<MemberExpression,string>();

        /// <summary>
        /// created by post-processing in QueryProcessor.BuildSqlString(), used in RowEnumerator
        /// </summary>
        public string SqlString;

        public int numParameters;

        public SessionVarsParsed(SessionVars vars)
            : base(vars)
        {
            SqlParts = new SqlExpressionParts(vars.Context.Vendor);
        }
    }
}

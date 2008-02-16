﻿#region MIT license
////////////////////////////////////////////////////////////////////
// MIT license:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//
// Authors:
//        Jiri George Moudry
////////////////////////////////////////////////////////////////////
#endregion

using System.Linq.Expressions;

namespace DBLinq.Linq
{
    /// <summary>
    /// the 'finalized' SessionVars.
    /// (meaning expressions have been parsed, after enumeration has started).
    /// 
    /// You create an instance via QueryProcessor.ProcessLambdas()
    /// </summary>
    public sealed class SessionVarsParsed : SessionVars
    {
        /// <summary>
        /// components of SQL expression (where clause, order, select ...)
        /// </summary>
        public SqlExpressionParts _sqlParts;

        public LambdaExpression groupByExpr;
        public LambdaExpression groupByNewExpr;

        /// <summary>
        /// list of reflected fields - this will be used to compile a row reader method
        /// </summary>
        public ProjectionData projectionData;

        /// <summary>
        /// in SelectMany, there is mapping c.Orders => o
        /// </summary>
        //public Dictionary<MemberExpression,string> memberExprNickames = new Dictionary<MemberExpression,string>();

        /// <summary>
        /// created by post-processing in QueryProcessor.build_SQL_string(), used in RowEnumerator
        /// </summary>
        public string sqlString;


        public SessionVarsParsed(SessionVars vars)
            : base(vars)
        {
            _sqlParts = new SqlExpressionParts(vars.Context.Vendor);
        }
    }
}

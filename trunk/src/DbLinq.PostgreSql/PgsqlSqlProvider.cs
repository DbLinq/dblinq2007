﻿#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne
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
using DbLinq.Data.Linq.Sql;
using DbLinq.Vendor.Implementation;

namespace DbLinq.PostgreSql
{
#if !MONO_STRICT
    public
#endif
    class PgsqlSqlProvider : SqlProvider
    {
        public override SqlStatement GetInsertIds(IList<SqlStatement> outputParameters, IList<SqlStatement> outputExpressions)
        {
            // no parameters? no need to get them back
            if (outputParameters.Count == 0)
                return SqlStatement.Empty;
            // otherwise we keep track of the new values
            var insertId = SqlStatement.Format("SELECT {0}",
                SqlStatement.Join(", ", (from outputExpression in outputExpressions select outputExpression.Replace("nextval(", "currval(", true)).ToArray())
                );
            return insertId;
        }

        public override SqlStatement GetLiteral(DateTime literal)
        {
            return "'" + literal.ToString("o") + "'";
        }        
        
        protected override SqlStatement GetLiteralStringToUpper(SqlStatement a)
        {
            return string.Format("UPPER({0})", a);
        }

        protected override SqlStatement GetLiteralStringToLower(SqlStatement a)
        {
            return string.Format("LOWER({0})", a);
        }
        
        protected override SqlStatement GetLiteralDateDiff(SqlStatement dateA, SqlStatement dateB)
        {
            return string.Format("(DATE_PART('Day',{0}-{1})*86400000+DATE_PART('Hour',{0}-{1})*3600000+DATE_PART('Minute',{0}-{1})*60000+DATE_PART('Second',{0}-{1})*1000+DATE_PART('Millisecond',{0}-{1}))::real", dateA, dateB);
        }
                
        protected override SqlStatement GetLiteralEqual(SqlStatement a, SqlStatement b)
        {
            // PostgreSQL return NULL (and not a boolean) for every comparaison involving
            // a NULL value, unless the operator used is "IS" (or "IS NOT"). Also,
            // using those two operators when the right-hand value is not a literal
            // NULL is an error. The only possibility is to explicitly check for NULL
            // literals and even swap the operands to make sure NULL gets to the
            // right place.
            
            if (b.Count == 1 && b[0].Sql == "NULL")
                return SqlStatement.Format("{0} IS {1}", a, b);
            else if (a.Count == 1 && a[0].Sql == "NULL")
                return SqlStatement.Format("{0} IS {1}", b, a);
            else
                return SqlStatement.Format("{0} = {1}", a, b);
        }
        
        protected override SqlStatement GetLiteralNotEqual(SqlStatement a, SqlStatement b)
        {
            // See comment above, in GetLiteralEqual().
            
            if (b.Count == 1 && b[0].Sql == "NULL")
                return SqlStatement.Format("{0} IS NOT {1}", a, b);
            else if (a.Count == 1 && a[0].Sql == "NULL")
                return SqlStatement.Format("{0} IS NOT {1}", b, a);
            else
                return SqlStatement.Format("{0} <> {1}", a, b);
        }        

        public static readonly Dictionary<Type, string> typeMapping = new Dictionary<Type, string>
                                                                          {
            {typeof(int),"integer"},
            {typeof(uint),"integer"},

            {typeof(long),"bigint"},
            {typeof(ulong),"bigint"},

            {typeof(float),"real"}, //TODO: could be float or real. check ranges.
            {typeof(double),"double precision"}, //TODO: could be float or real. check ranges.
            
            {typeof(decimal),"decimal"},

            {typeof(short),"smallint"},
            {typeof(ushort),"smallint"},

            {typeof(bool),"boolean"},

            {typeof(string),"text"}, 
            {typeof(char[]),"text"},

            {typeof(char),"char"},

            {typeof(DateTime),"timestamp"},
            //{typeof(Guid),"uniqueidentifier"}
        };

        public override SqlStatement GetLiteralConvert(SqlStatement a, Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                type = type.GetGenericArguments().First();

            string sqlTypeName;
            if (typeMapping.ContainsKey(type))
                sqlTypeName = typeMapping[type];
            else
                sqlTypeName = type.Name;

            return string.Format("({0})::{1}", a, sqlTypeName);
        }

        /// <summary>
        /// In PostgreSQL an insensitive name is lowercase
        /// </summary>
        /// <param name="dbName"></param>
        /// <returns></returns>
        protected override bool IsNameCaseSafe(string dbName)
        {
            return dbName == dbName.ToLower();
        }
    }
}

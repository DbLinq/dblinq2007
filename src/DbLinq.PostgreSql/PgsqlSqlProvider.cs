#region MIT license
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

using System.Collections.Generic;
using System.Linq;
using DbLinq.Util;
using DbLinq.Vendor.Implementation;
using System;

namespace DbLinq.PostgreSql
{
    public class PgsqlSqlProvider : SqlProvider
    {
        public override string GetInsertIds(IList<string> outputParameters, IList<string> outputExpressions)
        {
            // no parameters? no need to get them back
            if (outputParameters.Count == 0)
                return "";
            // otherwise we keep track of the new values
            return string.Format("SELECT {0}",
                string.Join(", ", (from outputExpression in outputExpressions select outputExpression.ReplaceCase("nextval(", "currval(", true)).ToArray())
                );
        }

        protected override string GetLiteralStringToUpper(string a)
        {
            return string.Format("UPPER({0})", a);
        }

        protected override string GetLiteralStringToLower(string a)
        {
            return string.Format("LOWER({0})", a);
        }
        protected override string GetLiteralDateDiff(string dateA, string dateB)
        {
            return string.Format("(DATE_PART('Day',{0}-{1})*86400000+DATE_PART('Hour',{0}-{1})*3600000+DATE_PART('Minute',{0}-{1})*60000+DATE_PART('Second',{0}-{1})*1000+DATE_PART('Millisecond',{0}-{1}))::real", dateA, dateB);
        }

        public static readonly Dictionary<Type, string> typeMapping = new Dictionary<Type, string>()
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

        public override string GetLiteralConvert(string a, System.Type type)
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

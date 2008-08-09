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

using System;
using System.Linq;
using DbLinq.Util;
using DbLinq.Vendor.Implementation;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DbLinq.SqlServer
{
#if MONO_STRICT
    internal
#else
    public
#endif
 class SqlServerSqlProvider : SqlProvider
    {
        protected override char SafeNameStartQuote { get { return '['; } }
        protected override char SafeNameEndQuote { get { return ']'; } }

        public override string GetParameterName(string nameBase)
        {
            return string.Format("@{0}", nameBase);
        }

        public override string GetLiteralLimit(string select, string limit)
        {
            var selectClause = "SELECT ";
            if (select.StartsWith(selectClause))
            {
                var selectContents = select.Substring(selectClause.Length);
                return string.Format("SELECT TOP ({0}) {1}", limit, selectContents);
            }
            throw new ArgumentException("S0051: Unknown select format");
        }

        protected override string GetLiteralCount(string a)
        {
            return string.Format("COUNT(*)");
        }

        protected override string GetLiteralStringLength(string a)
        {
            return string.Format("LEN({0})", a);
        }

        protected override string GetLiteralSubString(string baseString, string startIndex, string count)
        {
            //in standard sql base string index is 1 instead 0
            return string.Format("SUBSTRING({0}, {1}, {2})", baseString, (int.Parse(startIndex) + 1), count);
        }

        protected override string GetLiteralSubString(string baseString, string startIndex)
        {
            return GetLiteralSubString(baseString, startIndex, GetLiteralStringLength(baseString));
        }

        protected override string GetLiteralTrim(string a)
        {
            return string.Format("RTRIM(LTRIM({0}))", a);
        }

        protected override string GetLiteralStringConcat(string a, string b)
        {
            return string.Format("{0} + {1}", a, b);
        }

        protected override string GetLiteralStringToLower(string a)
        {
            return string.Format("LOWER({0})", a);
        }

        protected override string GetLiteralStringToUpper(string a)
        {
            return string.Format("UPPER({0})", a);
        }

        //http://msdn.microsoft.com/en-us/library/4e5xt97a(VS.71).aspx
        public static readonly Dictionary<Type, string> typeMapping = new Dictionary<Type, string>()
        {
            {typeof(int),"int"},
            {typeof(uint),"int"},

            {typeof(long),"bigint"},
            {typeof(ulong),"bigint"},

            {typeof(float),"float"}, //TODO: could be float or real. check ranges.
            {typeof(double),"float"}, //TODO: could be float or real. check ranges.
            
            {typeof(decimal),"numeric"},

            {typeof(short),"tinyint"},
            {typeof(ushort),"tinyint"},

            {typeof(bool),"bit"},

            // trunk? They could be: varchar, char,nchar, ntext,text... it should be the most flexible string type. TODO: check wich of them is better.
            {typeof(string),"text"}, 
            {typeof(char[]),"text"},

            {typeof(char),"char"},

            {typeof(DateTime),"datetime"},
            {typeof(Guid),"uniqueidentifier"}

            // there are more types: timestamps, images ... TODO: check what is the official behaviour
        };

        public override string GetLiteralConvert(string a, Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                type = type.GetGenericArguments().First();

            string sqlTypeName;
            if (typeMapping.ContainsKey(type))
                sqlTypeName = typeMapping[type];
            else
                sqlTypeName = "variant";

            return "CONVERT(" + sqlTypeName + "," + a + ")";
        }

    }
}

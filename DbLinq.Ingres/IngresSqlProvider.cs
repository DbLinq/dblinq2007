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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using DbLinq.Util;
using DbLinq.Vendor.Implementation;

namespace DbLinq.Ingres
{
    public class IngresSqlProvider : SqlProvider
    {
        public override string GetInsertIds(IList<string> outputParameters, IList<string> outputExpressions)
        {
            // no parameters? no need to get them back
            if (outputParameters.Count == 0)
                return "";
            // otherwise we keep track of the new values
            return string.Format("SELECT {0}",
                string.Join(", ", (from outputExpression in outputExpressions
                                   select outputExpression.ReplaceCase("next value", "current value", true)).ToArray())
                );
        }

        protected override string GetLiteralCount(string a)
        {
            return "COUNT(*)";
        }

        protected override string GetLiteralStringToLower(string a)
        {
            return string.Format("LOWER({0})", a);
        }

        protected override string GetLiteralStringToUpper(string a)
        {
            return string.Format("UPPER({0})", a);
        }

        public override string GetLiteralLimit(string select, string limit)
        {
            // return string.Format("SELECT FIRST {0} FROM ({1})", limit, select);
            string trimSelect = "SELECT ";
            if (select.StartsWith(trimSelect))
            {
                string remaining = select.Substring(trimSelect.Length);
                return string.Format("SELECT FIRST {0} {1}", limit, remaining);
            }
            throw new ArgumentException("Invalid SELECT format");
        }

        public override string GetLiteralLimit(string select, string limit, string offset, string offsetAndLimit)
        {
            // TODO: this is for you, Thomas...
            throw new NotImplementedException("OFFSET clause is not supported on Ingres");
        }

        public override string GetParameterName(string nameBase)
        {
            return "?";
        }

        protected override bool IsNameCaseSafe(string dbName)
        {
            return dbName == dbName.ToLower();
        }
    }
}
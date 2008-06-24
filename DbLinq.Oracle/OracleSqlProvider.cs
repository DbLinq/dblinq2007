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
using System.Text;
using DbLinq.Util;
using DbLinq.Vendor.Implementation;

namespace DbLinq.Oracle
{
    public class OracleSqlProvider : SqlProvider
    {
        protected override string GetInsertWrapper(string insert, IList<string> outputParameters, IList<string> outputExpressions)
        {
            // no parameters? no need to get them back
            if (outputParameters.Count == 0)
                return insert;
            // otherwise we keep track of the new values
            return string.Format("BEGIN {0};SELECT {1} INTO {2} FROM DUAL;END;",
                insert,
                string.Join(", ", (from outputExpression in outputExpressions select outputExpression.ReplaceCase(".NextVal", ".CurrVal", true)).ToArray()),
                string.Join(", ", outputParameters.ToArray()));
        }

        protected override string GetLiteralCount(string a)
        {
            return "COUNT(*)";
        }

        protected override string GetLiteralStringLength(string a)
        {
            return string.Format("LENGTH({0})", a);
        }

        protected override string GetLiteralStringToLower(string a)
        {
            return string.Format("LOWER({0})", a);
        }

        protected override string GetLiteralStringToUpper(string a)
        {
            return string.Format("UPPER({0})", a);
        }

        //          SELECT * FROM (
        //          SELECT a.*, rownum RN FROM (
        //          select c$.CustomerID, c$.CompanyName from Customers c$ order by c$.CUSTOMERID
        //          ) a  WHERE rownum <=5
        //          ) WHERE rn >2

        protected const string LimitedTableName = "LimitedTable___";
        protected const string LimitedRownum = "Limit___";

        public override string GetLiteralLimit(string select, string limit)
        {
            return string.Format(
                @"SELECT {2}.*, rownum {3} FROM ({4}{0}{4}) {2} WHERE rownum <= {1}",
                select, limit, LimitedTableName, LimitedRownum, NewLine);
        }

        public override string GetLiteralLimit(string select, string limit, string offset, string offsetAndLimit)
        {
            return string.Format(
                @"SELECT * FROM ({3}{0}{3}) WHERE {2} > {1}",
                GetLiteralLimit(select, offsetAndLimit), offset, LimitedRownum, NewLine);
        }
    }
}

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

using System;
using System.Collections.Generic;
using System.Text;

namespace DBLinq.Linq.clause
{
    /// <summary>
    /// Object which holds the pieces of a SELECT as it's being put together.
    /// The ToString() method is used to produce the final SQL statement.
    /// </summary>
    public class SqlExpressionParts //: ICloneable
    {
        static int s_serial = 0;
        int _serial;

        /// <summary>
        /// eg. {'Customers $c','Orders $o'}
        /// </summary>
        private readonly List<string> fromTableList = new List<string>();

        /// <summary>
        /// eg. {'$c.CustomerID','$o.Quantity'}
        /// </summary>
        readonly List<string> selectFieldList = new List<string>();

        /// <summary>
        /// eg. {'$c.CustomerID==$o.CustomerID', ...}
        /// </summary>
        public readonly List<string> joinList = new List<string>();

        /// <summary>
        /// keep track of which nickNames were already added via GetClause(), eg. {'$c','$o'}.
        /// Currently, author is not sure what's the best way to prevent duplicate addition.
        /// </summary>
        public readonly List<string> doneClauses = new List<string>();
        
        /// <summary>
        /// add 'Employee $e' FROM clause
        /// </summary>
        /// <param name="fromTable"></param>
        public void AddFrom(string fromTable)
        {
            //Martin Raucher reports a MySql problem with incorrect case
            //update: to see case-sensitivity, you must be on Linux?!
            //string fromLower = fromTable.ToLower(); 
            string fromLower = fromTable;

            if(fromTableList.Contains(fromLower))
                return; //prevent dupes
            fromTableList.Add(fromLower);
        }

        public void AddSelect(string column)
        {
            selectFieldList.Add(column);
        }
        public void AddSelect(List<string> columns)
        {
            selectFieldList.AddRange(columns);
        }
        public string GetSelect()
        {
            string joined = string.Join(",", selectFieldList.ToArray());
            return joined;
        }

        /// <summary>
        /// eg. {'$c.City==?P0'}
        /// </summary>
        private readonly List<string> whereList = new List<string>();

        public readonly List<string> orderByList = new List<string>();
        public string orderBy_desc;

        public readonly List<string> groupByList = new List<string>();

        public readonly List<string> havingList = new List<string>();

        public string countClause;
        public string distinctClause;

        /// <summary>
        /// 'Take(3)' gets translated into 'LIMIT 3'
        /// </summary>
        public int? limitClause;

        /// <summary>
        /// 'Skip(2)' gets translated into 'OFFSET 2'
        /// </summary>
        public int? offsetClause;

        /// <summary>
        /// parameters, eg {'?P0'=>'London'}
        /// </summary>
        public readonly Dictionary<string,object> paramMap = new Dictionary<string,object>();

        public SqlExpressionParts()
        {
            _serial = s_serial++;
        }
        public SqlExpressionParts(SqlExpressionParts orig)
        {
            _serial = s_serial++;
            fromTableList = new List<string>(orig.fromTableList);
            selectFieldList = new List<string>(orig.selectFieldList);
            joinList = new List<string>(orig.joinList);
            doneClauses = new List<string>(orig.doneClauses);
            whereList = new List<string>(orig.whereList);
            groupByList = new List<string>(orig.groupByList);
            orderByList = new List<string>(orig.orderByList);
            orderBy_desc = orig.orderBy_desc;
            countClause = orig.countClause;
            distinctClause = orig.distinctClause;
            limitClause = orig.limitClause;
            offsetClause = orig.offsetClause;
            paramMap = new Dictionary<string,object>(orig.paramMap);
        }

        public void AddWhere(string sqlExpr)
        {
            this.whereList.Add(sqlExpr);
        }
        public void AddWhere(List<string> sqlExprs)
        {
            this.whereList.AddRange(sqlExprs);
        }
        public void AddHaving(List<string> sqlExprs)
        {
            this.havingList.AddRange(sqlExprs);
        }

        #region ToString() produces a full SQL statement from parts
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(500);
            sb.Append("SELECT ");
            if(this.distinctClause!=null)
            {
                //SELECT COUNT(ProductID) FROM ...
                sb.Append(distinctClause).Append(" ");
            }

            if(this.countClause!=null)
            {
                //SELECT COUNT(ProductID) FROM ...
                sb.Append(countClause).Append("(")
                    .Append(selectFieldList[0]).Append(")");
            }
            else
            {
                //normal (non-count) select
                string opt_comma = "";
                foreach(string s in selectFieldList)
                {
                    sb.Append(opt_comma).Append(s);
                    opt_comma = ", ";
                }
            }
            //if(sb.Length>80){ sb.Append("\n"); } //for legibility, append a newline for long expressions
            appendCsvList(sb, "\n FROM ", fromTableList, ", ");

            //MySql docs for JOIN:
            //http://dev.mysql.com/doc/refman/4.1/en/join.html
            //for now, we will not be using the JOIN keyword
            List<string> whereAndjoins = new List<string>(joinList);
            whereAndjoins.AddRange(whereList);

            appendCsvList(sb, " WHERE ", whereAndjoins, " AND ");
            appendCsvList(sb, " GROUP BY ", groupByList, ", ");
            appendCsvList(sb, " HAVING ", havingList, ", ");

            if (limitClause == null && offsetClause != null)
            {
                //Mysql does not allow OFFSET without LIMIT.
                //use a hack:
                //change 'SELECT * FROM customers               OFFSET 2' 
                //into   'SELECT * FROM customers LIMIT 9999999 OFFSET 2' 
                limitClause = 9999999;
            }

            appendCsvList(sb, " ORDER BY ", orderByList, ", ");
            if (orderBy_desc != null)
                sb.Append(' ').Append(orderBy_desc).Append(' '); //' DESC '

            if (limitClause != null)
                sb.Append(" LIMIT " + limitClause.Value);

            if (offsetClause != null)
                sb.Append(" OFFSET " + offsetClause.Value);

            return sb.ToString();
        }

        void appendCsvList(StringBuilder sb, string header, List<string> list, string separator)
        {
            if (list.Count == 0)
                return;
            sb.Append(header);
            string currSeparator = "";
            foreach (string str in list)
            {
                sb.Append(currSeparator).Append(str);
                currSeparator = separator;
            }
        }

        #endregion
    
        public bool IsEmpty()
        {
            return selectFieldList.Count==0;
        }

        public SqlExpressionParts Clone()
        {
            //don't use MemberwiseClone, since our fields are marked 'readonly'
            //clone._serial = s_serial++;
            return new SqlExpressionParts(this);
        }
    }
}

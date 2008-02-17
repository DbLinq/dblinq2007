#region MIT license
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

using System;
using System.Collections.Generic;
using System.Text;
using DbLinq.Vendor;

namespace DbLinq.Linq
{
    /// <summary>
    /// Object which holds the pieces of a SELECT as it's being put together.
    /// The ToString() method is used to produce the final SQL statement.
    /// </summary>
    public class SqlExpressionParts //: ICloneable
    {
        // picrap: a good pattern is to avoid static members (there may be thread problems)
        static int s_serial = 0;
        int _serial;

        private IVendor _vendor;

        /// <summary>
        /// eg. {'Customers $c','Orders $o'}
        /// </summary>
        public List<string> FromTableList { get; private set; }

        /// <summary>
        /// eg. {'$c.CustomerID','$o.Quantity'}
        /// </summary>
        public List<string> SelectFieldList { get; private set; }

        /// <summary>
        /// eg. {'$c.CustomerID==$o.CustomerID', ...}
        /// </summary>
        public List<string> JoinList { get; private set; }

        /// <summary>
        /// keep track of which nickNames were already added via GetClause(), eg. {'$c','$o'}.
        /// Currently, author is not sure what's the best way to prevent duplicate addition.
        /// </summary>
        public List<string> DoneClauses { get; private set; }

        /// <summary>
        /// eg. {'$c.City==?P0'}
        /// </summary>
        public List<string> WhereList { get; private set; }

        public List<string> OrderByList { get; private set; }
        public string OrderDirection { get; set; }

        public List<string> GroupByList { get; private set; }

        public List<string> HavingList { get; private set; }

        public string CountClause { get; set; }
        public string DistinctClause { get; set; }

        /// <summary>
        /// 'Take(3)' gets translated into 'LIMIT 3'
        /// </summary>
        public int? LimitClause { get; set; }

        /// <summary>
        /// 'Skip(2)' gets translated into 'OFFSET 2'
        /// </summary>
        public int? OffsetClause { get; set; }

        /// <summary>
        /// parameters, eg {'?P0'=>'London'}
        /// </summary>
        public Dictionary<string, object> ParametersMap { get; private set; }

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

            if (FromTableList.Contains(fromLower))
                return; //prevent dupes
            FromTableList.Add(fromLower);
        }

        public void AddSelect(string column)
        {
            SelectFieldList.Add(column);
        }
        public void AddSelect(List<string> columns)
        {
            SelectFieldList.AddRange(columns);
        }
        public string GetSelect()
        {
            string joined = string.Join(",", SelectFieldList.ToArray());
            return joined;
        }

        public SqlExpressionParts(IVendor vendor)
        {
            _serial = s_serial++;
            _vendor = vendor;

            FromTableList = new List<string>();
            SelectFieldList = new List<string>();
            JoinList = new List<string>();
            DoneClauses = new List<string>();
            WhereList = new List<string>();
            GroupByList = new List<string>();
            OrderByList = new List<string>();
            ParametersMap = new Dictionary<string, object>();
            HavingList = new List<string>();
        }

        public SqlExpressionParts(SqlExpressionParts sqlExpressionParts)
        {
            _serial = s_serial++;
            _vendor = sqlExpressionParts._vendor;

            FromTableList = new List<string>(sqlExpressionParts.FromTableList);
            SelectFieldList = new List<string>(sqlExpressionParts.SelectFieldList);
            JoinList = new List<string>(sqlExpressionParts.JoinList);
            DoneClauses = new List<string>(sqlExpressionParts.DoneClauses);
            WhereList = new List<string>(sqlExpressionParts.WhereList);
            GroupByList = new List<string>(sqlExpressionParts.GroupByList);
            OrderByList = new List<string>(sqlExpressionParts.OrderByList);
            OrderDirection = sqlExpressionParts.OrderDirection;
            CountClause = sqlExpressionParts.CountClause;
            DistinctClause = sqlExpressionParts.DistinctClause;
            LimitClause = sqlExpressionParts.LimitClause;
            OffsetClause = sqlExpressionParts.OffsetClause;
            ParametersMap = new Dictionary<string, object>(sqlExpressionParts.ParametersMap);
            HavingList = new List<string>(sqlExpressionParts.HavingList);
        }

        public void AddWhere(string sqlExpr)
        {
            this.WhereList.Add(sqlExpr);
        }
        public void AddWhere(List<string> sqlExprs)
        {
            this.WhereList.AddRange(sqlExprs);
        }
        public void AddHaving(List<string> sqlExprs)
        {
            this.HavingList.AddRange(sqlExprs);
        }

        #region ToString() produces a full SQL statement from parts

        public override string ToString()
        {
            return _vendor.BuildSqlString(this);
        }

        #endregion

        public bool IsEmpty()
        {
            return SelectFieldList.Count == 0;
        }

        public SqlExpressionParts Clone()
        {
            //don't use MemberwiseClone, since our fields are marked 'readonly'
            //clone._serial = s_serial++;
            return new SqlExpressionParts(this);
        }
    }
}
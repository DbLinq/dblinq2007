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
using System.Text;
using System.Linq;
using DbLinq.Vendor;
using DbLinq.Util.ExprVisitor;

namespace DbLinq.Linq
{
    /// <summary>
    /// Object which holds the pieces of a SELECT as it's being put together.
    /// The ToString() method is used to produce the final SQL statement.
    /// </summary>
#if MONO_STRICT
    internal
#else
    public
#endif
 class SqlExpressionParts
    {
        private IVendor _vendor;

        /// <summary>
        /// eg. {'Customers $c','Orders $o'}
        /// </summary>
        public List<TableSpec> FromTableList { get; private set; }

        /// <summary>
        /// eg. {'$c.CustomerID','$o.Quantity'}
        /// </summary>
        public List<string> SelectFieldList { get; private set; }

        /// <summary>
        /// eg. {'$c.CustomerID==$o.CustomerID', ...}
        /// </summary>
        public List<JoinSpec> JoinList { get; private set; }

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
        /// some parameters are only obtained by calling a delegate, 
        /// eg. in 'where product.Name==someObject.someField'
        /// </summary>
        public Dictionary<string, FunctionReturningObject> ParametersMap2 { get; private set; }

        /// <summary>
        /// points to 2nd half of 'UNION' statement
        /// </summary>
        public SqlExpressionParts UnionPart2;

        /// <summary>
        /// add 'Employee $e' FROM clause
        /// </summary>
        /// <param name="fromTable"></param>
        public void AddFrom(TableSpec fromTable)
        {
            //Martin Raucher reports a MySql problem with incorrect case
            //update: to see case-sensitivity, you must be on Linux?!
            //string fromLower = fromTable.ToLower(); 
            TableSpec fromLower = fromTable;

            foreach (TableSpec existingTable in FromTableList)
                if (existingTable.Matches(fromLower))
                    return; //prevent dupes
            //if (FromTableList.Contains(fromLower))
            //    return; //prevent dupes
            if (JoinList.Count > 0)
            {
                foreach (JoinSpec js in JoinList)
                {
                    if (js.RightSpec != null && js.RightSpec.Matches(fromLower))
                        //string rightSpec = js.RightSpec.ToString();
                        //if (fromTable == rightSpec)
                        return; //prevent dupes
                }
            }
            FromTableList.Add(fromLower);
        }

        public void AddJoin(JoinSpec js)
        {
            foreach (JoinSpec prevJs in JoinList)
            {
                if (prevJs.Matches(js))
                    return; //ignore duplicate
            }
            this.JoinList.Add(js);
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
            _vendor = vendor;

            FromTableList = new List<TableSpec>();
            SelectFieldList = new List<string>();
            JoinList = new List<JoinSpec>();
            DoneClauses = new List<string>();
            WhereList = new List<string>();
            GroupByList = new List<string>();
            OrderByList = new List<string>();
            ParametersMap = new Dictionary<string, object>();
            ParametersMap2 = new Dictionary<string, FunctionReturningObject>();
            HavingList = new List<string>();
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
            string part1 = _vendor.BuildSqlString(this);
            // picrap to the author --> this should move to the vendor
            if (UnionPart2 != null)
            {
                string part2 = _vendor.BuildSqlString(UnionPart2);
                return part1 + "\n UNION \n" + part2;
            }
            return part1;
        }

        #endregion

        public bool IsEmpty()
        {
            return SelectFieldList.Count == 0;
        }

        /// <summary>
        /// retrieve al param names and their values.
        /// Warning - this may call into dynamically-compiled code 
        /// in cases such as 'where ProductName==object.Field'
        /// The result set is ordered by the name of the parameter
        /// </summary>
        /// <returns></returns>
        public IEnumerable<KeyValuePair<string, object>> EnumParams()
        {
            var result = from p in EnumParams_Unordered()
                         orderby p.Key
                         select p;
            return result;
        }

        /// <summary>
        /// retrieve al param names and their values.
        /// Warning - this may call into dynamically-compiled code 
        /// in cases such as 'where ProductName==object.Field'
        /// </summary>
        /// <returns></returns>
        protected IEnumerable<KeyValuePair<string, object>> EnumParams_Unordered()
        {
            #region EnumParams
            foreach (KeyValuePair<string, object> constParam in ParametersMap)
            {
                yield return constParam;
            }
            foreach (KeyValuePair<string, FunctionReturningObject> funcParamPair in ParametersMap2)
            {
                FunctionReturningObject func = funcParamPair.Value;
                object value = func();
                yield return new KeyValuePair<string, object>(funcParamPair.Key, value);
            }
            if (UnionPart2 != null)
            {
                foreach (KeyValuePair<string, object> param2 in UnionPart2.EnumParams())
                    yield return param2;
            }
            #endregion
        }
    }

    /// <summary>
    /// holds table name and nickname, e.g. TableSpec{ '[Order Details]', 'o$' }
    /// </summary>
#if MONO_STRICT
    internal
#else
    public
#endif
 class TableSpec
    {
        public string TableName;
        public string NickName;

        /// <summary>
        /// during LEFT JOINs, we have 'x' and 'o' refer to the same thing, mark one of them as hidden
        /// </summary>
        public bool isHidden = false;

        public bool Matches(TableSpec ts2) { return ts2.TableName == TableName && ts2.NickName == NickName; }
        public override string ToString() { return "" + TableName + " " + NickName; }
    }

    /// <summary>
    /// holds information for a JOIN - the left and right table specs, left and right fields
    /// </summary>
#if MONO_STRICT
    internal
#else
    public
#endif
    class JoinSpec
    {
        /// <summary>
        /// note: OUTER JOIN is not supported in LINQ.
        /// </summary>
        public enum JoinTypeEnum { Plain, Left }

        public JoinTypeEnum JoinType = JoinTypeEnum.Plain;

        /// <summary>
        /// eg. {Order,'o$'}
        /// </summary>
        public TableSpec LeftSpec;

        /// <summary>
        /// eg. {Product,'o$'}
        /// </summary>
        public TableSpec RightSpec;

        /// <summary>
        /// eg. 'o$.ProductID'
        /// </summary>
        public string LeftField;

        public string RightField;

        public override string ToString()
        {
            string joinTypeStr = JoinType == JoinTypeEnum.Plain ? "JOIN" : "LEFT JOIN";
            return "\n " + joinTypeStr + " " + RightSpec + " ON " + LeftField + " = " + RightField + " ";
        }

        public bool Matches(JoinSpec js2)
        {
            bool same1 = LeftSpec.Matches(js2.LeftSpec) && RightSpec.Matches(js2.RightSpec)
                && LeftField == js2.LeftField && RightField == js2.RightField;
            if (same1)
                return true;
            bool same2 = LeftSpec.Matches(js2.RightSpec) && RightSpec.Matches(js2.LeftSpec)
                && LeftField == js2.RightField && RightField == js2.LeftField;
            if (same2)
                return true;
            return false;
        }
    }
}
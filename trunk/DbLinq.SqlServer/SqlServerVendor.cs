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
using System.Reflection;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Linq.Mapping;
using DbLinq.Util;
using DbLinq.Linq;
using DataContext = DbLinq.Data.Linq.DataContext;
using ITable = DbLinq.Data.Linq.ITable;

namespace DbLinq.SqlServer
{
    public class SqlServerVendor : Vendor.Implementation.Vendor
    {
        public readonly Dictionary<ITable, int> UseBulkInsert = new Dictionary<ITable, int>();

        public override string VendorName { get { return "MsSqlServer"; } }
        //public const string SQL_PING_COMMAND = "SELECT 11";

        public SqlServerVendor()
            : base(new SqlServerSqlProvider())
        { }

        /// <summary>
        /// on Postgres or Oracle, return eg. ':P1', on Mysql, '?P1', @P1 for Microsoft
        /// </summary>
        public override string GetOrderableParameterName(int index)
        {
            return "@P" + index;
        }

        protected override string MakeNameSafe(string name)
        {
            return name.Enquote('[', ']');
        }

        public override IDbDataParameter CreateDbDataParameter(IDbCommand cmd, string dbTypeName, string paramName)
        {
            System.Data.SqlDbType dbType = SqlServerTypeConversions.ParseType(dbTypeName);
            SqlParameter param = new SqlParameter(paramName, dbType);
            return param;
        }

        //NOTE: for Oracle, we want to consider 'Array Binding'
        //http://download-west.oracle.com/docs/html/A96160_01/features.htm#1049674

        /// <summary>
        /// for large number of rows, we want to use BULK INSERT, 
        /// because it does not fill up the translation log.
        /// This is enabled for tables where Vendor.UserBulkInsert[db.Table] is true.
        /// </summary>
        public override void DoBulkInsert<T>(Data.Linq.Table<T> table, List<T> rows, IDbConnection connection)
        {
            //use TableLock for speed:
            SqlBulkCopy bulkCopy = new SqlBulkCopy((SqlConnection)connection, SqlBulkCopyOptions.TableLock, null);

            bulkCopy.DestinationTableName = AttribHelper.GetTableAttrib(typeof(T)).Name;
            //bulkCopy.SqlRowsCopied += new SqlRowsCopiedEventHandler(bulkCopy_SqlRowsCopied);

            DataTable dt = new DataTable();
            KeyValuePair<PropertyInfo, ColumnAttribute>[] columns = AttribHelper.GetColumnAttribs2(typeof(T));

            foreach (KeyValuePair<PropertyInfo, ColumnAttribute> pair in columns)
            {
                //if (pair.Value.IsDbGenerated)
                //    continue; //don't skip - all fields would be shifted

                DataColumn dc = new DataColumn();
                dc.ColumnName = pair.Value.Name;
                dc.DataType = pair.Key.PropertyType;
                dt.Columns.Add(dc);
            }

            //TODO: cross-check null values against CanBeNull specifier
            object[] indices = new object[] { };
            foreach (T row in rows)
            {
                DataRow dr = dt.NewRow();
                //use reflection to retrieve object's fields (TODO: optimize this later)
                foreach (KeyValuePair<PropertyInfo, ColumnAttribute> pair in columns)
                {
                    //if (pair.Value.IsDbGenerated)
                    //    continue; //don't assign IDENTITY col
                    object value = pair.Key.GetValue(row, indices);
                    dr[pair.Value.Name] = value;
                }
                //dr[1
                dt.Rows.Add(dr);
            }
            bulkCopy.WriteToServer(dt);

        }

        public override System.Data.Linq.IExecuteResult ExecuteMethodCall(DataContext context, System.Reflection.MethodInfo method, params object[] sqlParams)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Client code needs to specify: 'Vendor.UserBulkInsert[db.Products]=true' to enable bulk insert.
        /// </summary>
        //public static readonly Dictionary<DbLinq.Linq.IMTable, bool> UseBulkInsert = new Dictionary<DbLinq.Linq.IMTable, bool>();

        public override bool CanBulkInsert<T>(Data.Linq.Table<T> table)
        {
            return UseBulkInsert.ContainsKey(table);
        }

        public override void SetBulkInsert<T>(Data.Linq.Table<T> table, int pageSize)
        {
            UseBulkInsert[table] = pageSize;
        }
    }
}
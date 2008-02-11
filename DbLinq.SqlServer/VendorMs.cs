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
using System.Linq;
using System.Reflection;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Linq.Mapping;
using DBLinq.util;
using DBLinq.Linq;

namespace DBLinq.vendor.mssql
{
    public class VendorMssql : VendorBase, IVendor
    {
        public readonly Dictionary<DBLinq.Linq.IMTable, int> UseBulkInsert = new Dictionary<DBLinq.Linq.IMTable, int>();

        public string VendorName { get { return "MsSqlServer"; } }
        //public const string SQL_PING_COMMAND = "SELECT 11";

        /// <summary>
        /// Postgres string concatenation, eg 'a||b'
        /// </summary>
        public override string Concat(List<ExpressionAndType> parts)
        {
            string[] arr = parts.Select(p => p.expression).ToArray();
            return "CONCAT(" + string.Join(",", arr) + ")";
        }

        /// <summary>
        /// on Postgres or Oracle, return eg. ':P1', on Mysql, '?P1', @P1 for Microsoft
        /// </summary>
        public override string ParamName(int index)
        {
            return "@P" + index;
        }

        /// <summary>
        /// given 'User', return '[User]' to prevent a SQL keyword conflict
        /// </summary>
        public string FieldName_Safe(string name)
        {
            if (name.ToLower() == "user")
                return "[" + name + "]";
            return name;
        }

        public IDbDataParameter CreateSqlParameter(IDbCommand cmd, string dbTypeName, string paramName)
        {
            System.Data.SqlDbType dbType = DBLinq.util.SqlTypeConversions.ParseType(dbTypeName);
            SqlParameter param = new SqlParameter(paramName, dbType);
            return param;
        }

        public IDbDataParameter ProcessPkField(IDbCommand cmd, ProjectionData projData, ColumnAttribute colAtt
            , StringBuilder sb, StringBuilder sbValues, StringBuilder sbIdentity, ref int numFieldsAdded)
        {
            sbIdentity.Append("; SELECT @@IDENTITY");
            return null;
        }

        //NOTE: for Oracle, we want to consider 'Array Binding'
        //http://download-west.oracle.com/docs/html/A96160_01/features.htm#1049674

        /// <summary>
        /// for large number of rows, we want to use BULK INSERT, 
        /// because it does not fill up the translation log.
        /// This is enabled for tables where Vendor.UserBulkInsert[db.Table] is true.
        /// </summary>
        public override void DoBulkInsert<T>(DBLinq.Linq.Table<T> table, List<T> rows, IDbConnection conn)
        {
            //use TableLock for speed:
            SqlBulkCopy bulkCopy = new SqlBulkCopy((SqlConnection)conn, SqlBulkCopyOptions.TableLock, null);

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

        public override int ExecuteCommand(DBLinq.Linq.DataContext context, string sql, params object[] parameters)
        {
            IDbConnection conn = context.Connection;
            using (IDbCommand command = conn.CreateCommand())
            {
                command.CommandText = sql;
                return command.ExecuteNonQuery();
            }
        }

        public System.Data.Linq.IExecuteResult ExecuteMethodCall(DBLinq.Linq.DataContext context, System.Reflection.MethodInfo method, params object[] sqlParams)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Client code needs to specify: 'Vendor.UserBulkInsert[db.Products]=true' to enable bulk insert.
        /// </summary>
        //public static readonly Dictionary<DBLinq.Linq.IMTable, bool> UseBulkInsert = new Dictionary<DBLinq.Linq.IMTable, bool>();

        public IDataReader2 CreateDataReader2(IDataReader dataReader)
        {
            return new DataReader2(dataReader);
        }

        public override bool CanBulkInsert<T>(DBLinq.Linq.Table<T> table)
        {
            return UseBulkInsert.ContainsKey(table);
        }

        public override void SetBulkInsert<T>(DBLinq.Linq.Table<T> table, int pageSize)
        {
            UseBulkInsert[table] = pageSize;
        }
    }
}

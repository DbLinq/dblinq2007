////////////////////////////////////////////////////////////////////
//Initial author: Jiri George Moudry, 2006.
//License: LGPL. (Visit http://www.gnu.org)
//Commercial code may call into this library, if it's in a different module (DLL)
////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Linq.Mapping;
using DBLinq.util;

namespace DBLinq.vendor
{
    public class Vendor
    {
        /// <summary>
        /// Postgres string concatenation, eg 'a||b'
        /// </summary>
        public static string Concat(List<string> parts)
        {
            string[] arr = parts.ToArray();
            return "CONCAT("+string.Join(",",arr)+")";
        }

        /// <summary>
        /// on Postgres or Oracle, return eg. ':P1', on Mysql, '?P1', @P1 for Microsoft
        /// </summary>
        public static string ParamName(int index)
        {
            return "@P"+index;
        }

        /// <summary>
        /// given 'User', return '[User]' to prevent a SQL keyword conflict
        /// </summary>
        public static string FieldName_Safe(string name)
        {
            if (name.ToLower() == "user")
                return "[" + name + "]";
            return name;
        }

        public static SqlParameter CreateSqlParameter(string dbTypeName, string paramName)
        {
            System.Data.SqlDbType dbType = DBLinq.util.SqlTypeConversions.ParseType(dbTypeName);
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
        public static void DoBulkInsert<T>(DBLinq.Linq.MTable<T> table, List<T> rows, SqlConnection conn)
        {
            //use TableLock for speed:
            SqlBulkCopy bulkCopy = new SqlBulkCopy(conn, SqlBulkCopyOptions.TableLock, null);

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
            foreach(T row in rows)
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

        /// <summary>
        /// Client code needs to specify: 'Vendor.UserBulkInsert[db.Products]=true' to enable bulk insert.
        /// </summary>
        public static readonly Dictionary<DBLinq.Linq.IMTable, bool> UseBulkInsert = new Dictionary<DBLinq.Linq.IMTable, bool>();
    }
}

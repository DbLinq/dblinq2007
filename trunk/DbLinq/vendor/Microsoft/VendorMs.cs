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

        /// <summary>
        /// for large number of rows, we want to use BULK INSERT, 
        /// because it does not fill up the translation log.
        /// </summary>
        public static void DoBulkInsert<T>(List<T> rows, string connStr)
        {
            SqlBulkCopy bulkCopy = new SqlBulkCopy(connStr, SqlBulkCopyOptions.TableLock);
            bulkCopy.DestinationTableName = AttribHelper.GetTableAttrib(typeof(T)).Name;
            
            DataTable dt = new DataTable();
            KeyValuePair<PropertyInfo, ColumnAttribute>[] columns = AttribHelper.GetColumnAttribs2(typeof(T));
            
            foreach (KeyValuePair<PropertyInfo, ColumnAttribute> pair in columns)
            {
                DataColumn dc = new DataColumn();
                dc.ColumnName = pair.Value.Name;
                dc.DataType = pair.Key.PropertyType;
                dt.Columns.Add(dc);
            }

            object[] indices = new object[] { };
            foreach(T row in rows)
            {
                DataRow dr = dt.NewRow();
                //use reflection to retrieve object's fields (TODO: optimize this later)
                foreach (KeyValuePair<PropertyInfo, ColumnAttribute> pair in columns)
                {
                    object value = pair.Key.GetValue(row, indices);
                    dr[pair.Value.Name] = value;
                }
                //dr[1
                dt.Rows.Add(dr);
            }
            bulkCopy.WriteToServer(dt);

        }
    }
}

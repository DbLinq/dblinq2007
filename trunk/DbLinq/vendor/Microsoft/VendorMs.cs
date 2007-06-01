////////////////////////////////////////////////////////////////////
//Initial author: Jiri George Moudry, 2006.
//License: LGPL. (Visit http://www.gnu.org)
//Commercial code may call into this library, if it's in a different module (DLL)
////////////////////////////////////////////////////////////////////

using System;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Text;

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
    }
}

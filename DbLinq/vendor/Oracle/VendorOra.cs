using System;
using System.Collections.Generic;
using System.Text;
using System.Data.OracleClient;

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
            return string.Join("||",arr);
        }

        /// <summary>
        /// on Postgres or Oracle, return eg. ':P1', on Mysql, '?P1'
        /// </summary>
        public static string ParamName(int index)
        {
            return ":P"+index;
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

        public static OracleParameter CreateSqlParameter(string dbTypeName, string paramName)
        {
            OracleType dbType = OracleTypeConversions.ParseType(dbTypeName);
            OracleParameter param = new OracleParameter(paramName, dbType);
            return param;
        }

        public static int ExecuteCommand(DBLinq.Linq.MContext context, string sql, params object[] parameters)
        {
            OracleConnection conn = context.SqlConnection;
            using (OracleCommand command = new OracleCommand(sql, conn))
            {
                return command.ExecuteNonQuery();
            }
        }


    }
}

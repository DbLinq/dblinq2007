using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Data.OracleClient;
using DBLinq.util;

namespace DBLinq.vendor
{
    public class Vendor
    {
        public const string VENDOR_NAME = "Oracle";
        public const string SQL_PING_COMMAND = "SELECT 11 FROM DUAL";

        /// <summary>
        /// Postgres string concatenation, eg 'a||b'
        /// </summary>
        public static string Concat(List<ExpressionAndType> parts)
        {
            string[] arr = parts.Select(p => p.expression).ToArray();
            return string.Join("||", arr);
        }

        /// <summary>
        /// on Postgres or Oracle, return eg. ':P1', on Mysql, '?P1'
        /// </summary>
        public static string ParamName(int index)
        {
            return ":P" + index;
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
                //int ret = command.ExecuteNonQuery();
                object obj = command.ExecuteScalar();
                Type t = obj.GetType();
                if (t == typeof(int))
                    return (int)obj;
                else if (t == typeof(decimal))
                    return (int)(decimal)obj;
                return -1;
            }
        }


    }
}

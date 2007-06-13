using System;
using System.Collections.Generic;
using System.Text;
using Npgsql;

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

        public static NpgsqlParameter CreateSqlParameter(string dbTypeName, string paramName)
        {
            //System.Data.SqlDbType dbType = DBLinq.util.SqlTypeConversions.ParseType(dbTypeName);
            //SqlParameter param = new SqlParameter(paramName, dbType);
            NpgsqlTypes.NpgsqlDbType dbType = PgsqlTypeConversions.ParseType(dbTypeName);
            NpgsqlParameter param = new NpgsqlParameter(paramName, dbType);
            return param;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace DbLinq.Util
{
    public static class DataCommand
    {
        public delegate T ReadDelegate<T>(IDataReader reader);

        public static List<T> Find<T>(IDbConnection conn, string sql, string dbParameterName, string db, ReadDelegate<T> readDelegate)
        {
            using (IDbCommand command = conn.CreateCommand())
            {
                command.CommandText = sql;
                if (dbParameterName != null)
                {
                    IDbDataParameter parameter = command.CreateParameter();
                    parameter.ParameterName = dbParameterName;
                    parameter.Value = db;
                    command.Parameters.Add(parameter);
                }
                using (IDataReader rdr = command.ExecuteReader())
                {
                    List<T> list = new List<T>();
                    while (rdr.Read())
                    {
                        list.Add(readDelegate(rdr));
                    }
                    return list;
                }
            }
        }

        public static List<T> Find<T>(IDbConnection conn, string sql, ReadDelegate<T> readDelegate)
        {
            return Find<T>(conn, sql, null, null, readDelegate);
        }
    }
}

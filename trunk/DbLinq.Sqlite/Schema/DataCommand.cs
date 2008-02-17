using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace DbLinq.Sqlite.Schema
{
    public static class DataCommand
    {
        public delegate T ReadDelegate<T>(IDataReader reader, string table);

        public static List<T> Find<T>(IDbConnection connection, string sql, string pragma, ReadDelegate<T> readDelegate)
        {
            using (IDbCommand cmd = connection.CreateCommand())
            {
                cmd.CommandText = sql;
                using (IDataReader rdr = cmd.ExecuteReader())
                {
                    List<T> list = new List<T>();

                    while (rdr.Read())
                    {
                        string table = rdr.GetString(0);
                        //string sqlPragma = @"PRAGMA foreign_key_list('" + table + "');";
                        string sqlPragma = string.Format(pragma, table);
                        using (IDbCommand cmdPragma = connection.CreateCommand())
                        {
                            cmdPragma.CommandText = sqlPragma;
                            using (IDataReader rdrPragma = cmdPragma.ExecuteReader())
                            {
                                while (rdrPragma.Read())
                                {
                                    list.Add(readDelegate(rdrPragma, table));
                                }

                            }
                        }
                    }
                    return list;
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;

namespace SqlMetal.schema.mysql
{
    /// <summary>
    /// represents one row from MySQL's MYSQL.PROC table
    /// </summary>
    public class ProcRow
    {
        public string db;
        public string name;
        public string type;
        public string specific_name;
        public string param_list;
        public string returns;
        public string body;

        public override string ToString()
        {
            return "ProcRow " + name;
        }
    }

    /// <summary>
    /// class for reading from 'MYSQL.PROC'.
    /// We use mysql.PROC instead of information_schema.ROUTINES, because it saves us parsing of parameters.
    /// Note: higher permissions are required to access mysql.PROC.
    /// </summary>
    class ProcSql
    {
        ProcRow fromRow(MySqlDataReader rdr)
        {
            ProcRow p = new ProcRow();
            int field = 0;
            p.db = rdr.GetString(field++);
            p.name = rdr.GetString(field++);
            p.type = rdr.GetString(field++);
            p.specific_name = rdr.GetString(field++);
            
            object oo = rdr.GetFieldType(field);
            p.param_list = rdr.GetString(field++);
            p.returns = rdr.GetString(field++);
            p.body = rdr.GetString(field++);
            return p;
        }

        public List<ProcRow> getProcs(MySqlConnection conn, string db)
        {
            string sql = @"
SELECT db, name, type, specific_name, param_list, returns, body
FROM mysql.proc
WHERE db=?db AND type IN ('FUNCTION','PROCEDURE')";

            using (MySqlCommand cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.Add("?db", db.ToLower());
                using (MySqlDataReader rdr = cmd.ExecuteReader())
                {
                    List<ProcRow> list = new List<ProcRow>();
                    while (rdr.Read())
                    {
                        list.Add(fromRow(rdr));
                    }
                    return list;
                }
            }

        }
    }
}

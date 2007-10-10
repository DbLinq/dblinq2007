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

        /// <summary>
        /// dependencies are determined by analyzing foreign keys.
        /// </summary>
        public readonly List<TableRow> childTables = new List<TableRow>();

        public IEnumerable<TableRow> EnumChildTables(int depth)
        {
            if (depth > 99)
            {
                //prevent infinite recursion, in case of circular dependency
                throw new ApplicationException("Circular dependency suspected");
            }

            foreach (TableRow t in childTables)
            {
                yield return t;
                foreach (TableRow t2 in t.EnumChildTables(depth + 1))
                {
                    yield return t2;
                }
            }
        }
    }

    /// <summary>
    /// class for reading from 'MYSQL.PROC'
    /// (Note: we could also work with information_schema.routines table, but we would have to parse out param_list manually)
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

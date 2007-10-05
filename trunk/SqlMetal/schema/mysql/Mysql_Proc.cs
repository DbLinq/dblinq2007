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
            p.param_list = rdr.GetString(field++);
            return p;
        }

        public List<ProcRow> getProcs(MySqlConnection conn, string db)
        {
            string sql = @"
SELECT db, name, type, specific_name, param_list
FROM mysql.proc
WHERE db=?db AND type='PROCEDURE'";

            using (MySqlCommand cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.Add("?db", db);
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

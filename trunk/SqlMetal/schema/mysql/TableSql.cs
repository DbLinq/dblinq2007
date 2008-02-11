using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;

namespace SqlMetal.schema.mysql
{
        /// <summary>
    /// represents one row from MySQL's information_schema.`TABLES` table
    /// </summary>
    public class TableRow
    {
        public string table_catalog;
        public string table_schema;
        public string table_name;

        public override string ToString()
        {
            return "TableRow " + table_schema + "." + table_name;
        }
    }

    /// <summary>
    /// class for reading from "information_schema.`TABLES`"
    /// </summary>
    class TableSql
    {
        TableRow fromRow(MySqlDataReader rdr)
        {
            TableRow t = new TableRow();
            int field = 0;
            t.table_catalog = rdr.GetStringN(field++);
            t.table_schema  = rdr.GetStringN(field++);
            t.table_name    = rdr.GetStringN(field++);
            return t;
        }

        public List<TableRow> getTables(MySqlConnection conn, string db)
        {
            string sql = @"
SELECT table_catalog,table_schema,table_name
FROM information_schema.`TABLES`
WHERE table_schema=?db";

            using(MySqlCommand cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.Add("?db", db);
                using(MySqlDataReader rdr = cmd.ExecuteReader())
                {
                    List<TableRow> list = new List<TableRow>();
                    while(rdr.Read())
                    {
                        list.Add( fromRow(rdr) );
                    }
                    return list;
                }
            }

        }
    }

    static class MySqlDataReaderExtensions
    {
        public static string GetStringN(this MySqlDataReader rdr, int field)
        {
            return rdr.IsDBNull(field)
                ? null
                : rdr.GetString(field);
        }
    }
}

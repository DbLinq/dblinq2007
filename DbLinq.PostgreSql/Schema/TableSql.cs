using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using DbLinq.Util;

namespace DbLinq.PostgreSql.Schema
{
    /// <summary>
    /// represents one row from POSTGRES's information_schema.`TABLES` table
    /// </summary>
    public class TableRow
    {
        public string table_catalog;
        public string table_schema;
        public string table_name;
    }

    /// <summary>
    /// class for reading from "information_schema.`TABLES`"
    /// </summary>
    class TableSql
    {
        TableRow fromRow(IDataReader rdr)
        {
            TableRow t = new TableRow();
            int field = 0;
            t.table_catalog = rdr.GetString(field++);
            t.table_schema  = rdr.GetString(field++);
            t.table_name    = rdr.GetString(field++);
            return t;
        }

        public List<TableRow> getTables(IDbConnection conn, string db)
        {
            string sql = @"
SELECT table_catalog,table_schema,table_name
FROM information_schema.TABLES
WHERE table_catalog=:db
AND table_schema NOT IN ('pg_catalog','information_schema')";

            return DataCommand.Find<TableRow>(conn, sql, ":db", db, fromRow);
        }
    }
}
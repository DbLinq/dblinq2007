using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using DbLinq.MySql;
using DbLinq.Util;
using MySql.Data.MySqlClient;

namespace DbLinq.MySql.Schema
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
        TableRow fromRow(IDataReader rdr)
        {
            TableRow t = new TableRow();
            int field = 0;
            t.table_catalog = rdr.GetStringN(field++);
            t.table_schema  = rdr.GetStringN(field++);
            t.table_name    = rdr.GetStringN(field++);
            return t;
        }

        public List<TableRow> getTables(IDbConnection conn, string db)
        {
            string sql = @"
SELECT table_catalog,table_schema,table_name
FROM information_schema.`TABLES`
WHERE table_schema=?db";

            return DataCommand.Find<TableRow>(conn, sql, "?db", db, fromRow);
        }
    }

    static class MySqlDataReaderExtensions
    {
        public static string GetStringN(this IDataReader rdr, int field)
    {
        return rdr.IsDBNull(field)
                   ? null
                   : rdr.GetString(field);
    }
    }
}
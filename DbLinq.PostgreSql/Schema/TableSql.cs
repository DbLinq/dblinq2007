using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using DbLinq.Util;
using DbLinq.Vendor.Implementation;

namespace DbLinq.PostgreSql.Schema
{

    /// <summary>
    /// class for reading from "information_schema.`TABLES`"
    /// </summary>
    class TableSql
    {
        SchemaLoader.DataName fromRow(IDataReader rdr)
        {
            var t = new SchemaLoader.DataName();
            int field = 0;
            t.Schema  = rdr.GetString(field++);
            t.Name    = rdr.GetString(field++);
            return t;
        }

        public List<SchemaLoader.DataName> getTables(IDbConnection conn, string db)
        {
            string sql = @"
SELECT table_schema,table_name
FROM information_schema.TABLES
WHERE table_catalog=:db
AND table_schema NOT IN ('pg_catalog','information_schema')";

            return DataCommand.Find<SchemaLoader.DataName>(conn, sql, ":db", db, fromRow);
        }
    }
}
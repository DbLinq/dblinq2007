using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using DbLinq.Vendor.Implementation;

namespace DbLinq.Sqlite.Schema
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
            t.Schema  = "main";
            t.Name    = rdr.GetString(field++);
            return t;
        }

        public List<SchemaLoader.DataName> getTables(IDbConnection conn, string db)
        {
            // As there is no foreign key, we are sorting table by name
            string sql = @" SELECT tbl_name FROM sqlite_master WHERE type='table' order by tbl_name";

            return Util.DataCommand.Find<SchemaLoader.DataName>(conn, sql, fromRow);
        }
    }
}
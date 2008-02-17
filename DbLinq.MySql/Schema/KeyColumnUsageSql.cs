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
    /// represents one row from MySQL's information_schema.`Key_Column_Usage` table
    /// </summary>
    public class KeyColumnUsage
    {
        public string constraint_schema;
        public string constraint_name;
        public string table_schema;
        public string table_name;
        public string column_name;
        public string referenced_table_schema;
        public string referenced_table_name;
        public string referenced_column_name;

        public override string ToString()
        {
            string detail = constraint_name == "PRIMARY"
                                ? table_name + " PK"
                                : constraint_name;
            return "KeyColUsage " + detail;
        }
    }

    /// <summary>
    /// class for reading from "information_schema.`Key_Column_Usage`"
    /// </summary>
    class KeyColumnUsageSql
    {
        KeyColumnUsage fromRow(IDataReader rdr)
        {
            KeyColumnUsage t = new KeyColumnUsage();
            int field = 0;
            t.constraint_schema = rdr.GetStringN(field++);
            t.constraint_name = rdr.GetString(field++);
            t.table_schema  = rdr.GetStringN(field++);
            t.table_name    = rdr.GetString(field++);
            t.column_name    = rdr.GetString(field++);
            t.referenced_table_schema = rdr.GetStringN(field++);
            t.referenced_table_name = rdr.GetStringN(field++);
            t.referenced_column_name = rdr.GetStringN(field++);
            return t;
        }

        public List<KeyColumnUsage> getConstraints(IDbConnection conn, string db)
        {
            string sql = @"
SELECT constraint_schema,constraint_name,table_schema,table_name
    ,column_name,referenced_table_schema,referenced_table_name,referenced_column_name
FROM information_schema.`KEY_COLUMN_USAGE`
WHERE table_schema=?db";

            return DataCommand.Find<KeyColumnUsage>(conn, sql,"?db", db, fromRow);
        }
    }
}
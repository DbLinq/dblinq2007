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
    /// represents one row from information_schema.`COLUMNS`
    /// </summary>
    public class Column
    {
        public string table_catalog;
        public string table_schema;
        public string table_name;
        public string column_name;
        public bool isNullable;

        /// <summary>
        /// eg 'int' or 'datetime'
        /// </summary>
        public string datatype;
        public string extra;

        /// <summary>
        /// eg. 'int(10) unsigned'
        /// </summary>
        public string column_type;

        /// <summary>
        /// null or 'PRI' or 'MUL'
        /// </summary>
        public string column_key;

        /// <summary>
        /// eg. for column called 'int' we use csharpName='int_'
        /// </summary>
        public string csharpFieldName;


        public override string ToString()
        {
            return "info_schema.COLUMN: " + table_name + "." + column_name;
        }
    }

    /// <summary>
    /// class for sleecting from information_schema.`COLUMNS`
    /// </summary>
    class ColumnSql
    {
        Column fromRow(IDataReader rdr)
        {
            Column t = new Column();
            int field = 0;
            t.table_catalog = rdr.GetStringN(field++);
            t.table_schema  = rdr.GetString(field++);
            t.table_name    = rdr.GetString(field++);
            t.column_name   = rdr.GetString(field++);
            string nullableStr = rdr.GetString(field++);
            t.isNullable    = nullableStr=="YES";
            t.datatype      = rdr.GetString(field++);
            t.extra         = rdr.GetString(field++);
            t.column_type   = rdr.GetString(field++);
            t.column_key    = rdr.GetString(field++);
            return t;
        }

        public List<Column> getColumns(IDbConnection conn, string db)
        {
            string sql = @"
SELECT table_catalog,table_schema,table_name,column_name
    ,is_nullable,data_type,extra,column_type
    ,column_key
FROM information_schema.`COLUMNS`
WHERE table_schema=?db";

            return DataCommand.Find<Column>(conn, sql, "?db", db, fromRow);
        }
    }
}
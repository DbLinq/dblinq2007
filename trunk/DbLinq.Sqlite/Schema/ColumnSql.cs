using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using DbLinq.Sqlite.Schema;
using DbLinq.Util;
using DbLinq.Vendor.Implementation;

namespace DbLinq.Sqlite.Schema
{
    /// <summary>
    /// represents one row from information_schema.`COLUMNS`
    /// </summary>
    public class Column : SchemaLoader.DataType
    {
        public string table_catalog;
        public string table_schema;
        public string table_name;
        public string column_name;

        /// <summary>
        /// eg 'int' or 'datetime'
        /// </summary>
        public string extra;

        /// <summary>
        /// eg. 'int(10) unsigned'
        /// </summary>
        public string column_type;

        /// <summary>
        /// null or 'PRI' or 'MUL'
        /// </summary>
        public bool isPrimaryKey;

        /// <summary>
        /// eg. for column called 'int' we use csharpName='int_'
        /// </summary>

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
        private Column fromRow(IDataReader dataReader, string table)
        {
            Column t = new Column();
            t.table_catalog = "SQLite";
            t.table_schema = "main";
            t.table_name = table;
            t.column_name = dataReader.GetString(1);
            t.UnpackRawDbType(dataReader.GetString(2));
            t.column_type = dataReader.GetString(2);
            t.Nullable = dataReader.GetInt64(3) == 0;
            t.isPrimaryKey = dataReader.GetInt64(5) == 1;
            return t;
        }

        public List<Column> getColumns(IDbConnection conn, string db)
        {
            string sql = @" SELECT tbl_name FROM sqlite_master WHERE type='table' order by tbl_name";
            string pragma = @"PRAGMA table_info('{0}');";

            return DataCommand.Find<Column>(conn, sql, pragma, fromRow);
        }
    }
}
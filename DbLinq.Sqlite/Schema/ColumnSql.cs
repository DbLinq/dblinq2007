using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using DbLinq.Sqlite.Schema;

namespace DbLinq.Sqlite.Schema
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
        private Column fromRow(IDataReader dataReader, string table)
        {
            Column t = new Column();
            t.table_catalog = "SQLite";
            t.table_schema = "main";
            t.table_name = table;
            t.column_name = dataReader.GetString(1);
            t.datatype = Mappings.mapSqlTypeToCsType(dataReader.GetString(2), "");
            t.column_type = dataReader.GetString(2);
            Int64 nullableStr = dataReader.GetInt64(3);
            t.isNullable = nullableStr == 0;
            t.column_key = (dataReader.GetInt64(5) == 1 ? "PRI" : null);
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
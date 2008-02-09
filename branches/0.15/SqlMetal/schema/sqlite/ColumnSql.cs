using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SQLite;

namespace SqlMetal.schema.sqlite
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
        public List<Column> getColumns(SQLiteConnection conn, string db)
        {
            string sql = @" SELECT tbl_name FROM sqlite_master WHERE type='table' order by tbl_name";

            using (SQLiteCommand cmd = new SQLiteCommand(sql, conn))
            {
                using (SQLiteDataReader rdr = cmd.ExecuteReader())
                {
                    List<Column> list = new List<Column>();
                    while (rdr.Read())
                    {
                        string sqlPragma = @"PRAGMA table_info('" + rdr.GetString(0) + "');";
                        using (SQLiteCommand cmdPragma = new SQLiteCommand(sqlPragma, conn))
                        {
                            using (SQLiteDataReader rdrPragma = cmdPragma.ExecuteReader())
                            {
                                while (rdrPragma.Read())
                                {
                                    Column t = new Column();
                                    t.table_catalog = "SQLite";
                                    t.table_schema = "main";
                                    t.table_name = rdr.GetString(0);
                                    t.column_name = rdrPragma.GetString(1);
                                    t.datatype = Mappings.mapSqlTypeToCsType(rdrPragma.GetString(2), "");
                                    t.column_type = rdrPragma.GetString(2);
                                    Int64 nullableStr = rdrPragma.GetInt64(3);
                                    t.isNullable = nullableStr == 0;
                                    t.column_key = (rdrPragma.GetInt64(5) == 1 ? "PRI" : null);
                                    list.Add(t);
                                }
                            }
                        }
                    }
                    return list;
                }
            }
        }
    }
}

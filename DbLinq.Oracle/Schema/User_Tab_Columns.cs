using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using DbLinq.Util;
using DbLinq.Vendor.Implementation;

namespace DbLinq.Oracle.Schema
{
    /// <summary>
    /// represents one row from information_schema.`COLUMNS`
    /// </summary>
    public class User_Tab_Column: SchemaLoader.DataType
    {
        //public string table_catalog;
        public string table_schema;
        public string table_name;
        public string column_name;

        /// <summary>
        /// eg 'int' or 'datetime'
        /// </summary>
        public string data_type_mod;

        /// <summary>
        /// generated - DB column is actually "Nullable='Y'"
        /// </summary>
        public bool isNullable;

        public int column_id;


        /// <summary>
        /// eg. for column called 'int' we use csharpName='int_'
        /// </summary>
        public string csharpFieldName;
    }

    /// <summary>
    /// class for sleecting from information_schema.`COLUMNS`
    /// </summary>
    class User_Tab_Column_Sql
    {
        User_Tab_Column fromRow(IDataReader rdr)
        {
            User_Tab_Column t = new User_Tab_Column();
            int field = 0;
            //t.table_catalog = rdr.GetString(field++);
            t.table_schema  = rdr.GetString(field++);
            t.table_name = rdr.GetString(field++);
            t.column_name = rdr.GetString(field++);

            t.Type = rdr.GetString(field++);
            if (t.Type == "TIMESTAMP(6)")
            {
                t.Type = "TIMESTAMP"; //clip the '(6)' - don't know the meaning
            }

            //we use OraExtensions.GetNString():
            t.data_type_mod = rdr.GetNString(field++); //todo: null
            t.Length = rdr.GetNInt(field++);

            //Nicholas (.f1@free.fr) reports an error - Precision being decimal
            //t.Precision = rdr.GetNString(field++); //null
            t.Precision = rdr.GetNInt(field++); //null //

            t.Scale = rdr.GetNInt(field++); //null

            string nullableStr = rdr.GetString(field++);
            t.isNullable = nullableStr == "Y";
            t.column_id = Convert.ToInt32(rdr.GetValue(field++));
            return t;
        }


        public List<User_Tab_Column> getColumns(IDbConnection conn, string db)
        {
            string sql = @"
SELECT 
owner, table_name, column_name, data_type, data_type_mod, data_length, data_precision, data_scale, nullable, column_id
FROM ALL_TAB_COLUMNS
WHERE table_name NOT LIKE '%$%' 
AND table_name NOT LIKE 'LOGMNR%' 
AND table_name NOT LIKE 'MVIEW%' 
AND table_name NOT IN ('SQLPLUS_PRODUCT_PROFILE','HELP', 'PRODUCT_PRIVS')
and lower(owner) = :owner

ORDER BY table_name, Column_id";

            return DataCommand.Find<User_Tab_Column>(conn, sql, ":owner", db.ToLower(), fromRow);
        }

    }
}

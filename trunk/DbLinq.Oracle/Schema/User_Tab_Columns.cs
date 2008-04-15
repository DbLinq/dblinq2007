using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using DbLinq.Util;
using DbLinq.Vendor.Implementation;

namespace DbLinq.Oracle.Schema
{
    /// <summary>
    /// class for sleecting from information_schema.`COLUMNS`
    /// </summary>
    class User_Tab_Column_Sql
    {
        SchemaLoader.DataTableColumn fromRow(IDataReader rdr)
        {
            var t = new SchemaLoader.DataTableColumn();
            int field = 0;
            //t.table_catalog = rdr.GetString(field++);
            t.TableSchema  = rdr.GetString(field++);
            t.TableName = rdr.GetString(field++);
            t.ColumnName = rdr.GetString(field++);

            t.Type = rdr.GetString(field++);
            if (t.Type == "TIMESTAMP(6)")
            {
                t.Type = "TIMESTAMP"; //clip the '(6)' - don't know the meaning
            }

            //we use OraExtensions.GetNString():
            t.Length = rdr.GetNInt(field++);

            //Nicholas (.f1@free.fr) reports an error - Precision being decimal
            //t.Precision = rdr.GetNString(field++); //null
            t.Precision = rdr.GetNInt(field++); //null //

            t.Scale = rdr.GetNInt(field++); //null

            string nullableStr = rdr.GetString(field++);
            t.Nullable = nullableStr == "Y";
            return t;
        }


        public List<SchemaLoader.DataTableColumn> getColumns(IDbConnection conn, string db)
        {
            string sql = @"
SELECT 
owner, table_name, column_name, data_type, data_length, data_precision, data_scale, nullable
FROM ALL_TAB_COLUMNS
WHERE table_name NOT LIKE '%$%' 
AND table_name NOT LIKE 'LOGMNR%' 
AND table_name NOT LIKE 'MVIEW%' 
AND table_name NOT IN ('SQLPLUS_PRODUCT_PROFILE','HELP', 'PRODUCT_PRIVS')
and lower(owner) = :owner

ORDER BY table_name, Column_id";

            return DataCommand.Find<SchemaLoader.DataTableColumn>(conn, sql, ":owner", db.ToLower(), fromRow);
        }

    }
}

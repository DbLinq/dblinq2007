using System;
using System.Collections.Generic;
using System.Text;
using System.Data.OracleClient;

namespace SqlMetal.schema.oracle
{
    /// <summary>
    /// represents one row from information_schema.`COLUMNS`
    /// </summary>
    public class User_Tab_Column
    {
        //public string table_catalog;
        //public string table_schema;
        public string table_name;
        public string column_name;

        /// <summary>
        /// eg 'int' or 'datetime'
        /// </summary>
        public string data_type;
        public string data_type_mod;
        
        /// <summary>
        /// eg. null or 4000 for default string?
        /// </summary>
        public int? data_length;
        public decimal? data_scale;

        //Nicholas (.f1@free.fr) reports an error - data_precision being decimal
        //reference: http://oracle.kuriositaet.de/index/master_index_DATA.html
        //public string data_precision;
        public decimal? data_precision;


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
        User_Tab_Column fromRow(OracleDataReader rdr)
        {
            User_Tab_Column t = new User_Tab_Column();
            int field = 0;
            //t.table_catalog = rdr.GetString(field++);
            //t.table_schema  = rdr.GetString(field++);
            t.table_name    = rdr.GetString(field++);
            t.column_name   = rdr.GetString(field++);

            t.data_type      = rdr.GetString(field++);
            if(t.data_type=="TIMESTAMP(6)"){
                t.data_type = "TIMESTAMP"; //clip the '(6)' - don't know the meaning
            }

            //we use OraExtensions.GetNString():
            t.data_type_mod  = rdr.GetNString(field++); //todo: null
            t.data_length  = rdr.GetNInt(field++);

            //Nicholas (.f1@free.fr) reports an error - data_precision being decimal
            //t.data_precision = rdr.GetNString(field++); //null
            t.data_precision = rdr.GetNDecimal(field++); //null //
            
            t.data_scale  = rdr.GetNDecimal(field++); //null

            string nullableStr = rdr.GetString(field++);
            t.isNullable    = nullableStr=="Y";
            t.column_id         = rdr.GetInt32(field++);
            return t;
        }


        public List<User_Tab_Column> getColumns(OracleConnection conn, string db)
        {
            string sql = @"
SELECT 
table_name, column_name, data_type, data_type_mod, data_length, data_precision, data_scale, nullable, column_id
FROM USER_TAB_COLUMNS
WHERE table_name NOT LIKE '%$%' 
AND table_name NOT LIKE 'LOGMNR%' 
AND table_name NOT LIKE 'MVIEW%' 
AND table_name NOT IN ('SQLPLUS_PRODUCT_PROFILE','HELP', 'PRODUCT_PRIVS')";

            using(OracleCommand cmd = new OracleCommand(sql, conn))
            {
                //cmd.Parameters.Add("?db", db);
                using(OracleDataReader rdr = cmd.ExecuteReader())
                {
                    List<User_Tab_Column> list = new List<User_Tab_Column>();
                    while(rdr.Read())
                    {
                        list.Add( fromRow(rdr) );
                    }
                    return list;
                }
            }

        }

    }
}

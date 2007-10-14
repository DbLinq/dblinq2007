////////////////////////////////////////////////////////////////////
//Initial author: Jiri George Moudry, 2006.
//License: LGPL. (Visit http://www.gnu.org)
////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using Npgsql;

namespace SqlMetal.schema.pgsql
{
    /// <summary>
    /// represents one row from Postgres' information_schema.`Key_Column_Usage` table
    /// </summary>
    public class KeyColumnUsage
    {
        public string constraint_schema;
        public string constraint_name;
        public string table_schema;
        public string table_name;
        public string column_name;
        //public string referenced_table_schema;
        //public string referenced_table_name;
        //public string referenced_column_name;
        public override string ToString()
        {
            return "KeyColUsage "+constraint_name+":  "+table_name+"."+column_name;
        }
    }

    /// <summary>
    /// class for reading from "information_schema.`Key_Column_Usage`"
    /// </summary>
    class KeyColumnUsageSql
    {
        KeyColumnUsage fromRow(NpgsqlDataReader rdr)
        {
            KeyColumnUsage t = new KeyColumnUsage();
            int field = 0;
            t.constraint_schema = rdr.GetString(field++);
            t.constraint_name = rdr.GetString(field++);
            t.table_schema  = rdr.GetString(field++);
            t.table_name    = rdr.GetString(field++);
            t.column_name    = rdr.GetString(field++);
            //t.referenced_table_schema = rdr.GetString(field++);
            //t.referenced_table_name = rdr.GetString(field++);
            //t.referenced_column_name = rdr.GetString(field++);
            return t;
        }

        public List<KeyColumnUsage> getConstraints(NpgsqlConnection conn, string db)
        {
            string sql = @"
SELECT constraint_schema,constraint_name,table_schema,table_name
    ,column_name
FROM information_schema.KEY_COLUMN_USAGE
WHERE constraint_catalog=:db";

            using(NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
            {
                cmd.Parameters.Add(":db", db);
                using(NpgsqlDataReader rdr = cmd.ExecuteReader())
                {
                    List<KeyColumnUsage> list = new List<KeyColumnUsage>();
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

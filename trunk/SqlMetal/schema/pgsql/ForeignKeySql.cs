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
    /// row data from tables table_constraints, constraint_column_usage
    /// </summary>
    public class ForeignKeyCrossRef
    {
        public string constraint_name;
        public string table_name_Child;
        public string constraint_type;
        public string table_name_Parent;
        public string column_name;

        public override string ToString()
        {
            return "ForKeyXR "+constraint_name+": "+constraint_type+"  "+table_name_Child+"->"+table_name_Parent;
        }
    }

    /// <summary>
    /// class for reading from information_schema -
    /// (from tables table_constraints, constraint_column_usage)
    /// </summary>
    class ForeignKeySql
    {
        ForeignKeyCrossRef fromRow(NpgsqlDataReader rdr)
        {
            ForeignKeyCrossRef t = new ForeignKeyCrossRef();
            int field = 0;
            t.constraint_name = rdr.GetString(field++);
            t.table_name_Child    = rdr.GetString(field++);
            t.constraint_type = rdr.GetString(field++);
            t.table_name_Parent  = rdr.GetString(field++);
            t.column_name    = rdr.GetString(field++);
            return t;
        }

        public List<ForeignKeyCrossRef> getConstraints(NpgsqlConnection conn, string db)
        {
            string sql = @"
SELECT t.constraint_name, t.table_name, t.constraint_type,
    c.table_name, c.column_name
FROM information_schema.table_constraints t,
    information_schema.constraint_column_usage c
WHERE t.constraint_name = c.constraint_name
    and t.constraint_type IN  ('FOREIGN KEY','PRIMARY KEY')";

            using(NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
            {
                //cmd.Parameters.Add(":db", db);
                using(NpgsqlDataReader rdr = cmd.ExecuteReader())
                {
                    List<ForeignKeyCrossRef> list = new List<ForeignKeyCrossRef>();
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

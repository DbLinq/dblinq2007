using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Npgsql;

namespace SqlMetal.schema.pgsql
{
    /// <summary>
    /// represents one row from pg_proc table
    /// </summary>
    public class Pg_Proc
    {
        public string proname;
        public bool proretset;
        public long prorettype;
        public string formatted_prorettype;
        public string proargtypes;
        public string proargnames;

        /// <summary>
        /// parsed list of integer proargtypes
        /// </summary>
        public List<long> progargtypes2;

        public override string ToString() { return "Pg_Proc " + proname; }
    }

    /// <summary>
    /// class for reading from pg_proc table
    /// </summary>
    public class Pg_Proc_Sql
    {
        Pg_Proc fromRow(NpgsqlDataReader rdr)
        {
            Pg_Proc t = new Pg_Proc();
            int field = 0;
            t.proname = rdr.GetString(field++);
            t.proretset = rdr.GetBoolean(field++);
            t.prorettype = rdr.GetInt64(field++);
            object o2 = rdr.GetValue(field);
            t.formatted_prorettype = rdr.GetString(field++);
            object o3 = rdr.GetValue(field);
            t.proargtypes = rdr.GetString(field++);
            object o4 = rdr.GetValue(field);
            if (rdr.IsDBNull(field))
            {
                t.proargnames = null; field++;
            }
            else
            {
                t.proargnames = rdr.GetString(field++);
            }

            //post-process:
            if (t.proargtypes != null)
            {
                string[] parts = t.proargtypes.Split(' ');
                t.progargtypes2 = new List<long>();
                foreach (var part in parts)
                {
                    long longVal;
                    if (long.TryParse(part, out longVal))
                        t.progargtypes2.Add(longVal);
                }
            }
            return t;
        }

        public List<Pg_Proc> getProcs(NpgsqlConnection conn, string db)
        {
            string sql = @"
SELECT pr.proname, pr.proretset, pr.prorettype, pg_catalog.format_type(pr.prorettype, NULL) 
  ,pr.proargtypes, pr.proargnames
FROM pg_proc pr, pg_type tp 
WHERE tp.oid = pr.prorettype AND pr.proisagg = FALSE 
AND tp.typname <> 'trigger' 
AND pr.pronamespace IN ( SELECT oid FROM pg_namespace 
WHERE nspname NOT LIKE 'pg_%' AND nspname != 'information_schema' ); 

";

            using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
            {
                cmd.Parameters.Add(":db", db);
                using (NpgsqlDataReader rdr = cmd.ExecuteReader())
                {
                    List<Pg_Proc> list = new List<Pg_Proc>();
                    while (rdr.Read())
                    {
                        list.Add(fromRow(rdr));
                    }
                    return list;
                }
            }
        }

        public int getTypeNames(NpgsqlConnection conn, string db, Dictionary<long, string> oid_to_name_map)
        {
            string sql = @"
SELECT pg_catalog.format_type(:typeOid, NULL)
";
            int numDone = 0;

            //clone to prevent 'collection was modified' exception
            Dictionary<long, string> oid_to_name_map2 = new Dictionary<long, string>(oid_to_name_map);

            using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
            {
                foreach (var kv in oid_to_name_map2)
                {
                    if (kv.Value != null)
                        continue; //value already known

                    long typeOid = kv.Key;
                    cmd.Parameters.Add(":typeOid", typeOid);
                    numDone++;
                    object typeName1 = cmd.ExecuteScalar();
                    string typeName2 = typeName1 as string;
                    oid_to_name_map[typeOid] = typeName2; //eg. dic[23] = "integer"
                }
            }
            return numDone;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DbLinq.Util;

namespace DbLinq.PostgreSql.Schema
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

        /// <summary>
        /// species types of in-args, eg. '23 1043'
        /// </summary>
        public string proargtypes;

        /// <summary>
        /// species types of in,out args, eg. '{23,1043,1043}'
        /// </summary>
        public string proallargtypes;

        /// <summary>
        /// param names, eg {i1,i2,o2}
        /// </summary>
        public string proargnames;

        /// <summary>
        /// specifies in/out modes - eg. '{i,i,o}'
        /// </summary>
        public string proargmodes;

        public override string ToString() { return "Pg_Proc " + proname; }
    }

    /// <summary>
    /// class for reading from pg_proc table
    /// </summary>
    public class Pg_Proc_Sql
    {
        Pg_Proc fromRow(IDataReader rdr)
        {
            Pg_Proc t = new Pg_Proc();
            int field = 0;
            t.proname = rdr.GetString(field++);
            t.proretset = rdr.GetBoolean(field++);
            t.prorettype = rdr.GetInt64(field++);
            t.formatted_prorettype = rdr.GetString(field++);
            t.proargtypes = rdr.GetString(field++);
            t.proallargtypes = GetStringOrNull(rdr, field++);
            t.proargnames = GetStringOrNull(rdr, field++);
            t.proargmodes = GetStringOrNull(rdr, field++);
            return t;
        }

        static string GetStringOrNull(IDataReader rdr, int field)
        {
            if (rdr.IsDBNull(field))
                return null;
            else
                return rdr.GetString(field++);
        }

        public List<Pg_Proc> getProcs(IDbConnection conn, string db)
        {
            string sql = @"
SELECT pr.proname, pr.proretset, pr.prorettype, pg_catalog.format_type(pr.prorettype, NULL) 
  ,pr.proargtypes, pr.proallargtypes, pr.proargnames, pr.proargmodes
FROM pg_proc pr, pg_type tp 
WHERE tp.oid = pr.prorettype AND pr.proisagg = FALSE 
AND tp.typname <> 'trigger' 
AND pr.pronamespace IN ( SELECT oid FROM pg_namespace 
WHERE nspname NOT LIKE 'pg_%' AND nspname != 'information_schema' ); 

";

            return DataCommand.Find<Pg_Proc>(conn, sql, ":db", db, fromRow);
        }

        public int getTypeNames(IDbConnection conn, string db, Dictionary<long, string> oid_to_name_map)
        {
            string sql = @"
SELECT pg_catalog.format_type(:typeOid, NULL)
";
            int numDone = 0;

            //clone to prevent 'collection was modified' exception
            Dictionary<long, string> oid_to_name_map2 = new Dictionary<long, string>(oid_to_name_map);

            using (IDbCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                foreach (var kv in oid_to_name_map2)
                {
                    if (kv.Value != null)
                        continue; //value already known

                    long typeOid = kv.Key;
                    IDbDataParameter parameter = cmd.CreateParameter();
                    parameter.ParameterName=":typeOid";
                    parameter.Value=typeOid;
                    cmd.Parameters.Add(parameter);
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
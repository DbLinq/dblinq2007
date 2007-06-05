using System;
using System.Collections.Generic;
using System.Text;
using System.Data.OracleClient;

namespace SqlMetal.schema.oracle
{
    /// <summary>
    /// represents one row from Oracle's USER_TABLES table (subset)
    /// </summary>
    public class UserTablesRow
    {
        public string table_name;
        public string tablespace_name;

        /// <summary>
        /// dependencies are determined by analyzing foreign keys.
        /// </summary>
        public readonly List<UserTablesRow> childTables = new List<UserTablesRow>();

        public IEnumerable<UserTablesRow> EnumChildTables(int depth)
        {
            if(depth>99){
                //prevent infinite recursion, in case of circular dependency
                throw new ApplicationException("Circular dependency suspected");
            }

            foreach(UserTablesRow t in childTables)
            {
                yield return t;
                foreach(UserTablesRow t2 in t.EnumChildTables(depth+1))
                {
                    yield return t2;
                }
            }
        }
    }

    /// <summary>
    /// class for reading from "USER_TABLES"
    /// </summary>
    class UserTablesSql
    {
        UserTablesRow fromRow(OracleDataReader rdr)
        {
            UserTablesRow t = new UserTablesRow();
            int field = 0;
            t.table_name    = rdr.GetString(field++);
            t.tablespace_name    = rdr.GetString(field++);
            return t;
        }

        public List<UserTablesRow> getTables(OracleConnection conn, string db)
        {
            string sql = @"
SELECT table_name, tablespace_name 
FROM user_tables 
WHERE table_name NOT LIKE '%$%' 
AND table_name NOT LIKE 'LOGMNR%' 
AND table_name NOT IN ('SQLPLUS_PRODUCT_PROFILE','HELP')";

            using(OracleCommand cmd = new OracleCommand(sql, conn))
            {
                //cmd.Parameters.Add("?db", db);
                using(OracleDataReader rdr = cmd.ExecuteReader())
                {
                    List<UserTablesRow> list = new List<UserTablesRow>();
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

using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using DbLinq.Util;

namespace DbLinq.Oracle.Schema
{
    /// <summary>
    /// represents one row from Oracle's USER_TABLES table (subset)
    /// </summary>
    public class UserTablesRow
    {
        public string table_schema;
        public string table_name;
        public string tablespace_name;

        /// <summary>
        /// dependencies are determined by analyzing foreign keys.
        /// </summary>
        public readonly List<UserTablesRow> childTables = new List<UserTablesRow>();

        public IEnumerable<UserTablesRow> EnumChildTables(int depth)
        {
            if (depth > 99)
            {
                //prevent infinite recursion, in case of circular dependency
                throw new ApplicationException("Circular dependency suspected");
            }

            foreach (UserTablesRow t in childTables)
            {
                yield return t;
                foreach (UserTablesRow t2 in t.EnumChildTables(depth + 1))
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
        UserTablesRow fromRow(IDataReader rdr)
        {
            UserTablesRow t = new UserTablesRow();
            int field = 0;
            t.table_schema = rdr.GetString(field++);
            t.table_name = rdr.GetString(field++);
            t.tablespace_name = rdr.GetString(field++);
            return t;
        }

        public List<UserTablesRow> getTables(IDbConnection conn, string db)
        {
            string sql = @"
SELECT owner, table_name, tablespace_name 
FROM all_tables 
WHERE table_name NOT LIKE '%$%' 
AND table_name NOT LIKE 'LOGMNR%' 
AND table_name NOT IN ('SQLPLUS_PRODUCT_PROFILE','HELP')
and lower(owner) = :owner";

            return DataCommand.Find<UserTablesRow>(conn, sql, ":owner", db.ToLower(), fromRow);
        }
    }
}

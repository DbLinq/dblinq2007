using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using DbLinq.Util;
using DbLinq.Vendor.Implementation;

namespace DbLinq.Oracle.Schema
{
    /// <summary>
    /// class for reading from "USER_TABLES"
    /// </summary>
    class UserTablesSql
    {
        SchemaLoader.DataName fromRow(IDataReader rdr)
        {
            var t = new SchemaLoader.DataName();
            int field = 0;
            t.Schema = rdr.GetString(field++);
            t.Name = rdr.GetString(field++);
            return t;
        }

        public List<SchemaLoader.DataName> getTables(IDbConnection conn, string db)
        {
            string sql = @"
SELECT owner, table_name 
FROM all_tables 
WHERE table_name NOT LIKE '%$%' 
AND table_name NOT LIKE 'LOGMNR%' 
AND table_name NOT IN ('SQLPLUS_PRODUCT_PROFILE','HELP')
and lower(owner) = :owner";

            return DataCommand.Find<SchemaLoader.DataName>(conn, sql, ":owner", db.ToLower(), fromRow);
        }
    }
}

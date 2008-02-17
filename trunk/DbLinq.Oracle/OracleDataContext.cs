using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

#if ORACLE_IS_REFERENCED
//seems that Pascal does not reference Oracle DLLs from here?
using Oracle.Data.OracleClient;
#endif

namespace DbLinq.Oracle
{
    public class OracleDataContext : DbLinq.Linq.DataContext
    {
#if ORACLE_IS_REFERENCED
        public OracleDataContext(string connStr)
            : base(
#if ODP
            new Oracle.DataAccess.Client.OracleConnection(connStr) 
#else
            new System.Data.OracleClient.OracleConnection(connStr)
#endif
            ,new OracleVendor())
        {
        }
#endif

        public OracleDataContext(IDbConnection conn)
            : base(conn, new OracleVendor())
        {
        }

    }
}

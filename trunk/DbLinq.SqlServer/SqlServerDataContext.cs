using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace DbLinq.SqlServer
{
    public class SqlServerDataContext : DbLinq.Linq.DataContext
    {
        public SqlServerDataContext(string connStr)
            : base(new SqlConnection(connStr), new SqlServerVendor())
        {
        }

        public SqlServerDataContext(IDbConnection conn)
            : base((SqlConnection)conn, new SqlServerVendor())
        {
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Npgsql;

namespace DbLinq.PostgreSql
{
    /// <summary>
    /// PgsqlDataContext allows easier one-parameter creation of a data context.
    /// </summary>
    public class PgsqlDataContext : DBLinq.Linq.DataContext
    {
        public PgsqlDataContext(string connStr)
            : base(new NpgsqlConnection(connStr), new PgsqlVendor())
        {
        }

        public PgsqlDataContext(IDbConnection conn)
            : base((NpgsqlConnection)conn, new PgsqlVendor())
        {
        }

    }
}

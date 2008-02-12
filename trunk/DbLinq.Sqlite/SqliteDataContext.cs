using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SQLite;

namespace DbLinq.Sqlite
{
    public class SqliteDataContext : DBLinq.Linq.DataContext
    {
        public SqliteDataContext(string connStr)
            : base(new SQLiteConnection(connStr), new SqliteVendor())
        {
        }

        public SqliteDataContext(IDbConnection conn)
            : base((SQLiteConnection)conn, new SqliteVendor())
        {
        }

    }
}

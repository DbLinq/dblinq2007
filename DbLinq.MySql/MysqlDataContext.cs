using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using MySql.Data.MySqlClient;

namespace DbLinq.MySql
{
    public class MysqlDataContext : DBLinq.Linq.DataContext
    {
        public MysqlDataContext(string connStr)
            : base(new MySqlConnection(connStr), new MySqlVendor())
        {
        }

        public MysqlDataContext(IDbConnection conn)
            : base((MySqlConnection)conn, new MySqlVendor())
        {
        }

    }
}

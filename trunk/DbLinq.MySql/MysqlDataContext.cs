using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using MySql.Data.MySqlClient;

namespace DBLinq.vendor.mysql
{
    public class MysqlDataContext : DBLinq.Linq.DataContext
    {
        public MysqlDataContext(string connStr)
            : base(new MySqlConnection(connStr), new VendorMysql())
        {
        }

        public MysqlDataContext(IDbConnection conn)
            : base((MySqlConnection)conn, new VendorMysql())
        {
        }

    }
}

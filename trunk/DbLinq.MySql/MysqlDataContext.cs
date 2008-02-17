
using System.Data;
using MySql.Data.MySqlClient;

namespace DbLinq.MySql
{
    public class MySqlDataContext : DbLinq.Linq.DataContext
    {
        public MySqlDataContext(string connStr)
            : base(new MySqlConnection(connStr), new MySqlVendor())
        {
        }

        public MySqlDataContext(IDbConnection conn)
            : base(conn, new MySqlVendor())
        {
        }

    }
}

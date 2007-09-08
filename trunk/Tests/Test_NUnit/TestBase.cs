using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NUnit.Framework;

#if ORACLE
using ClientCodeOra;
using xint = System.Int32;
#elif POSTGRES
using Client2.user;
using xint = System.Int32;
using XSqlConnection = Npgsql.NpgsqlConnection;
using XSqlCommand = Npgsql.NpgsqlCommand;
#else
using XSqlConnection = MySql.Data.MySqlClient.MySqlConnection;
using XSqlCommand = MySql.Data.MySqlClient.MySqlCommand;
using Client2.user;
using xint = System.UInt32;
#endif

namespace Test_NUnit
{
    public abstract class TestBase
    {
#if ORACLE
        const string connStr = "server=localhost;user id=system; password=linq2";
#else //Mysql, Postgres
        const string connStr = "server=localhost;user id=LinqUser; password=linq2; database=LinqTestDB";

        XSqlConnection _conn;
        public XSqlConnection Conn
        {
            get
            {
                if (_conn == null) { _conn = new XSqlConnection(connStr); _conn.Open(); }
                return _conn;
            }
        }
#endif

#if POSTGRES
        //Postgres sorting: A,B,C,X,d
        const StringComparison stringComparisonType = StringComparison.Ordinal; 
#else
        //Mysql,Oracle sorting: A,B,C,d,X
        const StringComparison stringComparisonType = StringComparison.InvariantCulture;
#endif
        public LinqTestDB CreateDB()
        {
            LinqTestDB db = new LinqTestDB(connStr);
            db.Log = Console.Out;
            return db;
        }
    }
}

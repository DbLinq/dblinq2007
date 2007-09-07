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
    [TestFixture]
    public class ReadTest_AllTypes
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
        public LinqTestDB CreateDB()
        {
            LinqTestDB db = new LinqTestDB(connStr);
            db.Log = Console.Out;
            return db;
        }

        [Test]
        public void AT1_SelectRow()
        {
            LinqTestDB db = CreateDB();

            var q = from p in db.Alltypes select p;
            int count = q.ToList().Count;
            Assert.IsTrue(count > 0, "Expected some entries in AllTypes, got none");
        }

        [Test]
        public void AT2_SelectDateTimeN()
        {
            LinqTestDB db = CreateDB();

            var q = from p in db.Alltypes select p.DateTimeN;
            int count = q.ToList().Count;
            Assert.IsTrue(count > 0, "Expected some entries in AllTypes, got none");
        }

        [Test]
        public void AT3_SelectDecimalN()
        {
            LinqTestDB db = CreateDB();

            var q = from p in db.Alltypes select p.decimalN;
            int count = q.ToList().Count;
            Assert.IsTrue(count > 0, "Expected some entries in AllTypes, got none");
        }



    }
}

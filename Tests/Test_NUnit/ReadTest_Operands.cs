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
    public class ReadTest_Operands
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

        #region Tests 'C' do plain select - no aggregation
        [Test]
        public void H1_SelectConcat()
        {
            LinqTestDB db = CreateDB();

            var q = from p in db.Products select p.ProductName + " " + p.SupplierID;
            int count = 0;
            foreach (string s in q)
            {
                bool ok = Char.IsLetterOrDigit(s[0]) && s.Contains(' ');
                Assert.IsTrue(ok, "Concat string should start with product name, instead got:" + s);
                count++;
            }
            Assert.IsTrue(count > 0, "Expected concat strings, got none");
        }

        [Test]
        public void H2_SelectGreaterOrEqual()
        {
            LinqTestDB db = CreateDB();

            var q = db.Products.Where(p => p.ProductID >= 20);
            int count = 0;
            foreach (Product p in q)
            {
                Assert.IsTrue(p.ProductID >= 20, "Failed on ProductID>=20");
                count++;
            }
            Assert.IsTrue(count > 0, "Expected some products with ProductID>=20, got none");
        }

        public struct ProductWrapper1
        {
            public uint ProductID { get; set; }
            public uint SupplierID { get; set; }
        }

        [Test]
        public void H3_Select_MemberInit()
        {
            LinqTestDB db = CreateDB();

            var q = from p in db.Products
                    where p.ProductID > 20
                    select new ProductWrapper1 { ProductID = p.ProductID, SupplierID = p.SupplierID };

            int count = 0;
            foreach (ProductWrapper1 p in q)
            {
                Assert.IsTrue(p.ProductID > 20, "Failed on ProductID>=20");
                count++;
            }
            Assert.IsTrue(count > 0, "Expected some products with ProductID>=20, got none");

        }

        [Test]
        public void I1_GetQueryText()
        {
            LinqTestDB db = CreateDB();

            var q = from p in db.Products where p.ProductID > 1 select p;
            string s = db.GetQueryText(q); //MTable.GetQueryText()
        }

        [Test]
        public void I2_GetQueryText()
        {
            LinqTestDB db = CreateDB();

            var q = from p in db.Products select p.ProductName;
            string s = db.GetQueryText(q); //MTable_Projected.GetQueryText()
        }
        #endregion


    }
}

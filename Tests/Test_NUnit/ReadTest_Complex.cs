using System;
using System.Collections.Generic;
using System.Text;
using System.Query;
using System.Xml.XLinq;
using System.Data.DLinq;
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
    public class ReadTest_Complex
    {
#if ORACLE
        const string connStr = "server=localhost;user id=system; password=linq2";
#else
        const string connStr = "server=localhost;user id=LinqUser; password=linq2; database=LinqTestDB";
#endif
        
        XSqlConnection _conn;
        LinqTestDB db;
        
        public ReadTest_Complex()
        {
            db = new LinqTestDB(connStr);
        }
        public XSqlConnection Conn 
        { 
            get 
            { 
                if(_conn==null){ _conn=new XSqlConnection(connStr); _conn.Open(); }
                return _conn;
            }
        }

        #region Tests 'F' work on aggregation
        [Test]
        public void F1_ProductCount()
        {
            var q = from p in db.Products select p;
            int productCount = q.Count();
            Assert.Greater(productCount,0,"Expected non-zero product count");
        }

        [Test]
        public void F2_ProductCount_Projected()
        {
            var q = from p in db.Products select p.ProductID;
            int productCount = q.Count();
            Assert.Greater(productCount,0,"Expected non-zero product count");
            Console.WriteLine();
        }
        [Test]
        public void F2_ProductCount_Clause()
        {
            var q = from p in db.Products select p.ProductID;
            int productCount = q.Count(i => i < 3);
            Assert.Greater(productCount,0,"Expected non-zero product count");
        }

        [Test]
        public void F3_MaxProductId()
        {
            var q = from p in db.Products select p.ProductID;
            xint maxID = q.Max();
            Assert.Greater(maxID,0,"Expected non-zero product count");
        }

        [Test]
        public void F4_MinProductId()
        {
            var q = from p in db.Products select p.ProductID;
            xint maxID = q.Min();
            Assert.Greater(maxID,0,"Expected non-zero product count");
        }

        //[Test]
        //public void F5_AvgProductId()
        //{
        //    var q = from p in db.Products select p.ProductID;
        //    xint maxID = q.Average();
        //    Assert.Greater(maxID,0,"Expected non-zero product count");
        //}


        [Test]
        public void F7_ExplicitJoin()
        {
            var q =
	            from c in db.Customers
	            join o in db.Orders on c.CustomerID equals o.CustomerID
	            where c.City == "London"
	            select o;
        }

        [Test]
        public void F8_IncludingClause()
        {
            var q = (
	            from c in db.Customers
	            where c.City == "London"
	            select c)
	            .Including(c => c.Orders);
        }

#if ADD_TABLE_ORDERDETAILS
        [Test]
        public void F7_Including_Nested()
        {
            var q = (
	            from c in db.Customers
	            where c.City == "London"
	            select c)
	            .Including(c => c.Orders.Including(o => o.OrderDetails));
        }
#endif
        [Test]
        public void F9_Project_AndContinue()
        {
            var q =
	            from c in db.Customers
	            where c.City == "London"
	            select new {Name = c.ContactName, c.Phone} into x
	            orderby x.Name
	            select x;
        }

        #endregion
    }
}

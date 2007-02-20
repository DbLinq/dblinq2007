using System;
using System.Collections.Generic;
using System.Text;
using System.Query;
using System.Xml.XLinq;
using System.Data.DLinq;
using NUnit.Framework;
using MySql.Data.MySqlClient;
#if ORACLE
using ClientCodeOra;
using xint = System.Int32;
#else
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
        
        MySqlConnection _conn;
        LinqTestDB db;
        
        public ReadTest_Complex()
        {
            db = new LinqTestDB(connStr);
        }
        public MySqlConnection Conn 
        { 
            get 
            { 
                if(_conn==null){ _conn=new MySqlConnection(connStr); _conn.Open(); }
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
        public void F4_SimpleGroup()
        {
            var q2 =
		        from c in db.Customers
		        where c.Country == "France"
		        group new {c.PostalCode, c.ContactName} by c.City into g
		        select new {g.Key,g};
        }

        [Test]
        public void F5_ExplicitJoin()
        {
            var q =
	            from c in db.Customers
	            join o in db.Orders on c.CustomerID equals o.CustomerID
	            where c.City == "London"
	            select o;
        }

        [Test]
        public void F6_IncludingClause()
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
        public void F8_Project_AndContinue()
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

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
    public class ReadTest
    {
#if ORACLE
        const string connStr = "server=localhost;user id=system; password=linq2";
#else
        const string connStr = "server=localhost;user id=LinqUser; password=linq2; database=LinqTestDB";
        
        MySqlConnection _conn;
        public MySqlConnection Conn 
        { 
            get 
            { 
                if(_conn==null){ _conn=new MySqlConnection(connStr); _conn.Open(); }
                return _conn;
            }
        }
#endif

        #region Tests 'A' check for DB being ready

#if ORACLE
#else

        /// <summary>
        /// in NUnit, tests are executed in alpha order.
        /// We want to start by checking access to DB.
        /// </summary>
        [Test]
        public void A1_PingDatabase()
        {
            bool pingOK = Conn.Ping(); //Schildkroete - Ping throws NullRef if conn is not open
            Assert.IsTrue(pingOK,"Pinging database at "+connStr);
        }

        [Test]
        public void A2_ProductsTableHasEntries()
        {
            string sql = "SELECT count(*) FROM linqtestdb.Products";
            using(MySqlCommand cmd = new MySqlCommand(sql,Conn))
            {
                object oResult = cmd.ExecuteScalar();
                Assert.IsNotNull("Expecting count of Products, instead got null. (sql="+sql+")");
                Assert.IsInstanceOfType(typeof(Int64),oResult,"Expecting 'int' result from query "+sql+", instead got type "+oResult.GetType());
                long iResult = (long)oResult;
                Assert.Greater((int)iResult,0,"Expecting some rows in Products table, got:"+iResult+" (SQL:"+sql+")");
            }
        }

        [Test]
        public void A3_ProductsTableHasPen()
        {
            string sql = @"SELECT count(*) FROM linqtestdb.Products WHERE ProductName='Pen'";
            using(MySqlCommand cmd = new MySqlCommand(sql,Conn))
            {
                object oResult = cmd.ExecuteScalar();
                Assert.IsNotNull("Expecting count of Products-Pens, instead got null. (sql="+sql+")");
                Assert.IsInstanceOfType(typeof(Int64),oResult,"Expecting 'int' result from query "+sql+", instead got type "+oResult.GetType());
                long iResult = (long)oResult;
                Assert.AreEqual(iResult,1L,"Expecting one Pen in Products table, got:"+iResult+" (SQL:"+sql+")");
            }
        }
#endif

        [Test]
        public void A4_SelectSingleCustomer()
        {
            LinqTestDB db = new LinqTestDB(connStr);
            // Query for a specific customer
            var cust = db.Customers.Single(c => c.CompanyName == "airbus");
            Assert.IsNotNull(cust,"Expected one customer 'airbus'");
        }

        #endregion

        //TODO: group B, which checks AllTypes

        #region Tests 'C' do plain select - no aggregation
        [Test]
        public void C1_SelectProducts()
        {
            LinqTestDB db = new LinqTestDB(connStr);
            var q = from p in db.Products select p;
            List<Product> products = q.ToList();
            int productCount = products.Count;
            Assert.Greater(productCount,0,"Expected some products, got none");
        }

        [Test]
        public void C2_SelectPenId()
        {
            LinqTestDB db = new LinqTestDB(connStr);
            var q = from p in db.Products where p.ProductName=="Pen" select p.ProductID;
            List<xint> productIDs = q.ToList();
            int productCount = productIDs.Count;
            Assert.AreEqual(productCount,1,"Expected one pen, got count="+productCount);
        }

        [Test]
        public void C3_SelectPenIdName()
        {
            LinqTestDB db = new LinqTestDB(connStr);
            var q = from p in db.Products where p.ProductName=="Pen" 
                        select new{ProductId=p.ProductID,Name=p.ProductName};
            int count = 0;
            string penName;
            foreach(var v in q)
            {
                penName = v.Name;
                count++;
            }
            Assert.AreEqual(count,1,"Expected one pen, got count="+count);
        }

        #endregion

        #region region D - select first or last - calls IQueryable.Execute instead of GetEnumerator
        [Test]
        public void D1_SelectFirstPenID()
        {
            LinqTestDB db = new LinqTestDB(connStr);
            var q = from p in db.Products where p.ProductName=="Pen" select p.ProductID;
            xint productID = q.First();
            Assert.Greater(productID,0,"Expected penID>0, got "+productID);
        }

        [Test]
        public void D2_SelectFirstPen()
        {
            LinqTestDB db = new LinqTestDB(connStr);
            var q = from p in db.Products where p.ProductName=="Pen" select p;
            Product pen = q.First();
            Assert.IsNotNull(pen,"Expected non-null Product");
        }

        [Test]
        public void D3_SelectLastPenID()
        {
            LinqTestDB db = new LinqTestDB(connStr);
            var q = from p in db.Products where p.ProductName=="Pen" select p.ProductID;
            xint productID = q.Last();
            Assert.Greater(productID,0,"Expected penID>0, got "+productID);
        }

        [Test]
        public void D4_SelectProducts_OrderByName()
        {
            LinqTestDB db = new LinqTestDB(connStr);
            var q = from p in db.Products orderby p.ProductName select p;
            string prevProductName = null;
            foreach(Product p in q)
            {
                if(prevProductName!=null)
                {
                    int compareNames = prevProductName.CompareTo(p.ProductName);
                    Assert.Less(compareNames,0,"When ordering by names, expected "+prevProductName+" to come after "+p.ProductName);
                }
                prevProductName = p.ProductName;
            }
            //Assert.Greater(productID,0,"Expected penID>0, got "+productID);
        }

        [Test]
        public void D5_SelectOrdersForProduct()
        {
            LinqTestDB db = new LinqTestDB(connStr);
            //var q = from p in db.Products where "Pen"==p.ProductName select p.Order;
            //List<Order> penOrders = q.ToList();
            //Assert.Greater(penOrders.Count,0,"Expected some orders for product 'Pen'");

            var q =
	            from o in db.Orders
	            where o.Customer.City == "London"
	            select new { c = o.Customer, o };

            Assert.Greater(q.ToList().Count,0,"Expected some orders for London customers");
        }

        [Test]
        public void D6_OrdersFromLondon()
        {
            LinqTestDB db = new LinqTestDB(connStr);
            var q =
	            from o in db.Orders
	            where o.Customer.City == "London"
	            select new { c = o.Customer, o };

            Assert.Greater(q.ToList().Count,0,"Expected some orders for London customers");
        }
        [Test]
        public void D7_OrdersFromLondon_Alt()
        {
            LinqTestDB db = new LinqTestDB(connStr);
            var q =
	            from c in db.Customers
	            from o in c.Orders
	            where c.City == "London"
	            select new { c, o };

            Assert.Greater(q.ToList().Count,0,"Expected some orders for London customers");
        }

        #endregion

    }
}

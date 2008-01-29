using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NUnit.Framework;
using nwind;

#if ORACLE
using xint = System.Int32;
#elif POSTGRES
using xint = System.Int32;
#else
using xint = System.UInt32;
#endif

namespace Test_NUnit
{
    [TestFixture]
    public class ReadTest : TestBase
    {
        #region Tests 'A' check for DB being ready


        /// <summary>
        /// in NUnit, tests are executed in alpha order.
        /// We want to start by checking access to DB.
        /// </summary>
        [Test]
        public void A1_PingDatabase()
        {
            Northwind db = CreateDB();
            bool pingOK = db.DatabaseExists();
            //bool pingOK = Conn.Ping(); //Schildkroete - Ping throws NullRef if conn is not open
            Assert.IsTrue(pingOK, "Pinging database");
        }


        [Test]
        public void A2_ProductsTableHasEntries()
        {
            Northwind db = CreateDB();
            //string sql = "SELECT count(*) FROM Northwind.Products";
            int result = db.ExecuteCommand("SELECT count(*) FROM Products");
            //long iResult = base.ExecuteScalar(sql);
            Assert.Greater(result, 0, "Expecting some rows in Products table, got:" + result);
        }

        [Test]
        public void A3_ProductsTableHasPen()
        {
            Northwind db = CreateDB();
            //string sql = @"SELECT count(*) FROM linqtestdb.Products WHERE ProductName='Pen'";
            string sql = @"SELECT count(*) FROM Products WHERE ProductName='Pen'";
            long iResult = db.ExecuteCommand(sql);
            //long iResult = base.ExecuteScalar(sql);
            Assert.AreEqual(iResult, 1L, "Expecting one Pen in Products table, got:" + iResult + " (SQL:" + sql + ")");
        }

        [Test]
        public void A4_SelectSingleCustomer()
        {
            Northwind db = CreateDB();

            // Query for a specific customer
            var cust = db.Customers.Single(c => c.CompanyName == "airbus");
            Assert.IsNotNull(cust, "Expected one customer 'airbus'");
        }

        [Test]
        public void A5_SelectSingleOrDefault()
        {
            Northwind db = CreateDB();

            // Query for a specific customer
            var cust = db.Customers.SingleOrDefault(c => c.CompanyName == "airbus");
            Assert.IsNotNull(cust, "Expected one customer 'airbus'");
        }

        #endregion

        //TODO: group B, which checks AllTypes

        #region Tests 'C' do plain select - no aggregation
        [Test]
        public void C1_SelectProducts()
        {
            Northwind db = CreateDB();

            var q = from p in db.Products select p;
            List<Product> products = q.ToList();
            int productCount = products.Count;
            Assert.Greater(productCount, 0, "Expected some products, got none");
        }

        [Test]
        public void C2_SelectPenId()
        {
            Northwind db = CreateDB();

            var q = from p in db.Products where p.ProductName == "Pen" select p.ProductID;
            List<int> productIDs = q.ToList();
            int productCount = productIDs.Count;
            Assert.AreEqual(productCount, 1, "Expected one pen, got count=" + productCount);
        }

        [Test]
        public void C3_SelectPenIdName()
        {
            Northwind db = CreateDB();

            var q = from p in db.Products
                    where p.ProductName == "Pen"
                    select new { ProductId = p.ProductID, Name = p.ProductName };
            int count = 0;
            //string penName;
            foreach (var v in q)
            {
                Assert.AreEqual(v.Name, "Pen", "Expected ProductName='Pen'");
                count++;
            }
            Assert.AreEqual(count, 1, "Expected one pen, got count=" + count);
        }

        #endregion

        #region region D - select first or last - calls IQueryable.Execute instead of GetEnumerator
        [Test]
        public void D01_SelectFirstPenID()
        {
            Northwind db = CreateDB();

            var q = from p in db.Products where p.ProductName == "Pen" select p.ProductID;
            int productID = q.First();
            Assert.Greater(productID, 0, "Expected penID>0, got " + productID);
        }

        [Test]
        public void D02_SelectFirstPen()
        {
            Northwind db = CreateDB();

            var q = from p in db.Products where p.ProductName == "Pen" select p;
            Product pen = q.First();
            Assert.IsNotNull(pen, "Expected non-null Product");
        }

        [Test]
        public void D03_SelectLastPenID()
        {
            Northwind db = CreateDB();

            var q = from p in db.Products where p.ProductName == "Pen" select p.ProductID;
            int productID = q.Last();
            Assert.Greater(productID, 0, "Expected penID>0, got " + productID);
        }

        [Test]
        public void D04_SelectProducts_OrderByName()
        {
            Northwind db = CreateDB();

            var q = from p in db.Products orderby p.ProductName select p;
            string prevProductName = null;
            foreach (Product p in q)
            {
                if (prevProductName != null)
                {
                    //int compareNames = prevProductName.CompareTo(p.ProductName);
                    int compareNames = string.Compare(prevProductName, p.ProductName, stringComparisonType);
                    Assert.Less(compareNames, 0, "When ordering by names, expected " + prevProductName + " to come after " + p.ProductName);
                }
                prevProductName = p.ProductName;
            }
            //Assert.Greater(productID,0,"Expected penID>0, got "+productID);
        }

        [Test]
        public void D05_SelectOrdersForProduct()
        {
            Northwind db = CreateDB();
            //var q = from p in db.Products where "Pen"==p.ProductName select p.Order;
            //List<Order> penOrders = q.ToList();
            //Assert.Greater(penOrders.Count,0,"Expected some orders for product 'Pen'");

            var q =
                from o in db.Orders
                where o.Customer.City == "London"
                select new { c = o.Customer, o };

            var list1 = q.ToList();
            foreach (var co in list1)
            {
                Assert.IsNotNull(co.c, "Expected non-null customer");
                Assert.IsNotNull(co.c.City, "Expected non-null customer city");
                Assert.IsNotNull(co.o, "Expected non-null order");
            }
            Assert.Greater(list1.Count, 0, "Expected some orders for London customers");
        }

        [Test]
        public void D06_OrdersFromLondon()
        {
            Northwind db = CreateDB();
            var q =
                from o in db.Orders
                where o.Customer.City == "London"
                select new { c = o.Customer, o };

            var list1 = q.ToList();
            foreach (var co in list1)
            {
                Assert.IsNotNull(co.c, "Expected non-null customer");
                Assert.IsNotNull(co.o, "Expected non-null order");
            }
            Assert.Greater(list1.Count, 0, "Expected some orders for London customers");
        }
        [Test]
        public void D07_OrdersFromLondon_Alt()
        {
            //this is a "SelectMany" query:
            Northwind db = CreateDB();

            var q =
                from c in db.Customers
                from o in c.Orders
                where c.City == "London"
                select new { c, o };

            Assert.Greater(q.ToList().Count, 0, "Expected some orders for London customers");
        }

        [Test]
        public void D08_Products_Take5()
        {
            Northwind db = CreateDB();
            var q = (from p in db.Products select p).Take(5);
            List<Product> prods = q.ToList();
            Assert.AreEqual(5, prods.Count, "Expected five products");
        }

        [Test]
        public void D09_Products_LetterP_Take5()
        {
            Northwind db = CreateDB();

            //var q = (from p in db.Products where p.ProductName.Contains("p") select p).Take(5);
            var q = db.Products.Where(p => p.ProductName.Contains("p")).Take(5);
            List<Product> prods = q.ToList();
#if POSTGRES
            int expectedCount = 0; //Only 'Toilet Paper'
#else
            int expectedCount = 2; //Oracle, Mysql: 'Toilet Paper' and 'iPod'
#endif
            Assert.Greater(prods.Count, expectedCount, "Expected couple of products with letter 'p'");
        }

        [Test]
        public void D10_Products_LetterP_Desc()
        {
            Northwind db = CreateDB();

            var q = (from p in db.Products
                     where p.ProductName.Contains("P")
                     orderby p.ProductID descending
                     select p
            ).Take(5);
            //var q = db.Products.Where( p=>p.ProductName.Contains("p")).Take(5);
            List<Product> prods = q.ToList();
            Assert.Greater(prods.Count, 2, "Expected couple of products with letter 'p'");

            int prodID0 = prods[0].ProductID;
            int prodID1 = prods[1].ProductID;
            Assert.Greater(prodID0, prodID1, "Sorting is broken");
        }

        [Test]
        public void D11_Products_DoubleWhere()
        {
            Northwind db = CreateDB();
            var q1 = db.Products.Where(p => p.ProductID > 1).Where(q => q.ProductID < 10);
            int count1 = q1.Count();
        }
        #endregion

        [Test]
        public void E1_ConnectionOpenTest()
        {
            Northwind db = CreateDB(System.Data.ConnectionState.Open);
            Product p1 = db.Products.Single(p => p.ProductID == 1);
            Assert.IsTrue(p1.ProductID == 1);
        }

        [Test]
        public void E2_ConnectionClosedTest()
        {
            Northwind db = CreateDB(System.Data.ConnectionState.Closed);
            Product p1 = db.Products.Single(p => p.ProductID == 1);
            Assert.IsTrue(p1.ProductID == 1);
        }

    }
}

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
    public class ReadTest
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
                if(_conn==null){ _conn=new XSqlConnection(connStr); _conn.Open(); }
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

        #region Tests 'A' check for DB being ready

#if ORACLE
#else

#if MYSQL
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
#endif

        [Test]
        public void A2_ProductsTableHasEntries()
        {
            //string sql = "SELECT count(*) FROM LinqTestDB.Products";
            string sql = "SELECT count(*) FROM Products";
            using(XSqlCommand cmd = new XSqlCommand(sql,Conn))
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
            //string sql = @"SELECT count(*) FROM linqtestdb.Products WHERE ProductName='Pen'";
            string sql = @"SELECT count(*) FROM Products WHERE ProductName='Pen'";
            using(XSqlCommand cmd = new XSqlCommand(sql,Conn))
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
                        select new { ProductId=p.ProductID, Name=p.ProductName };
            int count = 0;
            //string penName;
            foreach(var v in q)
            {
                Assert.AreEqual(v.Name, "Pen", "Expected ProductName='Pen'");
                count++;
            }
            Assert.AreEqual(count,1,"Expected one pen, got count="+count);
        }

        #endregion

        #region region D - select first or last - calls IQueryable.Execute instead of GetEnumerator
        [Test]
        public void D01_SelectFirstPenID()
        {
            LinqTestDB db = new LinqTestDB(connStr);
            var q = from p in db.Products where p.ProductName=="Pen" select p.ProductID;
            xint productID = q.First();
            Assert.Greater(productID,0,"Expected penID>0, got "+productID);
        }

        [Test]
        public void D02_SelectFirstPen()
        {
            LinqTestDB db = new LinqTestDB(connStr);
            var q = from p in db.Products where p.ProductName=="Pen" select p;
            Product pen = q.First();
            Assert.IsNotNull(pen,"Expected non-null Product");
        }

        [Test]
        public void D03_SelectLastPenID()
        {
            LinqTestDB db = new LinqTestDB(connStr);
            var q = from p in db.Products where p.ProductName=="Pen" select p.ProductID;
            xint productID = q.Last();
            Assert.Greater(productID,0,"Expected penID>0, got "+productID);
        }

        [Test]
        public void D04_SelectProducts_OrderByName()
        {
            LinqTestDB db = new LinqTestDB(connStr);
            var q = from p in db.Products orderby p.ProductName select p;
            string prevProductName = null;
            foreach(Product p in q)
            {
                if(prevProductName!=null)
                {
                    //int compareNames = prevProductName.CompareTo(p.ProductName);
                    int compareNames = string.Compare( prevProductName, p.ProductName, stringComparisonType);
                    Assert.Less(compareNames,0,"When ordering by names, expected "+prevProductName+" to come after "+p.ProductName);
                }
                prevProductName = p.ProductName;
            }
            //Assert.Greater(productID,0,"Expected penID>0, got "+productID);
        }

        [Test]
        public void D05_SelectOrdersForProduct()
        {
            LinqTestDB db = new LinqTestDB(connStr);
            //var q = from p in db.Products where "Pen"==p.ProductName select p.Order;
            //List<Order> penOrders = q.ToList();
            //Assert.Greater(penOrders.Count,0,"Expected some orders for product 'Pen'");

            var q =
	            from o in db.Orders
	            where o.Customer.City == "London"
	            select new { c = o.Customer, o };

            var list1 = q.ToList();
            foreach(var co in list1)
            {
                Assert.IsNotNull(co.c,"Expected non-null customer");
                Assert.IsNotNull(co.c.City,"Expected non-null customer city");
                Assert.IsNotNull(co.o,"Expected non-null order");
            }
            Assert.Greater(list1.Count,0,"Expected some orders for London customers");
        }

        [Test]
        public void D06_OrdersFromLondon()
        {
            LinqTestDB db = new LinqTestDB(connStr);
            var q =
	            from o in db.Orders
	            where o.Customer.City == "London"
	            select new { c = o.Customer, o };

            var list1 = q.ToList();
            foreach(var co in list1)
            {
                Assert.IsNotNull(co.c,"Expected non-null customer");
                Assert.IsNotNull(co.o,"Expected non-null order");
            }
            Assert.Greater(list1.Count,0,"Expected some orders for London customers");
        }
        [Test]
        public void D07_OrdersFromLondon_Alt()
        {
            Func<int,int> func1 = i => i+1;
            Console.WriteLine("type="+func1.GetType());
            //this is a SelectMany query:
            LinqTestDB db = new LinqTestDB(connStr);
            var q =
	            from c in db.Customers
	            from o in c.Orders
	            where c.City == "London"
	            select new { c, o };

            Assert.Greater(q.ToList().Count,0,"Expected some orders for London customers");
        }

        [Test]
        public void D08_Products_Take5()
        {
            LinqTestDB db = new LinqTestDB(connStr);
            var q = (from p in db.Products select p).Take(5);
            List<Product> prods = q.ToList(); 
            Assert.AreEqual(5,prods.Count,"Expected five products");
        }

        [Test]
        public void D09_Products_LetterP_Take5()
        {
            LinqTestDB db = new LinqTestDB(connStr);

            //var q = (from p in db.Products where p.ProductName.Contains("p") select p).Take(5);
            var q = db.Products.Where( p=>p.ProductName.Contains("p")).Take(5);
            List<Product> prods = q.ToList();
#if POSTGRES
            int expectedCount = 0; //Only 'Toilet Paper'
#else
            int expectedCount = 2; //Oracle, Mysql: 'Toilet Paper' and 'iPod'
#endif
            Assert.Greater(prods.Count,expectedCount,"Expected couple of products with letter 'p'");
        }

        [Test]
        public void D10_Products_LetterP_Desc()
        {
            LinqTestDB db = new LinqTestDB(connStr);

            var q = (from p in db.Products where p.ProductName.Contains("p") 
                        orderby p.ProductID descending
                        select p 
            ).Take(5);
            //var q = db.Products.Where( p=>p.ProductName.Contains("p")).Take(5);
            List<Product> prods = q.ToList();
            Assert.Greater(prods.Count,2,"Expected couple of products with letter 'p'");

            xint prodID0 = prods[0].ProductID;
            xint prodID1 = prods[1].ProductID;
            Assert.Greater(prodID0,prodID1,"Sorting is broken");
        }

        [Test]
        public void D11_Products_DoubleWhere()
        {
            LinqTestDB db = new LinqTestDB(connStr);
            var q1 = db.Products.Where(p=>p.ProductID>1).Where(p=>p.ProductID<10);
            int count1 = q1.Count();
        }
        #endregion

    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
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
    public class ReadTest_Complex : TestBase
    {
        Northwind db;

        public ReadTest_Complex()
        {
            db = CreateDB();
            db.Log = Console.Out;
        }


        #region Tests 'F' work on aggregation
        [Test]
        public void F1_ProductCount()
        {
            var q = from p in db.Products select p;
            int productCount = q.Count();
            Assert.Greater(productCount, 0, "Expected non-zero product count");
        }

        [Test]
        public void F2_ProductCount_Projected()
        {
            var q = from p in db.Products select p.ProductID;
            int productCount = q.Count();
            Assert.Greater(productCount, 0, "Expected non-zero product count");
            Console.WriteLine();
        }
        [Test]
        public void F2_ProductCount_Clause()
        {
            var q = from p in db.Products select p.ProductID;
            int productCount = q.Count(i => i < 3);
            Assert.Greater(productCount, 0, "Expected non-zero product count");
            Assert.IsTrue(productCount < 4, "Expected product count < 3");
        }

        [Test]
        public void F3_MaxProductId()
        {
            var q = from p in db.Products select p.ProductID;
            int maxID = q.Max();
            Assert.Greater(maxID, 0, "Expected non-zero product count");
        }

        [Test]
        public void F4_MinProductId()
        {
            var q = from p in db.Products select p.ProductID;
            int minID = q.Min();
            Assert.Greater(minID, 0, "Expected non-zero product count");
        }

#if !ORACLE // picrap: this test causes an internal buffer overflow when marshaling with oracle win32 driver

        [Test]
        public void F5_AvgProductId()
        {
            var q = from p in db.Products select p.ProductID;
            double avg = q.Average();
            Assert.Greater(avg, 0, "Expected non-zero productID average");
        }

#endif

        [Test]
        public void F7_ExplicitJoin()
        {
            //a nice and light nonsense join:
            //bring in rows such as {Pen,AIRBU}
            var q =
                from p in db.Products
                join o in db.Orders on p.ProductID equals o.OrderID
                select new { p.ProductName, o.CustomerID };

            int rowCount = 0;
            foreach (var v in q)
            {
                rowCount++;
                Assert.IsTrue(v.ProductName != null);
                Assert.IsTrue(v.CustomerID != null);
            }
            Assert.IsTrue(rowCount > 2);
        }

        [Test]
        public void F7b_ExplicitJoin()
        {
            var q =
                from c in db.Customers
                join o in db.Orders on c.CustomerID equals o.CustomerID
                where c.City == "London"
                select o;
        }

#if INCLUDING_CLAUSE
        //Including() clause discontinued in Studio Orcas?
        [Test]
        public void F8_IncludingClause()
        {
            var q = (
	            from c in db.Customers
	            where c.City == "London"
	            select c)
	            .Including(c => c.Orders);
        }

        [Test]
        public void F8_Including_Nested()
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
                select new { Name = c.ContactName, c.Phone } into x
                orderby x.Name
                select x;
        }

        [Test]
        public void F10_DistinctCity()
        {
            var q1 = from c in db.Customers select c.City;
            var q2 = q1.Distinct();

            int numLondon = 0;
            foreach (string city in q2)
            {
                if (city == "London") { numLondon++; }
            }
            Assert.AreEqual(1, numLondon, "Expected to see London once");
        }

        [Test]
        public void F11_ConcatString()
        {
            var q4 = from p in db.Products select p.ProductName + p.ProductID;
            //var q4 = from p in db.Products select p.ProductID;
            var q5 = q4.ToList();
            Assert.Greater(q5.Count, 2, "Expected to see some concat strings");
            foreach (string s0 in q5)
            {
                bool startWithLetter = Char.IsLetter(s0[0]);
                bool endsWithDigit = Char.IsDigit(s0[s0.Length - 1]);
                Assert.IsTrue(startWithLetter && endsWithDigit, "String must start with letter and end with digit");
            }
        }

        [Test]
        public void F12_ConcatString_2()
        {
            var q4 = from p in db.Products
                     where (p.ProductName + p.ProductID).Contains("e")
                     select p.ProductName;
            //select p.ProductName+p.ProductID;
            //var q4 = from p in db.Products select p.ProductID;
            var q5 = q4.ToList();
            //Assert.Greater( q5.Count, 2, "Expected to see some concat strings");
            //foreach(string s0 in q5)
            //{
            //    bool startWithLetter = Char.IsLetter(s0[0]);
            //    bool endsWithDigit = Char.IsDigit(s0[s0.Length-1]);
            //    Assert.IsTrue(startWithLetter && endsWithDigit, "String must start with letter and end with digit");
            //}
        }
        #endregion

        [Test]
        public void F13_NewCustomer()
        {
            Northwind db = CreateDB();
            IQueryable<Customer> q = (from c in db.Customers
                                      select
                                      new Customer
                                      {
                                          CustomerID = c.CustomerID
                                      });
            Assert.Greater(q.ToList().Count(), 0, "Expected list");
        }

        /// <summary>
        /// the following three tests are from Jahmani's page
        /// LinqToSQL: Comprehensive Support for SQLite, MS Access, SQServer2000/2005
        /// http://www.codeproject.com/KB/linq/linqToSql_7.aspx?msg=2428251
        /// </summary>
        [Test(Description = "list of customers who have place orders that have all been shipped to the customers city.")]
        public void O1_OperatorAll()
        {
            var q = from c in db.Customers
                    where (from o in c.Orders
                           select o).All(o => o.ShipCity == c.City)
                    select new { c.CustomerID, c.ContactName };
            var list = q.ToList();
        }

        [Test(Description = "list of customers who have placed no orders")]
        public void O2_OperatorAny()
        {
            var q = from customer in db.Customers
                    where !customer.Orders.Any()
                    select new { customer.CustomerID, customer.ContactName };
            //var q = from customer in db.Customers
            //        where customer.Orders.Count() == 0
            //        select new { customer.CustomerID, customer.ContactName };
            var list = q.ToList();
        }

        [Test(Description = "provide a list of customers and employees who live in London.")]
        public void O3_OperatorUnion()
        {
            var q = (from c in db.Customers.Where(d => d.City == "London")
                     select new { ContactName = c.ContactName })
              .Union(from e in db.Employees.Where(f => f.City == "London")
                     select new { ContactName = e.LastName });
            var list = q.ToList();
        }

        [Test]
        public void O4_OperatorContains()
        {
            int[] ids = new int[] { 1, 2, 3 };
            Northwind db = CreateDB();

            //var q = from p in db.Products select p.ProductID;
            //int productCount = q.Count();

            var products = from p in db.Products
                           where ids.Contains(p.ProductID)
                           select p;

            Assert.AreEqual(3, products.Count());

        } 
    }
}

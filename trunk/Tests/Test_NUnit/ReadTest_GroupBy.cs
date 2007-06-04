////////////////////////////////////////////////////////////////////
//Initial author: Jiri George Moudry, 2006.
//License: LGPL. (Visit http://www.gnu.org)
//Commercial code may call into this library, if it's in a different module (DLL)
////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

#if LINQ_PREVIEW_2006
//Visual Studio 2005 with Linq Preview May 2006 - can run on Win2000
using System.Query;
using System.Expressions;
#else
//Visual Studio Orcas - requires WinXP
using System.Linq;
using System.Linq.Expressions;
#endif

using NUnit.Framework;
#if ORACLE
using ClientCodeOra;
using xint = System.Int32;
#elif POSTGRES
using Client2.user;
using xint = System.UInt32;
#else
using MySql.Data.MySqlClient;
using Client2.user;
using xint = System.UInt32;
#endif

namespace Test_NUnit
{
    [TestFixture]
    public class ReadTest_GroupBy
    {
#if ORACLE
        const string connStr = "server=localhost;user id=system; password=linq2";
#else
        const string connStr = "server=localhost;user id=LinqUser; password=linq2; database=LinqTestDB";
        
#endif

        [Test]
        public void G01_SimpleGroup()
        {
            //Note: this SQL is allowed in Mysql but illegal on Postgres 
            //(PostgreSql ERROR: column "c$.customerid" must appear in the GROUP BY clause or be used in an aggregate function - SQL state: 42803)
            //"SELECT City, customerid FROM customer GROUP BY City"
            LinqTestDB db = new LinqTestDB(connStr);
            var q2 = db.Customers.GroupBy( c=>c.City );

            foreach(var g in q2){
                int entryCount = 0;
                foreach(var c in g){
                    Assert.IsTrue(c.City!=null,"City must be non-null");
                    Assert.IsTrue(c.CustomerID>0,"CustomerID must be > 0");
                    entryCount++;
                }
                Assert.IsTrue(entryCount>0, "Must have some entries in group");
            }
        }

        [Test]
        public void G01_SimpleGroup_Count()
        {
            //Note: this SQL is allowed in Mysql but illegal on Postgres 
            //(PostgreSql ERROR: column "c$.customerid" must appear in the GROUP BY clause or be used in an aggregate function - SQL state: 42803)
            //"SELECT City, customerid FROM customer GROUP BY City"
            LinqTestDB db = new LinqTestDB(connStr);
            var q2 = db.Customers.GroupBy( c=>c.City ).Select( g=>new{g.Key, Count=g.Count()} );

            int rowCount=0;
            foreach(var g in q2){
                rowCount++;
                Assert.IsTrue(g.Count>0, "Must have Count");
                Assert.IsTrue(g.Key!=null, "Must have City");
            }
            Assert.IsTrue(rowCount>0,"Must have some rows");
        }

        [Test]
        public void G02_SimpleGroup_First()
        {
            //Note: this SQL is allowed in Mysql but illegal on Postgres 
            //(PostgreSql ERROR: column "c$.customerid" must appear in the GROUP BY clause or be used in an aggregate function - SQL state: 42803)
            //"SELECT City, customerid FROM customer GROUP BY City"
            LinqTestDB db = new LinqTestDB(connStr);
            var q2 = db.Customers.GroupBy( c=>c.City );
            var q3 = q2.First();

            Assert.IsTrue(q3!=null && q3.Key!=null,"Must have result with Key");
            foreach(var c in q3){
                Assert.IsTrue(c.City!=null,"City must be non-null");
            }
        }

        [Test]
        public void G03_SimpleGroup_WithSelector()
        {
            //Note: this SQL is allowed in Mysql but illegal on Postgres 
            //(PostgreSql ERROR: column "c$.customerid" must appear in the GROUP BY clause or be used in an aggregate function - SQL state: 42803)
            //"SELECT City, customerid FROM customer GROUP BY City"
            LinqTestDB db = new LinqTestDB(connStr);
            var q2 = db.Customers.GroupBy( c=>c.City, c=>new {c.City,c.CustomerID} );

            foreach(var g in q2){
                int entryCount = 0;
                foreach(var c in g){
                    Assert.IsTrue(c.City!=null,"City must be non-null");
                    entryCount++;
                }
                Assert.IsTrue(entryCount>0, "Must have some entries in group");
            }
        }

        [Test]
        public void G04_SimpleGroup_WithSelector()
        {
            //Note: this SQL is allowed in Mysql but illegal on Postgres 
            //(PostgreSql ERROR: column "c$.customerid" must appear in the GROUP BY clause or be used in an aggregate function - SQL state: 42803)
            //"SELECT City, customerid FROM customer GROUP BY City"
            LinqTestDB db = new LinqTestDB(connStr);
            var q2 = db.Customers.GroupBy( c=>c.City, c=>c.CustomerID );

            foreach(var g in q2){
                int entryCount = 0;
                foreach(var c in g){
                    Assert.IsTrue(c>0,"CustomerID must be >0");
                    entryCount++;
                }
                Assert.IsTrue(entryCount>0, "Must have some entries in group");
            }
        }

        [Test]
        public void G05_Group_Into()
        {
            LinqTestDB db = new LinqTestDB(connStr);
            var q2 =
                from c in db.Customers
                //where c.Country == "France"
                group new {c.PostalCode, c.ContactName} by c.City into g
                select g;
                //select new {g.Key.Length, g};
                //select new {42,g};
            var q3 = q2.ToList();
        }

        //[Test]
        //public void G01_SimpleGroup()
        //{
        //    Customer c0 = new Customer { City="London", ContactName="Bob", PostalCode="E14" };
        //    Customer c1 = new Customer { City="London", ContactName="Qag", PostalCode="SG2" };
        //    Customer[] customers = new Customer[]{ c0,c1 };
        //    var q2 =
        //        from c in customers
        //        group new {c.PostalCode, c.ContactName} by c.City into g
        //        select new {g.Key, g};
        //    //return q2;
        //    var q3 = q2.ToList();
        //    foreach(var v in q3)
        //    {
        //        Console.WriteLine("V="+v);
        //    }
        //}

        [Test]
        public void G06_OrderCountByCustomerID()
        {
            LinqTestDB db = new LinqTestDB(connStr);

            var q2 = from o in db.Orders
                    group o by o.CustomerID into g
                    //where g.Count()>1
                    select new { g.Key , OrderCount = g.Count() };

            var lst = q2.ToList();
            Assert.Greater(lst.Count,0,"Expected some grouped order results");
            var result0 = lst[0];
            Assert.Greater(result0.Key, 0,"Key must be > 0");
            Assert.Greater(result0.OrderCount, 0,"Count must be > 0");
                    //select new { g.Key , SumPerCustomer = g.Sum(o2=>o2.OrderID) };
        }

        [Test]
        public void G07_OrderCountByCustomerID_Where()
        {
            LinqTestDB db = new LinqTestDB(connStr);
            var q2 = from o in db.Orders
                    group o by o.CustomerID into g
                    where g.Count()>1
                    select new { g.Key , OrderCount = g.Count() };
        
            var lst = q2.ToList();
            Assert.Greater(lst.Count,0,"Expected some grouped order results");
            var result0 = lst[0];
            Assert.Greater(result0.Key, 0,"Key must be > 0");
            Assert.Greater(result0.OrderCount, 0,"Count must be > 0");
                    //select new { g.Key , SumPerCustomer = g.Sum(o2=>o2.OrderID) };
        }

        [Test]
        public void G08_OrderSumByCustomerID()
        {
            LinqTestDB db = new LinqTestDB(connStr);
            var q2 = from o in db.Orders
                    group o by o.CustomerID into g
                    //where g.Count()>1
                    select new { g.Key , OrderSum = g.Sum(o=>o.OrderID) };
            var lst = q2.ToList();
            Assert.Greater(lst.Count,0,"Expected some grouped order results");
            foreach(var result in lst){
                Console.WriteLine("  Result: custID="+result.Key+" sum="+result.OrderSum);
                Assert.Greater(result.Key, 0,"Key must be > 0");
                Assert.Greater(result.OrderSum, 0f,"OrderSum must be > 0");
            }
                    //select new { g.Key , SumPerCustomer = g.Sum(o2=>o2.OrderID) };
        }


    }
}

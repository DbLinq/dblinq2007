#region MIT license
////////////////////////////////////////////////////////////////////
// MIT license:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//
// Authors:
//        Jiri George Moudry
////////////////////////////////////////////////////////////////////
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using nwind;
using Test_NUnit;

#if MYSQL
    namespace Test_NUnit_MySql
#elif ORACLE
    namespace Test_NUnit_Oracle
#elif POSTGRES
    namespace Test_NUnit_PostgreSql
#elif SQLITE
    namespace Test_NUnit_Sqlite
#elif INGRES
    namespace Test_NUnit_Ingres
#else
    #error unknown target
#endif
{
    [TestFixture]
    public class ReadTest_GroupBy : TestBase
    {


        [Test]
        public void G01_SimpleGroup_Count()
        {
            Northwind db = base.CreateDB();

            var q2 = db.Customers.GroupBy(c => c.City)
                .Select(g => new { g.Key, Count = g.Count() });

            int rowCount = 0;
            foreach (var g in q2)
            {
                rowCount++;
                Assert.IsTrue(g.Count > 0, "Must have Count");
                Assert.IsTrue(g.Key != null, "Must have City");
            }
            Assert.IsTrue(rowCount > 0, "Must have some rows");
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void G02_SimpleGroup_First()
        {
            //Note: this SQL is allowed in Mysql but illegal on Postgres 
            //(PostgreSql ERROR: column "c$.customerid" must appear in the GROUP BY clause or be used in an aggregate function - SQL state: 42803)
            //"SELECT City, customerid FROM customer GROUP BY City"
            //that's why DbLinq disallows it
            Northwind db = base.CreateDB();
            var q2 = db.Customers.GroupBy(c => c.City);
            var q3 = q2.First();

            Assert.IsTrue(q3 != null && q3.Key != null, "Must have result with Key");
            foreach (var c in q3)
            {
                Assert.IsTrue(c.City != null, "City must be non-null");
            }
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void G03_SimpleGroup_WithSelector_Invalid()
        {
            //Note: this SQL is allowed in Mysql but illegal on Postgres 
            //(PostgreSql ERROR: column "c$.customerid" must appear in the GROUP BY clause or be used in an aggregate function - SQL state: 42803)
            //"SELECT City, customerid FROM customer GROUP BY City"
            Northwind db = base.CreateDB();

            var q2 = db.Customers.GroupBy(c => c.City, c => new { c.City, c.CustomerID });

            foreach (var g in q2)
            {
                int entryCount = 0;
                foreach (var c in g)
                {
                    Assert.IsTrue(c.City != null, "City must be non-null");
                    entryCount++;
                }
                Assert.IsTrue(entryCount > 0, "Must have some entries in group");
            }
        }

        [Test]
        public void G03_DoubleKey()
        {
            //Note: this SQL is allowed in Mysql but illegal on Postgres 
            //(PostgreSql ERROR: column "c$.customerid" must appear in the GROUP BY clause or be used in an aggregate function - SQL state: 42803)
            //"SELECT City, customerid FROM customer GROUP BY City"
            Northwind db = base.CreateDB();

            var q2 = from o in db.Orders
                     group o by new { o.CustomerID, o.EmployeeID } into g
                     select new { g.Key.CustomerID, g.Key.EmployeeID, Count = g.Count() };

            int entryCount = 0;
            foreach (var g in q2)
            {
                entryCount++;
                Assert.IsTrue(g.CustomerID != null, "Must have non-null customerID");
                Assert.IsTrue(g.EmployeeID > 0, "Must have >0 employeeID");
                Assert.IsTrue(g.Count >= 0, "Must have non-neg Count");
            }
            Assert.IsTrue(entryCount > 0, "Must have some entries in group");
        }


        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void G04_SimpleGroup_WithSelector()
        {
            //Note: this SQL is allowed in Mysql but illegal on Postgres 
            //(PostgreSql ERROR: column "c$.customerid" must appear in the GROUP BY clause or be used in an aggregate function - SQL state: 42803)
            //"SELECT City, customerid FROM customer GROUP BY City"
            Northwind db = base.CreateDB();
            var q2 = db.Customers.GroupBy(c => c.City, c => c.CustomerID);

            foreach (var g in q2)
            {
                int entryCount = 0;
                foreach (var c in g)
                {
                    Assert.IsTrue(c != null, "CustomerID must be non-null");
                    entryCount++;
                }
                Assert.IsTrue(entryCount > 0, "Must have some entries in group");
            }
        }

        [Test]
        public void G05_Group_Into()
        {
            Northwind db = base.CreateDB();
            var q2 =
                from c in db.Customers
                //where c.Country == "France"
                group new { c.PostalCode, c.ContactName } by c.City into g
                select g;
            var q3 = from g in q2 select new { FortyTwo = 42, g.Key, Count = g.Count() };
            //select new {g.Key.Length, g};
            //select new {42,g};

            int entryCount = 0;
            foreach (var g in q3)
            {
                Assert.IsTrue(g.FortyTwo == 42, "Forty42 must be there");
                Assert.IsTrue(g.Count > 0, "Positive count");
                entryCount++;
            }
            Assert.IsTrue(entryCount > 0, "Must have some entries in group");
        }


        [Test]
        public void G06_OrderCountByCustomerID()
        {
            Northwind db = base.CreateDB();

            var q2 = from o in db.Orders
                     group o by o.CustomerID into g
                     //where g.Count()>1
                     select new { g.Key, OrderCount = g.Count() };

            var lst = q2.ToList();
            Assert.Greater(lst.Count, 0, "Expected some grouped order results");
            var result0 = lst[0];
            Assert.IsTrue(result0.Key != null, "Key must be non-null");
            Assert.Greater(result0.OrderCount, 0, "Count must be > 0");
            //select new { g.Key , SumPerCustomer = g.Sum(o2=>o2.OrderID) };
        }

        [Test]
        public void G07_OrderCountByCustomerID_Where()
        {
            Northwind db = base.CreateDB();

            var q2 = from o in db.Orders
                     group o by o.CustomerID into g
                     where g.Count() > 1
                     select new { g.Key, OrderCount = g.Count() };

            var lst = q2.ToList();
            Assert.Greater(lst.Count, 0, "Expected some grouped order results");
            var result0 = lst[0];
            Assert.IsTrue(result0.Key != null, "Key must be non-null");
            Assert.Greater(result0.OrderCount, 0, "Count must be > 0");
            //select new { g.Key , SumPerCustomer = g.Sum(o2=>o2.OrderID) };
        }

        [Test]
        public void G08_OrderSumByCustomerID()
        {
            Northwind db = base.CreateDB();

            var q2 = from o in db.Orders
                     group o by o.CustomerID into g
                     //where g.Count()>1
                     select new { g.Key, OrderSum = g.Sum(o => o.OrderID) };
            var lst = q2.ToList();
            Assert.Greater(lst.Count, 0, "Expected some grouped order results");
            foreach (var result in lst)
            {
                Console.WriteLine("  Result: custID=" + result.Key + " sum=" + result.OrderSum);
                Assert.IsTrue(result.Key != null, "Key must be non-null");
                Assert.Greater(result.OrderSum, 0f, "OrderSum must be > 0");
            }
            //select new { g.Key , SumPerCustomer = g.Sum(o2=>o2.OrderID) };
        }


    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbLinq.Logging;
using NUnit.Framework;
using nwind;
using Test_NUnit;

#if MYSQL
    namespace Test_NUnit_MySql.Linq_101_Samples
#elif ORACLE
    namespace Test_NUnit_Oracle.Linq_101_Samples
#elif POSTGRES
    namespace Test_NUnit_PostgreSql.Linq_101_Samples
#elif SQLITE
    namespace Test_NUnit_Sqlite.Linq_101_Samples
#else
    #error unknown target
#endif
{
    /// <summary>
    /// Source:  http://msdn2.microsoft.com/en-us/vbasic/bb737929.aspx
    /// manually translated from VB into C#.
    /// </summary>
    [TestFixture]
    public class Join : TestBase
    {
        [Test(Description = "This sample uses foreign key navigation in the from clause to select all orders for customers in London")]
        public void LinqToSqlJoin01()
        {
            Northwind db = CreateDB();

            var q = from c in db.Customers
                    from o in c.Orders 
                    where c.City == "London" 
                    select o;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0,"No rows returned");
            Assert.IsTrue(list[0].CustomerID!=null,"Missing CustomerID");
        }

        [Test(Description = "This sample uses foreign key navigation in the from clause to select all orders for customers in London")]
        public void LinqToSqlJoin01_b()
        {
            Northwind db = CreateDB();

            var q = from c in db.Customers
                    from o in c.Orders
                    where c.City == "London"
                    select new { o.CustomerID, o.OrderID };

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test(Description = "This sample uses foreign key navigation in the where clause to filter for Products whose Supplier is in the USA that are out of stock")]
        public void LinqToSqlJoin02()
        {
            Northwind db = CreateDB();

            var q = from p in db.Products
                    where p.Supplier.Country == "USA" && p.UnitsInStock == 0
                    select p;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test(Description = "This sample uses foreign key navigation in the from clause to filter for employees in Seattle, and also list their territories")]
        public void LinqToSqlJoin03()
        {
            //Logger.Write(Level.Information, "\nLinq.Join03()");
            Northwind db = CreateDB();

            var q = from e in db.Employees
                    from et in e.EmployeeTerritories
                    where e.City == "Seattle"
                    select new { e.FirstName, e.LastName, et.Territory.TerritoryDescription };

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test(Description = "SelectMany - Self-Join.  filter for pairs of employees where one employee reports to the other and where both employees are from the same City")]
        public void LinqToSqlJoin04()
        {
            //Logger.Write(Level.Information, "\nLinq.Join04()");
            Northwind db = CreateDB();

            var q = from e1 in db.Employees
                    from e2 in e1.Employees
                    where e1.City == e2.City
                    select new
                    {
                        FirstName1 = e1.FirstName,
                        LastName1 = e1.LastName,
                        FirstName2 = e2.FirstName,
                        LastName2 = e2.LastName,
                        e1.City
                    };

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
            foreach (var v in list)
            {
                Assert.IsTrue(v.LastName1 != v.LastName2, "Last names must be different");
            }
        }

        //TODO 5 - 9
        /// <summary>
        /// This sample shows how to construct a join where one side is nullable and the other isn't.
        /// </summary>
        [Test(Description = "GroupJoin - Nullable\\Nonnullable Key Relationship")]
        public void LinqToSqlJoin10()
        {
            Logger.Write(Level.Information, "\nLinq.Join10()");
            Northwind db = CreateDB();

            var q = from o in db.Orders
                    join e in db.Employees on o.EmployeeID equals e.EmployeeID into emps
                    from e in emps
                    select new { o.OrderID, e.FirstName };

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }
        // picrap: commented out, it doesn't build because of db.Orderdetails (again, a shared source file...)
/*
        [Test(Description = "Problem discovered by Laurent")]
        public void Join_Laurent()
        {
            Logger.Write(Level.Information, "\nJoin_Laurent()");
            Northwind db = CreateDB();

            var q1 = (from p in db.Products
                      join o in db.Orderdetails on p.ProductID equals o.ProductID
                      where p.ProductID>1
                      select new
                      {
                          p.ProductName,
                          o.OrderID,
                          o.ProductID,
                      }
                      ).ToList();

            Assert.IsTrue(q1.Count > 0);
        }
        */
    }
}

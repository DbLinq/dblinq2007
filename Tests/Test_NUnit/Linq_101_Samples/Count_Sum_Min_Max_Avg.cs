using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
#elif INGRES
    namespace Test_NUnit_Ingres.Linq_101_Samples
#else
    #error unknown target
#endif
{
    /// <summary>
    /// Source:  http://msdn2.microsoft.com/en-us/vbasic/bb737922.aspx
    /// manually translated from VB into C#.
    /// </summary>
    [TestFixture]
    public class Count_Sum_Min_Max_Avg : TestBase
    {
        [Test]
        public void LiqnToSqlCount01()
        {
            Northwind db = CreateDB();
            var q = db.Customers.Count();

            Assert.IsTrue(q > 0, "Expect non-zero count");
        }

        [Test]
        public void LiqnToSqlCount02()
        {
            Northwind db = CreateDB();
            var q = (from p in db.Products where !p.Discontinued select p)
                .Count();

            Assert.IsTrue(q > 0, "Expect non-zero count");
        }
        [Test(Description = "This sample uses Sum to find the total freight over all Orders.")]
        public void LinqToSqlCount03()
        {
            Northwind db = CreateDB();
            var q = (from o in db.Orders select o.Freight).Sum();
            Assert.IsTrue(q > 0, "Freight sum must be > 0");
        }

        [Test(Description = "This sample uses Sum to find the total number of units on order over all Products.")]
        public void LinqToSqlCount04()
        {
            Northwind db = CreateDB();
            var q = (from p in db.Products select (int)p.UnitsOnOrder.Value).Sum();
            Assert.IsTrue(q > 0, "Freight sum must be > 0");
        }

        [Test(Description = "This sample uses Min to find the lowest unit price of any Product")]
        public void LinqToSqlCount05()
        {
            Northwind db = CreateDB();
            var q = (from p in db.Products select p.UnitsOnOrder).Min();
            Assert.IsTrue(q == 0, "Min UnitsOnOrder must be 0");
        }

        [Test(Description = "This sample uses Min to find the lowest freight of any Order.")]
        public void LinqToSqlCount06()
        {
            Northwind db = CreateDB();
            var q = (from o in db.Orders select o.Freight).Min();
            Assert.IsTrue(q > 0, "Freight sum must be > 0");
        }

        [Test(Description = "This sample uses Min to find the Products that have the lowest unit price in each category")]
        public void LinqToSqlCount07()
        {
            Northwind db = CreateDB();
            var categories = (from p in db.Products
                              group p by p.CategoryID into g
                              select new
                              {
                                  CategoryID = g.Key,
                                  CheapestProducts = from p2 in g
                                                     where p2.UnitPrice == g.Min(p3 => p3.UnitPrice)
                                                     select p2
                              });

            var list = categories.ToList();
            Assert.IsTrue(list.Count > 0, "Expected count > 0");
        }

        [Test(Description = "This sample uses Max to find the latest hire date of any Employee")]
        public void LinqToSqlCount08()
        {
            Northwind db = CreateDB();
            var q = (from e in db.Employees select e.HireDate).Max();
            Assert.IsTrue(q > new DateTime(1990, 1, 1), "Hire date must be > 2000");
        }

        [Test(Description = "This sample uses Max to find the most units in stock of any Product")]
        public void LinqToSqlCount09()
        {
            Northwind db = CreateDB();
            var q = (from p in db.Products select p.UnitsInStock).Max();
            Assert.IsTrue(q > 0, "Max UnitsInStock must be > 0");
        }

        [Test(Description = "This sample uses Max to find the Products that have the highest unit price in each category")]
        public void LinqToSqlCount10()
        {
            Northwind db = CreateDB();
            var q = from p in db.Products
                    group p by p.CategoryID into g
                    select new
                    {
                        g,
                        MostExpensiveProducts = from p2 in g
                                                where p2.UnitPrice == g.Max(p3 => p3.UnitPrice)
                                                select p2
                    };
            var list = q.ToList();
            Assert.IsTrue(list.Count > 0, "Got most expensive items > 0");
        }

    }
}

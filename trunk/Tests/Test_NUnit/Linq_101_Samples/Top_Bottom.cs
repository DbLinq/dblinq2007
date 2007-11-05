using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using nwind;

namespace Test_NUnit.Linq_101_Samples
{
    /// <summary>
    /// Source:  http://msdn2.microsoft.com/en-us/vbasic/bb737940.aspx
    /// manually translated from VB into C#.
    /// </summary>
    [TestFixture]
    public class Top_Bottom : TestBase
    {

        [Test(Description="This sample uses Take to select the first 5 Employees hired.")]
        public void LinqToSqlTop01()
        {
            Northwind db = CreateDB();

            var q = (from e in db.Employees 
                orderby e.HireDate select e). Take(5);

            List<Employee> list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test(Description = "This sample uses Skip to select all but the 10 most expensive Products.")]
        public void LinqToSqlTop02()
        {
            Northwind db = CreateDB();

            var q = (from p in db.Products
                     orderby p.UnitPrice descending
                     select p).Skip(4);

            List<Product> list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test(Description = "This bug was submitted by Andrus")]
        public void LinqToSqlTop03_Ex_Andrus()
        {
            Northwind db = CreateDB();

            var q = db.Customers.Skip(3).Take(5);

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);

        }

    }
}

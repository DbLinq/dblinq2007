using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using nwind;

namespace Test_NUnit.Linq_101_Samples
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
        }

        //TODO 5 - 9
        [Test(Description = "GroupJoin - Nullable\\Nonnullable Key Relationship")]
        public void LinqToSqlJoin10()
        {
            Northwind db = CreateDB();

            var q = from o in db.Orders
                    join e in db.Employees on o.EmployeeID equals e.EmployeeID into emps
                    from e in emps
                    select new { o.OrderID, e.FirstName };

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

    }
}

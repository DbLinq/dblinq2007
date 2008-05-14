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
#if ODP
        namespace Test_NUnit_OracleODP.Linq_101_Samples
#else
        namespace Test_NUnit_Oracle.Linq_101_Samples
#endif
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
            Assert.IsTrue(list.Count > 0, "No rows returned");
            Assert.IsTrue(list[0].CustomerID != null, "Missing CustomerID");
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
            //Microsoft Linq-to-SQL generated statement that we want to match:
            //SELECT [t0].[OrderID], [t1].[FirstName]
            //FROM [dbo].[Orders] AS [t0], [dbo].[Employees] AS [t1]
            //WHERE [t0].[EmployeeID] = ([t1].[EmployeeID])

            Logger.Write(Level.Information, "\nLinq.Join10()");
            Northwind db = CreateDB();

            var q = from o in db.Orders
                    join e in db.Employees on o.EmployeeID equals e.EmployeeID into emps
                    from e in emps
                    select new { o.OrderID, e.FirstName };

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test(Description = "example by Frans Brouma: select all customers that have no orders")]
        public void LeftJoin_DefaultIfEmpty()
        {
            //example by Frans Brouma on Matt Warren's site
            //select all customers that have no orders
            //http://blogs.msdn.com/mattwar/archive/2007/09/04/linq-building-an-iqueryable-provider-part-vii.aspx
            //http://weblogs.asp.net/fbouma/archive/2007/11/23/developing-linq-to-llblgen-pro-part-9.aspx

            Northwind db = CreateDB();

            var q = from c in db.Customers
                    join o in db.Orders on c.CustomerID equals o.CustomerID into oc
                    from x in oc.DefaultIfEmpty()
                    where x.OrderID == null
                    select c;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
            int countALFKI = list.Count(item => item.CustomerID == "ALFKI");
            Assert.IsTrue(countALFKI == 1);
        }

        [Test]
        public void LeftOuterJoin_Suppliers()
        {
            //http://blogs.class-a.nl/blogs/anko/archive/2008/03/14/linq-to-sql-outer-joins.aspx
            //example by Anko Duizer (NL)
            Northwind db = CreateDB();
            var query = from s in db.Suppliers
                        join c in db.Customers on s.City equals c.City into temp
                        from t in temp.DefaultIfEmpty()
                        select new
                        {
                            SupplierName = s.CompanyName,
                            CustomerName = t.CompanyName,
                            City = s.City
                        };

            var list = query.ToList();

            bool foundMelb = false, foundNull = false;
            foreach (var item in list)
            {
                foundMelb = foundMelb || item.City == "Melbourne";
                foundNull = foundNull || item.City == null;
            }
            Assert.IsTrue(foundMelb, "Expected rows with City=Melbourne");
            Assert.IsTrue(foundNull, "Expected rows with City=null");
        }

        // picrap: commented out, it doesn't build because of db.Orderdetails (again, a shared source file...)

        [Test(Description = "Problem discovered by Laurent")]
        public void Join_Laurent()
        {
            Logger.Write(Level.Information, "\nJoin_Laurent()");
            Northwind db = CreateDB();

            var q1 = (from p in db.Products
                      join o in db.OrderDetails on p.ProductID equals o.ProductID
                      where p.ProductID > 1
                      select new
                      {
                          p.ProductName,
                          o.OrderID,
                          o.ProductID,
                      }
                      ).ToList();

            Assert.IsTrue(q1.Count > 0);
        }

        [Test]
        public void RetrieveParentAssociationProperty()
        {
            Northwind dbo = CreateDB();
            Northwind1 db = new Northwind1(dbo.DatabaseContext.Connection);
            var t = db.GetTable<Northwind1.ExtendedOrder>();
            var q = from order in t
                    select new
                    {
                        order.OrderID,
                        order.CustomerShipCity.ContactName
                    };
            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test]
        public void DifferentParentAndAssociationPropertyNames()
        {
            Northwind dbo = CreateDB();
            Northwind1 db = new Northwind1(dbo.DatabaseContext.Connection);
            var query = db.GetTable<Northwind1.ExtendedOrder>() as IQueryable<Northwind1.ExtendedOrder>;

            var q2 = query.Select(e => new Northwind1.ExtendedOrder
            {
                OrderID = e.OrderID,
                ShipAddress = e.CustomerShipCity.ContactName
            });
            var list = q2.ToList();
            Assert.IsTrue(list.Count > 0);
        }



        [Test]
        public void SelectCustomerContactNameFromOrder()
        {
            Northwind dbo = CreateDB();
            Northwind1 db = new Northwind1(dbo.DatabaseContext.Connection);
            var t = db.GetTable<Northwind1.ExtendedOrder>();

            var q = from order in t
                    select new
                    {
                        order.CustomerContactName
                    };
            var list = q.ToList();
            Assert.AreEqual(db.Orders.Count(), list.Count());
            foreach (var s in list)
                Assert.AreEqual("Test", s);
        }

        public class Northwind1 : Northwind
        {
            public Northwind1(System.Data.IDbConnection connection)
                : base(connection) { }

            // Linq-SQL requires this: [System.Data.Linq.Mapping.Table(Name = "orders")]
            public class ExtendedOrder : Order
            {

                System.Data.Linq.EntityRef<Customer> _x_Customer;

                [System.Data.Linq.Mapping.Association(Storage = "_x_Customer",
                    ThisKey = "ShipCity", Name =
#if MYSQL
"orders_ibfk_1"
#elif ORACLE
 "SYS_C004742"
#elif POSTGRES
 "fk_order_customer"
#elif SQLITE
 "fk_Orders_1"
#elif INGRES
 "fk_order_customer"
#else
#error unknown target
#endif
)]
                public Customer CustomerShipCity
                {
                    get { return _x_Customer.Entity; }
                    set { _x_Customer.Entity = value; }
                }

                public string CustomerContactName
                {
                    get
                    {
                        return "Test";
                    }
                }

            }

            public DbLinq.Linq.Table<ExtendedOrder> ExtendedOrders
            {
                get { return base.GetTable<ExtendedOrder>(); }
            }
        }

        [Test]
        public void WhereBeforeSelect()
        {
            Northwind db = CreateDB();
            var t = db.GetTable<Order>();

            var query = t.Where(o => o.OrderID != 0);

            query = query.Select(dok => new Order
            {
                OrderID = dok.OrderID,
                OrderDate = dok.OrderDate,
                ShipCity = dok.Customer.ContactName,
                Freight = dok.Freight
            });
            var list = query.ToList();
            Assert.IsTrue(list.Count > 0);
        }

    }
}

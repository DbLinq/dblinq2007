using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Test_NUnit;
using NUnit.Framework;
using Test_NUnit.Linq_101_Samples;
using System.Data.Linq;

#if MONO_STRICT
using DataLoadOptions = System.Data.Linq.DataLoadOptions;
#else
using DataLoadOptions = DbLinq.Data.Linq.DataLoadOptions;
#endif


#if !MONO_STRICT
using nwind;
#else
using MsNorthwind;
#endif

#if MYSQL
namespace Test_NUnit_MySql
#elif ORACLE
#if ODP
        namespace Test_NUnit_OracleODP
#else
        namespace Test_NUnit_Oracle
#endif
#elif POSTGRES
    namespace Test_NUnit_PostgreSql
#elif SQLITE
    namespace Test_NUnit_Sqlite
#elif INGRES
    namespace Test_NUnit_Ingres
#elif MSSQL
#if MONO_STRICT
namespace Test_NUnit_MsSql_Strict
#else
namespace Test_NUnit_MsSql
#endif
#elif FIREBIRD
    namespace Test_NUnit_Firebird
#else
    #error unknown target
#endif
{
    [TestFixture]
    public class EntitySet : TestBase
    {

        [Test]
        public void SimpleMemberAccess01()
        {
            var customer = new Customer();
            var orders = customer.Orders;
        }

        [Test]
        public void SimpleMemberAccess02()
        {
            var db = CreateDB();
            var customer = db.Customers.First();
            Assert.Greater(customer.Orders.Count, 0);
        }

        [Test]
        public void EntitySetEnumerationProjection()
        {
            var db = CreateDB();
            var results = (from c in db.Customers select c.Orders).ToList();

            Assert.Greater(results.Count, 0);
        }

        [Test]
        public void HasLoadedOrAsignedValues01()
        {
            var db = CreateDB();
            var customer = db.Customers.First();
            Assert.IsFalse(customer.Orders.HasLoadedOrAssignedValues);

            customer.Orders.Add(new Order());
            Assert.IsTrue(customer.Orders.HasLoadedOrAssignedValues);
        }

        [Test]
        public void HasLoadedOrAsignedValues02()
        {
            var db = CreateDB();
            var customer = db.Customers.First();
            Assert.IsFalse(customer.Orders.HasLoadedOrAssignedValues);

            customer.Orders.Assign(System.Linq.Enumerable.Empty<Order>());
            Assert.IsTrue(customer.Orders.HasLoadedOrAssignedValues);
        }


        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void InvalidSourceChange()
        {
            var db = CreateDB();
            var customer = db.Customers.First();

            Assert.Greater(customer.Orders.Count, 0);
            customer.Orders.SetSource(System.Linq.Enumerable.Empty<Order>());
        }

        [Test]
        public void SourceChange()
        {
            var db = CreateDB();

            int ordersCount = (from cust in db.Customers
                               select cust.Orders.Count).First();

            Assert.Greater(ordersCount, 0);

            var customer2 = db.Customers.First();
            customer2.Orders.SetSource(System.Linq.Enumerable.Empty<Order>());
            Assert.AreEqual(customer2.Orders.Count, 0);
        }


        [Test]
        public void Refresh01()
        {
            var db = CreateDB();
            var c = db.Customers.First();

            int beforeCount = c.Orders.Count;
            Assert.Greater(beforeCount, 0);
            c.Orders.Clear();
            Assert.AreEqual(c.Orders.Count, 0);
            c.Orders.AddRange(db.Orders);
            Assert.Greater(c.Orders.Count, beforeCount);
            db.Refresh(RefreshMode.OverwriteCurrentValues, c.Orders);

            Assert.AreEqual(c.Orders.Count, beforeCount);
        }

        [Test]
        public void Refresh02()
        {
            var db = CreateDB();
            var c = db.Customers.First();

            int beforeCount = c.Orders.Count;
            Assert.Greater(beforeCount, 0);
            c.Orders.Clear();
            Assert.AreEqual(c.Orders.Count, 0);
            c.Orders.AddRange(db.Orders);

            int middleCount = c.Orders.Count;
            Assert.Greater(c.Orders.Count, beforeCount);

            db.Refresh(RefreshMode.KeepCurrentValues, c.Orders);
            Assert.AreEqual(c.Orders.Count, middleCount);

            db.Refresh(RefreshMode.KeepChanges, c.Orders);
            Assert.AreEqual(c.Orders.Count, middleCount);
        }


        [Test]
        public void Refresh03()
        {
            var db = CreateDB();
            var c = db.Customers.First();

            var order = c.Orders.First();
            string newcustomerId = "NEWCUSTOMERID";
            order.CustomerID = newcustomerId;

            db.Refresh(RefreshMode.OverwriteCurrentValues, c.Orders);
            Assert.AreNotEqual(order.CustomerID, newcustomerId);
        }

        [Test]
        public void Refresh04()
        {
            var db = CreateDB();
            var c = db.Customers.First();

            var order = c.Orders.First();
            string newcustomerId = "NEWCUSTOMERID";
            order.CustomerID = newcustomerId;

            db.Refresh(RefreshMode.KeepCurrentValues, c.Orders);
            Assert.AreEqual(order.CustomerID, newcustomerId);

            db.Refresh(RefreshMode.KeepChanges, c.Orders);
            Assert.AreEqual(order.CustomerID, newcustomerId);
        }


        [Test]
        public void ListChangedEvent()
        {
            var db = CreateDB();
            var customer = db.Customers.First();
            bool ok;
            customer.Orders.ListChanged += delegate { ok = true; };

            ok = false;
            customer.Orders.Remove(customer.Orders.First());
            Assert.IsTrue(ok);

            ok = false;
            customer.Orders.Assign(Enumerable.Empty<Order>());
            Assert.IsTrue(ok);

            ok = false;
            customer.Orders.Add(db.Orders.First(o => !customer.Orders.Contains(o)));
            Assert.IsTrue(ok);

            ok = false;
            customer.Orders.Clear();
            Assert.IsTrue(ok);

            ok = false;
            customer.Orders.Insert(0, new Order());
            Assert.IsTrue(ok);

            ok = false;
            customer.Orders.RemoveAt(0);
            Assert.IsTrue(ok);
        }

        [Test]
        public void Load()
        {
            var db = CreateDB();
            var customer = db.Customers.First();
            var orders = customer.Orders;

            Assert.IsFalse(orders.HasLoadedOrAssignedValues);
            orders.Load();
            Assert.IsTrue(orders.HasLoadedOrAssignedValues);
        }

        [Test]
        public void DeferedExecution()
        {
            var db = CreateDB();
            var customer = db.Customers.First();
            Assert.IsTrue(customer.Orders.IsDeferred);

            customer.Orders.Load();
            Assert.IsFalse(customer.Orders.IsDeferred);
        }

        [Test]
        public void DeferedExecutionAndLoadWith()
        {
            var db = CreateDB();
            DataLoadOptions loadoptions = new DataLoadOptions();
            loadoptions.LoadWith<Customer>(c => c.Orders);
            db.LoadOptions = loadoptions;

            var customer = db.Customers.First();
            Assert.IsFalse(customer.Orders.IsDeferred);
            Assert.IsTrue(customer.Orders.HasLoadedOrAssignedValues);
        }

        [Test]
        public void Add()
        {
            var db = CreateDB();
            var customer = db.Customers.First();
            int beforeCount = customer.Orders.Count;
            customer.Orders.Add(new Order());
            Assert.AreEqual(customer.Orders.Count, beforeCount + 1);
        }

        [Test]
        public void Clear()
        {
            var db = CreateDB();
            var customer = db.Customers.First();

            if (customer.Orders.Count == 0)
                Assert.Ignore();

            customer.Orders.Clear();
            Assert.AreEqual(customer.Orders.Count, 0);
        }

        [Test]
        public void AddRange()
        {
            var db = CreateDB();
            var customer = db.Customers.First();
            int beforeCount = customer.Orders.Count;
            customer.Orders.AddRange(new Order[] { new Order(), new Order() });
            Assert.AreEqual(customer.Orders.Count, beforeCount + 2);
        }

        [Test]
        public void Remove()
        {
            var db = CreateDB();
            var customer = db.Customers.First();
            int beforeCount = customer.Orders.Count;

            if (beforeCount == 0)
                Assert.Ignore();

            customer.Orders.Remove(customer.Orders.First());
            Assert.AreEqual(customer.Orders.Count, beforeCount - 1);
        }

        [Test]
        public void RemoveAt()
        {
            var db = CreateDB();
            var customer = db.Customers.First();
            int beforeCount = customer.Orders.Count;

            if (beforeCount == 0)
                Assert.Ignore();

            customer.Orders.RemoveAt(0);
            Assert.AreEqual(customer.Orders.Count, beforeCount - 1);
        }

        [Test]
        public void RemoveAll()
        {
            Clear();
        }
    }
}

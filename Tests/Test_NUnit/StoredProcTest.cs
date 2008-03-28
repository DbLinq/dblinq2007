using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
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
    public class StoredProcTest : TestBase
    {

#if !SQLITE
        [Test]
        public void SP1_CallHello0()
        {
            Northwind db = base.CreateDB();
            string result = db.Hello0();
            Assert.IsNotNull(result);
        }

        [Test]
        public void SP2_CallHello1()
        {
            Northwind db = base.CreateDB();
            string result = db.Hello1("xx");
            Assert.IsTrue(result!=null && result.Contains("xx"));
        }

        [Test]
        public void SP3_GetOrderCount_SelField()
        {
            Northwind db = base.CreateDB();
            var q = from c in db.Customers 
                    select new { c.CustomerID, OrderCount = db.GetOrderCount(c.CustomerID) };

            int count = 0;
            foreach (var c in q)
            {
                Assert.IsNotNull(c.CustomerID);
                Assert.Greater(c.OrderCount, -1);
                count++;
            }
            Assert.Greater(count, 0);
        }

        [Test]
        public void SP4_GetOrderCount_SelField_B()
        {
            Northwind db = base.CreateDB();
            var q = from c in db.Customers 
                    select new {c, OrderCount=db.GetOrderCount(c.CustomerID)};

            int count = 0;
            foreach (var v in q)
            {
                Assert.IsNotNull(v.c.CustomerID);
                Assert.Greater(v.OrderCount, -1);
                count++;
            }
            Assert.Greater(count, 0);
        }

        [Test]
        public void SPB_GetOrderCount_Having()
        {
            Northwind db = base.CreateDB();
            var q = from c in db.Customers where db.GetOrderCount(c.CustomerID) > 1 select c;

            int count = 0;
            foreach (var c in q)
            {
                Assert.IsTrue(c.CustomerID!=null, "Non-null customerID required");
                count++;
            }
            Assert.Greater(count, 0);
        }
#endif
    }

}

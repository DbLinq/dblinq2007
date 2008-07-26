﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Test_NUnit;
using NUnit.Framework;
using Test_NUnit.Linq_101_Samples;

#if !MONO_STRICT
using nwind;
#else
using MsNorthwind;
using System.Data.Linq;
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
#else
#error unknown target
#endif
{
    [TestFixture]
    public class Any : TestBase
    {
        [Test]
        public void AnyInternal01()
        {
            Northwind db = CreateDB();

            var q = from c in db.Customers
                    where !c.Orders.Any()
                    select c;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test]
        public void AnyInternal02()
        {
            Northwind db = CreateDB();

            var q = from c in db.Customers
                    where !c.Orders.Any(o => o.Customer.ContactName == "WARTH")
                    select c;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test]
        public void AnyExternal01()
        {
            Northwind db = CreateDB();

            var q = (from c in db.Customers
                     where c.Country == "USA"
                     select c).Any();

        }

        [Test]
        public void AnyExternal02()
        {
            Northwind db = CreateDB();

            var q = (from c in db.Customers
                     where c.Country == "USA"
                     select c).Any(cust => cust.City == "Seatle");
        }

        [Test]
        public void AnyExternal03()
        {
            Northwind db = CreateDB();
            var q = db.Customers.Any();
        }

        [Test]
        public void AnyExternal04()
        {
            Northwind db = CreateDB();
            var q = db.Customers.Any(cust => cust.City == "Seatle");
        }

        [Test]
        public void CountInternal01()
        {
            Northwind db = CreateDB();

            var q = from c in db.Customers
                    where c.Orders.Count() % 2 == 0
                    select c;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test]
        public void CountInternal02()
        {
            Northwind db = CreateDB();

            var q = from c in db.Customers
                    where c.Orders.Count(o => o.Customer.ContactName == "WARTH") % 2 == 0
                    select c;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test]
        public void CountExternal01()
        {
            Northwind db = CreateDB();

            var q = (from c in db.Customers
                     where c.Country == "USA"
                     select c).Count();
        }

        [Test]
        public void CountExternal02()
        {
            Northwind db = CreateDB();

            var q = (from c in db.Customers
                     where c.Country == "USA"
                     select c).Count(cust => cust.City == "Seatle");
        }

        [Test]
        public void CountExternal03()
        {
            Northwind db = CreateDB();
            var q = db.Customers.Count();
        }

        [Test]
        public void CountExternal04()
        {
            Northwind db = CreateDB();
            var q = db.Customers.Count(cust => cust.City == "Seatle");
        }
    }
}

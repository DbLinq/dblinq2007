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
#elif MSSQL
#if MONO_STRICT
    namespace Test_NUnit_MsSql_Strict.Linq_101_Samples
#else
    namespace Test_NUnit_MsSql.Linq_101_Samples
#endif
#else
#error unknown target
#endif
{
    [TestFixture]
    public class ExsistIn_Any_All : TestBase
    {
        [Test(Description = "Any - Simple. This sample uses Any to return only Customers that have no Orders.")]
        public void LinqToSqlExists01()
        {
            Northwind db = CreateDB();

            var q = from c in db.Customers
                    where !c.Orders.Any()
                    select c;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Linq101SamplesModified("Strange casting, It seems like original northwind discontinued were boolean")]
        [Test(Description = "Any - Conditional. This sample uses Any to return only Categories that have at least one Discontinued product.")]
        public void LinqToSqlExists02()
        {
            Northwind db = CreateDB();
#if !SQLITE
            var q = from c in db.Categories
                    where (from p in c.Products where Convert.ToBoolean(p.Discontinued) select p).Any()
                    select c;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
#else
#warning this precomipiled stentence must be deleted earlier as possible, when sqlite Northwind have a Category table with a relation with products.
            Assert.Ignore();
#endif

        }

        [Test(Description = "All - Conditional. This sample uses All to return Customers whom all of their orders have been shipped to their own city or whom have no orders.")]
        public void LinqToSqlExists03()
        {
            Northwind db = CreateDB();

            var q = from c in db.Customers
                    where c.Orders.All(o => o.ShipCity == c.City)
                    select c;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }
    }
}

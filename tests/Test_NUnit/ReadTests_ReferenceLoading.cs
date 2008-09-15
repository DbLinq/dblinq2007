using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Test_NUnit;
using NUnit.Framework;
using Test_NUnit.Linq_101_Samples;
using System.Data.Linq;

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
#else
#error unknown target
#endif
{
    [TestFixture]
    public class ReadTests_ReferenceLoading:TestBase
    {

        [Test]
        public void ReferenceLoading01()
        {
            var db = CreateDB();
            var order = db.Orders.First();
            Assert.IsNotNull(order.Employee);
        }

        [Test]
        public void ReferenceLoading02()
        {
            var db = CreateDB();
            var c = db.Customers.First();
            Assert.IsNotNull(c.Orders.First().Employee);
        }

        [Test]
        public void ReferenceLoading03()
        {
            var db = CreateDB();
            var employeeTerritory = db.EmployeeTerritories.First();
            Assert.IsNotNull(employeeTerritory.Territory.Region.RegionID);
        }
    }
}

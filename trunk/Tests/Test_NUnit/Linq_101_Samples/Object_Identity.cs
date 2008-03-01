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
#else
    #error unknown target
#endif
{
    /// <summary>
    /// Source:  http://msdn2.microsoft.com/en-us/vbasic/bb737931.aspx
    /// manually translated from VB into C#.
    /// </summary>
    [TestFixture]
    public class Object_Identity : TestBase
    {

        /// <summary>
        /// This sample demonstrates how, upon executing the same query twice, you will receive a reference to the same object in memory each time.
        /// </summary>
        [Test(Description = "Object Caching - 1.")]
        public void LinqToSqlObjectIdentity01()
        {
            Northwind db = CreateDB();

            Customer cust1 = db.Customers.First(c => c.CustomerID == "BONAP");

            Customer cust2 = (from c in db.Customers
                              where c.CustomerID == "BONAP"
                              select c).First();

            bool isSameObject = Object.ReferenceEquals(cust1, cust2);
            Assert.IsTrue(isSameObject);
            Assert.IsTrue(cust1.CustomerID == "BONAP", "CustomerID must be BONAP - was: " + cust1.CustomerID);
        }

    }
}

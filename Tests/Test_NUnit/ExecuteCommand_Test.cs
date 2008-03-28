using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public class ExecuteCommand_Test : TestBase
    {
        [Test]
        public void A2_ProductsTableHasEntries()
        {
            Northwind db = CreateDB();
            //string sql = "SELECT count(*) FROM Northwind.Products";
            int result = db.ExecuteCommand("SELECT count(*) FROM Products");
            //long iResult = base.ExecuteScalar(sql);
            Assert.Greater(result, 0, "Expecting some rows in Products table, got:" + result);
        }

        /// <summary>
        /// like above, but includes one parameter.
        /// </summary>
        [Test]
        public void A3_ProductCount_Param()
        {
            Northwind db = CreateDB();
            int result = db.ExecuteCommand("SELECT count(*) FROM Products WHERE ProductID>{0}", 3);
            //long iResult = base.ExecuteScalar(sql);
            Assert.Greater(result, 0, "Expecting some rows in Products table, got:" + result);
        }

    }
}

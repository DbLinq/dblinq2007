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
#elif INGRES
    namespace Test_NUnit_Ingres.Linq_101_Samples
#else
    #error unknown target
#endif
{
    /// <summary>
    /// Source:  http://msdn2.microsoft.com/en-us/vbasic/bb737930.aspx
    /// manually translated from VB into C#.
    /// </summary>
    [TestFixture]
    public class String_Date_functions : TestBase
    {
        [Test(Description = "This sample uses the & operator to concatenate string fields and string literals in forming the Customers' calculated Location value")]
        public void LinqToSqlString01()
        {
            Northwind db = CreateDB();

            var q = from c in db.Customers
                    select new { c.CustomerID, Location = c.City + ", " + c.Country };

            //bool foundBerlin = false;
            foreach (var v in q)
            {
                if (v.Location == "Berlin, Germany")
                {
                    //foundBerlin = true;
                    return;
                }
            }
            Assert.Fail("Expected to find location 'Berlin, Germany'");
        }

        [Test]
        public void LinqToSqlString02()
        {
            Northwind db = CreateDB();

            var q = from p in db.Products
                    where p.ProductName.Length < 10
                    select p;

            List<Product> prods = q.ToList();
            Assert.IsTrue(prods.Count > 0, "Expected some products");
        }

    }
}

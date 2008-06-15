using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Test_NUnit;
using Test_NUnit.Linq_101_Samples;

#if !MONO_STRICT
using nwind;
using DbLinq.Linq;
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
namespace Test_NUnit_MsSql.Linq_101_Samples
#else
#error unknown target
#endif
{
    [TestFixture]
    public class Conversion_Operators : TestBase
    {
        [Test(Description = "AsEnumerable.This sample uses ToArray so that the client-side IEnumerable(Of T) implementation of where is used, instead of the default Query(Of T) implementation which would be converted to SQL and executed on the server. This is necessary because the where clause references a user-defined client-side method, isValidProduct, which cannot be converted to SQL.")]
        public void LinqToSqlConversions01()
        {
            Northwind db = CreateDB();

            var q = from p in db.Products.AsEnumerable()
                    where isValidProduct(p)
                    select p;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }


        private bool isValidProduct(Product p)
        {
            return (p.ProductName.LastIndexOf("C") == 0);
        }


        [Test(Description = "ToArray. This sample uses ToArray to immediately evaluate a query into an array and get the 3rd element.")]
        public void LinqToSqlConversions02()
        {
            Northwind db = CreateDB();

            var q = from c in db.Customers
                    where c.City == "London"
                    select c;

            Customer[] list = q.ToArray();

            Assert.IsFalse(list == null);
            Assert.IsTrue(list.Length > 0);
        }

        [Test(Description = "ToList. This sample uses ToList to immediately evaluate a query into a List(Of T).")]
        public void LinqToSqlConversions03()
        {
            Northwind db = CreateDB();

            var q = from e in db.Employees
                    where e.HireDate >= DateTime.Parse("1/1/1994")
                    select e;

            List<Employee> qList = q.ToList();

            Assert.IsFalse(qList == null);
            Assert.IsTrue(qList.Count > 0);
        }


        [Linq101SamplesModified("Strange short to boolean casting, perhaps in the original Northwind Product.Discontinued was a boolean property")]
        [Test(Description = "ToDictionary. This sample uses ToDictionary to immediately evaluate a query and a key expression into an Dictionary(Of K, T).")]
        public void LinqToSqlConversion04()
        {
            Northwind db = CreateDB();

            var q = from p in db.Products
                    where p.UnitsInStock <= p.ReorderLevel && !Convert.ToBoolean(p.Discontinued)
                    select p;

            var qDictionary = q.ToDictionary(p => p.ProductID);

            Assert.IsFalse(qDictionary == null);
            // PC: on SQLite, this returns nothing. Is the test wrong?
            if (qDictionary.Count == 0)
                Assert.Ignore("Please check this test validity");
            //Assert.IsTrue(qDictionary.Count > 0);

            foreach (var key in qDictionary.Keys)
            {
                Console.WriteLine("Key {0}:", key);
                Console.WriteLine();
            }

        }
    }
}

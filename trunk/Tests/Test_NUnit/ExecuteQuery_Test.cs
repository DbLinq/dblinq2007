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
    public class ExecuteQuery_Test : TestBase
    {
        [Test]
        public void X1_SimpleQuery()
        {
            var db = CreateDB();

            IList<Category> categories1 = (from c in db.Categories orderby c.CategoryName select c).ToList();
            IList<Category> categories2 = db.ExecuteQuery<Category>(
                @"select categoryid, categoryname, description, picture \
                    from categories \
                     order by categoryname").ToList();

            Assert.AreEqual(categories1.Count, categories2.Count);
            for (int index = 0; index < categories2.Count; index++)
            {
                Assert.AreEqual(categories1[index].CategoryID, categories2[index].CategoryID);
                Assert.AreEqual(categories1[index].CategoryName, categories2[index].CategoryName);
                Assert.AreEqual(categories1[index].Description, categories2[index].Description);
            }
        }
    }
}

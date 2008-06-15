using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Test_NUnit;

#if !MONO_STRICT
using nwind;
using DbLinq.Linq;
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
    namespace Test_NUnit_MsSql
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
                @"select 
                        <<Description>>, 
                        <<CategoryName>>, 
                        <<Picture>>,
                        <<CategoryID>>
                    from <<Categories>>
                     order by <<CategoryName>>").ToList();

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

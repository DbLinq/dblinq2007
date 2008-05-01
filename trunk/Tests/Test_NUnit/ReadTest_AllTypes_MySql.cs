using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NUnit.Framework;
using AllTypesExample;


namespace Test_NUnit_MySql
{

    /// <summary>
    /// this test will exercise reading of columns of all MySQL types 
    /// (such as decimal, decimal?, DateTime? etc)
    /// </summary>
    [TestFixture]
    public class ReadTest_AllTypes
    {
        public AllTypes CreateDB()
        {
            string DbServer = Environment.GetEnvironmentVariable("DbLinqServer") ?? "localhost";
            string connStr = string.Format("server={0};user id=LinqUser; password=linq2; database=AllTypes", DbServer);

            //return CreateDB(System.Data.ConnectionState.Closed);
            AllTypes db = new AllTypes(new MySql.Data.MySqlClient.MySqlConnection(connStr));
            
            return db;
        }

        [Test]
        public void AT1_SelectAllIntTypes()
        {
            AllTypes db = CreateDB();

            var q = from p in db.Allinttypes select p;
            int count = q.ToList().Count;
            Assert.IsTrue(count > 0, "Expected some entries in AllIntTypes, got none");
        }

        [Test]
        public void AT2_SelectAllFloatTypes()
        {
            AllTypes db = CreateDB();

            var q = from p in db.Floattypes select p;
            int count = q.ToList().Count;
            Assert.IsTrue(count > 0, "Expected some entries in FloatTypes, got none");
        }

        [Test]
        public void AT2_SelectOtherTypes()
        {
            AllTypes db = CreateDB();

            var q = from p in db.Othertypes select p.DateTimeN;
            int count = q.ToList().Count;
            Assert.IsTrue(count > 0, "Expected some entries in AllTypes, got none");
        }

        [Test]
        public void AT3_SelectDecimalN()
        {
            AllTypes db = CreateDB();

            var q = from p in db.Floattypes select p.DecimalN;
            int count = q.ToList().Count;
            Assert.IsTrue(count > 0, "Expected some entries in AllTypes, got none");
        }

#if DBLINQ_ENUMTEST
        [Test]
        public void AT4_SelectEnum()
        {
            AllTypes db = CreateDB();

            var q = from p in db.Allinttypes select p.DbLinq_EnumTest;
            int count = q.ToList().Count;
            Assert.IsTrue(count > 0, "Expected some enums in AllTypes, got none");
        }

        [Test]
        public void AT5_SelectEnum_()
        {
            AllTypes db = CreateDB();

            var q = from p in db.Allinttypes select p.DbLinq_EnumTest;
            string sql_string = db.GetQueryText(q);

            DbLinq_EnumTest enumValue = q.First();
            Assert.IsTrue(enumValue > 0, "Expected enum value>0 in AllTypes, got enumValue=" + enumValue);
        }
#endif

        [Test]
        public void AT6_ReadBlob()
        {
            //DbLinq could not read byte[]
            //This test was contributed by Anatoli Koutsevol 

            Console.WriteLine("from p in db.Othertypes orderby p.DateTime_ select p.blob;");
            AllTypes db = CreateDB();

            var result = from p in db.Othertypes orderby p.DateTime select p.Blob;
            foreach (var blob in result)
            {
                Console.WriteLine("blob[{0}]", blob.Length);
            }
            Console.WriteLine("Press enter to continue.");
        }

        [Test]
        public void Test_Unknown()
        {
            AllTypes db = CreateDB();

            var result = from p in db.Othertypes
                         orderby p.DateTime
                         select
                             p.Blob;
            foreach (var blob in result)
            {
                Console.WriteLine("blob[{0}]", blob.Length);
            }
        }
    }
}

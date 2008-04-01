using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NUnit.Framework;
using AllTypesExample;


namespace Test_NUnit_Mysql
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
            string DbServer = "localhost";
            string connStr = string.Format("server={0};user id=LinqUser; password=linq2; database=AllTypes", DbServer);

            //return CreateDB(System.Data.ConnectionState.Closed);
            AllTypes db = new AllTypes(connStr);
            db.Log = Console.Out;
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

            var q = from p in db.Floattypes select p.decimalN;
            int count = q.ToList().Count;
            Assert.IsTrue(count > 0, "Expected some entries in AllTypes, got none");
        }

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

        [Test]
        public void Test(string connStr)
        {
            Console.Clear();
            Console.WriteLine("from p in db.Othertypes orderby p.DateTime_ select p.blob;");
            AllTypes db = new AllTypes(connStr);
            var result = from p in db.Othertypes
                         orderby p.DateTime_
                         select
                             p.blob;
            foreach (var blob in result)
            {
                Console.WriteLine("blob[{0}]", blob.Length);
            }
            Console.WriteLine("Press enter to continue.");
            Console.ReadLine();
        }
    }
}

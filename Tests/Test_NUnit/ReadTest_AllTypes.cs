using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NUnit.Framework;
using Client2.user;


namespace Test_NUnit
{
    /// <summary>
    /// this test will exercise reading of columns of types such as decimal, decimal?, DateTime?
    /// </summary>
    [TestFixture]
    public class ReadTest_AllTypes : TestBase
    {

        [Test]
        public void AT1_SelectRow()
        {
            LinqTestDB db = CreateDB();

            var q = from p in db.Alltypes select p;
            int count = q.ToList().Count;
            Assert.IsTrue(count > 0, "Expected some entries in AllTypes, got none");
        }

        [Test]
        public void AT2_SelectDateTimeN()
        {
            LinqTestDB db = CreateDB();

            var q = from p in db.Alltypes select p.DateTimeN;
            int count = q.ToList().Count;
            Assert.IsTrue(count > 0, "Expected some entries in AllTypes, got none");
        }

        [Test]
        public void AT3_SelectDecimalN()
        {
            LinqTestDB db = CreateDB();

            var q = from p in db.Alltypes select p.decimalN;
            int count = q.ToList().Count;
            Assert.IsTrue(count > 0, "Expected some entries in AllTypes, got none");
        }

        [Test]
        public void AT4_SelectEnum()
        {
            LinqTestDB db = CreateDB();

            var q = from p in db.Alltypes select p.DbLinq_EnumTest;
            int count = q.ToList().Count;
            Assert.IsTrue(count > 0, "Expected some enums in AllTypes, got none");
        }

        [Test]
        public void AT5_SelectEnum_()
        {
            LinqTestDB db = CreateDB();

            var q = from p in db.Alltypes select p.DbLinq_EnumTest;
            string sql_string = db.GetQueryText(q);

            DbLinq_EnumTest enumValue = q.First();
            Assert.IsTrue(enumValue > 0, "Expected enum value>0 in AllTypes, got enumValue=" + enumValue);
        }



    }
}

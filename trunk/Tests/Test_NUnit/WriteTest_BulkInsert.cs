using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Client2.user;

namespace Test_NUnit
{
    [TestFixture]
    public class WriteTest_BulkInsert : TestBase
    {
        [Test]
        public void BI01_InsertProducts()
        {
            int initialCount = 0, countAfterBulkInsert = 0;

            LinqTestDB db = CreateDB();
            initialCount = db.Products.Count();

            DBLinq.vendor.Vendor.UseBulkInsert[db.Products] = 3; //insert three rows at a time

            db.Products.Add(new Product(0, "tmp_ProductA", 0, 0, "00", 1M, null, 1, 2, false));
            db.Products.Add(new Product(0, "tmp_ProductB", 0, 0, "11", 1M, null, 1, 2, false));
            db.Products.Add(new Product(0, "tmp_ProductC", 0, 0, "22", 1M, null, 1, 2, false));
            db.Products.Add(new Product(0, "tmp_ProductD", 0, 0, "33", 1M, null, 1, 2, false));
            db.SubmitChanges();

            //confirm that we indeed inserted four rows:
            LinqTestDB db2 = CreateDB();
            countAfterBulkInsert = db2.Products.Count();
            Assert.IsTrue(countAfterBulkInsert == initialCount + 4);

            //clean up
            base.ExecuteNonQuery("DELETE FROM Products WHERE ProductName LIKE 'tmp_%'");
        }
    }
}

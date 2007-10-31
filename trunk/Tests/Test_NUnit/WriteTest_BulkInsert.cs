using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using nwind;

namespace Test_NUnit
{
    [TestFixture]
    public class WriteTest_BulkInsert : TestBase
    {
        [Test]
        public void BI01_InsertProducts()
        {
            int initialCount = 0, countAfterBulkInsert = 0;

            Northwind db = CreateDB();
            initialCount = db.Products.Count();

            DBLinq.vendor.Vendor.UseBulkInsert[db.Products] = 3; //insert three rows at a time

            db.Products.Add(new Product(0, "tmp_ProductA", 1, 1, "00", 1M, null, 1, 2, false));
            db.Products.Add(new Product(0, "tmp_ProductB", 1, 1, "11", 1M, null, 1, 2, false));
            db.Products.Add(new Product(0, "tmp_ProductC", 1, 1, "22", 1M, null, 1, 2, false));
            db.Products.Add(new Product(0, "tmp_ProductD", 1, 1, "33", 1M, null, 1, 2, false));
            db.SubmitChanges();

            //confirm that we indeed inserted four rows:
            Northwind db2 = CreateDB();
            countAfterBulkInsert = db2.Products.Count();
            Assert.IsTrue(countAfterBulkInsert == initialCount + 4);

            //clean up
            base.ExecuteNonQuery("DELETE FROM Products WHERE ProductName LIKE 'tmp_%'");
        }
    }
}

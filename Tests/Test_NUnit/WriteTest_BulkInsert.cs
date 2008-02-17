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
#if !SQLITE
            int initialCount = 0, countAfterBulkInsert = 0;

            Northwind db = CreateDB();
            initialCount = db.Products.Count();

            //DbLinq.vendor.mysql.MySqlVendor.UseBulkInsert[db.Products] = 3; //insert three rows at a time
            // picrap: inject this information in the IVendor (and check this is necessary)

            db.Products.Add(NewProduct("tmp_ProductA"));
            db.Products.Add(NewProduct("tmp_ProductB"));
            db.Products.Add(NewProduct("tmp_ProductC"));
            db.Products.Add(NewProduct("tmp_ProductD"));
            db.SubmitChanges();

            //confirm that we indeed inserted four rows:
            Northwind db2 = CreateDB();
            countAfterBulkInsert = db2.Products.Count();
            Assert.IsTrue(countAfterBulkInsert == initialCount + 4);

            //clean up
            base.ExecuteNonQuery("DELETE FROM Products WHERE ProductName LIKE 'tmp_%'");
#endif
        }
    }
}

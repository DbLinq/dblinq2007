using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using nwind;
using System.Linq.Dynamic;

namespace Test_NUnit
{
    [TestFixture]
    public class DynamicLinqTest : TestBase
    {
        [Test]
        public void DL1_Products()
        {
            Northwind db = CreateDB();

            var q = db.Products.Where("SupplierID=1 And UnitsInStock>2")
                .OrderBy("ProductID");
            var list = q.ToList();
            Assert.IsTrue(list.Count > 0, "Expected results from dynamic query");
        }

#if DECIDE_IF_THIS_CAST_IS_LEGAL
        [Test]
        public void DL2_Cast()
        {
            Northwind db = CreateDB();
            
            IQueryable<Customer> q =
            db.Customers.Select("new(CustomerID,City)").Cast<Customer>();

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0, "Expected results from dynamic query");
        }
#endif
    }
}

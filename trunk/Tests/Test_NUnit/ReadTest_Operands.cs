using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Client2.user;

#if POSTGRES
using xint = System.Int32;
#else
using xint = System.UInt32;
#endif

namespace Test_NUnit
{
    /// <summary>
    /// this test class will exercise various operands, such as 'a&&b', 'a>=b', ""+a, etc.
    /// </summary>
    [TestFixture]
    public class ReadTest_Operands : TestBase
    {

        [Test]
        public void H1_SelectConcat()
        {
            LinqTestDB db = CreateDB();

            var q = from p in db.Products select p.ProductName + " " + p.SupplierID;
            int count = 0;
            foreach (string s in q)
            {
                bool ok = Char.IsLetterOrDigit(s[0]) && s.Contains(' ');
                Assert.IsTrue(ok, "Concat string should start with product name, instead got:" + s);
                count++;
            }
            Assert.IsTrue(count > 0, "Expected concat strings, got none");
        }

        [Test]
        public void H2_SelectGreaterOrEqual()
        {
            LinqTestDB db = CreateDB();

            var q = db.Products.Where(p => p.ProductID >= 20);
            int count = 0;
            foreach (Product p in q)
            {
                Assert.IsTrue(p.ProductID >= 20, "Failed on ProductID>=20");
                count++;
            }
            Assert.IsTrue(count > 0, "Expected some products with ProductID>=20, got none");
        }

        public struct ProductWrapper1
        {
            public xint ProductID { get; set; }
            public xint SupplierID { get; set; }
        }

        public class ProductWrapper2
        {
            public xint ProductID { get; set; }
            public xint SupplierID { get; set; }
        }

        public class ProductWrapper3
        {
            public xint ProductID { get; set; }
            public xint SupplierID { get; set; }
            public ProductWrapper3(xint p, xint s) { ProductID = p; SupplierID = s; }
            public ProductWrapper3(xint p, xint s, bool unused) { ProductID = p; SupplierID = s; }
        }

        [Test]
        public void H3_Select_MemberInit_Struct()
        {
            LinqTestDB db = CreateDB();
            var q = from p in db.Products
                    where p.ProductID > 20
                    select new ProductWrapper1 { ProductID = p.ProductID, SupplierID = p.SupplierID };
            int count = 0;
            foreach (ProductWrapper1 p in q)
            {
                Assert.IsTrue(p.ProductID > 20, "Failed on ProductID>=20");
                count++;
            }
            Assert.IsTrue(count > 0, "Expected some products with ProductID>=20, got none");
        }

        [Test]
        public void H4_Select_MemberInit_Class()
        {
            LinqTestDB db = CreateDB();
            var q = from p in db.Products
                    where p.ProductID > 20
                    select new ProductWrapper2 { ProductID = p.ProductID, SupplierID = p.SupplierID };
            int count = 0;
            foreach (ProductWrapper2 p in q)
            {
                Assert.IsTrue(p.ProductID > 20, "Failed on ProductID>=20");
                count++;
            }
            Assert.IsTrue(count > 0, "Expected some products with ProductID>=20, got none");
        }

        [Test]
        public void H5_Select_MemberInit_Class2()
        {
            LinqTestDB db = CreateDB();
            var q = from p in db.Products
                    where p.ProductID > 20
                    select new ProductWrapper3 ( p.ProductID, p.SupplierID );
            int count = 0;
            foreach (ProductWrapper3 p in q)
            {
                Assert.IsTrue(p.ProductID > 20, "Failed on ProductID>=20");
                count++;
            }
            Assert.IsTrue(count > 0, "Expected some products with ProductID>=20, got none");
        }

        [Test]
        public void I1_GetQueryText()
        {
            LinqTestDB db = CreateDB();

            var q = from p in db.Products where p.ProductID > 1 select p;
            string s = db.GetQueryText(q); //MTable.GetQueryText()
        }

        [Test]
        public void I2_GetQueryText()
        {
            LinqTestDB db = CreateDB();

            var q = from p in db.Products select p.ProductName;
            string s = db.GetQueryText(q); //MTable_Projected.GetQueryText()
        }


    }
}

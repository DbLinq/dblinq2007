﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using nwind;

#if ORACLE
using xint = System.Int32;
#elif POSTGRES
using xint = System.Int32;
#else
using MySql.Data.MySqlClient;
using xint = System.UInt32;
#endif

namespace Test_NUnit
{
    [TestFixture]
    public class WriteTest : TestBase
    {

        #region Tests 'E' test live object cache
        [Test]
        public void E1_LiveObjectsAreUnique()
        {
            //grab an object twice, make sure we get the same object each time
            Northwind db = CreateDB();
            var q = from p in db.Products select p;
            Product pen1 = q.First();
            Product pen2 = q.First();
            string uniqueStr = "Unique" + Environment.TickCount;
            pen1.QuantityPerUnit = uniqueStr;
            bool isSameObject1 = pen2.QuantityPerUnit == uniqueStr;
            Assert.IsTrue(isSameObject1, "Expected pen1 and pen2 to be the same live object, but their fields are different");
            object oPen1 = pen1;
            object oPen2 = pen2;
            bool isSameObject2 = oPen1 == oPen2;
            Assert.IsTrue(isSameObject2, "Expected pen1 and pen2 to be the same live object, but their fields are different");
        }

        [Test]
        public void E2_LiveObjectsAreUnique_Scalar()
        {
            //grab an object twice, make sure we get the same object each time
            Northwind db = CreateDB();
            var q = from p in db.Products select p;
            Product pen1 = q.First(p => p.ProductName == "Pen");
            Product pen2 = q.Single(p => p.ProductName == "Pen");
            bool isSame = object.ReferenceEquals(pen1, pen2);
            Assert.IsTrue(isSame, "Expected pen1 and pen2 to be the same live object");
        }

#if MYSQL
        [Test]
        public void E3_UpdateEnum()
        {
            Northwind db = CreateDB();

            var q = from at in db.Alltypes where at.int_ == 1 select at;

            Alltype row = q.First();
            DbLinq_EnumTest newValue = row.DbLinq_EnumTest == DbLinq_EnumTest.BB
                ? DbLinq_EnumTest.CC
                : DbLinq_EnumTest.BB;

            row.DbLinq_EnumTest = newValue;

            db.SubmitChanges();
        }
#endif
        #endregion


        #region Tests 'G' do insertion
        private int insertProduct_priv()
        {
            Northwind db = CreateDB();

            Product newProd = new Product();
            newProd.CategoryID = 1;
            newProd.ProductName = "Temp." + Environment.TickCount;
            newProd.QuantityPerUnit = "33 1/2";
            db.Products.Add(newProd);
            db.SubmitChanges();
            Assert.Greater(newProd.ProductID, 0, "After insertion, ProductID should be non-zero");
            Assert.IsFalse(newProd.IsModified, "After insertion, Product.IsModified should be false");
            return newProd.ProductID; //this test cab be used from delete tests
        }

        [Test]
        public void G1_InsertProduct()
        {
            insertProduct_priv();
        }

        [Test]
        public void G2_DeleteTest()
        {
            int insertedID = insertProduct_priv();
            Assert.Greater(insertedID, 0, "DeleteTest cannot operate if row was not inserted");

            Northwind db = CreateDB();

            var q = from p in db.Products where p.ProductID == insertedID select p;
            List<Product> insertedProducts = q.ToList();
            foreach (Product insertedProd in insertedProducts)
            {
                db.Products.Remove(insertedProd);
            }
            db.SubmitChanges();

            int numLeft = (from p in db.Products where p.ProductID == insertedID select p).Count();
            Assert.AreEqual(numLeft, 0, "After deletion, expected count of Products with ID=" + insertedID + " to be zero, instead got " + numLeft);
        }

        #endregion
    }
}

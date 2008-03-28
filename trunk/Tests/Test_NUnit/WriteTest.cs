﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using nwind;
using Test_NUnit;

#if MYSQL
    namespace Test_NUnit_MySql
#elif ORACLE
    namespace Test_NUnit_Oracle
#elif POSTGRES
    namespace Test_NUnit_PostgreSql
#elif SQLITE
    namespace Test_NUnit_Sqlite
#elif INGRES
    namespace Test_NUnit_Ingres
#else
    #error unknown target
#endif
{
    [SetUpFixture]
    public class WriteTestSetup : TestBase
    {
        [SetUp]
        public void TestSetup()
        {
            Northwind db = CreateDB();
            db.ExecuteCommand("DELETE FROM Products WHERE ProductName like 'temp%'");
        }
    }

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

#if MYSQL && USE_ALLTYPES
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

        [Test]
        public void G3_DeleteTest()
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

        [Test]
        public void G4_DuplicateSubmitTest()
        {
            Northwind db = CreateDB();
            int productCount1 = db.Products.Count();
            Product p_temp = new Product { ProductName = "temp_g4", Discontinued = false };
            db.Products.Add(p_temp);
            db.SubmitChanges();
            db.SubmitChanges();
            int productCount2 = db.Products.Count();
            Assert.IsTrue(productCount2 == productCount1 + 1, "Expected product count to grow by one");
        }

        /// <summary>
        /// there is a bug in v0.14 where fields cannot be updated to be null.
        /// </summary>
        [Test]
        public void G5_SetFieldToNull()
        {
            string productName = "temp_G5_" + Environment.TickCount;
            Northwind db = CreateDB();
#if ORACLE
            //todo fix Oracle
            Product p1 = new Product { ProductName = productName, Discontinued = false, UnitPrice = 11 };
#else
            Product p1 = new Product { ProductName = productName, Discontinued = false, UnitPrice = 11m };
#endif
            db.Products.Add(p1);
            db.SubmitChanges();

            p1.UnitPrice = null;
            db.SubmitChanges();

            Northwind db3 = CreateDB();
            Product p3 = db3.Products.Single(p => p.ProductName == productName);
            Assert.IsNull(p3.UnitPrice);
        }

        /// <summary>
        /// there is a bug in v0.14 where table Customers cannot be updated,
        /// because quotes where missing around the primaryKey in the UPDATE statement.
        /// </summary>
        [Test]
        public void G6_UpdateTableWithStringPK()
        {
            Northwind db = CreateDB();
            Customer BT = db.Customers.Single(c => c.CustomerID == "BT___");
            BT.Country = "U.K.";
            db.SubmitChanges();
        }

        [Test]
        public void G7_InsertTableWithStringPK()
        {
            Northwind db = CreateDB();
            db.ExecuteCommand("DELETE FROM Customers WHERE CustomerID='TEMP_'");

            Customer custTemp = new Customer
            {
                CustomerID = "TEMP_",
                CompanyName = "Magellan",
                ContactName = "Antonio Pigafetta",
                City = "Lisboa",
            };
            db.Customers.Add(custTemp);
            db.SubmitChanges();
        }

        [Test]
        public void G8_DeleteTableWithStringPK()
        {
            Northwind db = CreateDB();
            Customer cust = (from c in db.Customers
                             where c.CustomerID == "TEMP_"
                             select c).Single();
            db.Customers.Remove(cust);
            db.SubmitChanges();
        }

        [Test]
        public void G9_UpdateOnlyChangedProperty()
        {
            Northwind db = CreateDB();
            var cust = (from c in db.Customers
                        select
                        new Customer
                        {
                            CustomerID = c.CustomerID,
                            City = c.City

                        }).First();

            var old = cust.City;
            cust.City = "Tallinn";
            db.SubmitChanges();
            db.SubmitChanges(); // A second call does not update anything

            //exposes bug:
            //Npgsql.NpgsqlException was unhandled
            //Message="ERROR: 23502: null value in column \"companyname\" violates not-null constraint" 
            cust.City = old;
            db.SubmitChanges();

        }


#if POSTGRES
        public class Northwind1 : Northwind {
          public Northwind1(System.Data.IDbConnection connection)
            : base(connection) { }

          [System.Data.Linq.Mapping.Table(Name = "Cust1")]
          public class Cust1 {
            [DbLinq.Linq.Mapping.AutoGenId]
            string _customerid;

            [System.Data.Linq.Mapping.Column(Storage = "_customerid",
            Name = "customerid", IsPrimaryKey = true,
            DbType = "char(10)",
            IsDbGenerated = true,
            Expression = "nextval('seq8')")]
            public string CustomerId {
              get { return _customerid; }
              set { _customerid = value; }
            }

            // Dummy property is required only as workaround over empty insert list bug
            // If this bug is fixed this may be removed
            string _dummy;
            [System.Data.Linq.Mapping.Column(Storage = "_dummy",
            DbType = "text", Name = "dummy")]
            public string Dummy {
              get;
              set;
            }

          }

          public DbLinq.Linq.Table<Cust1> Cust1s {

            get {
              return base.GetTable<Cust1>();
            }
          }
        }

        [Test]
        public void G10_InsertCharSerialPrimaryKey() {
          Northwind dbo = CreateDB();
          Northwind1 db = new Northwind1(dbo.DatabaseContext.Connection);
          db.ExecuteCommand(@"create sequence seq8;
create temp table cust1 ( CustomerID char(10) DEFAULT nextval('seq8'),
dummy text
);
");

          DbLinq.Linq.Table<Northwind1.Cust1> Cust1s =
             db.GetTable<Northwind1.Cust1>();

          var Cust1 = new Northwind1.Cust1();
          Cust1.Dummy = "";
          db.Cust1s.Add(Cust1);
          db.SubmitChanges();
          db.ExecuteCommand("drop table cust1; drop sequence seq8;");
          Assert.IsNotNull(Cust1.CustomerId);
        }
#endif 


#if POSTGRES
        public class NorthwindG11 : Northwind {
          public NorthwindG11(System.Data.IDbConnection connection)
            : base(connection) { }

	[System.Data.Linq.Mapping.Table(Name = "rid")]
	public  class Rid :DbLinq.Linq.IModified {

    
          [DbLinq.Linq.Mapping.AutoGenId]
          protected int _id;
          [DbLinq.Linq.Mapping.AutoGenId]
          protected int _reanr;

          [System.Data.Linq.Mapping.Column(Storage = "_id", Name = "id", DbType = "integer(32,0)", IsPrimaryKey = true, IsDbGenerated = true, Expression = "nextval('rid_id1_seq')")]
          public int Id {
            get { return _id; }
            set { _id = value; IsModified = true; }
          }


          [System.Data.Linq.Mapping.Column(Storage = "_reanr", Name = "reanr", DbType = "integer(32,0)", IsDbGenerated = true, CanBeNull = false, Expression = "nextval('rid_reanr_seq')")]
          public int Reanr {
            get { return _reanr; }
            set { _reanr = value; IsModified = true; }
          }
          public bool IsModified { get; set; }

  }

          public DbLinq.Linq.Table<Rid> Rids {
            get {
              return base.GetTable<Rid>();
            }
          }
        }

        [Test]
        public void G11_TwoSequencesInTable() {
          Northwind dbo = CreateDB();
          NorthwindG11 db = new NorthwindG11(dbo.DatabaseContext.Connection);
          db.ExecuteCommand(@"create sequence rid_id1_seq;
create sequence rid_reanr_seq;
create temp table Rid ( id int primary key DEFAULT nextval('rid_id1_seq'),
reanr int DEFAULT nextval('rid_reanr_seq'));
");

          DbLinq.Linq.Table<NorthwindG11.Rid> Rids =
          db.GetTable<NorthwindG11.Rid>();

          var Rid = new NorthwindG11.Rid();
          Rid.Reanr = 22;
          db.Rids.Add(Rid);

          Rid = new NorthwindG11.Rid();
          Rid.Reanr = 23;
          db.Rids.Add(Rid);
          db.SubmitChanges();

          db.ExecuteCommand("drop table rid; drop sequence rid_reanr_seq;drop sequence rid_id1_seq;");
          Assert.AreEqual(Rid.Id,2);
          Assert.AreEqual(Rid.Reanr, 23);
        }
#endif


        #endregion
    }
}

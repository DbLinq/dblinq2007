using System;
using System.Globalization;
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
    /// <summary>
    /// this test class will exercise various operands, such as 'a&&b', 'a>=b', ""+a, etc.
    /// </summary>
    [TestFixture]
    public class ReadTest_Operands : TestBase
    {

        [Test]
        public void H1_SelectConcat()
        {
            Northwind db = CreateDB();

            var q = from p in db.Products select p.ProductName + " " + p.SupplierID;
            int count = 0;
            foreach (string s in q)
            {
                if (s == null)
                    continue; //concat('X',NULL) -> NULL 

                bool ok = Char.IsLetterOrDigit(s[0]) && s.Contains(' ');
                Assert.IsTrue(ok, "Concat string should start with product name, instead got:" + s);
                count++;
            }
            Assert.IsTrue(count > 0, "Expected concat strings, got none");
        }

        [Test]
        public void H2_SelectGreaterOrEqual()
        {
            Northwind db = CreateDB();

            var q = db.Products.Where(p => p.ProductID >= 5);
            int count = 0;
            foreach (Product p in q)
            {
                Assert.IsTrue(p.ProductID >= 5, "Failed on ProductID>=20");
                count++;
            }
            Assert.IsTrue(count > 0, "Expected some products with ProductID>=5, got none");
        }

        public struct ProductWrapper1
        {
            public int ProductID { get; set; }
            public int? SupplierID { get; set; }
        }

        public class ProductWrapper2
        {
            public int ProductID { get; set; }
            public int? SupplierID { get; set; }
        }

        public class ProductWrapper3
        {
            public int ProductID { get; set; }
            public int? SupplierID { get; set; }
            public ProductWrapper3(int p, int? s) { ProductID = p; SupplierID = s; }
            public ProductWrapper3(int p, int? s, bool unused) { ProductID = p; SupplierID = s; }
        }

        [Test]
        public void H3_Select_MemberInit_Struct()
        {
            Northwind db = CreateDB();
            var q = from p in db.Products
                    where p.ProductID > 5
                    select new ProductWrapper1 { ProductID = p.ProductID, SupplierID = p.SupplierID };
            int count = 0;
            foreach (ProductWrapper1 p in q)
            {
                Assert.IsTrue(p.ProductID > 5, "Failed on ProductID>=20");
                count++;
            }
            Assert.IsTrue(count > 0, "Expected some products with ProductID>5, got none");
        }

        [Test]
        public void H4_Select_MemberInit_Class()
        {
            Northwind db = CreateDB();
            var q = from p in db.Products
                    where p.ProductID > 5
                    select new ProductWrapper2 { ProductID = p.ProductID, SupplierID = p.SupplierID };
            int count = 0;
            foreach (ProductWrapper2 p in q)
            {
                Assert.IsTrue(p.ProductID > 5, "Failed on ProductID>=20");
                count++;
            }
            Assert.IsTrue(count > 0, "Expected some products with ProductID>5, got none");
        }

        [Test]
        public void H5_Select_MemberInit_Class2()
        {
            Northwind db = CreateDB();
            var q = from p in db.Products
                    where p.ProductID > 5
                    select new ProductWrapper3(p.ProductID, p.SupplierID);
            int count = 0;
            foreach (ProductWrapper3 p in q)
            {
                Assert.IsTrue(p.ProductID > 5, "Failed on ProductID>=20");
                count++;
            }
            Assert.IsTrue(count > 0, "Expected some products with ProductID>5, got none");
        }

        [Test]
        public void H6_SelectNotEqual()
        {
            Northwind db = CreateDB();
            var q = from p in db.Products
                    where p.ProductID != 1
                    select p;
            int count = 0;
            foreach (Product p in q)
            {
                Assert.IsFalse(p.ProductID == 1, "Failed on ProductID != 1");
                count++;
            }
            Assert.IsTrue(count > 0, "Expected some products with ProductID != 1, got none");
        }


        [Test]
        public void I1_GetQueryText()
        {
            Northwind db = CreateDB();

            var q = from p in db.Products where p.ProductID > 1 select p;
            string s = db.GetQueryText(q); //MTable.GetQueryText()
        }

        [Test]
        public void I2_GetQueryText()
        {
            Northwind db = CreateDB();

            var q = from p in db.Products select p.ProductName;
            string s = db.GetQueryText(q); //MTable_Projected.GetQueryText()
        }

        [Test]
        public void J1_LocalFunction_DateTime_ParseExact()
        {
            Northwind db = CreateDB();

            //Lookup EmployeeID 1:
            //Andy Fuller - HireDate: 1989-01-01 00:00:00

            string hireDate = "1989.01.01";

            var q = from e in db.Employees
                    where e.HireDate == DateTime.ParseExact(hireDate, "yyyy.MM.dd", CultureInfo.InvariantCulture)
                    select e.EmployeeID;
            int empID = q.Single(); //MTable_Projected.GetQueryText()
            Assert.IsTrue(empID == 1);
        }

        //there was a bug where intField1 would be listed multiple times in a select statement:
        public class Class1 { protected int intField1; }
        public class Class2 : Class1 { protected int intField2; }

        [Test]
        public void Z1_AttribHelper_ShouldNotReturnDuplicateFields()
        {
            //Andrus pointed out that one of the internal classes that help with reflection
            //returns fields in duplicate, which kills SQL SELECT and UPDATEs.
            System.Reflection.MemberInfo[] members = DbLinq.Util.AttribHelper.GetMemberFields(typeof(Class2));
            Assert.IsTrue(members.Length == 2);
        }


    }
}

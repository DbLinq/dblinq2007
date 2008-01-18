using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using nwind;

namespace Test_NUnit.Linq_101_Samples
{
    /// <summary>
    /// Source:  http://msdn2.microsoft.com/en-us/vbasic/bb737930.aspx
    /// manually translated from VB into C#.
    /// </summary>
    [TestFixture]
    public class String_Date_functions : TestBase
    {
        [Test(Description = "This sample uses the & operator to concatenate string fields and string literals in forming the Customers' calculated Location value")]
        public void LinqToSqlString01()
        {
            Northwind db = CreateDB();

            var q = from c in db.Customers
                    select new { c.CustomerID, Location = c.City + ", " + c.Country };

            //bool foundBerlin = false;
            foreach (var v in q)
            {
                if (v.Location == "Berlin, Germany")
                {
                    //foundBerlin = true;
                    return;
                }
            }
            Assert.Fail("Expected to find location 'Berlin, Germany'");
        }

        [Test]
        public void LinqToSqlString02()
        {
            Northwind db = CreateDB();

            var q = from p in db.Products
                    where p.ProductName.Length < 10
                    select p;

            List<Product> prods = q.ToList();
            Assert.IsTrue(prods.Count > 0, "Expected some products");
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using nwind;

namespace Test_NUnit.Linq_101_Samples
{
    /// <summary>
    /// Source:  http://msdn2.microsoft.com/en-us/vbasic/bb737931.aspx
    /// manually translated from VB into C#.
    /// </summary>
    [TestFixture]
    public class Object_Identity : TestBase
    {

        /// <summary>
        /// This sample demonstrates how, upon executing the same query twice, you will receive a reference to the same object in memory each time.
        /// </summary>
        [Test(Description = "Object Caching - 1.")]
        public void LinqToSqlObjectIdentity01()
        {
            Northwind db = CreateDB();

            Customer cust1 = db.Customers.First(c => c.CustomerID == "BONAP");

            Customer cust2 = (from c in db.Customers
                              where c.CustomerID == "BONAP"
                              select c).First();

            bool isSameObject = Object.ReferenceEquals(cust1, cust2);
            Assert.IsTrue(isSameObject);
            Assert.IsTrue(cust1.CustomerID=="BONAP", "CustomerID must be BONAP");
        }

    }
}

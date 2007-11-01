using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using nwind;

namespace Test_NUnit.Linq_101_Samples
{
    /// <summary>
    /// Source:  http://msdn2.microsoft.com/en-us/vbasic/bb737922.aspx
    /// manually translated from VB into C#.
    /// </summary>
    [TestFixture]
    public class Count_Sum_Min_Max_Avg : TestBase
    {
        [Test]
        public void LiqnToSqlCount01()
        {
            Northwind db = CreateDB();

            var q = db.Customers.Count();

            Assert.IsTrue(q > 0, "Expect non-zero count");
        }

        [Test]
        public void LiqnToSqlCount02()
        {
            Northwind db = CreateDB();

            var q = (from p in db.Products where !p.Discontinued select p)
                .Count();

            Assert.IsTrue(q > 0, "Expect non-zero count");
        }

    }
}

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
    public class NullTest : TestBase
    {
        [Test]
        public void Null()
        {
            Northwind db = CreateDB();

            var q = from e in db.Employees 
                    where e.ReportsTo==null select e;

            List<Employee> list = q.ToList();
        }

        [Test]
        public void NullableT_HasValue()
        {
            Northwind db = CreateDB();

            var q = from e in db.Employees 
                    where !e.ReportsTo.HasValue select e;

            List<Employee> list = q.ToList();
        }

        [Test]
        public void NullableT_Value()
        {
            Northwind db = CreateDB();

            var q = from e in db.Employees 
                    where !e.ReportsTo.HasValue 
                    select new { e.FirstName, e.LastName, ReportsTo = e.ReportsTo.Value };

            var list = q.ToList();
        }

    }
}

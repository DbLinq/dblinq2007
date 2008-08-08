#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne, Pascal Craponne, Pascal Craponne, Pascal Craponne, Pascal Craponne, Pascal Craponne, Pascal Craponne, Pascal Craponne
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion
using System;
using System.Globalization;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Test_NUnit;


#if !MONO_STRICT
using nwind;
using DbLinq.Data.Linq;
using DataLinq = DbLinq.Data.Linq;
using DbLinq.Logging;
using System.Data.Linq;
#else
using MsNorthwind;
using System.Data.Linq;
using DataLinq = System.Data.Linq;
#endif

#if MYSQL
    namespace Test_NUnit_MySql
#elif ORACLE
#if ODP
        namespace Test_NUnit_OracleODP
#else
        namespace Test_NUnit_Oracle
#endif
#elif POSTGRES
    namespace Test_NUnit_PostgreSql
#elif SQLITE
    namespace Test_NUnit_Sqlite
#elif INGRES
    namespace Test_NUnit_Ingres
#elif MSSQL
#if MONO_STRICT
namespace Test_NUnit_MsSql_Strict
#else
namespace Test_NUnit_MsSql
#endif
#else
#error unknown target
#endif
{
    [TestFixture]
    public class ReadTests_StringFunctions : TestBase
    {
        [Test]
        public void Insert01()
        {
            Northwind db = CreateDB();

            var q = from e in db.Employees
                    select e.LastName.Insert(3, ":");


            var list = q.ToList();
            Assert.IsTrue(list.All(lastname => lastname.Contains(":")));
        }

        [Test]
        public void Insert02()
        {
            Northwind db = CreateDB();

            var q = from e in db.Employees
                    where e.LastName.Insert(3, ":").Contains(":")
                    select e.LastName.Insert(3, ":");


            var list = q.ToList();
            Assert.IsTrue(list.All(lastname => lastname.Contains(":")));
        }

        [Test]
        public void Replace01()
        {
            Northwind db = CreateDB();

            var q = from e in db.Employees
                    where " .".Replace('.', 'a') == " a"
                    select e;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);

        }

        [Test]
        public void Replace02()
        {
            Northwind db = CreateDB();

            var q = from e in db.Employees
                    where " .".Replace(" ", "f") == "f."
                    select e;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }
        [Test]
        public void Replace03()
        {
            Northwind db = CreateDB();

            var q = from e in db.Employees
                    select " .".Replace('.', 'a');

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);

        }

        [Test]
        public void Replace04()
        {
            Northwind db = CreateDB();

            var q = from e in db.Employees
                    select " .".Replace(" ", "f");
            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);

        }
 
        [Test]
        public void H7_String_StartsWith()
        {
            Northwind db = CreateDB();

            var q = from c in db.Customers
                    where c.CustomerID.StartsWith("ALF")
                    select c.CustomerID;

            string custID = q.Single();
            Assert.IsTrue(custID == "ALFKI");
        }

        [Test]
        public void H8_String_StartsWith()
        {
            Northwind db = CreateDB();

            var q = from c in db.Customers
                    where c.CustomerID == "ALFKI"
                    select c.CustomerID.StartsWith("ALF");

            bool matchStart = q.Single();
            Assert.IsTrue(matchStart);
        }

        [Test]
        public void H9_String_EndsWith()
        {
            Northwind db = CreateDB();

            var q = from c in db.Customers
                    where c.CustomerID.EndsWith("LFKI")
                    select c.CustomerID;

            string custID = q.Single();
            Assert.IsTrue(custID == "ALFKI");
        }

        [Test]
        public void H10_String_EndsWith()
        {
            string param = "LFKI";
            Northwind db = CreateDB();

            var q = from c in db.Customers
                    where c.CustomerID.EndsWith(param)
                    select c.CustomerID;

            string custID = q.Single();
            Assert.IsTrue(custID == "ALFKI");
        }


        [Test]
        public void H11_String_StartsWithPercent()
        {
            string param = "%";
            Northwind db = CreateDB();

            var q = from c in db.Customers
                    where c.CustomerID.StartsWith(param)
                    select c.CustomerID;

            int cnt = q.Count();
            Assert.AreEqual(0, cnt);
        }
    }
}

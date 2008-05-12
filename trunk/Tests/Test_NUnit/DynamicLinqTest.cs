#region MIT license
// 
// Copyright (c) 2007-2008 Jiri Moudry
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using nwind;
using System.Linq.Dynamic;
using Test_NUnit;
using System.Linq.Expressions;
using System.Reflection;

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
#else
#error unknown target
#endif
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

        [Test]
        public void DL2_ProductCount()
        {
            Northwind db = CreateDB();

            int numProducts = db.Products.Where("SupplierID=1").Count();
            Assert.IsTrue(numProducts > 0, "Expected results from dynamic query");
        }

        //note:
        //user Sqlite reports problems with DynamicLinq Count() -
        //but neither DL2 nor DL3 tests seem to hit the problem.

        [Test]
        public void DL3_ProductCount()
        {
            Northwind db = CreateDB();

            int numProducts = db.Products.Count();
            Assert.IsTrue(numProducts > 0, "Expected results from dynamic query");
        }

        [Test]
        public void DL4_DynamicAssociationProperty()
        {

            Northwind db = CreateDB();
            var orders = db.GetTable<Order>();
            var res = orders.Select(@"new (OrderID,Customer.ContactName)");

            List<object> list = new List<object>();
            foreach (var u in res)
                list.Add(u);
            Assert.IsTrue(list.Count > 0);
        }


        [Test]
        public void DL5_DynamicAssociatonWithExtensionMethod()
        {

            Northwind db = CreateDB();
            var orders = db.GetTable<Order>();
            var res = orders.SelectDynamic(new string[] { "OrderID", "Customer.ContactName" });

            List<Order> list = res.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test]
        public void DL6_StaticVersionOfDynamicAssociatonWithExtensionMethodTest()
        {

            Northwind db = CreateDB();
            var orders = db.GetTable<Order>().ToArray().AsQueryable();

            var query = from order in orders
                        select new Order
                        {
                            OrderID = order.OrderID,
                            Customer = new Customer
                            {
                                ContactName = order.Customer.ContactName,
                                ContactTitle = order.Customer.ContactTitle
                            }
                        };
            var list = query.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test]
        public void DL7_DynamicAssociatonUsingDoubleProjection()
        {

            Northwind db = CreateDB();

            // Double projection works in Linq-SQL:
            var orders = db.GetTable<Order>().ToArray().AsQueryable();
            var query = orders.SelectDynamic(new string[] { "OrderID", "Customer.ContactName" });
            var list = query.ToList();
            Assert.IsTrue(list.Count > 0);
        }


        //[Test]
        //public void DLX()
        //{
        //    Northwind db = CreateDB();
        //    var q = db.Products.Where("ProductID>1")
        //        .OrderBy("ProductID");
        //    var q2 = q.Count();
        //    var list = q.ToList();
        //    System.Windows.Forms.Form frm = new System.Windows.Forms.Form() { Text = "DynLinq" };
        //    System.Windows.Forms.DataGridView grd = new System.Windows.Forms.DataGridView();
        //    frm.Controls.Add(grd);
        //    grd.DataSource = list;
        //    System.Windows.Forms.Application.Run(frm);
        //}

    }

    // Extension method written by Marc Gravell
    public static class SelectUsingSingleProjection
    {
        public static IQueryable<T> SelectDynamic<T>(this IQueryable<T> source, params string[] propertyNames)
            where T : new()
        {
            Type type = typeof(T);
            var sourceItem = Expression.Parameter(type, "t");
            Expression exp = CreateAndInit(type, sourceItem, propertyNames);
            return source.Select(Expression.Lambda<Func<T, T>>(exp, sourceItem));
        }

        static Expression CreateAndInit(Type type, Expression source, string[] propertyNames)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (source == null) throw new ArgumentNullException("source");
            if (propertyNames == null) throw new ArgumentNullException("propertyNames");

            var newExpr = Expression.New(type.GetConstructor(Type.EmptyTypes));
            // take "Foo.A", "Bar", "Foo.B" to "Foo" ["A","B"], "Bar" []
            var groupedNames = from name in propertyNames
                               let dotIndex = name.IndexOf('.')
                               let primary = dotIndex < 0 ? name : name.Substring(0, dotIndex)
                               let aux = dotIndex < 0 ? null : name.Substring(dotIndex + 1)
                               group aux by primary into grouped
                               select new
                               {
                                   Primary = grouped.Key,
                                   Aux = grouped.Where(x => x != null).ToArray()
                               };
            List<MemberBinding> bindings = new List<MemberBinding>();
            foreach (var grp in groupedNames)
            {
                PropertyInfo dest = type.GetProperty(grp.Primary);
                Expression value, readFrom = Expression.Property(source, grp.Primary);
                if (grp.Aux.Length == 0)
                {
                    value = readFrom;
                }
                else
                {
                    value = CreateAndInit(dest.PropertyType, readFrom, grp.Aux);
                }
                bindings.Add(Expression.Bind(dest, value));
            }
            return Expression.MemberInit(newExpr, bindings);
        }
    }



}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using NUnit.Framework;
using Test_NUnit;
using Test_NUnit.Linq_101_Samples;

#if !MONO_STRICT
using nwind;
using DbLinq.Linq;
#else
using MsNorthwind;
using System.Data.Linq;
#endif

#if MYSQL
    namespace Test_NUnit_MySql.Linq_101_Samples
#elif ORACLE
#if ODP
        namespace Test_NUnit_OracleODP.Linq_101_Samples
#else
        namespace Test_NUnit_Oracle.Linq_101_Samples
#endif
#elif POSTGRES
namespace Test_NUnit_PostgreSql.Linq_101_Samples
#elif SQLITE
    namespace Test_NUnit_Sqlite.Linq_101_Samples
#elif INGRES
    namespace Test_NUnit_Ingres.Linq_101_Samples
#elif MSSQL
    namespace Test_NUnit_MsSql.Linq_101_Samples
#else
#error unknown target
#endif
    {
    /// <summary>
    /// Source:  http://msdn2.microsoft.com/en-us/vbasic/bb737920.aspx
    /// manually translated from VB into C#.
    /// </summary>
    [TestFixture]
    public class AdvancedTest : TestBase
    {
        [Test(Description = "This sample builds a query dynamically to return the contact name of each customer.")]
        public void LinqToSqlAdvanced01()
        {
            Northwind db = CreateDB();

            ParameterExpression param = Expression.Parameter(typeof(Customer), "c");
            Expression selector = Expression.Property(param, typeof(Customer).GetProperty("ContactName"));
            var pred = Expression.Lambda(selector, param);

            var custs = db.Customers;
            var expr = Expression.Call(typeof(Queryable), "Select"
                , new Type[] { typeof(Customer), typeof(string) }, Expression.Constant(custs), pred);
            var query = db.Customers.AsQueryable().Provider.CreateQuery<string>(expr);

            var list = query.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        //TODO - 2,3,4,5

        [Test(Description = "This sample builds a query dynamically to filter for Customers in London.")]
        public void LinqToSqlAdvanced02()
        {
            Northwind db = CreateDB();

            var custs = db.Customers;
            var param = Expression.Parameter(typeof(Customer), "c");
            var right = Expression.Constant("London");
            var left = Expression.Property(param, typeof(Customer).GetProperty("City"));
            var filter = Expression.Equal(left, right);
            var pred = Expression.Lambda(filter, param);

            var expr = Expression.Call(typeof(Queryable), "Where", new Type[] { typeof(Customer) }, Expression.Constant(custs), pred);
            var query = db.Customers.AsQueryable().Provider.CreateQuery<Customer>(expr);

            var list = query.ToList();
            Assert.IsTrue(list.Count > 0, "Got London citiens > 0");
        }

        [Test(Description = "This sample builds a query dynamically to filter for Customers in London and order them by ContactName.")]
        public void LinqToSqlAdvanced03()
        {
            Northwind db = CreateDB();

            var param = Expression.Parameter(typeof(Customer), "c");

            var left = Expression.Property(param, typeof(Customer).GetProperty("City"));
            var right = Expression.Constant("London");
            var filter = Expression.Equal(left, right);
            var pred = Expression.Lambda(filter, param);

            var selector = Expression.Property(param, typeof(Customer).GetProperty("ContactName"));
            IQueryable custs = db.Customers;
            var expr = Expression.Call(typeof(Queryable), "Where", new Type[] { typeof(Customer) }, Expression.Constant(custs), pred);
            expr = Expression.Call(typeof(Queryable), "OrderBy", new Type[] { typeof(Customer), typeof(String) }, custs.Expression, Expression.Lambda(Expression.Property(param, "ContactName"), param));
            var query = db.Customers.AsQueryable().Provider.CreateQuery<Customer>(expr);

            var list = query.ToList();
            Assert.IsTrue(list.Count > 0);
        }


        [Test(Description = "This sample dynamically builds a Union to return a sequence of all countries where either a customer or an employee live.")]
        public void LinqToSqlAdvanced04()
        {
            Northwind db = CreateDB();

            var custs = db.Customers;
            var param1 = Expression.Parameter(typeof(Customer), "e");
            var left1 = Expression.Property(param1, typeof(Customer).GetProperty("City"));
            var pred1 = Expression.Lambda(left1, param1);

            var employees = db.Employees;
            var param2 = Expression.Parameter(typeof(Employee), "c");
            var left2 = Expression.Property(param2, typeof(Employee).GetProperty("City"));
            var pred2 = Expression.Lambda(left2, param2);

            var expr1 = Expression.Call(typeof(Queryable), "Select", new Type[] { typeof(Customer), typeof(String) }, Expression.Constant(custs), pred1);
            var expr2 = Expression.Call(typeof(Queryable), "Select", new Type[] { typeof(Employee), typeof(String) }, Expression.Constant(employees), pred2);

            var q1 = db.Customers.AsQueryable().Provider.CreateQuery<String>(expr1);
            var q2 = db.Employees.AsQueryable().Provider.CreateQuery<String>(expr2);

            var q3 = q1.Union(q2);

            Assert.Greater(q1.Count(), 0);
            Assert.IsTrue(q1.Count() + q2.Count() >= q3.Count());

        }


        [Linq101SamplesModified("Replaced Contact by Customer")]
        [Test(Description="This sample demonstrates how we insert a new Contact and retrieve the newly assigned ContactID from the database.")]
        public void LinqToSqlAdvanced05()
        {
            Northwind db = CreateDB();

            Console.WriteLine("ContactID is marked as an identity column");
            var con = new Customer() { CompanyName = "New Era", Phone = "(123)-456-7890" };

            db.Customers.InsertOnSubmit(con);
            db.SubmitChanges();

            Console.WriteLine();
            Console.WriteLine("The ContactID of the new record is {0}", con.CustomerID);

            Customer customerReloaded=db.Customers.First(c=>c.CustomerID==con.CustomerID);
            Assert.AreEqual(customerReloaded.CompanyName, con.CompanyName);
            Assert.AreEqual(customerReloaded.Phone, con.Phone);
        }


        [Test(Description = "This sample uses orderbyDescending and Take to return the discontinued products of the top 10 most expensive products")]
        public void LinqToSqlAdvanced06()
        {
            Northwind db = CreateDB();
#if INGRES && !MONO_STRICT
            var prods = from p in db.Products.OrderByDescending(p=> p.UnitPrice).Take(10) 
                       where p.Discontinued == 1 select p;
#else
            var prods = from p in db.Products.OrderByDescending(p => p.UnitPrice).Take(10)
                        where p.Discontinued
                        select p;
#endif

            var list = prods.ToList();
            Assert.IsTrue(list.Count > 0);
        }


    }
}

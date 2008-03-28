using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using NUnit.Framework;
using nwind;
using Test_NUnit;

#if MYSQL
    namespace Test_NUnit_MySql.Linq_101_Samples
#elif ORACLE
    namespace Test_NUnit_Oracle.Linq_101_Samples
#elif POSTGRES
    namespace Test_NUnit_PostgreSql.Linq_101_Samples
#elif SQLITE
    namespace Test_NUnit_Sqlite.Linq_101_Samples
#elif INGRES
    namespace Test_NUnit_Ingres.Linq_101_Samples
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
        [Test(Description = "his sample builds a query dynamically to return the contact name of each customer.")]
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

        [Test(Description = "This sample uses orderbyDescending and Take to return the discontinued products of the top 10 most expensive products")]
        public void LinqToSqlAdvanced06()
        {
            Northwind db = CreateDB();

            var prods = from p in db.Products.OrderByDescending(p=> p.UnitPrice).Take(10) 
                       where p.Discontinued select p;

            var list = prods.ToList();
            Assert.IsTrue(list.Count > 0);
        }


    }
}

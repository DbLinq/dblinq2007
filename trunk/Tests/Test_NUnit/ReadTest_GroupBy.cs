using System;
using System.Collections.Generic;
using System.Text;
using System.Query;
using System.Xml.XLinq;
using System.Data.DLinq;
using NUnit.Framework;
#if ORACLE
using ClientCodeOra;
using xint = System.Int32;
#elif POSTGRES
using Client2.user;
using xint = System.UInt32;
#else
using MySql.Data.MySqlClient;
using Client2.user;
using xint = System.UInt32;
#endif

namespace Test_NUnit
{
    [TestFixture]
    public class ReadTest_GroupBy
    {
#if ORACLE
        const string connStr = "server=localhost;user id=system; password=linq2";
#else
        const string connStr = "server=localhost;user id=LinqUser; password=linq2; database=LinqTestDB";
        
        //MySqlConnection _conn;
        //public MySqlConnection Conn 
        //{ 
        //    get 
        //    { 
        //        if(_conn==null){ _conn=new MySqlConnection(connStr); _conn.Open(); }
        //        return _conn;
        //    }
        //}
#endif

        //[Test]
        //public void G01_SimpleGroup()
        //{
        //    LinqTestDB db = new LinqTestDB(connStr);
        //    var q2 =
        //        from c in db.Customers
        //        //where c.Country == "France"
        //        group new {c.PostalCode, c.ContactName} by c.City into g
        //        select new {g.Key.Length, g};
        //        //select new {42,g};
        //    var q3 = q2.ToList();
        //}
        [Test]
        public object G01_SimpleGroup()
        {
            //Customer c0 = new Customer { City="London", ContactName="Bob", PostalCode="E14" };
            //Customer c1 = new Customer { City="London", ContactName="Qag", PostalCode="SG2" };
            Customer[] customers = null; //new Customer[]{ c0,c1 };
            var q2 =
                from c in customers
                group new {c.PostalCode, c.ContactName} by c.City into g
                select new {g.Key, g};
            return q2;
            //var q3 = q2.ToList();
            //foreach(var v in q3)
            //{
            //    Console.WriteLine("V="+v);
            //}
        }

        //[Test]
        //public void G02_OrderCountByCustomerID()
        //{
        //    LinqTestDB db = new LinqTestDB(connStr);

        //    var q2 = from o in db.Orders
        //            group o by o.CustomerID into g
        //            //where g.Count()>1
        //            select new { g.Key , OrderCount = g.Count() };

        //    var lst = q2.ToList();
        //    Assert.Greater(lst.Count,0,"Expected some grouped order results");
        //    var result0 = lst[0];
        //    Assert.Greater(result0.Key, 0,"Key must be > 0");
        //    Assert.Greater(result0.OrderCount, 0,"Count must be > 0");
        //            //select new { g.Key , SumPerCustomer = g.Sum(o2=>o2.OrderID) };
        //}

        //[Test]
        //public void G03_OrderCountByCustomerID_Where()
        //{
        //    LinqTestDB db = new LinqTestDB(connStr);
        //    var q2 = from o in db.Orders
        //            group o by o.CustomerID into g
        //            where g.Count()>1
        //            select new { g.Key , OrderCount = g.Count() };
        //
        //    var lst = q2.ToList();
        //    Assert.Greater(lst.Count,0,"Expected some grouped order results");
        //    var result0 = lst[0];
        //    Assert.Greater(result0.Key, 0,"Key must be > 0");
        //    Assert.Greater(result0.OrderCount, 0,"Count must be > 0");
        //            //select new { g.Key , SumPerCustomer = g.Sum(o2=>o2.OrderID) };
        //}

        //[Test]
        //public void G04_OrderSumByCustomerID()
        //{
        //    LinqTestDB db = new LinqTestDB(connStr);
        //    var q2 = from o in db.Orders
        //            group o by o.CustomerID into g
        //            //where g.Count()>1
        //            select new { g.Key , OrderSum = g.Sum(o=>o.OrderID) };
        //    var lst = q2.ToList();
        //    Assert.Greater(lst.Count,0,"Expected some grouped order results");
        //    var result0 = lst[0];
        //    Assert.Greater(result0.Key, 0,"Key must be > 0");
        //    Assert.Greater(result0.OrderSum, 0f,"OrderSum must be > 0");
        //            //select new { g.Key , SumPerCustomer = g.Sum(o2=>o2.OrderID) };
        //}


    }
}

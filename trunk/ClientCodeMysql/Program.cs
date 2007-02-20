using System;
using System.Expressions;
using System.Collections.Generic;
using System.Text;
using System.Query;
using System.Xml.XLinq;
using System.Data.DLinq;
using Client2.user;
using MySql.Data.MySqlClient;
//using LinqMysql.mysql;

namespace ClientCode2
{
    class Program
    {
        static void Main(string[] args)
        {
            testProj();

            string connStr = "server=localhost;user id=LinqUser; password=linq2; database=LinqTestDB";
            MySqlConnection conn = new MySqlConnection(connStr);
            conn.Open();
            string sql = "";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            //TestContext db = new TestContext(connStr);
            LinqTestDB db = new LinqTestDB(connStr);
            //var q = from at in db.alltypes select at;
            //var q = from p in db.products orderby p.ProductName select p;
            //var q = from c in db.customers from o in c.Orders 
            //        where c.City == "London" select new { c, o };

            uint insertedID = 7;
            //var q = from p in db.Products where p.ProductID==insertedID select p;
            var q = from p in db.Products where p.ProductID==insertedID select p;
            int ii = q.Count();
            //var q = from c in db.Customers from o in c.Orders where c.City == "London" select new { c, o };
            //It’s also possible to do the reverse.
            //var q1 = from c in cc where c.CustomerID==0 select c.CustomerID.Select(0);
            //var q = from o in db.Orders where o.Customer.City == "London" select new { c = o.Customer, o };


            //string queryText = db.GetQueryText(q);
            //Console.WriteLine("User sees sql:"+queryText);


            foreach(var v in q){
                Console.WriteLine("OBJ:"+v);
            }
            //db.SaveChanges();
        }

        void insertChildRecord()
        {
        }

        //Customer prototypeLiveObjectCache()
        //{
        //    Dictionary<int,Customer> liveObjects;
        //    Customer c1 = null;//fromDB();
        //    Customer c2;
        //    //if(liveObjects.TryGetValue(c1.CustomerID, out c2))
        //    //{
        //    //    return c2; //discard c1
        //    //}
        //    //liveObjects[c1.CustomerID] = c1;
        //    return c1;
        //}

        static void testProj()
        {
            //A[] aaa = new A[]{ new A() };
            //B[] bbb = new B[]{ new B() };
            //var q = from a in aaa from _a in bbb select new {a,_a};
            //foreach(var v in q){
            //    Console.WriteLine("OBJ:"+v);
            //}
            
        }
    }
    public class A { }
    public class B { }
}

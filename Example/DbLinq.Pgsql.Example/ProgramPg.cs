using System;
using System.Expressions;
using System.Collections.Generic;
using System.Text;
using System.Query;
using System.Xml.XLinq;
using System.Data.DLinq;
using Client2.user;
using Npgsql;

namespace ClientCode2
{
    class Program
    {
        static void Main(string[] args)
        {

            string connStr = "server=localhost;user id=LinqUser; password=linq2; database=LinqTestDB";
            //NpgsqlConnection conn = new NpgsqlConnection(connStr);
            //conn.Open();
            ////string sql = "INSERT City (Name) VALUES ('B'); SELECT @@IDENTITY";
            //string sql = "INSERT City (Name) VALUES ('C1'), ('C2'); SELECT @@IDENTITY; SELECT @@IDENTITY";
            //NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            ////object obj1 = cmd.ExecuteScalar();
            ////string s1 = obj1.ToString();
            //NpgsqlDataReader rdr = cmd.ExecuteReader();
            //int fields = rdr.FieldCount;
            //while(rdr.Read())
            //{
            //    object obj1 = rdr.GetValue(0);
            //    string s1 = obj1.ToString();
            //}

            //TestContext db = new TestContext(connStr);
            int[] arr = new int[]{ 2,3,2,5 };
            var q3 = from a in arr select a.ToString()+"i";
            var bb = q3.ToList();

            //arr.
            //var q7 = arr.Any(.Distinct().ToArray();

            LinqTestDB db = new LinqTestDB(connStr);
            //var q = from at in db.alltypes select at;
            //var q = from p in db.products orderby p.ProductName select p;
            //var q = from c in db.customers from o in c.Orders 
            //        where c.City == "London" select new { c, o };

            var q4 = from p in db.Products select p.ProductName+p.ProductID;
            var q5 = q4.ToList();

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

    }
}

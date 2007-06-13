using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Linq;
using System.Linq;

using MySql.Data.MySqlClient;

using Client2.user;  // contains LinqTestDB context

namespace DbLinq.MySql.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            string connStr = "server=localhost;user id=LinqUser; password=linq2; database=linqtestdb";
            MySqlConnection conn = new MySqlConnection(connStr);
            conn.Open();
            //string sql = "INSERT City (Name) VALUES ('B'); SELECT @@IDENTITY";
            string sql = "INSERT City (Name) VALUES ('C1'), ('C2'); SELECT @@IDENTITY; SELECT @@IDENTITY";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            //object obj1 = cmd.ExecuteScalar();
            //string s1 = obj1.ToString();
            MySqlDataReader rdr = cmd.ExecuteReader();
            int fields = rdr.FieldCount;
            while(rdr.Read())
            {
                object obj1 = rdr.GetValue(0);
                string s1 = obj1.ToString();
            }

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
    }
}

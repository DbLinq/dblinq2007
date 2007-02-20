using System;
using System.Collections.Generic;
using System.Text;
using System.Query;
using System.Xml.XLinq;
using System.Data.DLinq;

#if DEBUG
using System.Data.OracleClient;
using XSqlCommand = System.Data.OracleClient.OracleCommand;
#endif

namespace ClientCodeOra
{
    class Program
    {
        static void Main(string[] args)
        {
            string connStr = "server=localhost;user=system;password=linq2";
            LinqTestDB db = new LinqTestDB(connStr);
            //var q = from at in db.alltypes select at;
            //var q = from p in db.products orderby p.ProductName select p;
            //var q = from c in db.customers from o in c.Orders 
            //        where c.City == "London" select new { c, o };

            int insertedID = 7;
            //var q = from p in db.Products where p.ProductID==insertedID select p;
            var q = from p in db.Products //where p.ProductID==insertedID 
                    select p;
            //int ii = q.Count();
            //var q = from c in db.Customers from o in c.Orders where c.City == "London" select new { c, o };
            //It’s also possible to do the reverse.
            //var q1 = from c in cc where c.CustomerID==0 select c.CustomerID.Select(0);
            //var q = from o in db.Orders where o.Customer.City == "London" select new { c = o.Customer, o };


            //string queryText = db.GetQueryText(q);
            //Console.WriteLine("User sees sql:"+queryText);


            foreach(var v in q){
                Console.WriteLine("OBJ:"+v);
            }        
        }

        //static void Main()
        //{
        //    //OracleDataReader returning no rows or data even though data exists
        //    //www.devnewsgroups.net/group/microsoft.public.dotnet.framework.adonet/topic3406.aspx

        //    string sql = @"SELECT ProductID FROM products WHERE ProductName=:p1"; //returns no rows
        //    //string sql = @"SELECT * FROM USER_ALL_TABLES"; //returns many rows

        //    OracleConnection conn = new OracleConnection("server=localhost;user=system;password=linq2");
        //    conn.Open();
        //    OracleCommand cmd = new OracleCommand(sql,conn);
        //    cmd.Parameters.Add(":p1", "Pen");
        //    OracleDataReader rdr = cmd.ExecuteReader(System.Data.CommandBehavior.Default);
        //    //OracleDataReader rdr = cmd.exe.exe();
        //    string x = rdr.FieldCount + " "+ rdr.HasRows;
        //}
    }
}

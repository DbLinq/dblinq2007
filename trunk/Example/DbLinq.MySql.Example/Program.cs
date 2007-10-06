#define USE_STORED_PROCS

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Linq;

using MySql.Data.MySqlClient;

using Client2.user;  // contains LinqTestDB context

namespace DbLinq.MySql.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 4)
            {
                Console.WriteLine("Usage: DbLinq.MySql.Example.exe server user password database");
                Console.WriteLine("Debug arguments can be set on project properties in visual studio.");
                Console.WriteLine("Press enter to continue.");
                Console.ReadLine();
                return;
            }

            string connStr = String.Format("server={0};user id={1}; password={2}; database={3}", args);

#if true
            MySqlCommand cmd = new MySqlCommand("select hello(?s)", new MySqlConnection(connStr));
            //cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("?s", "xx");
            cmd.Parameters[0].Direction = ParameterDirection.Input; //.Value = "xx";
            cmd.Connection.Open();
            //MySqlDataReader dr = cmd.ExecuteReader();
            object obj = cmd.ExecuteScalar();
#endif
            // BUG: contexts must to be disposable
            LinqTestDB db = new LinqTestDB(connStr);

#if USE_STORED_PROCS
            string reply0 = db.hello0();
            string reply = db.hello("Pigafetta");
#endif

            Console.Clear();
            Console.WriteLine("from at in db.Alltypes select at;");
            var q1 = from at in db.Alltypes select at;
            foreach (var v in q1)
                ObjectDumper.Write(v);
            Console.WriteLine("Press enter to continue.");
            Console.ReadLine();


            Console.Clear();
            Console.WriteLine("from p in db.Products orderby p.ProductName select p;");
            var q2 = from p in db.Products orderby p.ProductName select p;
            foreach (var v in q2)
                ObjectDumper.Write(v);
            Console.WriteLine("Press enter to continue.");
            Console.ReadLine();

            // BUG: This one throws a null reference for some reason.
            //Console.Clear();
            //var q3 = from c in db.Customers
            //         from o in c.Orders 
            //        where c.City == "London" select new { c, o };
            //foreach (var v in q3)
            //    ObjectDumper.Write(v);
            //Console.ReadLine();

            Console.Clear();
            Console.WriteLine("from p in db.Products where p.ProductID == 7 select p;");
            var q4 = from p in db.Products where p.ProductID == 7 select p;
            foreach (var v in q4)
                ObjectDumper.Write(v);
            Console.WriteLine("Press enter to continue.");
            Console.ReadLine();

            Console.Clear();
            Console.WriteLine("from c in db.Customers from o in c.Orders where c.City == \"London\" select new { c, o };");
            var q5 = from c in db.Customers from o in c.Orders where c.City == "London" select new { c, o };
            foreach (var v in q4)
                ObjectDumper.Write(v);
            Console.WriteLine("Press enter to continue.");
            Console.ReadLine();

            Console.Clear();
            Console.WriteLine("from o in db.Orders where o.Customer.City == \"London\" select new { c = o.Customer, o };");
            var q6 = from o in db.Orders where o.Customer.City == "London" select new { c = o.Customer, o };
            foreach (var v in q4)
                ObjectDumper.Write(v);
            Console.WriteLine("Press enter to continue.");
            Console.ReadLine();

            Console.Clear();
            Console.WriteLine("db.Orders");
            foreach (var v in db.Orders)
                ObjectDumper.Write(v);
            Console.WriteLine("Press enter to continue.");
            Console.ReadLine();

            // BUG: This currently will insert 3 rows when it should insert only 2
            // SubmitChanges isn't clearing the client side transaction data
            Console.Clear();
            Console.WriteLine("db.Orders.Add(new Order { ProductID = 7, CustomerID = 1, OrderDate = DateTime.Now });");            
            db.Orders.Add(new Order { ProductID = 7, CustomerID = 1, OrderDate = DateTime.Now });
            db.SubmitChanges();
            Console.WriteLine("db.Orders.Add(new Order { ProductID = 2, CustomerID = 2, OrderDate = DateTime.Now });");
            db.Orders.Add(new Order { ProductID = 2, CustomerID = 2, OrderDate = DateTime.Now });
            db.SubmitChanges();
            foreach (var v in db.Orders)
                ObjectDumper.Write(v);
            Console.WriteLine("Press enter to continue.");
            Console.ReadLine();

            Console.Clear();
            Console.WriteLine("db.Orders.Remove(db.Orders.First());");
            db.Orders.Remove(db.Orders.First());
            db.SubmitChanges();
            foreach (var v in db.Orders)
                ObjectDumper.Write(v);
            Console.WriteLine("Press enter to continue.");
            Console.ReadLine();

        }
    }
}
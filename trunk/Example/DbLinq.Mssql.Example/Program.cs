using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DBLinq.vendor;

namespace DbLinq.Mssql.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            string connStr = "Data Source=.\\SQLExpress;Integrated Security=True;Initial Catalog=Northwind";
            Northwind db = new Northwind(connStr);

            var regions = db.Regions.ToList();

            Vendor.UseBulkInsert[db.Regions] = true;

            db.Regions.Add(new Region(-1, "tmp_region1"));
            db.Regions.Add(new Region(-2, "tmp_region2"));

            db.SubmitChanges();

        }
    }
}

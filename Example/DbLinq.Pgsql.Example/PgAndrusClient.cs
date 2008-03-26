using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using AndrusDB;

namespace ClientCodePg
{
    public class XReader { public int read() { return 11; } }

    public class PgAndrusClient
    {
        static void Main(string[] args)
        {
            Expression<Func<XReader, Employee>> newExpr = r => r.read() == 0
                ? new HourlyEmployee() { EmployeeID = r.read() } as Employee
                : new SalariedEmployee() { EmployeeID = r.read() } as Employee;


            Console.WriteLine("newExpr=" + newExpr);

            string connStr = "server=localhost;user id=LinqUser; password=linq2; database=andrus";

            using (Andrus db = new Andrus(connStr))
            {
                db.Log = Console.Out;
                foreach (Employee emp in db.Employees)
                    Console.WriteLine(emp.Employeename);
            }

            Console.ReadLine();
            //Andrus db = new Andrus(connStr);
            //db.Log = Console.Out;
            //Char_Pk charpk = db.Char_Pks.Single(c => c.Col1 == "a");
            //charpk.Val1 = 22;
            //db.SubmitChanges();
        }

    }
}

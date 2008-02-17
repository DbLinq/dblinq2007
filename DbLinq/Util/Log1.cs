using System;
using System.Collections.Generic;
using System.Text;

namespace DbLinq.Util
{
    public class Log1
    {
        public static void Info(string msg)
        {
            string msg2 = msg;
            msg2 = msg2.Replace("DbLinq.Linq.","");
            msg2 = msg2.Replace("Client2.","");
            msg2 = msg2.Replace("System.Query.",".");
            msg2 = msg2.Replace("System.Int32","int");
            msg2 = msg2.Replace("System.String","string");
            Console.WriteLine(msg2);
        }
    }
}

////////////////////////////////////////////////////////////////////
//Initial author: Jiri George Moudry, 2006.
//License: LGPL. (Visit http://www.gnu.org)
////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

#if LINQ_PREVIEW_2006
//Visual Studio 2005 with Linq Preview May 2006 - can run on Win2000
using System.Query;
#else
//Visual Studio Orcas - requires WinXP
using System.Linq;
#endif

using SqlMetal.schema;
using SqlMetal.codeGen;

namespace SqlMetal
{
    class Program_Oracle
    {
        static void Main(string[] args)
        {
            if( ! parseArgs(args) ){
                errorExit(); 
                return;
            }

            try 
            {
                //string connStr = string.Format("server={0};user id={1}; password={2}; database={3}; pooling=false"
                SqlMetal.util.Util.InitRenames();
                string connStr = string.Format("server={0};user id={1}; password={2}"
                    , mmConfig.server, mmConfig.user, mmConfig.password, mmConfig.database);

                SqlMetal.schema.oracle.OracleVendor vendorO = new SqlMetal.schema.oracle.OracleVendor();
                DlinqSchema.Database dbSchema = vendorO.LoadSchema();
                if(dbSchema==null)
                {
                    Console.Error.WriteLine("Failed to load schema - exiting");
                    return;
                }

                CodeGenAll codeGen = new CodeGenAll();
                string fileBody = codeGen.generateAll(dbSchema, vendorO.VendorName());
                string fname = mmConfig.database+".cs";
                File.WriteAllText(fname, fileBody);
                Console.WriteLine("SqlMetal: Written file "+fname);
            }
            catch(Exception ex)
            {
                Console.WriteLine("SqlMetal failed:"+ex);
            }
        }

        static bool parseArgs(string[] args)
        {
            #region parseArgs
            string[] knownArgs = { "user", "db", "database", "password", "namespace", "ns", "server", "renamesFile" };
            foreach (string sArg in args)
            {
                int colon = sArg.IndexOf(":");
                if(colon==-1)
                {
                    Console.WriteLine("ERROR - Unknown arg:"+sArg);
                    return false;
                }

                string argName = sArg.Substring(1,colon-1);
                if( knownArgs.Contains(argName))
                {
                    //ok, value stored in mmConfig.cctor
                }
                else
                {
                    Console.WriteLine("ERROR - Unknown arg:"+sArg);
                    return false;
                }

                //if(arg.StartsWith("-user:"))
                //    mmConfig.username = parse(arg);
                //else if(arg.StartsWith("-db:"))
                //    mmConfig.database = parse(arg);
                //else if(arg.StartsWith("-server:"))
                //    mmConfig.server = parse(arg);
                //else if(arg.StartsWith("-password:"))
                //    mmConfig.password = parse(arg);
                //else if(arg.StartsWith("-ns:"))
                //    mmConfig.@namespace = parse(arg);
                //else  {
                //    return false;
                //}
            }

            if(     mmConfig.server==null //|| mmConfig.database==null 
                ||  mmConfig.user==null || mmConfig.password==null)
            {
                Console.WriteLine("ERROR - missing server/username/password");
                return false;
            }
            return true;
            #endregion
        }

        static void errorExit()
        {
            Console.WriteLine("SqlMetal usage:");
            Console.WriteLine("SqlMetal.exe -server:xx -db:yy -user:zz -password:**");
            Console.WriteLine("Result: produces file yy.cs in local directory");
        }

        static string parse(string arg)
        {
            int colon = arg.IndexOf(':');
            string tail = arg.Substring(colon+1);
            return tail;
        }
    }
}

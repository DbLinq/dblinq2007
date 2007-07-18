////////////////////////////////////////////////////////////////////
//Initial author: Jiri George Moudry, 2006.
//License: LGPL. (Visit http://www.gnu.org)
////////////////////////////////////////////////////////////////////

using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using SqlMetal.schema;
using SqlMetal.codeGen;

namespace SqlMetal
{
    class Program
    {
        static void Main(string[] args)
        {
            if( ! parseArgs(args) ){
                errorExit(); 
                return;
            }

            try 
            {
                SqlMetal.util.Util.InitRenames();
                
                DlinqSchema.Database dbSchema = null;

#if SERIALIZATION_TEST
                dbSchema = new DlinqSchema.Database();
                dbSchema.Name = "XX";
                DlinqSchema.ColumnSpecifier primKey = new DlinqSchema.ColumnSpecifier();
                primKey.Name = "PRIM_KEY";
                DlinqSchema.Table tbl = new DlinqSchema.Table();
                tbl.Name = "Aggr";
                tbl.PrimaryKey.Add(primKey);
                DlinqSchema.Schema sch = new DlinqSchema.Schema();
                sch.Name = "dbo";
                sch.Property = "dbo";
                sch.Tables.Add(tbl);
                dbSchema.Schemas.Add(sch);
                XmlSerializer xser = new XmlSerializer(typeof(DlinqSchema.Database));
                System.IO.StringWriter writer = new StringWriter();
                xser.Serialize(writer,dbSchema);
                string s1 = writer.ToString();
                s1 = s1.Replace("xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"", "");
                Console.WriteLine(s1);
#endif

                if (mmConfig.schemaXmlFile == null)
                {
                    string connStr = string.Format("server={0};user id={1}; password={2}; database={3}; pooling=false"
                        , mmConfig.server, mmConfig.user, mmConfig.password, mmConfig.database);

                    //SqlMetal.schema.mysql.MySqlVendor vendorM = new SqlMetal.schema.mysql.MySqlVendor();
                    //string vendorName = vendorM.VendorName();
                    //dbSchema = vendorM.LoadSchema();
                }
                else
                {
                    dbSchema = DlinqSchema.Database.LoadFromFile(mmConfig.schemaXmlFile);
                }

                CodeGenAll codeGen = new CodeGenAll();
                string fileBody = codeGen.generateAll(dbSchema, "Microsoft");
                //string fname = mmConfig.database + ".cs";
                string fname = mmConfig.code==null ? mmConfig.database + ".cs"
                    : (mmConfig.code.EndsWith(".cs") ? mmConfig.code : mmConfig.code+".cs");

                File.WriteAllText(fname, fileBody);
                Console.WriteLine("MysqlMetal: Written file "+fname);
            }
            catch(Exception ex)
            {
                Console.WriteLine("MysqlMetal failed:"+ex);
            }
        }

        static bool parseArgs(string[] args)
        {
            #region parseArgs
            string[] knownArgs = 
            { 
                "user", "db", "database", "password"
                , "namespace", "ns", "server", "renamesFile"
                , "code", "language" 
            };

            bool gotXmlFile = false;
            foreach (string sArg in args)
            {
                if (sArg.ToLower().EndsWith(".xml"))
                {
                    mmConfig.schemaXmlFile = sArg;
                    gotXmlFile = true;
                    continue; //ok
                }
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

            if (gotXmlFile)
                return true;

            if(     mmConfig.server==null || mmConfig.database==null 
                ||  mmConfig.user==null || mmConfig.password==null)
            {
                Console.WriteLine("ERROR - missing server/database/username/password");
                return false;
            }
            return true;
            #endregion
        }

        static void errorExit()
        {
            Console.WriteLine("MysqlMetal usage:");
            Console.WriteLine("MysqlMetal.exe -server:xx -db:yy -user:zz -password:**");
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

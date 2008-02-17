#region MIT License
////////////////////////////////////////////////////////////////////
// MIT license:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//
// Authors:
//        Jiri George Moudry
////////////////////////////////////////////////////////////////////
#endregion

using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using DbLinq.Linq;
using DbLinq.Vendor;
using SqlMetal.CSharpGenerator;

//OK this is rather primitive and needs fixing:
//in one of these namespaces, there is a class Vendor.
//todo: implement VendorFactory, get rid of this garbage.
    //in SQLiteMetal, imports class Vendor
    //in MySqlMetal, imports class Vendor
    //in PgsqlMetal, imports class Vendor
    //in MicrosoftMetal, imports class Vendor

//in MicrosoftMetal, imports class Vendor

//using Vendor = Vendor;
//#if SQLITE
//    using Vendor = SqlMetal.schema.sqlite.Vendor;
//#else
//    using Vendor = SqlMetal.schema.mysql.Vendor;
//#endif

namespace SqlMetal
{
    class Program
    {
        static void Main(string[] args)
        {
            SqlMetalParameters mmConfig;
            try
            {
                mmConfig = new SqlMetalParameters(args);
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(e.Message);
                errorExit();
                return;
            }

            try
            {
                //SqlMetal.util.Util.InitRenames();

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

                //Vendor vendor = new Vendor();
                //string vendorName = vendor.VendorName(); //"Microsoft" or "Postgres" or ...
                // we always need a factory, even if generating from a DBML file, because we need a namespace
                LoaderFactory loaderFactory = new LoaderFactory();
                ISchemaLoader loader = loaderFactory.Load(mmConfig);

                IDictionary<string, string> tableAliases = TableAlias.Load(mmConfig);

                if (mmConfig.SchemaXmlFile == null)
                {
                    //dbSchema = vendor.LoadSchema();
                    dbSchema = loader.Load(mmConfig.Database, tableAliases, mmConfig.SProcs);
                    //SchemaPostprocess.PostProcess_DB(dbSchema);
                }
                else
                {
                    dbSchema = DlinqSchema.Database.LoadFromFile(mmConfig.SchemaXmlFile);
                }

                if (mmConfig.Dbml != null)
                {
                    //we are supposed to write out a DBML file and exit
                    DlinqSchema.Database.SaveDbmlFile(mmConfig.Dbml, dbSchema);
                    Console.WriteLine("Written file " + mmConfig.Dbml);
                    return;
                }

                Generator codeGen = new Generator();
                string fileBody = codeGen.GetAll(dbSchema, loader, mmConfig);
                //string fname = mmConfig.Database + ".cs";

                if (mmConfig.Database.Contains("\""))
                    mmConfig.Database = mmConfig.Database.Replace("\"", "");

                string fname = mmConfig.Code == null ? mmConfig.Database + ".cs"
                    : (mmConfig.Code.EndsWith(".cs") ? mmConfig.Code : mmConfig.Code + ".cs");

                File.WriteAllText(fname, fileBody);
                //Console.WriteLine(vendorName + "Metal: Written file " + fname);
            }
            catch (Exception ex)
            {
                string assyName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
                Console.WriteLine(assyName + " failed:" + ex);
            }

            if (mmConfig.ReadLineAtExit)
            {
                // '-readLineAtExit' flag: useful when running from Visual Studio
                Console.ReadKey();
            }
        }

        static void errorExit()
        {
            string appName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            Console.WriteLine(appName + " usage:");
            Console.WriteLine(appName + ".exe -server:xx -db:yy -user:zz -password:**");
            Console.WriteLine("Result: produces file yy.cs in local directory");
        }
    }
}

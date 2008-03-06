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
using SqlMetal.schema;

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
            SqlMetalParameters parameters;
            try
            {
                parameters = new SqlMetalParameters(args);
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(e.Message);
                PrintUsage();
                return;
            }

            try
            {
                DlinqSchema.Database dbSchema = null;

                // we always need a factory, even if generating from a DBML file, because we need a namespace
                LoaderFactory loaderFactory = new LoaderFactory();
                ISchemaLoader loader = loaderFactory.Load(parameters);

                IDictionary<string, string> tableAliases = TableAlias.Load(parameters);

                if (parameters.SchemaXmlFile == null)
                {
                    dbSchema = loader.Load(parameters.Database, tableAliases, parameters.SProcs);
                    SchemaPostprocess.PostProcess_DB(dbSchema);
                }
                else
                    dbSchema = DlinqSchema.Database.LoadFromFile(parameters.SchemaXmlFile);

                if (parameters.Dbml != null)
                {
                    //we are supposed to write out a DBML file and exit
                    DlinqSchema.Database.SaveDbmlFile(parameters.Dbml, dbSchema);
                    Console.WriteLine("Written file " + parameters.Dbml);
                    return;
                }

                Generator codeGen = new Generator();
                string fileBody = codeGen.GetAll(dbSchema, loader, parameters);

                if (parameters.Database.Contains("\""))
                    parameters.Database = parameters.Database.Replace("\"", "");

                string fname = parameters.Code == null ? parameters.Database + ".cs"
                    : (parameters.Code.EndsWith(".cs") ? parameters.Code : parameters.Code + ".cs");

                File.WriteAllText(fname, fileBody);
            }
            catch (Exception ex)
            {
                string assyName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
                Console.WriteLine(assyName + " failed:" + ex);
            }

            if (parameters.ReadLineAtExit)
            {
                // '-readLineAtExit' flag: useful when running from Visual Studio
                Console.ReadKey();
            }
        }

        static void PrintUsage()
        {
            string appName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            Console.WriteLine(appName + " usage:");
            Console.WriteLine(appName + ".exe -Server:xx -Database:yy -User:zz -Password:** -Provider=[MySql|Oracle|OracleODP|PostgreSql|Sqlite]");
            Console.WriteLine("Result: produces file yy.cs in local directory");
        }
    }
}
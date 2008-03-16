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
using System.IO;
using System.Collections.Generic;
using DbLinq.Schema;
using DbLinq.Vendor;
using SqlMetal.Generator;
using SqlMetal.Generator.Implementation;
using SqlMetal.Schema;

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
                DbLinq.Schema.Dbml.Database dbSchema;

                // we always need a factory, even if generating from a DBML file, because we need a namespace
                var loaderFactory = new LoaderFactory();
                var schemaLoader = loaderFactory.Load(parameters);

                var tableAliases = TableAlias.Load(parameters);
                var dbmlSerializer = new DbmlSerializer();

                if (parameters.SchemaXmlFile == null)
                {
                    dbSchema = schemaLoader.Load(parameters.Database, tableAliases, parameters.SProcs);
                    dbSchema.Provider = parameters.Provider;
                    SchemaPostprocess.PostProcess_DB(dbSchema);
                }
                else
                {
                    using (Stream dbmlFile = File.OpenRead(parameters.SchemaXmlFile))
                    {
                        dbSchema = dbmlSerializer.Read(dbmlFile);
                    }
                }

                if (parameters.Dbml != null)
                {
                    //we are supposed to write out a DBML file and exit
                    using (Stream dbmlFile = File.OpenWrite(parameters.Dbml))
                    {
                        dbmlSerializer.Write(dbmlFile, dbSchema);
                    }
                    Console.WriteLine("Written file " + parameters.Dbml);
                }
                else
                {
                    // picrap: if CSCodeGenerator causes problem, use CSharpCodeGenerator
                    ICodeGenerator codeGen = new CSCodeGenerator();

                    string filename = parameters.Code ?? parameters.Database.Replace("\"", "");
                    if (string.IsNullOrEmpty(Path.GetExtension(filename)))
                        filename += codeGen.Extension;

                    using (StreamWriter streamWriter = new StreamWriter(filename))
                    {
                        var generationContext = new GenerationContext(parameters, schemaLoader);
                        codeGen.Write(streamWriter, dbSchema, generationContext);
                    }
                }
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
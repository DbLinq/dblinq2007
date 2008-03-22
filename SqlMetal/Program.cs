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
using DbLinq.Logging;
using DbLinq.Logging.Implementation;
using DbLinq.Schema;
using DbLinq.Util;
using DbLinq.Vendor;
using SqlMetal.Generator;
using SqlMetal.Generator.Implementation;
using SqlMetal.Schema;
using SqlMetal.Util;

namespace SqlMetal
{
    public class SqlMetalProgram
    {
        static void Main(string[] args)
        {
            SqlMetalProgram program = new SqlMetalProgram();
            program.Process(args);
        }

        public ILogger Logger { get; set; }

        public SqlMetalProgram()
        {
            Logger = LoggerInstance.Default;
        }

        public void Process(string[] args)
        {
            bool readLineAtExit = false;

            try
            {
                foreach (var parameters in SqlMetalParameters.GetBatch(args))
                {
                    ProcessSchema(parameters);
                    if (parameters.ReadLineAtExit)
                        readLineAtExit = true;
                }
            }
            catch (ArgumentException e)
            {
                Logger.Write(Level.Error, e.Message);
                PrintUsage();
                return;
            }


            if (readLineAtExit)
            {
                // '-readLineAtExit' flag: useful when running from Visual Studio
                Console.ReadKey();
            }
        }

        private void ProcessSchema(SqlMetalParameters parameters)
        {
            try
            {
                DbLinq.Schema.Dbml.Database dbSchema;

                // we always need a factory, even if generating from a DBML file, because we need a namespace
                var loaderFactory = new LoaderFactory();
                var schemaLoader = loaderFactory.Load(parameters);

                dbSchema = LoadSchema(parameters, schemaLoader);

                if (parameters.Dbml != null)
                {
                    //we are supposed to write out a DBML file and exit
                    Logger.Write(Level.Information, "<<< Writing file '{0}'", parameters.Dbml);
                    using (Stream dbmlFile = File.OpenWrite(parameters.Dbml))
                    {
                        DbmlSerializer.Write(dbmlFile, dbSchema);
                    }
                }
                else
                {
                    string filename = parameters.Code ?? parameters.Database.Replace("\"", "");
                    Logger.Write(Level.Information, "<<< writing C# classes in file '{0}'", filename);
                    GenerateCSharp(parameters, dbSchema, schemaLoader, filename);

                }
            }
            catch (Exception ex)
            {
                string assyName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
                Logger.Write(Level.Error, assyName + " failed:" + ex);
            }
        }

        public void GenerateCSharp(SqlMetalParameters parameters, DbLinq.Schema.Dbml.Database dbSchema, ISchemaLoader schemaLoader, string filename)
        {
            ICodeGenerator codeGen = new CSCodeGenerator();

            if (String.IsNullOrEmpty(Path.GetExtension(filename)))
                filename += codeGen.Extension;

            using (StreamWriter streamWriter = new StreamWriter(filename))
            {
                var generationContext = new GenerationContext(parameters, schemaLoader);
                codeGen.Write(streamWriter, dbSchema, generationContext);
            }
        }

        public DbLinq.Schema.Dbml.Database LoadSchema(SqlMetalParameters parameters, ISchemaLoader schemaLoader)
        {
            DbLinq.Schema.Dbml.Database dbSchema;
            var tableAliases = TableAlias.Load(parameters);
            if (parameters.SchemaXmlFile == null) // read schema from DB
            {
                Logger.Write(Level.Information, ">>> Reading schema from {0} database", schemaLoader.VendorName);
                dbSchema = schemaLoader.Load(parameters.Database, tableAliases, parameters.Pluralize, parameters.SProcs);
                dbSchema.Provider = parameters.Provider;
                dbSchema.Tables.Sort(new LambdaComparer<DbLinq.Schema.Dbml.Table>((x, y) => (x.Type.Name.CompareTo(y.Type.Name))));
                foreach (var table in dbSchema.Tables)
                    table.Type.Columns.Sort(new LambdaComparer<DbLinq.Schema.Dbml.Column>((x, y) => (x.Member.CompareTo(y.Member))));
                dbSchema.Functions.Sort(new LambdaComparer<DbLinq.Schema.Dbml.Function>((x, y) => (x.Method.CompareTo(y.Method))));
                SchemaPostprocess.PostProcess_DB(dbSchema);
            }
            else // load DBML
            {
                Logger.Write(Level.Information, ">>> Reading schema from DBML file '{0}'", parameters.SchemaXmlFile);
                using (Stream dbmlFile = File.OpenRead(parameters.SchemaXmlFile))
                {
                    dbSchema = DbmlSerializer.Read(dbmlFile);
                }
            }
            return dbSchema;
        }

        private void PrintUsage()
        {
            string appName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            Console.WriteLine(appName + " usage:");
            Console.WriteLine(appName + ".exe -Server:xx -Database:yy -User:zz -Password:** -Provider=[MySql|Oracle|OracleODP|PostgreSql|Sqlite]");
            Console.WriteLine("Result: produces file yy.cs in local directory");
        }
    }
}

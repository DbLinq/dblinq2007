#region MIT license
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
using System.Globalization;
using System.IO;
using DbLinq.Factory;
using DbLinq.Linq;
using DbLinq.Logging;
using DbLinq.Schema;
using DbLinq.Schema.Dbml;
using DbLinq.Util;
using DbLinq.Vendor;
using DbMetal.Schema;

namespace DbMetal.Generator.Implementation
{
    public class Processor : IProcessor
    {
        public ILogger Logger { get; set; }
        public ISchemaLoaderFactory SchemaLoaderFactory { get; set; }

        public Processor()
        {
            Logger = ObjectFactory.Get<ILogger>();
            SchemaLoaderFactory = ObjectFactory.Get<ISchemaLoaderFactory>();
        }

        public void Process(string[] args)
        {
            if (args.Length == 0)
                PrintUsage();

            else
            {
                bool readLineAtExit = false;

                // TODO add WriteHeader() here

                try
                {
                    foreach (var parameters in Parameters.GetBatch(args))
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

            Logger.Write(Level.Information, "");

        }

        private void ProcessSchema(Parameters parameters)
        {
            try
            {
                // we always need a factory, even if generating from a DBML file, because we need a namespace
                var schemaLoader = SchemaLoaderFactory.Load(parameters);

                Database dbSchema = LoadSchema(parameters, schemaLoader);

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

        public void GenerateCSharp(Parameters parameters, Database dbSchema, ISchemaLoader schemaLoader, string filename)
        {
            ICodeGenerator codeGen = new CSCodeGenerator();

            if (String.IsNullOrEmpty(Path.GetExtension(filename)))
                filename += codeGen.Extension;

            using (var streamWriter = new StreamWriter(filename))
            {
                var generationContext = new GenerationContext(parameters, schemaLoader);
                codeGen.Write(streamWriter, dbSchema, generationContext);
            }
        }

        public Database LoadSchema(Parameters parameters, ISchemaLoader schemaLoader)
        {
            Database dbSchema;
            var tableAliases = TableAlias.Load(parameters);
            if (parameters.SchemaXmlFile == null) // read schema from DB
            {
                Logger.Write(Level.Information, ">>> Reading schema from {0} database", schemaLoader.VendorName);
                dbSchema = schemaLoader.Load(parameters.Database, tableAliases,
                    new NameFormat { Case = Case.PascalCase, Pluralize = parameters.Pluralize, Culture = new CultureInfo("en") },
                    parameters.SProcs);
                dbSchema.Provider = parameters.Provider;
                dbSchema.Tables.Sort(new LambdaComparer<Table>((x, y) => (x.Type.Name.CompareTo(y.Type.Name))));
                foreach (var table in dbSchema.Tables)
                    table.Type.Columns.Sort(new LambdaComparer<Column>((x, y) => (x.Member.CompareTo(y.Member))));
                dbSchema.Functions.Sort(new LambdaComparer<Function>((x, y) => (x.Method.CompareTo(y.Method))));
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
            var parameters = new Parameters();
            parameters.WriteHelp();
        }
    }
}

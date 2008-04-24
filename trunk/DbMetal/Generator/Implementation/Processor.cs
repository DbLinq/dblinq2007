#region MIT license
// 
// Copyright (c) 2007-2008 Jiri Moudry
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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
            var parameters = new Parameters();

            if (args.Length == 0)
                PrintUsage(parameters);

            else
            {
                bool readLineAtExit = false;

                parameters.WriteHeader();

                try
                {
                    foreach (var parametersBatch in parameters.GetBatch(args))
                    {
                        ProcessSchema(parametersBatch);
                        if (parametersBatch.ReadLineAtExit)
                            readLineAtExit = true;
                    }
                }
                catch (ArgumentException e)
                {
                    Logger.Write(Level.Error, e.Message);
                    PrintUsage(parameters);
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
                    using (Stream dbmlFile = File.Create(parameters.Dbml))
                    {
                        DbmlSerializer.Write(dbmlFile, dbSchema);
                    }
                }
                else
                {
                    if (!parameters.Schema)
                        RemoveSchema(dbSchema);

                    string filename = parameters.Code;
                    if (string.IsNullOrEmpty(filename) && !string.IsNullOrEmpty(parameters.Database))
                        filename = parameters.Database.Replace("\"", "");
                    if (string.IsNullOrEmpty(filename))
                        filename = dbSchema.Name;
                    Logger.Write(Level.Information, "<<< writing C# classes in file '{0}'", filename);
                    Generate(parameters, dbSchema, schemaLoader, filename);

                }
            }
            catch (Exception ex)
            {
                string assyName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
                Logger.Write(Level.Error, assyName + " failed:" + ex);
            }
        }

        protected void RemoveSchema(Database schema)
        {
            foreach (var table in schema.Table)
            {
                string[] nameAndSchema = table.Name.Split('.');
                table.Name = nameAndSchema[nameAndSchema.Length - 1];
            }
        }

        public virtual IEnumerable<ICodeGenerator> EnumerateCodeGenerators()
        {
            foreach (var codeGeneratorType in ObjectFactory.Current.GetImplementations(typeof(ICodeGenerator)))
            {
                yield return (ICodeGenerator)ObjectFactory.Current.GetInstance(codeGeneratorType, false);
            }
        }

        protected virtual ICodeGenerator FindCodeGeneratorByLanguage(string languageCode)
        {
            return (from codeGenerator in EnumerateCodeGenerators()
                    where codeGenerator.LanguageCode == languageCode
                    select codeGenerator).SingleOrDefault();
        }

        protected virtual ICodeGenerator FindCodeGeneratorByExtension(string extension)
        {
            return (from codeGenerator in EnumerateCodeGenerators()
                    where codeGenerator.Extension == extension
                    select codeGenerator).SingleOrDefault();
        }

        public virtual ICodeGenerator FindCodeGenerator(Parameters parameters, string filename)
        {
            if (!string.IsNullOrEmpty(parameters.Language))
                return FindCodeGeneratorByLanguage(parameters.Language);
            return FindCodeGeneratorByExtension(Path.GetExtension(filename));
        }

        public void Generate(Parameters parameters, Database dbSchema, ISchemaLoader schemaLoader, string filename)
        {
            ICodeGenerator codeGen = FindCodeGenerator(parameters, filename);
            if (codeGen == null)
                throw new ArgumentException("Please specify either a /language or a /code file");

            if (string.IsNullOrEmpty(filename))
                filename = dbSchema.Class;
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
            var nameAliases = NameAliasesLoader.Load(parameters.Aliases);
            if (parameters.SchemaXmlFile == null) // read schema from DB
            {
                Logger.Write(Level.Information, ">>> Reading schema from {0} database", schemaLoader.VendorName);
                dbSchema = schemaLoader.Load(parameters.Database, nameAliases,
                    new NameFormat { Case = Case.PascalCase, Pluralize = parameters.Pluralize, Culture = new CultureInfo(parameters.Culture) },
                    parameters.Sprocs, parameters.Namespace, parameters.Namespace);
                dbSchema.Provider = parameters.Provider;
                dbSchema.Tables.Sort(new LambdaComparer<Table>((x, y) => (x.Type.Name.CompareTo(y.Type.Name))));
                foreach (var table in dbSchema.Tables)
                    table.Type.Columns.Sort(new LambdaComparer<Column>((x, y) => (x.Member.CompareTo(y.Member))));
                dbSchema.Functions.Sort(new LambdaComparer<Function>((x, y) => (x.Method.CompareTo(y.Method))));
                SchemaPostprocess.PostProcess_DB(dbSchema);
            }
            else // load DBML
                dbSchema = LoadSchema(parameters.SchemaXmlFile);
            return dbSchema;
        }

        public Database LoadSchema(string filename)
        {
            Logger.Write(Level.Information, ">>> Reading schema from DBML file '{0}'", filename);
            using (Stream dbmlFile = File.OpenRead(filename))
            {
                return DbmlSerializer.Read(dbmlFile);
            }
        }

        private void PrintUsage(Parameters parameters)
        {
            parameters.WriteHelp();
        }
    }
}

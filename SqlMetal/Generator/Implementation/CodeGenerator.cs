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
using System.Collections.Generic;
using System.IO;
using DbLinq.Linq;

namespace SqlMetal.Generator.Implementation
{
    public abstract partial class CodeGenerator : ICodeGenerator
    {
        public abstract string Extension { get; }

        protected abstract CodeWriter CreateCodeWriter(TextWriter textWriter);

        public void Write(TextWriter textWriter, DlinqSchema.Database dbSchema, GenerationContext context)
        {
            context["dataContextBase"] = context.SchemaLoader.DataContextType.FullName;
            context["namespace"] = context.Parameters.Namespace;
            context["database"] = dbSchema.Name;

            if (dbSchema == null || dbSchema.Tables == null)
            {
                Console.WriteLine("CodeGenAll ERROR: incomplete dbSchema, cannot start generating code");
                return;
            }

            using (var codeWriter = CreateCodeWriter(textWriter))
            {
                WriteBanner(codeWriter, context);
                WriteUsings(codeWriter, context);
                using (WriteNamespace(codeWriter, context))
                {
                    WriteDataContext(codeWriter, dbSchema, context);
                    WriteClasses(codeWriter, dbSchema, context);
                }
            }
        }

        private void WriteBanner(CodeWriter writer, GenerationContext context)
        {
            using (writer.WriteRegion("Header"))
            {
                // http://www.network-science.de/ascii/
                // http://www.network-science.de/ascii/ascii.php?TEXT=MetalSequel&x=14&y=14&FONT=_all+fonts+with+your+text_&RICH=no&FORM=left&STRE=no&WIDT=80 
                writer.WriteComments(
                    @"
             _        _ __                       _ 
  /\/\   ___| |_ __ _| / _\ ___  __ _ _   _  ___| |
 /    \ / _ \ __/ _` | \ \ / _ \/ _` | | | |/ _ \ |
/ /\/\ \  __/ || (_| | |\ \  __/ (_| | |_| |  __/ |
\/    \/\___|\__\__,_|_\__/\___|\__, |\__,_|\___|_|
                                   |_|");
                writer.WriteComments(string.Format("Auto-generated from {0} on {1:u}", context.Parameters.Database, DateTime.Now));
                writer.WriteComments("Please visit http://linq.to/db for more information");
            }
        }

        private void WriteUsings(CodeWriter writer, GenerationContext context)
        {
            writer.WriteUsingNamespace("System");
            writer.WriteUsingNamespace("System.ComponentModel");
            writer.WriteUsingNamespace("System.Diagnostics");
            writer.WriteUsingNamespace("System.Collections.Generic");
            writer.WriteUsingNamespace("System.Text");
            writer.WriteUsingNamespace("System.Linq");
            writer.WriteUsingNamespace("System.Data");
            writer.WriteUsingNamespace("System.Data.Linq.Mapping");
            writer.WriteUsingNamespace("System.Reflection");
            writer.WriteUsingNamespace("DbLinq.Linq");
            writer.WriteUsingNamespace("DbLinq.Linq.Mapping");
            writer.WriteLine();
        }

        private IDisposable WriteNamespace(CodeWriter writer, GenerationContext context)
        {
            if (!string.IsNullOrEmpty(context.Parameters.Namespace))
                return writer.WriteNamespace(context.Parameters.Namespace);
            return null;
        }

        private void WriteDataContext(CodeWriter writer, DlinqSchema.Database schema, GenerationContext context)
        {
            if (schema.Tables.Count == 0)
            {
                writer.WriteComment("L69 no tables found");
                return;
            }
            using (writer.WriteClass(Specifications.Partial, schema.Name, context.SchemaLoader.DataContextType.FullName))
            {
                WriteDataContextCtors(writer, schema, context);
                WriteDataContextTables(writer, schema, context);
                WriteDataContextProcedures(writer, schema, context);
            }
        }

        protected abstract void WriteDataContextCtors(CodeWriter writer, DlinqSchema.Database schema, GenerationContext context);

        private void WriteDataContextTables(CodeWriter writer, DlinqSchema.Database schema, GenerationContext context)
        {
            foreach (var table in schema.Tables)
                WriteDataContextTables(writer, table);
            writer.WriteLine();
        }

        protected abstract void WriteDataContextTables(CodeWriter writer, DlinqSchema.Table table);

        private void WriteClasses(CodeWriter writer, DlinqSchema.Database schema, GenerationContext context)
        {
        }

        // this method will be removed when we won't use literal types in dbml
        protected virtual Type GetType(string literalType)
        {
            switch (literalType)
            {
            case "string":
                return typeof(string);
            case "long":
                return typeof(long);
            case "short":
                return typeof(short);
            case "int":
                return typeof(int);
            case "char":
                return typeof(char);
            case "byte":
                return typeof(byte);
            case "double":
                return typeof(double);
            case "decimal":
                return typeof(decimal);
            default:
                return Type.GetType(literalType);
            }
        }
    }
}

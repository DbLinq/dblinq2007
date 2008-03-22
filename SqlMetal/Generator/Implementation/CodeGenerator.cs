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
using DbLinq.Logging;
using DbLinq.Schema;

namespace SqlMetal.Generator.Implementation
{
    public abstract partial class CodeGenerator : ICodeGenerator
    {
        public abstract string Extension { get; }

        protected abstract CodeWriter CreateCodeWriter(TextWriter textWriter);

        public void Write(TextWriter textWriter, DbLinq.Schema.Dbml.Database dbSchema, GenerationContext context)
        {
            if (dbSchema == null || dbSchema.Tables == null)
            {
                Logger.Write(Level.Error, "CodeGenAll ERROR: incomplete dbSchema, cannot start generating code");
                return;
            }

            context["dataContextBase"] = context.SchemaLoader.DataContextType.FullName;
            context["namespace"] = context.Parameters.Namespace;
            context["database"] = dbSchema.Name;
            context["generationTime"] = DateTime.Now.ToString("u");

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
            using (writer.WriteRegion(context.Evaluate("Auto-generated classes for ${database} database on ${generationTime}")))
            {
                // http://www.network-science.de/ascii/
                // http://www.network-science.de/ascii/ascii.php?TEXT=MetalSequel&x=14&y=14&FONT=_all+fonts+with+your+text_&RICH=no&FORM=left&STRE=no&WIDT=80 
                writer.WriteCommentLines(
                    @"
 ____  _     __  __      _        _ 
|  _ \| |__ |  \/  | ___| |_ __ _| |
| | | | '_ \| |\/| |/ _ \ __/ _` | |
| |_| | |_) | |  | |  __/ || (_| | |
|____/|_.__/|_|  |_|\___|\__\__,_|_|
");
                writer.WriteCommentLines(context.Evaluate("Auto-generated from ${database} on ${generationTime}"));
                writer.WriteCommentLines("Please visit http://linq.to/db for more information");
            }
        }

        private void WriteUsings(CodeWriter writer, GenerationContext context)
        {
            writer.WriteUsingNamespace("System");
            writer.WriteUsingNamespace("System.Collections.Generic");
            writer.WriteUsingNamespace("System.ComponentModel");
            writer.WriteUsingNamespace("System.Data");
            writer.WriteUsingNamespace("System.Data.Linq.Mapping");
            writer.WriteUsingNamespace("System.Diagnostics");
            writer.WriteUsingNamespace("System.Linq");
            writer.WriteUsingNamespace("System.Reflection");
            writer.WriteUsingNamespace("System.Text");
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

        private void WriteDataContext(CodeWriter writer, DbLinq.Schema.Dbml.Database schema, GenerationContext context)
        {
            if (schema.Tables.Count == 0)
            {
                writer.WriteCommentLine("L69 no tables found");
                return;
            }
            using (writer.WriteClass(SpecificationDefinition.Partial, schema.Class, context.SchemaLoader.DataContextType.FullName))
            {
                WriteDataContextCtors(writer, schema, context);
                WriteDataContextTables(writer, schema, context);
                WriteDataContextProcedures(writer, schema, context);
            }
        }

        protected abstract void WriteDataContextCtors(CodeWriter writer, DbLinq.Schema.Dbml.Database schema, GenerationContext context);

        private void WriteDataContextTables(CodeWriter writer, DbLinq.Schema.Dbml.Database schema, GenerationContext context)
        {
            foreach (var table in schema.Tables)
                WriteDataContextTable(writer, table);
            writer.WriteLine();
        }

        protected abstract void WriteDataContextTable(CodeWriter writer, DbLinq.Schema.Dbml.Table table);

        // this method will be removed when we won't use literal types in dbml
        protected virtual Type GetType(string literalType)
        {
            bool isNullable = literalType.EndsWith("?");
            if (isNullable)
                literalType = literalType.Substring(0, literalType.Length - 1);
            bool isArray = literalType.EndsWith("[]");
            if (isArray)
                literalType = literalType.Substring(0, literalType.Length - 2);
            Type type = GetSimpleType(literalType);
            if (type == null)
                return type;
            if (isArray)
                type = type.MakeArrayType();
            if (isNullable)
                type = typeof(Nullable<>).MakeGenericType(type);
            return type;
        }

        private Type GetSimpleType(string literalType)
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
            case "float":
                return typeof(float);
            case "double":
                return typeof(double);
            case "decimal":
                return typeof(decimal);
            case "bool":
                return typeof(bool);
            case "DateTime":
                return typeof(DateTime);
            case "object":
                return typeof(object);
            default:
                return Type.GetType(literalType);
            }
        }

        protected string GetAttributeShortName<T>()
            where T : Attribute
        {
            string literalAttribute = typeof(T).Name;
            string end = "Attribute";
            if (literalAttribute.EndsWith(end))
                literalAttribute = literalAttribute.Substring(0, literalAttribute.Length - end.Length);
            return literalAttribute;
        }

        protected AttributeDefinition NewAttributeDefinition<T>()
            where T : Attribute
        {
            return new AttributeDefinition(GetAttributeShortName<T>());
        }
    }
}

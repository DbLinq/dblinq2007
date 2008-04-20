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
using System.IO;
using DbLinq.Logging;
using DbLinq.Schema;

namespace DbMetal.Generator.Implementation.CodeTextGenerator
{
    public abstract partial class CodeGenerator : ICodeGenerator
    {
        public abstract string LanguageCode { get; }
        public abstract string Extension { get; }

        protected class MassDisposer : IDisposable
        {
            public IList<IDisposable> Disposables = new List<IDisposable>();

            public void Dispose()
            {
                for (int index = Disposables.Count - 1; index > 0; index--)
                {
                    Disposables[index].Dispose();
                }
            }
        }

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
            context["class"] = dbSchema.Class;

            using (var codeWriter = CreateCodeWriter(textWriter))
            {
                WriteBanner(codeWriter, context);
                WriteUsings(codeWriter, context);

                string contextNamespace = context.Parameters.Namespace;
                if (string.IsNullOrEmpty(contextNamespace))
                    contextNamespace = dbSchema.ContextNamespace;

                string entityNamespace = context.Parameters.Namespace;
                if (string.IsNullOrEmpty(entityNamespace))
                    entityNamespace = dbSchema.EntityNamespace;

                if (contextNamespace == entityNamespace)
                {
                    using (WriteNamespace(codeWriter, contextNamespace))
                    {
                        WriteDataContext(codeWriter, dbSchema, context);
                        WriteClasses(codeWriter, dbSchema, context);
                    }
                }
                else
                {
                    using (WriteNamespace(codeWriter, contextNamespace))
                        WriteDataContext(codeWriter, dbSchema, context);
                    using (WriteNamespace(codeWriter, entityNamespace))
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

        private IDisposable WriteNamespace(CodeWriter writer, string nameSpace)
        {
            if (!string.IsNullOrEmpty(nameSpace))
                return writer.WriteNamespace(nameSpace);
            return null;
        }

        private void WriteDataContext(CodeWriter writer, DbLinq.Schema.Dbml.Database schema, GenerationContext context)
        {
            if (schema.Tables.Count == 0)
            {
                writer.WriteCommentLine("L69 no tables found");
                return;
            }

            string contextBase = schema.BaseType;
            if (string.IsNullOrEmpty(contextBase))
                contextBase = context.SchemaLoader.DataContextType.FullName;

            using (writer.WriteClass(SpecificationDefinition.Partial, schema.Class, contextBase))
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
        protected virtual Type GetType(string literalType, bool canBeNull)
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
            else if (canBeNull)
            {
                if (type.IsValueType)
                    type = typeof(Nullable<>).MakeGenericType(type);
            }
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

        protected IDisposable WriteAttributes(CodeWriter writer, params AttributeDefinition[] definitions)
        {
            var massDisposer = new MassDisposer();
            foreach (var definition in definitions)
                massDisposer.Disposables.Add(writer.WriteAttribute(definition));
            return massDisposer;
        }

        protected IDisposable WriteAttributes(CodeWriter writer, params string[] definitions)
        {
            var attributeDefinitions = new List<AttributeDefinition>();
            foreach (string definition in definitions)
                attributeDefinitions.Add(new AttributeDefinition(definition));
            return WriteAttributes(writer, attributeDefinitions.ToArray());
        }
    }
}

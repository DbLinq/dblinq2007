#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne
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
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using Microsoft.CSharp;
using Microsoft.VisualBasic;

using DbLinq.Schema.Dbml;
using DbLinq.Util;
using System.Data.Linq.Mapping;

namespace DbMetal.Generator.Implementation.CodeDomGenerator
{
#if !MONO_STRICT
    public
#endif
    abstract class AbstractCodeDomGenerator : ICodeGenerator
    {
        public abstract string LanguageCode { get; }
        public abstract string Extension { get; }

        protected abstract CodeDomProvider CreateProvider();
        protected abstract void AddConditionalImports(CodeNamespaceImportCollection imports,
                string firstImport,
                string conditional,
                string[] importsIfTrue,
                string[] importsIfFalse,
                string lastImport);
        protected abstract CodeTypeMember CreatePartialMethod(string methodName, params CodeParameterDeclarationExpression[] parameters);

        public void Write(TextWriter textWriter, Database dbSchema, GenerationContext context)
        {
            Context = context;
            CreateProvider().CreateGenerator(textWriter).GenerateCodeFromNamespace(
                GenerateCodeDomModel(dbSchema), textWriter, new CodeGeneratorOptions() { BracingStyle = "C" });
        }

        CodeThisReferenceExpression thisReference = new CodeThisReferenceExpression();

        protected GenerationContext Context { get; set; }

        protected virtual CodeNamespace GenerateCodeDomModel(Database database)
        {
            CodeNamespace _namespace = new CodeNamespace(database.ContextNamespace);

            _namespace.Imports.Add(new CodeNamespaceImport("System"));
            _namespace.Imports.Add(new CodeNamespaceImport("System.ComponentModel"));
#if MONO_STRICT
            _namespace.Imports.Add(new CodeNamespaceImport("System.Data"));
            _namespace.Imports.Add(new CodeNamespaceImport("System.Data.Linq"));
            _namespace.Imports.Add(new CodeNamespaceImport("System.Data.Linq.Mapping"));
#else
            AddConditionalImports(_namespace.Imports,
                "System.Data",
                "MONO_STRICT",
                new[] { "System.Data.Linq" },
                new[] { "DbLinq.Data.Linq", "DbLinq.Vendor" },
                "System.Data.Linq.Mapping");
#endif
            _namespace.Imports.Add(new CodeNamespaceImport("System.Diagnostics"));

            var time = Context.Parameters.GenerateTimestamps ? DateTime.Now.ToString("u") : "[TIMESTAMP]";
            var header = new CodeCommentStatement(GenerateCommentBanner(database, time));
            _namespace.Comments.Add(header);

            _namespace.Types.Add(GenerateContextClass(database));

            foreach (Table table in database.Tables)
                _namespace.Types.Add(GenerateTableClass(table));
            return _namespace;
        }

        private string GenerateCommentBanner(Database database, string time)
        {
            var result = new StringBuilder();

            // http://www.network-science.de/ascii/
            // http://www.network-science.de/ascii/ascii.php?TEXT=MetalSequel&x=14&y=14&FONT=_all+fonts+with+your+text_&RICH=no&FORM=left&STRE=no&WIDT=80 
            result.Append(
                @"
  ____  _     __  __      _        _ 
 |  _ \| |__ |  \/  | ___| |_ __ _| |
 | | | | '_ \| |\/| |/ _ \ __/ _` | |
 | |_| | |_) | |  | |  __/ || (_| | |
 |____/|_.__/|_|  |_|\___|\__\__,_|_|

");
            result.AppendLine(String.Format(" Auto-generated from {0} on {1}.", database.Name, time));
            result.AppendLine(" Please visit http://code.google.com/p/dblinq2007/ for more information.");

            return result.ToString();
        }

        protected virtual CodeTypeDeclaration GenerateContextClass(Database database)
        {
            var _class = new CodeTypeDeclaration() {
                IsClass         = true, 
                IsPartial       = true, 
                Name            = database.Class, 
                TypeAttributes  = TypeAttributes.Public 
            };

            var contextBaseType = string.IsNullOrEmpty(database.BaseType)
                ? "DataContext"
                : TypeLoader.Load(database.BaseType).Name;
            _class.BaseTypes.Add(contextBaseType);

            var onCreated = CreatePartialMethod("OnCreated");
            onCreated.StartDirectives.Add(new CodeRegionDirective(CodeRegionMode.Start, "Extensibility Method Declarations"));
            onCreated.EndDirectives.Add(new CodeRegionDirective(CodeRegionMode.End, null));
            _class.Members.Add(onCreated);

            // Implement Constructor
            var constructor = new CodeConstructor() {
                Attributes = MemberAttributes.Public, 
                Parameters = { new CodeParameterDeclarationExpression("IDbConnection", "connection") }, 
            };
            constructor.BaseConstructorArgs.Add(new CodeArgumentReferenceExpression("connection"));
            constructor.Statements.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(thisReference, "OnCreated")));
            _class.Members.Add(constructor);

            // todo: override other constructors

            foreach (Table table in database.Tables)
            {
                var tableType = new CodeTypeReference(table.Member);
                var property = new CodeMemberProperty() {
                    Attributes  = MemberAttributes.Public | MemberAttributes.Final,
                    Name        = table.Member, 
                    Type        = new CodeTypeReference("Table", tableType), 
                };
                property.GetStatements.Add(
                    new CodeMethodReturnStatement(
                        new CodeMethodInvokeExpression(
                            new CodeMethodReferenceExpression(thisReference, "GetTable", tableType))));
                _class.Members.Add(property);
            }

            return _class;
        }

        protected virtual CodeTypeDeclaration GenerateTableClass(Table table)
        {
            var _class = new CodeTypeDeclaration() { IsClass = true, IsPartial = true, Name = table.Member, TypeAttributes = TypeAttributes.Public };

            _class.CustomAttributes.Add(new CodeAttributeDeclaration("Table", new CodeAttributeArgument("Name", new CodePrimitiveExpression(table.Name))));

            GenerateINotifyPropertyChanging(_class);
            GenerateINotifyPropertyChanged(_class);

            // Implement Constructor
            var constructor = new CodeConstructor() { Attributes = MemberAttributes.Public };
            constructor.Statements.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(thisReference, "OnCreated")));
            _class.Members.Add(constructor);

            GenerateExtensibilityDeclarations(_class, table);

            // todo: add these when the actually get called
            //partial void OnLoaded();
            //partial void OnValidate(System.Data.Linq.ChangeAction action);

            // columns
            foreach (Column column in table.Type.Columns)
            {
                var type = new CodeTypeReference(column.Type);
                var columnMember = column.Member ?? column.Name;

                var field = new CodeMemberField(type, column.Storage);
                _class.Members.Add(field);
                var fieldReference = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), field.Name);

                var onChanging  = GetChangingMethodName(columnMember);
                var onChanged   = GetChangedMethodName(columnMember);

                var property = new CodeMemberProperty();
                property.Type = type;
                property.Name = columnMember;
                property.Attributes = MemberAttributes.Public | MemberAttributes.Final;

                var defAttrValues = new ColumnAttribute();
                var args = new List<CodeAttributeArgument>() {
                    new CodeAttributeArgument("Storage", new CodePrimitiveExpression(column.Storage)),
                    new CodeAttributeArgument("Name", new CodePrimitiveExpression(column.Name)),
                    new CodeAttributeArgument("DbType", new CodePrimitiveExpression(column.DbType)),
                };
                if (defAttrValues.IsPrimaryKey != column.IsPrimaryKey)
                    args.Add(new CodeAttributeArgument("IsPrimaryKey", new CodePrimitiveExpression(column.IsPrimaryKey)));
                if (column.AutoSync != DbLinq.Schema.Dbml.AutoSync.Default)
                    args.Add(new CodeAttributeArgument("AutoSync", 
                        new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("AutoSync"), column.AutoSync.ToString())));
                if (defAttrValues.CanBeNull != column.CanBeNull)
                    args.Add(new CodeAttributeArgument("CanBeNull", new CodePrimitiveExpression(column.CanBeNull)));
                if (column.Expression != null)
                    args.Add(new CodeAttributeArgument("Expression", new CodePrimitiveExpression(column.Expression)));
                property.CustomAttributes.Add(
                    new CodeAttributeDeclaration("Column", args.ToArray()));
                property.CustomAttributes.Add(new CodeAttributeDeclaration("DebuggerNonUserCode"));
                property.GetStatements.Add(new CodeMethodReturnStatement(fieldReference));
                property.SetStatements.Add(
                    new CodeConditionStatement(
                        new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression(field.Name), CodeBinaryOperatorType.IdentityInequality, new CodePropertySetValueReferenceExpression()),
                        new CodeExpressionStatement(new CodeMethodInvokeExpression(thisReference, onChanging, new CodePropertySetValueReferenceExpression())),
                        new CodeExpressionStatement(new CodeMethodInvokeExpression(thisReference, "SendPropertyChanging")),
                        new CodeAssignStatement(fieldReference, new CodePropertySetValueReferenceExpression()),
                        new CodeExpressionStatement(new CodeMethodInvokeExpression(thisReference, "SendPropertyChanged", new CodePrimitiveExpression(property.Name))),
                        new CodeExpressionStatement(new CodeMethodInvokeExpression(thisReference, onChanged))));
                _class.Members.Add(property);
            }

            // TODO: implement associations

            // TODO: implement functions / procedures

            // TODO: Override Equals and GetHashCode

            return _class;
        }

        void GenerateExtensibilityDeclarations(CodeTypeDeclaration entity, Table table)
        {
            var partialMethods = new[] { CreatePartialMethod("OnCreated") }
                .Concat(table.Type.Columns.Select(c => new[] { CreateChangedMethodDecl(c), CreateChangingMethodDecl(c) })
                    .SelectMany(md => md)).ToArray();
            partialMethods.First().StartDirectives.Add(new CodeRegionDirective(CodeRegionMode.Start, "Extensibility Method Declarations"));
            partialMethods.Last().EndDirectives.Add(new CodeRegionDirective(CodeRegionMode.End, null));
            entity.Members.AddRange(partialMethods);
        }

        static string GetChangedMethodName(string columnName)
        {
            return string.Format("On{0}Changed", columnName);
        }

        CodeTypeMember CreateChangedMethodDecl(Column column)
        {
            return CreatePartialMethod(GetChangedMethodName(column.Name));
        }

        static string GetChangingMethodName(string columnName)
        {
            return string.Format("On{0}Changing", columnName);
        }

        CodeTypeMember CreateChangingMethodDecl(Column column)
        {
            return CreatePartialMethod(GetChangingMethodName(column.Name),
                    new CodeParameterDeclarationExpression(column.Type, "value"));
        }

        private void GenerateINotifyPropertyChanging(CodeTypeDeclaration entity)
        {
            entity.BaseTypes.Add(typeof(INotifyPropertyChanging));
            var propertyChangingEvent = new CodeMemberEvent() {
                Attributes  = MemberAttributes.Public,
                Name        = "PropertyChanging",
                Type        = new CodeTypeReference(typeof(PropertyChangingEventHandler)),
                ImplementationTypes = {
                    new CodeTypeReference(typeof(INotifyPropertyChanging))
                },
            };
            var eventArgs = new CodeMemberField(new CodeTypeReference(typeof(PropertyChangingEventArgs)), "emptyChangingEventArgs") {
                Attributes      = MemberAttributes.Static | MemberAttributes.Private,
                InitExpression  = new CodeObjectCreateExpression(new CodeTypeReference(typeof(PropertyChangingEventArgs)),
                    new CodePrimitiveExpression("")),
            };
            var method = new CodeMemberMethod() {
                Attributes  = MemberAttributes.Family,
                Name        = "SendPropertyChanging",
            };
            method.Statements.Add(new CodeVariableDeclarationStatement(typeof(PropertyChangingEventHandler), "h") {
                InitExpression  = new CodeEventReferenceExpression(thisReference, "PropertyChanging"),
            });
            method.Statements.Add(new CodeConditionStatement(
                    new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("h"), CodeBinaryOperatorType.IdentityInequality, new CodePrimitiveExpression(null)),
                    new CodeExpressionStatement(
                        new CodeDelegateInvokeExpression(new CodeVariableReferenceExpression("h"), thisReference, new CodeFieldReferenceExpression(null, "emptyChangingEventArgs")))));

            entity.Members.Add(propertyChangingEvent);
            entity.Members.Add(eventArgs);
            entity.Members.Add(method);
        }

        private void GenerateINotifyPropertyChanged(CodeTypeDeclaration entity)
        {
            entity.BaseTypes.Add(typeof(INotifyPropertyChanged));

            var propertyChangedEvent = new CodeMemberEvent() {
                Attributes = MemberAttributes.Public,
                Name = "PropertyChanged",
                Type = new CodeTypeReference(typeof(PropertyChangedEventHandler)),
                ImplementationTypes = {
                    new CodeTypeReference(typeof(INotifyPropertyChanged))
                },
            };

            var method = new CodeMemberMethod() { 
                Attributes = MemberAttributes.Family, 
                Name = "SendPropertyChanged", 
                Parameters = { new CodeParameterDeclarationExpression(typeof(System.String), "propertyName") } 
            };
            method.Statements.Add(new CodeVariableDeclarationStatement(typeof(PropertyChangedEventHandler), "h") {
                InitExpression = new CodeEventReferenceExpression(thisReference, "PropertyChanged"),
            });
            method.Statements.Add(new CodeConditionStatement(
                    new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("h"), CodeBinaryOperatorType.IdentityInequality, new CodePrimitiveExpression(null)),
                    new CodeExpressionStatement(
                        new CodeDelegateInvokeExpression(new CodeVariableReferenceExpression("h"), thisReference, new CodeObjectCreateExpression(typeof(PropertyChangedEventArgs), new CodeVariableReferenceExpression("propertyName"))))));

            entity.Members.Add(propertyChangedEvent);
            entity.Members.Add(method);
        }
    }
}

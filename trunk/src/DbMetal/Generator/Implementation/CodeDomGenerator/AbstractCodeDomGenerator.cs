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

namespace DbMetal.Generator.Implementation.CodeDomGenerator
{
#if !MONO_STRICT
    public
#endif
    abstract class AbstractCodeDomGenerator : ICodeGenerator
    {
        public abstract string LanguageCode { get; }
        public abstract string Extension { get; }
        public abstract void Write(TextWriter textWriter, Database dbSchema, GenerationContext context);
        public abstract void AddConditionalImports(CodeNamespaceImportCollection imports,
                string firstImport,
                string conditional,
                string[] importsIfTrue,
                string[] importsIfFalse,
                string lastImport);

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

            if (database.BaseType != null)
                _class.BaseTypes.Add(database.BaseType);
            else
                _class.BaseTypes.Add(new CodeTypeReference("DataContext"));

            // CodeDom does not currently support partial methods.  This will be a problem for VB.  Will probably be fixed in .net 4
            _class.Members.Add(new CodeSnippetTypeMember("\tpartial void OnCreated();"));

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
                var property = new CodeMemberProperty() { Type = new CodeTypeReference("Table", tableType), Name = table.Member, Attributes = MemberAttributes.Public | MemberAttributes.Final };
                property.GetStatements.Add
                    (
                    new CodeMethodReturnStatement
                        (
                        new CodeMethodInvokeExpression
                            (
                            new CodeMethodReferenceExpression(thisReference, "GetTable", tableType)
                            )
                        )
                    );
                _class.Members.Add(property);
            }

            return _class;
        }

        protected virtual CodeTypeDeclaration GenerateTableClass(Table table)
        {
            var _class = new CodeTypeDeclaration() { IsClass = true, IsPartial = true, Name = table.Member, TypeAttributes = TypeAttributes.Public };

            _class.CustomAttributes.Add(new CodeAttributeDeclaration("Table", new CodeAttributeArgument("Name", new CodePrimitiveExpression(table.Name))));

            // Implement Constructor
            var constructor = new CodeConstructor() { Attributes = MemberAttributes.Public };
            constructor.Statements.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(thisReference, "OnCreated")));
            _class.Members.Add(constructor);

            // todo: implement INotifyPropertyChanging

            // Implement INotifyPropertyChanged
            _class.BaseTypes.Add(typeof(INotifyPropertyChanged));

            var propertyChangedEvent = new CodeMemberEvent() { Type = new CodeTypeReference(typeof(PropertyChangedEventHandler)), Name = "PropertyChanged", Attributes = MemberAttributes.Public };
            _class.Members.Add(propertyChangedEvent);

            var sendPropertyChangedMethod = new CodeMemberMethod() { Attributes = MemberAttributes.Family, Name = "SendPropertyChanged", Parameters = { new CodeParameterDeclarationExpression(typeof(System.String), "propertyName") } };
            sendPropertyChangedMethod.Statements.Add
                (
                new CodeConditionStatement
                    (
                    new CodeSnippetExpression(propertyChangedEvent.Name + " != null"), // todo: covert this to CodeBinaryOperatorExpression
                    new CodeExpressionStatement
                        (
                        new CodeMethodInvokeExpression
                            (
                            new CodeMethodReferenceExpression(thisReference, propertyChangedEvent.Name),
                            thisReference,
                            new CodeObjectCreateExpression(typeof(PropertyChangedEventArgs), new CodeArgumentReferenceExpression("propertyName"))
                            )
                        )
                    )
                );
            _class.Members.Add(sendPropertyChangedMethod);

            // CodeDom does not currently support partial methods.  This will be a problem for VB.  Will probably be fixed in .net 4
            _class.Members.Add(new CodeSnippetTypeMember("\tpartial void OnCreated();"));

            // todo: add these when the actually get called
            //partial void OnLoaded();
            //partial void OnValidate(System.Data.Linq.ChangeAction action);

            // columns
            foreach (Column column in table.Type.Columns)
            {
                var type = new CodeTypeReference(column.Type);
                var columnMember = column.Member ?? column.Name;

                var field = new CodeMemberField(type, "_" + columnMember);
                _class.Members.Add(field);
                var fieldReference = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), field.Name);

                // CodeDom does not currently support partial methods.  This will be a problem for VB.  Will probably be fixed in .net 4
                string onChangingPartialMethodName = String.Format("On{0}Changing", columnMember);
                _class.Members.Add(new CodeSnippetTypeMember(String.Format("\tpartial void {0}({1} instance);", onChangingPartialMethodName, column.Type)));
                string onChangedPartialMethodName = String.Format("On{0}Changed", columnMember);
                _class.Members.Add(new CodeSnippetTypeMember(String.Format("\tpartial void {0}();", onChangedPartialMethodName)));

                var property = new CodeMemberProperty();
                property.Type = type;
                property.Name = columnMember;
                property.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                property.CustomAttributes.Add
                    (
                    new CodeAttributeDeclaration
                        (
                        "Column",
                        new CodeAttributeArgument("Storage", new CodePrimitiveExpression(column.Storage)),
                        new CodeAttributeArgument("Name", new CodePrimitiveExpression(column.Name)),
                        new CodeAttributeArgument("DbType", new CodePrimitiveExpression(column.DbType)),
                        new CodeAttributeArgument("CanBeNull", new CodePrimitiveExpression(column.CanBeNull)),
                        new CodeAttributeArgument("IsPrimaryKey", new CodePrimitiveExpression(column.IsPrimaryKey))
                        )
                    );
                property.CustomAttributes.Add(new CodeAttributeDeclaration("DebuggerNonUserCode"));
                property.GetStatements.Add(new CodeMethodReturnStatement(fieldReference));
                property.SetStatements.Add
                    (
                    new CodeConditionStatement
                        (
                        new CodeSnippetExpression(field.Name + " != value"), // todo: covert this to CodeBinaryOperatorExpression
                        new CodeExpressionStatement(new CodeMethodInvokeExpression(thisReference, onChangingPartialMethodName, new CodePropertySetValueReferenceExpression())),
                        new CodeAssignStatement(fieldReference, new CodePropertySetValueReferenceExpression()),
                        new CodeExpressionStatement(new CodeMethodInvokeExpression(thisReference, sendPropertyChangedMethod.Name, new CodePrimitiveExpression(property.Name))),
                        new CodeExpressionStatement(new CodeMethodInvokeExpression(thisReference, onChangedPartialMethodName))
                        )
                    );
                _class.Members.Add(property);
            }

            // TODO: implement associations

            // TODO: implement functions / procedures

            // TODO: Override Equals and GetHashCode

            return _class;
        }
    }
}

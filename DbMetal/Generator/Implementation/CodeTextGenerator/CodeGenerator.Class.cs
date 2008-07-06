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
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Diagnostics;
using System.Linq;
using DbLinq.Data.Linq;
using DbLinq.Factory;
using DbLinq.Logging;
using DbLinq.Schema.Dbml;
using DbMetal.Generator.EntityInterface;


namespace DbMetal.Generator.Implementation.CodeTextGenerator
{
    public partial class CodeGenerator
    {
        public ILogger Logger { get; set; }

        public CodeGenerator()
        {
            Logger = ObjectFactory.Get<ILogger>();
        }

        protected virtual void WriteClasses(CodeWriter writer, Database schema, GenerationContext context)
        {
            foreach (var table in schema.Tables)
                WriteClass(writer, table, schema, context);
        }

        protected virtual void WriteClass(CodeWriter writer, Table table, Database schema, GenerationContext context)
        {
            writer.WriteLine();

            string entityBase = context.Parameters.EntityBase;
            if (string.IsNullOrEmpty(entityBase))
                entityBase = schema.EntityBase;

            var specifications = SpecificationDefinition.Partial;
            if (table.Type.AccessModifierSpecified)
                specifications |= GetSpecificationDefinition(table.Type.AccessModifier);
            else
                specifications |= SpecificationDefinition.Public;
            if (table.Type.ModifierSpecified)
                specifications |= GetSpecificationDefinition(table.Type.Modifier);

            var tableAttribute = NewAttributeDefinition<TableAttribute>();
            tableAttribute["Name"] = table.Name;
            using (WriteAttributes(writer, context.Parameters.EntityExposedAttributes))
            using (writer.WriteAttribute(tableAttribute))
            using (writer.WriteClass(specifications,
                                     table.Type.Name, entityBase, context.Parameters.EntityImplementedInterfaces))
            {
                WriteClassHeader(writer, table, context);
                WriteCustomTypes(writer, table, schema, context);
                WriteClassProperties(writer, table, context);
                if (context.Parameters.GenerateEqualsAndHash)
                    WriteClassEqualsAndHash(writer, table, context);
                WriteClassChildren(writer, table, schema, context);
                WriteClassParents(writer, table, schema, context);
            }
        }

        protected virtual void WriteClassEqualsAndHash(CodeWriter writer, Table table, GenerationContext context)
        {
            List<DbLinq.Schema.Dbml.Column> primaryKeys = table.Type.Columns.Where(c => c.IsPrimaryKey).ToList();
            if (primaryKeys.Count == 0)
            {
                writer.WriteLine("#warning L189 table {0} has no primary key. Multiple C# objects will refer to the same row.",
                                 table.Name);
                return;
            }

            using (writer.WriteRegion(string.Format("GetHashCode(), Equals() - uses column {0} to look up objects in liveObjectMap",
                                                    string.Join(", ", primaryKeys.Select(pk => pk.Member).ToList().ToArray()))))
            {
                // GetHashCode
                using (writer.WriteMethod(SpecificationDefinition.Public | SpecificationDefinition.Override,
                                          "GetHashCode", typeof(int)))
                {
                    string hashCode = null;

                    foreach (var primaryKey in primaryKeys)
                    {
                        var member = writer.GetVariableExpression(primaryKey.Storage);
                        string primaryKeyHashCode = writer.GetMethodCallExpression(writer.GetMemberExpression(member, "GetHashCode"));
                        if (primaryKey.CanBeNull
                        || primaryKey.ExtendedType == null
                        || GetType(primaryKey.Type, false).IsClass) // this patch to ensure that even if DB does not allow nulls,
                        // our in-memory object won't generate a fault
                        {
                            var isNullExpression = writer.GetEqualExpression(member, writer.GetNullExpression());
                            var nullExpression = writer.GetLiteralValue(0);
                            primaryKeyHashCode = writer.GetTernaryExpression(isNullExpression, nullExpression, primaryKeyHashCode);
                        }
                        if (string.IsNullOrEmpty(hashCode))
                            hashCode = primaryKeyHashCode;
                        else
                            hashCode = writer.GetXOrExpression(hashCode, primaryKeyHashCode);
                    }
                    writer.WriteLine(writer.GetReturnStatement(hashCode));
                }
                writer.WriteLine();

                // Equals
                string otherAsObject = "o";
                using (writer.WriteMethod(SpecificationDefinition.Public | SpecificationDefinition.Override,
                                          "Equals", typeof(bool), new ParameterDefinition { Type = typeof(object), Name = otherAsObject }))
                {
                    string other = "other";
                    writer.WriteLine(writer.GetStatement(writer.GetAssignmentExpression(
                                                             writer.GetDeclarationExpression(other, table.Type.Name),
                                                             writer.GetCastExpression(otherAsObject, table.Type.Name,
                                                                                      false))));
                    using (writer.WriteIf(writer.GetEqualExpression(other, writer.GetNullExpression())))
                    {
                        writer.WriteLine(writer.GetReturnStatement(writer.GetLiteralValue(false)));
                    }
                    string andExpression = null;
                    foreach (var primaryKey in primaryKeys)
                    {
                        var member = writer.GetVariableExpression(primaryKey.Storage);
                        string primaryKeyTest = writer.GetMethodCallExpression(writer.GetMemberExpression(writer.GetLiteralType(typeof(object)), "Equals"),
                                                                               member,
                                                                               writer.GetMemberExpression(other, member));
                        if (string.IsNullOrEmpty(andExpression))
                            andExpression = primaryKeyTest;
                        else
                            andExpression = writer.GetAndExpression(andExpression, primaryKeyTest);
                    }
                    writer.WriteLine(writer.GetReturnStatement(andExpression));
                }
            }
        }

        private void WriteClassHeader(CodeWriter writer, Table table, GenerationContext context)
        {
            foreach (IImplementation implementation in context.Implementations())
                implementation.WriteHeader(writer, table, context);
        }

        protected virtual void WriteClassProperties(CodeWriter writer, Table table, GenerationContext context)
        {
            foreach (var property in table.Type.Columns)
                WriteClassProperty(writer, property, context);
        }

        protected virtual string GetTypeOrExtendedType(CodeWriter writer, Column property)
        {
            object extendedType = property.ExtendedType;
            var enumType = extendedType as EnumType;
            if (enumType != null)
                return writer.GetEnumType(enumType.Name);
            return writer.GetLiteralType(GetType(property.Type, property.CanBeNull));
        }

        protected virtual void WriteClassProperty(CodeWriter writer, Column property, GenerationContext context)
        {
            using (writer.WriteRegion(string.Format("{0} {1}", GetTypeOrExtendedType(writer, property), property.Member)))
            {
                WriteClassPropertyBackingField(writer, property, context);
                WriteClassPropertyAccessors(writer, property, context);
            }
        }

        protected virtual void WriteClassPropertyBackingField(CodeWriter writer, Column property, GenerationContext context)
        {
            //AttributeDefinition autoGenAttribute = null;
            //if (property.IsDbGenerated)
            //    autoGenAttribute = NewAttributeDefinition<AutoGenIdAttribute>();
            //using (writer.WriteAttribute(autoGenAttribute))
            writer.WriteField(SpecificationDefinition.Private, property.Storage, GetTypeOrExtendedType(writer, property));
        }

        protected virtual void WriteClassPropertyAccessors(CodeWriter writer, Column property, GenerationContext context)
        {
            //generate [Column(...)] attribute
            var column = NewAttributeDefinition<ColumnAttribute>();
            column["Storage"] = property.Storage;
            column["Name"] = property.Name;
            column["DbType"] = property.DbType;
            // be smart: we only write attributes when they differ from the default values
            var columnAttribute = new ColumnAttribute();
            if (property.IsPrimaryKey != columnAttribute.IsPrimaryKey)
                column["IsPrimaryKey"] = property.IsPrimaryKey;
            if (property.IsDbGenerated != columnAttribute.IsDbGenerated)
                column["IsDbGenerated"] = property.IsDbGenerated;
            if (property.CanBeNull != columnAttribute.CanBeNull)
                column["CanBeNull"] = property.CanBeNull;
            if (property.Expression != null)
                column["Expression"] = property.Expression;

            var specifications = property.AccessModifierSpecified
                                     ? GetSpecificationDefinition(property.AccessModifier)
                                     : SpecificationDefinition.Public;
            if (property.ModifierSpecified)
                specifications |= GetSpecificationDefinition(property.Modifier);

            using (WriteAttributes(writer, context.Parameters.MemberExposedAttributes))
            using (writer.WriteAttribute(NewAttributeDefinition<DebuggerNonUserCodeAttribute>()))
            using (writer.WriteAttribute(column))
            using (writer.WriteProperty(specifications, property.Member, GetTypeOrExtendedType(writer, property)))
            {
                using (writer.WritePropertyGet())
                {
                    writer.WriteLine(writer.GetReturnStatement(writer.GetVariableExpression(property.Storage)));
                }
                using (writer.WritePropertySet())
                {
                    using (writer.WriteIf(writer.GetDifferentExpression(writer.GetPropertySetValueExpression(), writer.GetVariableExpression(property.Storage))))
                    {
                        foreach (IImplementation implementation in context.Implementations())
                            implementation.WritePropertyBeforeSet(writer, property, context);
                        writer.WriteLine(writer.GetStatement(writer.GetAssignmentExpression(writer.GetVariableExpression(property.Storage), writer.GetPropertySetValueExpression())));
                        foreach (IImplementation implementation in context.Implementations())
                            implementation.WritePropertyAfterSet(writer, property, context);
                    }
                }
            }
        }

        protected virtual void WriteClassChildren(CodeWriter writer, Table table, Database schema, GenerationContext context)
        {
            var children = table.Type.Associations.Where(a => !a.IsForeignKey).ToList();
            if (children.Count > 0)
            {
                using (writer.WriteRegion("Children"))
                {
                    foreach (var child in children)
                    {
                        WriteClassChild(writer, child, schema, context);
                    }
                }
            }
        }

        private void WriteClassChild(CodeWriter writer, Association child, Database schema, GenerationContext context)
        {
            // the following is apparently useless
            DbLinq.Schema.Dbml.Table targetTable = schema.Tables.FirstOrDefault(t => t.Type.Name == child.Type);
            if (targetTable == null)
            {
                Logger.Write(Level.Error, "ERROR L143 target table class not found:" + child.Type);
                return;
            }

            var storageAttribute = NewAttributeDefinition<AssociationAttribute>();
            storageAttribute["Storage"] = null;
            storageAttribute["OtherKey"] = child.OtherKey;
            storageAttribute["Name"] = child.Name;

            SpecificationDefinition specifications;
            if (child.AccessModifierSpecified)
                specifications = GetSpecificationDefinition(child.AccessModifier);
            else
                specifications = SpecificationDefinition.Public;
            if (child.ModifierSpecified)
                specifications |= GetSpecificationDefinition(child.Modifier);

            using (writer.WriteAttribute(storageAttribute))
            using (writer.WriteAttribute(NewAttributeDefinition<DebuggerNonUserCodeAttribute>()))
            using (writer.WriteProperty(specifications, child.Member,
                                        writer.GetGenericName(typeof(EntitySet<>).Name.Split('`')[0], child.Type)))
            {
                using (writer.WritePropertyGet())
                {
                    writer.WriteCommentLine("L212 - child data available only when part of query");
                    writer.WriteLine(writer.GetReturnStatement(writer.GetNullExpression()));
                }
            }
            writer.WriteLine();
        }

        protected virtual void WriteClassParents(CodeWriter writer, Table table, Database schema, GenerationContext context)
        {
            var parents = table.Type.Associations.Where(a => a.IsForeignKey).ToList();
            if (parents.Count > 0)
            {
                using (writer.WriteRegion("Parents"))
                {
                    foreach (var parent in parents)
                    {
                        WriteClassParent(writer, parent, schema, context);
                    }
                }
            }
        }

        protected virtual void WriteClassParent(CodeWriter writer, Association parent, Database schema, GenerationContext context)
        {
            // the following is apparently useless
            DbLinq.Schema.Dbml.Table targetTable = schema.Tables.FirstOrDefault(t => t.Type.Name == parent.Type);
            if (targetTable == null)
            {
                Logger.Write(Level.Error, "ERROR L191 target table type not found: " + parent.Type + "  (processing " + parent.Name + ")");
                return;
            }

            string member = parent.Member;
            string storageField = parent.Storage;
            if (member == parent.ThisKey)
            {
                member = parent.ThisKey + targetTable.Type.Name; //repeat name to prevent collision (same as Linq)
                storageField = "_x_" + parent.Member;
            }

            writer.WriteField(SpecificationDefinition.Private, storageField,
                              writer.GetGenericName(typeof(EntityRef<>).FullName.Split('`')[0], targetTable.Type.Name));

            var storageAttribute = NewAttributeDefinition<AssociationAttribute>();
            storageAttribute["Storage"] = storageField;
            storageAttribute["ThisKey"] = parent.ThisKey;
            storageAttribute["Name"] = parent.Name;
            storageAttribute["IsForeignKey"] = parent.IsForeignKey;

            SpecificationDefinition specifications;
            if (parent.AccessModifierSpecified)
                specifications = GetSpecificationDefinition(parent.AccessModifier);
            else
                specifications = SpecificationDefinition.Public;
            if (parent.ModifierSpecified)
                specifications |= GetSpecificationDefinition(parent.Modifier);

            using (writer.WriteAttribute(storageAttribute))
            using (writer.WriteAttribute(NewAttributeDefinition<DebuggerNonUserCodeAttribute>()))
            using (writer.WriteProperty(specifications, member, targetTable.Type.Name))
            {
                string storage = writer.GetMemberExpression(storageField, "Entity");
                using (writer.WritePropertyGet())
                {
                    writer.WriteLine(writer.GetReturnStatement(storage));
                }
                using (writer.WritePropertySet())
                {
                    writer.WriteLine(writer.GetStatement(writer.GetAssignmentExpression(storage, writer.GetPropertySetValueExpression())));
                }
            }
            writer.WriteLine();
        }
    }
}
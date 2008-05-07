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
using System.Collections;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Reflection;
using DbLinq.Util;
using System.Collections.Generic;

namespace DbLinq.Linq.Mapping
{
    /// <summary>
    /// This class is a stateless attribute meta model (it does not depend on any provider)
    /// So the MappingSource can use singletons
    /// </summary>
    internal class AttributedMetaModel : MetaModel
    {
        public AttributedMetaModel(Type dataContextType)
        {
            contextType = dataContextType;
            Load();
        }

        protected virtual void Load()
        {
            // global attributes
            var database = GetDatabaseAttribute();
            databaseName = database != null ? database.Name : null;

            // stored procedures
            metaFunctions = new Dictionary<MethodInfo, MetaFunction>();
            var functionAttributes = GetFunctionsAttributes();
            foreach (var functionPair in functionAttributes)
            {
                metaFunctions[functionPair.Key] = new AttributedMetaFunction(functionPair.Key, functionPair.Value);
            }

            // tables
            tables = new Dictionary<Type, MetaTable>();
            var tableAttributes = GetTablesAttributes();
            foreach (var tablePair in tableAttributes)
            {
                var type = new AttributedMetaType(tablePair.Key);
                var table = new AttributedMetaTable(tablePair.Value, type);
                tables[tablePair.Key] = table;
                type.SetMetaTable(table);
            }

            // reverse associations
            foreach (var table in GetTables())
            {
                foreach (var association in table.RowType.Associations)
                {
                    // we cast to call the SetOtherKey method
                    var attributedAssociation = association as AttributedMetaAssociation;
                    if (attributedAssociation != null)
                    {
                        var memberInfo = attributedAssociation.ThisMember.Member;
                        var associationAttribute = memberInfo.GetAttribute<AssociationAttribute>();
                        var memberType = memberInfo.GetMemberType();
                        Type otherTableType;
                        if (memberType.IsGenericType)
                            otherTableType = memberType.GetGenericArguments()[0];
                        else
                            otherTableType = memberType;
                        var otherTable = GetTable(otherTableType);
                        // then we lookup by the attribute if we have a match
                        MetaDataMember otherAssociationMember = null;
                        foreach (var member in otherTableType.GetMembers())
                        {
                            var otherAssociationAttribute = member.GetAttribute<AssociationAttribute>();
                            if (otherAssociationAttribute != null && otherAssociationAttribute.ThisKey == associationAttribute.OtherKey)
                            {
                                otherAssociationMember =
                                    (from a in otherTable.RowType.Associations
                                     where a.ThisMember.Member == member
                                     select a.ThisMember).SingleOrDefault();
                                break;
                            }
                        }
                        attributedAssociation.SetOtherKey(associationAttribute.OtherKey, otherTable, otherAssociationMember);
                    }
                }
            }
        }

        protected virtual DatabaseAttribute GetDatabaseAttribute()
        {
            return contextType.GetAttribute<DatabaseAttribute>();
        }

        protected virtual IDictionary<MethodInfo, FunctionAttribute> GetFunctionsAttributes()
        {
            var functionAttributes = new Dictionary<MethodInfo, FunctionAttribute>();
            foreach (var methodInfo in contextType.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                var function = methodInfo.GetAttribute<FunctionAttribute>();
                if (function != null)
                    functionAttributes[methodInfo] = function;
            }
            return functionAttributes;
        }

        protected virtual IDictionary<Type, TableAttribute> GetTablesAttributes()
        {
            var tableAttributes = new Dictionary<Type, TableAttribute>();
            // to find the tables, we list all properties/fields contained in the DataContext inheritor
            // if the return type has a TableAttribute, then it is ours (muhahahah!)
            foreach (var memberInfo in contextType.GetMembers(BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                var memberType = memberInfo.GetMemberType();
                if (memberType == null)
                    continue;
                var classType = GetClassType(memberType);
                if (classType == null)
                    continue;
                var table = classType.GetAttribute<TableAttribute>();
                if (table != null)
                    tableAttributes[classType] = table;
            }
            return tableAttributes;
        }

        protected virtual Type GetClassType(Type t)
        {
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Table<>))
                return t.GetGenericArguments()[0];
            return null;
        }

        private Type contextType;
        public override Type ContextType
        {
            get { return contextType; }
        }

        private string databaseName;
        public override string DatabaseName
        {
            get { return databaseName; }
        }

        private IDictionary<MethodInfo, MetaFunction> metaFunctions;
        public override MetaFunction GetFunction(MethodInfo method)
        {
            MetaFunction metaFunction;
            metaFunctions.TryGetValue(method, out metaFunction);
            return metaFunction;
        }

        public override IEnumerable<MetaFunction> GetFunctions()
        {
            return metaFunctions.Values;
        }

        public override MetaType GetMetaType(Type type)
        {
            var metaTable = GetTable(type);
            if (metaTable == null)
                return null;
            return metaTable.RowType;
        }

        private IDictionary<Type, MetaTable> tables;
        public override MetaTable GetTable(Type rowType)
        {
            MetaTable metaTable;
            tables.TryGetValue(rowType, out metaTable);
            return metaTable;
        }

        public override IEnumerable<MetaTable> GetTables()
        {
            return tables.Values;
        }

        private Type providerType;
        public override Type ProviderType
        {
            get { throw new NotImplementedException(); }
        }

        // this is by design: it is not this class responsibility to return a mapping source type
        public override MappingSource MappingSource
        {
            get { throw new NotImplementedException(); }
        }
    }
}

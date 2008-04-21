﻿#region MIT license
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
using System.Data;
using System.Linq;
using DbLinq.Factory;
using DbLinq.Linq;
using DbLinq.Linq.Implementation;
using DbLinq.Logging;
using DbLinq.Schema;
using DbLinq.Schema.Dbml;

namespace DbLinq.Vendor.Implementation
{
    public abstract partial class SchemaLoader : ISchemaLoader
    {
        public virtual string VendorName { get { return Vendor.VendorName; } }
        public abstract IVendor Vendor { get; }
        public abstract System.Type DataContextType { get; }
        public IDbConnection Connection { get; set; }
        public INameFormatter NameFormatter { get; set; }
        public ILogger Logger { get; set; }

        public virtual Database Load(string databaseName, INameAliases nameAliases, NameFormat nameFormat,
            bool loadStoredProcedures, string contextNamespace, string entityNamespace)
        {
            // check if connection is open. Note: we may use something more flexible
            if (Connection.State != ConnectionState.Open)
                Connection.Open();

            // get the database name. If we don't have one, take it from connection string...
            if (string.IsNullOrEmpty(databaseName))
                databaseName = Connection.Database;
            // ... and if connection string doesn't provide a name, then throw an error
            if (string.IsNullOrEmpty(databaseName))
                throw new ArgumentException("A database name is required. Please specify /database=<databaseName>");

            var schemaName = NameFormatter.GetSchemaName(databaseName, GetExtraction(databaseName), nameFormat);
            var names = new Names();
            var schema = new Database
                             {
                                 Name = schemaName.DbName,
                                 Class = schemaName.ClassName,
                                 BaseType = typeof(DataContext).FullName,
                                 ContextNamespace = contextNamespace,
                                 EntityNamespace = entityNamespace,
                             };

            // order is important, we must have:
            // 1. tables
            // 2. columns
            // 3. constraints
            LoadTables(schema, schemaName, Connection, nameAliases, nameFormat, names);
            LoadColumns(schema, schemaName, Connection, nameAliases, nameFormat, names);
            LoadConstraints(schema, schemaName, Connection, nameFormat, names);
            if (loadStoredProcedures)
                LoadStoredProcedures(schema, schemaName, Connection, nameFormat);

            return schema;
        }

        protected SchemaLoader()
        {
            Logger = ObjectFactory.Get<ILogger>();
            NameFormatter = ObjectFactory.Create<INameFormatter>(); // the Pluralize property is set dynamically, so no singleton
        }

        protected virtual WordsExtraction GetExtraction(string dbColumnName)
        {
            bool isMixedCase = dbColumnName != dbColumnName.ToLower() && dbColumnName != dbColumnName.ToUpper();
            return isMixedCase ? WordsExtraction.FromCase : WordsExtraction.FromDictionary;
        }

        protected virtual string GetFullCaseSafeDbName(string dbName, string dbSchema)
        {
            string fullDbName;
            if (dbSchema == null)
                fullDbName = dbName;
            else
                fullDbName = string.Format("{0}.{1}", dbSchema, dbName);
            fullDbName = Vendor.GetSqlCaseSafeName(fullDbName);
            return fullDbName;
        }

        protected virtual TableName CreateTableName(string dbTableName, string dbSchema, INameAliases nameAliases, NameFormat nameFormat, WordsExtraction extraction)
        {
            // if we have an alias, use it, and don't try to analyze it (a human probably already did the job)
            var tableTypeAlias = nameAliases != null ? nameAliases.GetTableTypeAlias(dbTableName, dbSchema) : null;
            if (tableTypeAlias != null)
                extraction = WordsExtraction.None;
            else
                tableTypeAlias = dbTableName;

            var tableName = NameFormatter.GetTableName(tableTypeAlias, extraction, nameFormat);

            // alias for member
            var tableMemberAlias = nameAliases != null ? nameAliases.GetTableMemberAlias(dbTableName, dbSchema) : null;
            if (tableMemberAlias != null)
                tableName.MemberName = tableMemberAlias;

            tableName.DbName = GetFullCaseSafeDbName(dbTableName, dbSchema);
            return tableName;
        }

        protected virtual TableName CreateTableName(string dbTableName, string dbSchema, INameAliases nameAliases, NameFormat nameFormat)
        {
            return CreateTableName(dbTableName, dbSchema, nameAliases, nameFormat, GetExtraction(dbTableName));
        }

        protected virtual ColumnName CreateColumnName(string dbColumnName, string dbTableName, string dbSchema, INameAliases nameAliases, NameFormat nameFormat)
        {
            var columnNameAlias = nameAliases != null ? nameAliases.GetColumnMemberAlias(dbColumnName, dbTableName, dbSchema) : null;
            WordsExtraction extraction;
            if (columnNameAlias != null)
            {
                extraction = WordsExtraction.None;
            }
            else
            {
                extraction = GetExtraction(dbColumnName);
                columnNameAlias = dbColumnName;
            }
            var columnName = NameFormatter.GetColumnName(columnNameAlias, extraction, nameFormat);
            columnName.DbName = Vendor.GetSqlCaseSafeName(dbColumnName);
            return columnName;
        }

        protected virtual ProcedureName CreateProcedureName(string dbProcedureName, string dbSchema, NameFormat nameFormat)
        {
            var procedureName = NameFormatter.GetProcedureName(dbProcedureName, GetExtraction(dbProcedureName), nameFormat);
            procedureName.DbName = GetFullCaseSafeDbName(dbProcedureName, dbSchema);
            return procedureName;
        }

        protected virtual AssociationName CreateAssociationName(string dbManyName, string dbManySchema,
            string dbOneName, string dbOneSchema, string dbConstraintName, NameFormat nameFormat)
        {
            var associationName = NameFormatter.GetAssociationName(dbManyName, dbOneName,
                dbConstraintName, GetExtraction(dbManyName), nameFormat);
            associationName.DbName = GetFullCaseSafeDbName(dbManyName, dbManySchema);
            return associationName;
        }

        protected virtual SchemaName CreateSchemaName(string databaseName, IDbConnection connection, NameFormat nameFormat)
        {
            if (string.IsNullOrEmpty(databaseName))
            {
                databaseName = connection.Database;
                if (string.IsNullOrEmpty(databaseName))
                    throw new ArgumentException("Could not deduce database name from connection string. Please specify /database=<databaseName>");
            }
            return NameFormatter.GetSchemaName(databaseName, GetExtraction(databaseName), nameFormat);
        }

        protected virtual ParameterName CreateParameterName(string dbParameterName, NameFormat nameFormat)
        {
            var parameterName = NameFormatter.GetParameterName(dbParameterName, GetExtraction(dbParameterName), nameFormat);
            return parameterName;
        }

        protected class Names
        {
            public IDictionary<string, TableName> TablesNames = new Dictionary<string, TableName>();
            public IDictionary<string, IDictionary<string, ColumnName>> ColumnsNames = new Dictionary<string, IDictionary<string, ColumnName>>();

            public void AddColumn(string dbTableName, ColumnName columnName)
            {
                IDictionary<string, ColumnName> columns;
                if (!ColumnsNames.TryGetValue(dbTableName, out columns))
                {
                    columns = new Dictionary<string, ColumnName>();
                    ColumnsNames[dbTableName] = columns;
                }
                columns[columnName.DbName] = columnName;
            }
        }

        protected virtual void LoadTables(Database schema, SchemaName schemaName, IDbConnection conn, INameAliases nameAliases, NameFormat nameFormat, Names names)
        {
            var tables = ReadTables(conn, schemaName.DbName);
            foreach (var row in tables)
            {
                var tableName = CreateTableName(row.Name, row.Schema, nameAliases, nameFormat);
                names.TablesNames[tableName.DbName] = tableName;

                var table = new Table();
                table.Name = tableName.DbName;
                table.Member = tableName.MemberName;
                table.Type.Name = tableName.ClassName;
                schema.Tables.Add(table);
            }
        }

        protected void LoadColumns(Database schema, SchemaName schemaName, IDbConnection conn, INameAliases nameAliases, NameFormat nameFormat, Names names)
        {
            var columns = ReadColumns(conn, schemaName.DbName);
            foreach (var columnRow in columns)
            {
                var columnName = CreateColumnName(columnRow.ColumnName, columnRow.TableName, columnRow.TableSchema, nameAliases, nameFormat);
                names.AddColumn(columnRow.TableName, columnName);

                //find which table this column belongs to
                string fullColumnDbName = GetFullCaseSafeDbName(columnRow.TableName, columnRow.TableSchema);
                DbLinq.Schema.Dbml.Table tableSchema = schema.Tables.FirstOrDefault(tblSchema => fullColumnDbName == tblSchema.Name);
                if (tableSchema == null)
                {
                    Logger.Write(Level.Error, "ERROR L46: Table '" + columnRow.TableName + "' not found for column " + columnRow.ColumnName);
                    continue;
                }
                var colSchema = new Column();
                colSchema.Name = columnName.DbName;
                colSchema.Member = columnName.PropertyName;
                colSchema.Storage = columnName.StorageFieldName;
                colSchema.DbType = columnRow.FullType;

                if (columnRow.PrimaryKey.HasValue)
                    colSchema.IsPrimaryKey = columnRow.PrimaryKey.Value;

                if (columnRow.Generated.HasValue)
                    colSchema.IsDbGenerated = columnRow.Generated.Value;

                if (colSchema.IsDbGenerated)
                    colSchema.Expression = columnRow.DefaultValue;

                colSchema.CanBeNull = columnRow.Nullable;
                colSchema.Type = MapDbType(columnRow).ToString();

                tableSchema.Type.Columns.Add(colSchema);
            }
        }
    }
}

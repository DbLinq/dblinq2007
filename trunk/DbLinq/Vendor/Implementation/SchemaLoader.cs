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

        public virtual Database Load(string databaseName, IDictionary<string, string> tableAliases, NameFormat nameFormat, bool loadStoredProcedures)
        {
            if (Connection.State != ConnectionState.Open)
                Connection.Open();
            if (string.IsNullOrEmpty(databaseName))
                databaseName = Connection.Database;
            if (string.IsNullOrEmpty(databaseName))
                throw new ArgumentException("A database name is required. Please specify /database=<databaseName>");
            var schemaName = NameFormatter.GetSchemaName(databaseName, GetExtraction(databaseName), nameFormat);
            return Load(schemaName, tableAliases, nameFormat, loadStoredProcedures);
        }

        protected abstract Database Load(SchemaName schemaName, IDictionary<string, string> tableAliases, NameFormat nameFormat, bool loadStoredProcedures);

        protected SchemaLoader()
        {
            Logger = ObjectFactory.Get<ILogger>();
            NameFormatter = ObjectFactory.Create<INameFormatter>(); // the Pluralize property is set dynamically, so no singleton
        }

        protected virtual WordsExtraction GetExtraction(string dbColumnName)
        {
            return Vendor.IsCaseSensitiveName(dbColumnName) ? WordsExtraction.FromCase : WordsExtraction.FromDictionary;
        }

        protected virtual string GetFullDbName(string dbName, string dbSchema)
        {
            if (dbSchema == null)
                return dbName;
            return string.Format("{0}.{1}", dbSchema, dbName);
        }

        protected virtual TableName CreateTableName(string dbTableName, string dbSchema, IDictionary<string, string> tableAliases, NameFormat nameFormat)
        {
            WordsExtraction extraction = GetExtraction(dbTableName);
            // if we have an alias, use it, and don't try to analyze it (a human probably already did the job)
            if (tableAliases != null && tableAliases.ContainsKey(dbTableName))
            {
                extraction = WordsExtraction.FromCase;
                dbTableName = tableAliases[dbTableName];
            }
            var tableName = NameFormatter.GetTableName(dbTableName, extraction, nameFormat);
            tableName.DbName = GetFullDbName(dbTableName, dbSchema);
            return tableName;
        }

        protected virtual ColumnName CreateColumnName(string dbColumnName, NameFormat nameFormat)
        {
            return NameFormatter.GetColumnName(dbColumnName, GetExtraction(dbColumnName), nameFormat);
        }

        protected virtual ProcedureName CreateProcedureName(string dbProcedureName, string dbSchema, NameFormat nameFormat)
        {
            var procedureName = NameFormatter.GetProcedureName(dbProcedureName, GetExtraction(dbProcedureName), nameFormat);
            procedureName.DbName = GetFullDbName(dbProcedureName, dbSchema);
            return procedureName;
        }

        protected virtual AssociationName CreateAssociationName(string dbManyName, string dbManySchema,
            string dbOneName, string dbOneSchema, string dbConstraintName, NameFormat nameFormat)
        {
            var associationName = NameFormatter.GetAssociationName(dbManyName, dbOneName,
                dbConstraintName, GetExtraction(dbManyName), nameFormat);
            associationName.DbName = GetFullDbName(dbManyName, dbManySchema);
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

        protected virtual void LoadTables(Database schema, SchemaName schemaName, IDbConnection conn, IDictionary<string, string> tableAliases, NameFormat nameFormat, Names names)
        {
            var tables = ReadTables(conn, schemaName.DbName);
            foreach (var row in tables)
            {
                var tableName = CreateTableName(row.Name, row.Schema, tableAliases, nameFormat);
                names.TablesNames[tableName.DbName] = tableName;

                var table = new Table();
                table.Name = tableName.DbName;
                table.Member = tableName.MemberName;
                table.Type.Name = tableName.ClassName;
                schema.Tables.Add(table);
            }
        }

        protected void LoadColumns(Database schema, SchemaName schemaName, IDbConnection conn, NameFormat nameFormat, Names names)
        {
            var columns = ReadColumns(conn, schemaName.DbName);
            foreach (var columnRow in columns)
            {
                var columnName = CreateColumnName(columnRow.ColumnName, nameFormat);
                names.AddColumn(columnRow.TableName, columnName);

                //find which table this column belongs to
                string fullColumnDbName = GetFullDbName(columnRow.TableName, columnRow.TableSchema);
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

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
using System.Data;
using DbLinq.Factory;
using DbLinq.Linq;
using DbLinq.Linq.Implementation;
using DbLinq.Logging;
using DbLinq.Schema;
using DbLinq.Schema.Dbml;

namespace DbLinq.Vendor.Implementation
{
    public abstract class SchemaLoader : ISchemaLoader
    {
        public virtual string VendorName { get { return Vendor.VendorName; } }
        protected abstract IVendor Vendor { get; }
        public abstract System.Type DataContextType { get; }
        public IDbConnection Connection { get; set; }
        public INameFormatter NameFormatter { get; set; }
        public ILogger Logger { get; set; }

        public abstract Database Load(string databaseName, IDictionary<string, string> tableAliases, bool pluralize, bool loadStoredProcedures);

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
            return NameFormatter.GetFullDbName(dbName, dbSchema);
        }

        protected virtual TableName CreateTableName(string dbTableName, string dbSchema, IDictionary<string, string> tableAliases)
        {
            WordsExtraction extraction = GetExtraction(dbTableName);
            // if we have an alias, use it, and don't try to analyze it (a human probably already did the job)
            if (tableAliases != null && tableAliases.ContainsKey(dbTableName))
            {
                extraction = WordsExtraction.FromCase;
                dbTableName = tableAliases[dbTableName];
            }
            return NameFormatter.GetTableName(dbTableName, dbSchema, extraction);
        }

        protected virtual ColumnName CreateColumnName(string dbColumnName)
        {
            return NameFormatter.GetColumnName(dbColumnName, GetExtraction(dbColumnName));
        }

        protected virtual ProcedureName CreateProcedureName(string dbProcedureName, string dbSchema)
        {
            return NameFormatter.GetProcedureName(dbProcedureName, dbSchema, GetExtraction(dbProcedureName));
        }

        protected virtual AssociationName CreateAssociationName(string dbManyName, string dbManySchema,
            string dbOneName, string dbOneSchema, string dbConstraintName)
        {
            return NameFormatter.GetAssociationName(dbManyName, dbManySchema, dbOneName, dbOneSchema,
                dbConstraintName, GetExtraction(dbManyName));
        }

        protected virtual SchemaName CreateSchemaName(string databaseName, IDbConnection connection)
        {
            if (string.IsNullOrEmpty(databaseName))
            {
                databaseName = connection.Database;
                if (string.IsNullOrEmpty(databaseName))
                    throw new ArgumentException("Could not deduce database name from connection string. Please specify /database=<databaseName>");
            }
            return NameFormatter.GetSchemaName(databaseName, GetExtraction(databaseName));
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

#if OBSOLETE

        [Obsolete("Use CreateTableName instead")]
        protected string GetTableName(string name, IDictionary<string, string> tableAliases)
        {
            if (tableAliases.ContainsKey(name))
                return tableAliases[name];
            return NameFormatter.AdjustTableName(name);
        }

        [Obsolete("Use CreateColumnName instead")]
        protected string GetColumnName(string name)
        {
            return NameFormatter.AdjustColumnName(name);
        }

        [Obsolete("Use CreateColumnName instead")]
        protected string GetColumnFieldName(string name)
        {
            return NameFormatter.AdjustColumnFieldName(name);
        }

        [Obsolete("Use CreateTableName instead")]
        protected string GetMethodName(string name)
        {
            return NameFormatter.AdjustMethodName(name);
        }

        [Obsolete("Use CreateAssociationName instead")]
        public virtual string GetManyToOneColumnName(string referencedTableName, string thisTableName)
        {
            // TODO: handle aliases?
            return NameFormatter.AdjustManyToOneColumnName(referencedTableName, thisTableName);
        }

        [Obsolete("Use CreateAssociationName instead")]
        public virtual string GetOneToManyColumnName(string referencedTableName)
        {
            // TODO: handle aliases?
            return NameFormatter.AdjustOneToManyColumnName(referencedTableName);
        }

#endif
    }
}
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;

using DbLinq.Schema.Dbml;
using DbLinq.Vendor.Implementation;

namespace DbLinq.Vendor
{
#if !MONO_STRICT
    public
#endif
    class DbSchemaLoader : SchemaLoader
    {
        public DbSchemaLoader()
        {
        }

        public override IVendor Vendor { get; set; }

        public new DbConnection Connection {
            get { return base.Connection as DbConnection; }
            set { base.Connection = value; }
        }

        protected override void LoadConstraints(Database schema, DbLinq.Schema.SchemaName schemaName, IDbConnection conn, DbLinq.Schema.NameFormat nameFormat, Names names)
        {
            throw new NotImplementedException();
        }

        private DataTable GetSchema(DbConnection connection, string schema)
        {
            var schemas = connection.GetSchema();
            var iCollectionName = schemas.Columns.IndexOf("CollectionName");
            if (!schemas.Rows.Cast<DataRow>().Any(r => r[iCollectionName].ToString() == schema))
                return null;
            return connection.GetSchema(schema);
        }

        protected override void LoadForeignKey(Database schema, Table table, string columnName, string tableName, string tableSchema, string referencedColumnName, string referencedTableName, string referencedTableSchema, string constraintName, DbLinq.Schema.NameFormat nameFormat, SchemaLoader.Names names)
        {
#if false
            var foriegnKeys = GetSchema("ForeignKeys");
            if (foriegnKeys == null)
                return;
#endif
            throw new NotImplementedException();
        }

        protected override void LoadStoredProcedures(Database schema, DbLinq.Schema.SchemaName schemaName, IDbConnection conn, DbLinq.Schema.NameFormat nameFormat)
        {
#if false
            var foriegnKeys = GetSchema("Procedures");
            if (foriegnKeys == null)
                return;
#endif
            throw new NotImplementedException();
        }

        protected override IList<IDataTableColumn> ReadColumns(IDbConnection connection, string databaseName)
        {
            var db = (DbConnection) connection;

            var typeMap   = GetSqlToManagedTypeMapping(db);

            var dbColumns = db.GetSchema("Columns");
            var iColumn   = dbColumns.Columns.IndexOf("COLUMN_NAME");
            var iDefValue = dbColumns.Columns.IndexOf("COLUMN_DEFAULT");
            var iNullable = dbColumns.Columns.IndexOf("IS_NULLABLE");
            var iMaxLen   = dbColumns.Columns.IndexOf("CHARACTER_MAXIMUM_LENGTH");
            var iNumPrec  = dbColumns.Columns.IndexOf("NUMERIC_PRECISION");
            var iDatPrec  = dbColumns.Columns.IndexOf("DATETIME_PRECISION");
            var iTable    = dbColumns.Columns.IndexOf("TABLE_NAME");
            var iSchema   = dbColumns.Columns.IndexOf("TABLE_SCHEMA");
            var iSqlType  = dbColumns.Columns.IndexOf("DATA_TYPE");

            var columns = new List<IDataTableColumn>();
            foreach (DataRow c in dbColumns.Rows)
            {
                var v = new DataTableColumn()
                {
                    ColumnName      = (string) c[iColumn],
                    DefaultValue    = c[iDefValue].ToString(),
                    Length          = (long) (int) c[iMaxLen],
                    ManagedType     = typeMap[(string) c[iSqlType]],
                    Nullable        = (bool) c[iNullable],
                    Precision       = c[iNumPrec] is DBNull ? (int?) null : (int) c[iNumPrec],
                    SqlType         = (string) c[iSqlType],
                    TableName       = (string) c[iTable],
                    TableSchema     = (string) c[iSchema],
                };
                columns.Add(v);
            }
            return columns;
        }

        private Dictionary<string, string> GetSqlToManagedTypeMapping(DbConnection connection)
        {
            var dataTypes = connection.GetSchema("DataTypes");
            var iSqlType = dataTypes.Columns.IndexOf("TypeName");
            var iNetType = dataTypes.Columns.IndexOf("DataType");
            return dataTypes.Rows.Cast<DataRow>()
                .ToDictionary(r => r[iSqlType].ToString(), r => r[iNetType].ToString());
        }

        public override IList<IDataName> ReadTables(IDbConnection connection, string databaseName)
        {
            DbConnection db = (DbConnection) connection;
            var dbTables  = db.GetSchema("Tables");
            var iName     = dbTables.Columns.IndexOf("TABLE_NAME");
            var iSchema   = dbTables.Columns.IndexOf("TABLE_SCHEMA");
            List<IDataName> tables = new List<IDataName>();
            foreach (DataRow table in dbTables.Rows)
            {
                tables.Add(new DataName()
                {
                    Name    = table[iName].ToString(),
                    Schema  = table[iSchema].ToString(),
                });
            }
            return tables;
        }
    }
}

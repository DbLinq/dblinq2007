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
//        Thomas Glaser
////////////////////////////////////////////////////////////////////
#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DbLinq.Ingres.Schema;
using DbLinq.Schema;
using DbLinq.Schema.Dbml;
using DbLinq.Util;
using DbLinq.Vendor.Implementation;

namespace DbLinq.Ingres
{
    class IngresSchemaLoader : SchemaLoader
    {
        private readonly Vendor.IVendor vendor = new IngresVendor();
        protected override Vendor.IVendor Vendor { get { return vendor; } }

        public override string VendorName { get { return "Ingres"; } }
        public override System.Type DataContextType { get { return typeof(IngresDataContext); } }
        public override Database Load(string databaseName, IDictionary<string, string> tableAliases, bool pluralize, bool loadStoredProcedures)
        {
            NameFormatter.Pluralize = pluralize; // TODO: this could go in a context (instead of service class)

            IDbConnection conn = Connection;
            conn.Open();

            var names = new Names();

            var schema = new Database();

            var schemaName = CreateSchemaName(databaseName, conn);
            schema.Name = schemaName.DbName;
            schema.Class = schemaName.ClassName;

            //##################################################################
            //step 1 - load tables
            TableSql tsql = new TableSql();
            List<TableRow> tables = tsql.getTables(conn, databaseName);
            if (tables == null || tables.Count == 0)
            {
                Console.WriteLine("No tables found for schema " + databaseName + ", exiting");
                return null;
            }

            foreach (TableRow tblRow in tables)
            {
                var tableName = CreateTableName(tblRow.table_name, tblRow.table_owner, tableAliases);
                names.TablesNames[tableName.DbName] = tableName;

                DbLinq.Schema.Dbml.Table tblSchema = new DbLinq.Schema.Dbml.Table();
                tblSchema.Name = tableName.DbName;
                tblSchema.Member = tableName.MemberName;
                tblSchema.Type.Name = tableName.ClassName;
                schema.Tables.Add(tblSchema);
            }

            //##################################################################
            //step 2 - load columns
            ColumnSql csql = new ColumnSql();
            List<Schema.Column> columns = csql.getColumns(conn, databaseName);

            foreach (Schema.Column columnRow in columns)
            {
                var columnName = CreateColumnName(columnRow.column_name);
                names.AddColumn(columnRow.table_name, columnName);

                //find which table this column belongs to
                DbLinq.Schema.Dbml.Table tableSchema = schema.Tables.FirstOrDefault(tblSchema => columnRow.TableNameWithSchema == tblSchema.Name);
                if (tableSchema == null)
                {
                    Console.WriteLine("ERROR L46: Table '" + columnRow.table_name + "' not found for column " + columnRow.column_name);
                    continue;
                }
                DbLinq.Schema.Dbml.Column colSchema = new DbLinq.Schema.Dbml.Column();
                colSchema.Name = columnName.DbName;
                colSchema.Member = columnName.PropertyName;
                colSchema.Storage = columnName.StorageFieldName;
                colSchema.DbType = columnRow.DataTypeWithWidth; //columnRow.datatype;

                colSchema.IsPrimaryKey = columnRow.key_sequence != 0;

                if (columnRow.column_default != null && columnRow.column_default.StartsWith("nextval("))
                    colSchema.IsDbGenerated = true;

                //parse sequence name from string such as "nextval('suppliers_supplierid_seq'::regclass)"
                if (colSchema.IsDbGenerated)
                    colSchema.Expression = columnRow.column_default.Replace("::regclass)", ")");

                //colSchema.IsVersion = ???
                colSchema.CanBeNull = columnRow.isNullable;
                colSchema.Type = Mappings.mapSqlTypeToCsType(
                    columnRow.datatype, 
                    columnRow.column_type, 
                    (columnRow.column_length.HasValue ? columnRow.column_length.Value : 0));
                if (CSharp.IsValueType(colSchema.Type) && columnRow.isNullable)
                    colSchema.Type += "?";

                if (columnRow.column_name == "employeetype" && columnRow.table_name == "employee" && databaseName == "Andrus")
                {
                    //Andrus DB - Employee table: hardcoded for testing of vertical Partitioning
                    colSchema.IsDiscriminator = true;
                }

                //tableSchema.Types[0].Columns.Add(colSchema);
                tableSchema.Type.Columns.Add(colSchema);
            }

            return schema;
        }
    }
}

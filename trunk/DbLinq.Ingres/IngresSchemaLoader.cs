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
using DbLinq.Logging;
using DbLinq.Schema;
using DbLinq.Schema.Dbml;
using DbLinq.Util;
using DbLinq.Vendor;
using DbLinq.Vendor.Implementation;

namespace DbLinq.Ingres
{
    class IngresSchemaLoader : SchemaLoader
    {
        private readonly Vendor.IVendor vendor = new IngresVendor();
        public override Vendor.IVendor Vendor { get { return vendor; } }

        public override System.Type DataContextType { get { return typeof(IngresDataContext); } }

        protected override Database Load(SchemaName schemaName, IDictionary<string, string> tableAliases, bool loadStoredProcedures)
        {
            IDbConnection conn = Connection;

            var names = new Names();

            var schema = new Database();

            schema.Name = schemaName.DbName;
            schema.Class = schemaName.ClassName;

            //##################################################################
            //step 1 - load tables
            TableSql tsql = new TableSql();
            List<TableRow> tables = tsql.getTables(conn, schemaName.DbName);
            if (tables == null || tables.Count == 0)
            {
                Console.WriteLine("No tables found for schema " + schemaName.DbName + ", exiting");
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
            List<Schema.Column> columns = csql.getColumns(conn, schemaName.DbName);

            foreach (Schema.Column columnRow in columns)
            {
                var columnName = CreateColumnName(columnRow.column_name);
                names.AddColumn(columnRow.table_name, columnName);

                //find which table this column belongs to
                string fullColumnDbName = GetFullDbName(columnRow.table_name, columnRow.table_owner);
                DbLinq.Schema.Dbml.Table tableSchema = schema.Tables.FirstOrDefault(tblSchema => fullColumnDbName == tblSchema.Name);
                if (tableSchema == null)
                {
                    Console.WriteLine("ERROR L46: Table '" + columnRow.table_name + "' not found for column " + columnRow.column_name);
                    continue;
                }
                DbLinq.Schema.Dbml.Column colSchema = new DbLinq.Schema.Dbml.Column();
                colSchema.Name = columnName.DbName;
                colSchema.Member = columnName.PropertyName;
                colSchema.Storage = columnName.StorageFieldName;
                colSchema.DbType = columnRow.DataTypeWithWidth; //columnRow.Type;

                colSchema.IsPrimaryKey = false;

                if (columnRow.column_default != null && columnRow.column_default.StartsWith("next value for"))
                {
                    colSchema.IsDbGenerated = true;
                    colSchema.Expression = columnRow.column_default;
                }

                //colSchema.IsVersion = ???
                colSchema.CanBeNull = columnRow.isNullable;
                colSchema.Type = MapDbType(columnRow).ToString();
                if (CSharp.IsValueType(colSchema.Type) && columnRow.isNullable)
                    colSchema.Type += "?";

                //tableSchema.Types[0].Columns.Add(colSchema);
                tableSchema.Type.Columns.Add(colSchema);
            }

            //##################################################################
            //step 3 - analyse foreign keys etc

            //TableSorter.Sort(tables, constraints); //sort tables - parents first

            ForeignKeySql fsql = new ForeignKeySql();

            List<ForeignKeyCrossRef> foreignKeys = fsql.getConstraints(conn, schemaName.DbName);

            foreach (ForeignKeyCrossRef keyColRow in foreignKeys)
            {
                //find my table:
                string constraintFullDbName = GetFullDbName(keyColRow.table_name_parent, keyColRow.schema_name_parent);
                DbLinq.Schema.Dbml.Table table = schema.Tables.FirstOrDefault(t => constraintFullDbName == t.Name);
                if (table == null)
                {
                    Logger.Write(Level.Error, "ERROR L138: Table '"
                        + keyColRow.table_name_parent
                        + "' not found for column "
                        + keyColRow.column_name_parent);
                    continue;
                }

                if (keyColRow.constraint_type.Equals("P")) //'PRIMARY KEY'
                {
                    foreach (string pk_name in keyColRow.column_name_primaries)
                    {
                        DbLinq.Schema.Dbml.Column primaryKeyCol = table.Type.Columns.First(c => c.Name == pk_name);
                        primaryKeyCol.IsPrimaryKey = true;
                    }
                    continue;
                }

                var associationName = CreateAssociationName(
                    keyColRow.table_name_parent,
                    keyColRow.schema_name_parent,
                    keyColRow.table_name_child,
                    keyColRow.schema_name_child,
                    keyColRow.constraint_name);

                //if not PRIMARY, it's a foreign key.
                //both parent and child table get an [Association]
                DbLinq.Schema.Dbml.Association assoc = new DbLinq.Schema.Dbml.Association();
                assoc.IsForeignKey = true;
                assoc.Name = keyColRow.constraint_name;
                assoc.Type = null;
                assoc.ThisKey = names.ColumnsNames[keyColRow.table_name_parent][keyColRow.column_name_parent].PropertyName;
                assoc.Member = associationName.ManyToOneMemberName;
                assoc.Storage = associationName.ForeignKeyStorageFieldName;
                table.Type.Associations.Add(assoc);

                //and insert the reverse association:
                DbLinq.Schema.Dbml.Association assoc2 = new DbLinq.Schema.Dbml.Association();
                assoc2.Name = keyColRow.constraint_name;
                assoc2.Type = table.Type.Name;
                assoc2.Member = associationName.OneToManyMemberName;
                assoc2.OtherKey = names.ColumnsNames[keyColRow.table_name_child][keyColRow.column_name_child].PropertyName; // GetColumnName(keyColRow.referenced_column_name);

                string parentFullDbName = GetFullDbName(keyColRow.table_name_child, keyColRow.schema_name_child);
                DbLinq.Schema.Dbml.Table parentTable = schema.Tables.FirstOrDefault(t => parentFullDbName == t.Name);
                if (parentTable == null)
                    Logger.Write(Level.Error, "ERROR L151: parent table not found: " + keyColRow.table_name_parent);
                else
                {
                    parentTable.Type.Associations.Add(assoc2);
                    assoc.Type = parentTable.Type.Name;
                }

            }

            return schema;
        }

        protected override System.Type MapDbType(IDataType dataType)
        {
            switch (dataType.Type.ToLower())
            {
            case "float":
                return typeof(Double);
            case "integer":
                switch (dataType.Length)
                {
                case 1:
                    return typeof(Byte);
                case 2:
                    return typeof(Int16);
                case 4:
                    return typeof(Int32);
                case 8:
                    return typeof(Int64);
                }
                return MapDbType(null);
            default:
                return base.MapDbType(dataType);
            }
        }
    }
}

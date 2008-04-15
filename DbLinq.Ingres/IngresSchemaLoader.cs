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
using DbLinq.Linq;
using DbLinq.Logging;
using DbLinq.Schema;
using DbLinq.Schema.Dbml;
using DbLinq.Util;
using DbLinq.Vendor;
using DbLinq.Vendor.Implementation;

namespace DbLinq.Ingres
{
    public partial class IngresSchemaLoader : SchemaLoader
    {
        private readonly Vendor.IVendor vendor = new IngresVendor();
        public override Vendor.IVendor Vendor { get { return vendor; } }

        public override System.Type DataContextType { get { return typeof(IngresDataContext); } }

        protected override Database Load(SchemaName schemaName, IDictionary<string, string> tableAliases, NameFormat nameFormat, bool loadStoredProcedures)
        {
            IDbConnection conn = Connection;

            var names = new Names();

            var schema = new Database();

            schema.Name = schemaName.DbName;
            schema.Class = schemaName.ClassName;

            LoadTables(schema, schemaName, conn, tableAliases, nameFormat, names);

            //##################################################################
            //step 2 - load columns
            ColumnSql csql = new ColumnSql();
            List<Schema.Column> columns = csql.getColumns(conn, schemaName.DbName);

            foreach (Schema.Column columnRow in columns)
            {
                var columnName = CreateColumnName(columnRow.ColumnName, nameFormat);
                names.AddColumn(columnRow.TableName, columnName);

                //find which table this column belongs to
                string fullColumnDbName = GetFullDbName(columnRow.TableName, columnRow.TableSchema);
                DbLinq.Schema.Dbml.Table tableSchema = schema.Tables.FirstOrDefault(tblSchema => fullColumnDbName == tblSchema.Name);
                if (tableSchema == null)
                {
                    Console.WriteLine("ERROR L46: Table '" + columnRow.TableName + "' not found for column " + columnRow.ColumnName);
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
                colSchema.CanBeNull = columnRow.Nullable;
                colSchema.Type = MapDbType(columnRow).ToString();

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

                if (keyColRow.constraint_type.Equals("R")) //'FOREIGN KEY'
                {
                    // This is very bad...
                    if (!names.ColumnsNames[keyColRow.table_name_child].ContainsKey(keyColRow.column_name_child))
                        continue;

                    var associationName = CreateAssociationName(
                        keyColRow.table_name_parent,
                        keyColRow.schema_name_parent,
                        keyColRow.table_name_child,
                        keyColRow.schema_name_child,
                        keyColRow.constraint_name,
                        nameFormat);

                    var foreignKey = names.ColumnsNames[keyColRow.table_name_parent][keyColRow.column_name_parent].PropertyName;
                    var reverseForeignKey = names.ColumnsNames[keyColRow.table_name_child][keyColRow.column_name_child].PropertyName;

                    //if not PRIMARY, it's a foreign key.
                    //both parent and child table get an [Association]
                    DbLinq.Schema.Dbml.Association assoc = new DbLinq.Schema.Dbml.Association();
                    assoc.IsForeignKey = true;
                    assoc.Name = keyColRow.constraint_name;
                    assoc.Type = null;
                    assoc.ThisKey = foreignKey;
                    assoc.OtherKey = reverseForeignKey;
                    assoc.Member = associationName.ManyToOneMemberName;
                    assoc.Storage = associationName.ForeignKeyStorageFieldName;
                    table.Type.Associations.Add(assoc);

                    //and insert the reverse association:
                    DbLinq.Schema.Dbml.Association assoc2 = new DbLinq.Schema.Dbml.Association();
                    assoc2.Name = keyColRow.constraint_name;
                    assoc2.Type = table.Type.Name;
                    assoc2.Member = associationName.OneToManyMemberName;
                    assoc2.ThisKey = reverseForeignKey;
                    assoc2.OtherKey = foreignKey;

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

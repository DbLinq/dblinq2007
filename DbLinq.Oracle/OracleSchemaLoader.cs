
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DbLinq.Linq;
using DbLinq.Logging;
using DbLinq.Oracle.Schema;
using DbLinq.Schema;
using DbLinq.Schema.Dbml;
using DbLinq.Util;
using DbLinq.Vendor;
using DbLinq.Vendor.Implementation;

namespace DbLinq.Oracle
{
    class OracleSchemaLoader : SchemaLoader
    {
        private readonly Vendor.IVendor vendor = new OracleVendor();
        public override IVendor Vendor { get { return vendor; } }

        public override System.Type DataContextType { get { return typeof(OracleDataContext); } }

        protected override Database Load(SchemaName schemaName, IDictionary<string, string> tableAliases, NameFormat nameFormat, bool loadStoredProcedures)
        {
            IDbConnection conn = Connection;

            var names = new Names();

            var schema = new Database();

            schema.Name = schemaName.DbName;
            schema.Class = schemaName.ClassName;


            //##################################################################
            //step 1 - load tables
            UserTablesSql utsql = new UserTablesSql();
            var tables = utsql.getTables(conn, schemaName.DbName);
            if (tables == null || tables.Count == 0)
            {
                Logger.Write(Level.Warning, "No tables found for schema " + schemaName.DbName + ", exiting");
                return null;
            }

            foreach (var tblRow in tables)
            {
                var tableName = CreateTableName(tblRow.Name, tblRow.Schema, tableAliases, nameFormat);
                names.TablesNames[tableName.DbName] = tableName;

                var tblSchema = new DbLinq.Schema.Dbml.Table();
                tblSchema.Name = tableName.DbName;
                tblSchema.Member = tableName.MemberName;
                tblSchema.Type.Name = tableName.ClassName;
                schema.Tables.Add(tblSchema);
            }

            //ensure all table schemas contain one type:
            //foreach(DbLinq.Schema.Dbml.Table tblSchema in schema0.Tables)
            //{
            //    tblSchema.Types.Add( new DbLinq.Schema.Dbml.Type());
            //}

            //##################################################################
            //step 2 - load columns
            User_Tab_Column_Sql csql = new User_Tab_Column_Sql();
            List<User_Tab_Column> columns = csql.getColumns(conn, schemaName.DbName);

            foreach (User_Tab_Column columnRow in columns)
            {
                var columnName = CreateColumnName(columnRow.column_name, nameFormat);
                names.AddColumn(columnRow.table_name, columnName);

                //find which table this column belongs to
                string columnFullDbName = GetFullDbName(columnRow.table_name, columnRow.table_schema);
                DbLinq.Schema.Dbml.Table tableSchema = schema.Tables.FirstOrDefault(tblSchema => columnFullDbName == tblSchema.Name);
                if (tableSchema == null)
                {
                    Logger.Write(Level.Error, "ERROR L46: Table '" + columnRow.table_name + "' not found for column " + columnRow.column_name);
                    continue;
                }
                DbLinq.Schema.Dbml.Column colSchema = new DbLinq.Schema.Dbml.Column();
                colSchema.Name = columnName.DbName;
                colSchema.Member = columnName.PropertyName;
                colSchema.Storage = columnName.StorageFieldName;

                colSchema.DbType = columnRow.Type; //.column_type ?
                //colSchema.IsPrimaryKey = false;
                //colSchema.IsDbGenerated = false;
                //colSchema.IsVersion = ???
                colSchema.CanBeNull = columnRow.isNullable;

                colSchema.Type = MapDbType(columnRow).ToString();

                //bool isPossibleBoolean = columnRow.Type == "NUMBER(1)"
                //    || columnRow.Type == "NUMBER";
                //if (isPossibleBoolean && columnRow.column_name == "DISCONTINUED")
                //{
                //    //hack to support Northwind boolean fields out of the box
                //    colSchema.Type = "bool";
                //}

                //tableSchema.Types[0].Columns.Add(colSchema);
                tableSchema.Type.Columns.Add(colSchema);
            }

            //##################################################################
            //step 3 - load foreign keys etc
            User_Constraints_Sql ksql = new User_Constraints_Sql();
            List<User_Constraints_Row> constraints = ksql.getConstraints1(conn, schemaName.DbName);


            foreach (User_Constraints_Row constraint in constraints)
            {
                //find my table:
                string constraintFullDbName = GetFullDbName(constraint.table_name, constraint.table_schema);
                DbLinq.Schema.Dbml.Table table = schema.Tables.FirstOrDefault(t => constraintFullDbName == t.Name);
                if (table == null)
                {
                    Logger.Write(Level.Error, "ERROR L100: Table '" + constraint.table_name + "' not found for column " + constraint.column_name);
                    continue;
                }

                //if (table.Name.StartsWith("E"))
                //    Logger.Write("---Dbg");

                if (constraint.constraint_type == "P")
                {
                    //A) add primary key
                    DbLinq.Schema.Dbml.Column pkColumn = table.Type.Columns.Where(c => c.Name == constraint.column_name).First();
                    pkColumn.IsPrimaryKey = true;
                }
                else
                {
                    //if not PRIMARY, it's a foreign key. (constraint_type=="R")
                    //both parent and child table get an [Association]
                    User_Constraints_Row referencedConstraint = constraints.FirstOrDefault(c => c.constraint_name == constraint.R_constraint_name);
                    if (constraint.R_constraint_name == null || referencedConstraint == null)
                    {
                        Logger.Write(Level.Error, "ERROR L127: given R_contraint_name='" + constraint.R_constraint_name + "', unable to find parent constraint");
                        continue;
                    }

                    var associationName = CreateAssociationName(constraint.table_name, constraint.table_schema,
                        referencedConstraint.table_name, referencedConstraint.table_schema, constraint.constraint_name,
                        nameFormat);

                    var foreignKey = names.ColumnsNames[constraint.table_name][constraint.column_name].PropertyName;
                    var reverseForeignKey = names.ColumnsNames[referencedConstraint.table_name][referencedConstraint.column_name].PropertyName;

                    //if not PRIMARY, it's a foreign key.
                    //both parent and child table get an [Association]
                    DbLinq.Schema.Dbml.Association assoc = new DbLinq.Schema.Dbml.Association();
                    assoc.IsForeignKey = true;
                    assoc.Name = constraint.constraint_name;
                    assoc.Type = null;
                    assoc.ThisKey = foreignKey;
                    assoc.OtherKey = reverseForeignKey;
                    assoc.Member = associationName.ManyToOneMemberName;
                    assoc.Storage = associationName.ForeignKeyStorageFieldName;
                    table.Type.Associations.Add(assoc);

                    //and insert the reverse association:
                    DbLinq.Schema.Dbml.Association assoc2 = new DbLinq.Schema.Dbml.Association();
                    assoc2.Name = constraint.constraint_name;
                    assoc2.Type = table.Type.Name;
                    assoc2.Member = associationName.OneToManyMemberName;
                    assoc2.ThisKey = reverseForeignKey;
                    assoc2.OtherKey = foreignKey;

                    string referencedFullDbName = GetFullDbName(referencedConstraint.table_name, referencedConstraint.table_schema);
                    DbLinq.Schema.Dbml.Table parentTable = schema.Tables.FirstOrDefault(t => referencedFullDbName == t.Name);
                    if (parentTable == null)
                    {
                        Logger.Write(Level.Error, "ERROR 148: parent table not found: " + referencedConstraint.table_name);
                    }
                    else
                    {
                        parentTable.Type.Associations.Add(assoc2);
                        assoc.Type = parentTable.Type.Name;
                    }

                }
            }

            GuessSequencePopulatedFields(schema);

            return schema;
        }

        /// <summary>
        /// guess which fields are populated by sequences.
        /// Mark them with [AutoGenId].
        /// </summary>
        public static void GuessSequencePopulatedFields(DbLinq.Schema.Dbml.Database schema)
        {
            if (schema == null)
                return;
            foreach (DbLinq.Schema.Dbml.Table tbl in schema.Tables)
            {
                var q = from col in tbl.Type.Columns
                        where col.IsPrimaryKey
                        select col;
                List<DbLinq.Schema.Dbml.Column> cols = q.ToList();
                bool canBeFromSequence = cols.Count == 1
                    && (!cols[0].CanBeNull)
                    && (cols[0].DbType == "NUMBER" || cols[0].DbType == "INTEGER");
                if (canBeFromSequence)
                {
                    //TODO: query sequences, store sequence name.
                    //in the meantime, assume naming convention similar to 'Products_seq'
                    cols[0].IsDbGenerated = true;
                }
            }
        }
    }
}
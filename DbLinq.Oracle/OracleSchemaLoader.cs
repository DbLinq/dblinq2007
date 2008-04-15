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
    partial class OracleSchemaLoader : SchemaLoader
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


            LoadTables(schema, schemaName, conn, tableAliases, nameFormat, names);

            LoadColumns(schema, schemaName, conn, nameFormat, names);

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
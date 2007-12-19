using System;
using System.Collections.Generic;
using System.Text;
using System.Data.OracleClient;
using System.Linq;
using SqlMetal.util;

namespace SqlMetal.schema.mysql { } //namespace only used from other csproj
namespace SqlMetal.schema.pgsql { } //namespace only used from other csproj
namespace SqlMetal.schema.mssql { } //namespace only used from other csproj

namespace SqlMetal.schema.oracle
{
    class Vendor : IDBVendor
    {
        public string VendorName() { return "Oracle"; }

        public DlinqSchema.Database LoadSchema()
        {
            //string connStr = string.Format("server={0};user id={1}; password={2}; database={3}; pooling=false"
            string connStr = string.Format("server={0};user id={1}; password={2}"
                , mmConfig.server, mmConfig.user, mmConfig.password, mmConfig.database);

            OracleConnection conn = new OracleConnection(connStr);
            conn.Open();

            if (mmConfig.database == null)
                throw new ArgumentException("Missing database name - used for schema.Name");

            DlinqSchema.Database schema = new DlinqSchema.Database();
            schema.Name = mmConfig.database;

            //schema.Class = FormatTableName(schema.Name);
            schema.Class = Util.TableNameSingular(schema.Name);


            //##################################################################
            //step 1 - load tables
            UserTablesSql utsql = new UserTablesSql();
            List<UserTablesRow> tables = utsql.getTables(conn, mmConfig.database);
            if (tables == null || tables.Count == 0)
            {
                Console.WriteLine("No tables found for schema " + mmConfig.database + ", exiting");
                return null;
            }

            foreach (UserTablesRow tblRow in tables)
            {
                DlinqSchema.Table tblSchema = new DlinqSchema.Table();
                tblSchema.Name = tblRow.table_name;
                tblSchema.Member = FormatTableName(tblRow.table_name).Pluralize();
                tblSchema.Type.Name = FormatTableName(tblRow.table_name);
                schema.Tables.Add(tblSchema);
            }

            //ensure all table schemas contain one type:
            //foreach(DlinqSchema.Table tblSchema in schema0.Tables)
            //{
            //    tblSchema.Types.Add( new DlinqSchema.Type());
            //}

            //##################################################################
            //step 2 - load columns
            User_Tab_Column_Sql csql = new User_Tab_Column_Sql();
            List<User_Tab_Column> columns = csql.getColumns(conn, mmConfig.database);

            foreach (User_Tab_Column columnRow in columns)
            {
                //find which table this column belongs to
                DlinqSchema.Table tableSchema = schema.Tables.FirstOrDefault(tblSchema => columnRow.table_name == tblSchema.Name);
                if (tableSchema == null)
                {
                    Console.WriteLine("ERROR L46: Table '" + columnRow.table_name + "' not found for column " + columnRow.column_name);
                    continue;
                }
                DlinqSchema.Column colSchema = new DlinqSchema.Column();
                colSchema.Name = columnRow.column_name;
                colSchema.DbType = columnRow.data_type; //.column_type ?
                colSchema.IsPrimaryKey = false; //columnRow.column_key=="PRI";
                colSchema.IsDbGenerated = false; //columnRow.extra=="auto_increment";
                //colSchema.IsVersion = ???
                colSchema.CanBeNull = columnRow.isNullable;
                colSchema.Type = "qqq"; //Mappings.mapSqlTypeToCsType(columnRow.data_type, columnRow.column_type);

                //this will be the c# field name
                colSchema.Member = Util.Rename(columnRow.column_name);

                colSchema.Type = OraTypeMap.mapSqlTypeToCsType(columnRow.data_type, columnRow.data_precision);
                if (CSharp.IsValueType(colSchema.Type) && columnRow.isNullable)
                    colSchema.Type += "?";

                //tableSchema.Types[0].Columns.Add(colSchema);
                tableSchema.Type.Columns.Add(colSchema);
            }

            //##################################################################
            //step 3 - load foreign keys etc
            User_Constraints_Sql ksql = new User_Constraints_Sql();
            List<User_Constraints_Row> constraints = ksql.getConstraints1(conn, mmConfig.database);


            foreach (User_Constraints_Row constraint in constraints)
            {
                //find my table:
                DlinqSchema.Table table = schema.Tables.FirstOrDefault(t => constraint.table_name == t.Name);
                if (table == null)
                {
                    Console.WriteLine("ERROR L100: Table '" + constraint.table_name + "' not found for column " + constraint.column_name);
                    continue;
                }

                if (constraint.constraint_type == "P")
                {
                    //A) add primary key
                    DlinqSchema.Column pkColumn = table.Type.Columns.Where(c => c.Name == constraint.column_name).First();
                    pkColumn.IsPrimaryKey = true;
                }
                else
                {
                    //if not PRIMARY, it's a foreign key. (constraint_type=="R")
                    //both parent and child table get an [Association]
                    User_Constraints_Row referencedConstraint = constraints.FirstOrDefault(c => c.constraint_name == constraint.R_constraint_name);
                    if (constraint.R_constraint_name == null || referencedConstraint == null)
                    {
                        Console.WriteLine("ERROR L127: given R_contraint_name='" + constraint.R_constraint_name + "', unable to find parent constraint");
                        continue;
                    }

                    //if not PRIMARY, it's a foreign key.
                    //both parent and child table get an [Association]
                    DlinqSchema.Association assoc = new DlinqSchema.Association();
                    assoc.IsForeignKey = true;
                    assoc.Name = constraint.constraint_name;
                    //assoc.Type = keyColRow.referenced_table_name; //see below instead
                    assoc.ThisKey = constraint.column_name;
                    assoc.Member = FormatTableName(referencedConstraint.table_name);
                    table.Type.Associations.Add(assoc);

                    //and insert the reverse association:
                    DlinqSchema.Association assoc2 = new DlinqSchema.Association();
                    assoc2.Name = constraint.constraint_name;
                    assoc2.Type = table.Type.Name; //keyColRow.table_name;
                    assoc2.Member = FormatTableName(constraint.table_name).Pluralize();
                    assoc2.OtherKey = referencedConstraint.column_name; // referenced_column_name;

                    //assoc2.Columns.Add(new DlinqSchema.ColumnName(constraint.column_name));

                    DlinqSchema.Table parentTable = schema.Tables.FirstOrDefault(t => referencedConstraint.table_name == t.Name);
                    if (parentTable == null)
                    {
                        Console.WriteLine("ERROR 148: parent table not found: " + referencedConstraint.table_name);
                    }
                    else
                    {
                        parentTable.Type.Associations.Add(assoc2);
                        assoc.Type = parentTable.Type.Name;
                    }

                }

            }


            return schema;
        }

        public static string FormatTableName(string table_name)
        {
            //TODO: allow custom renames via config file - 
            //- this could solve keyword conflict etc
            string name1 = table_name;
            string name2 = mmConfig.forceUcaseTableName
                ? name1.Capitalize() //Char.ToUpper(name1[0])+name1.Substring(1)
                : name1;

            //heuristic to convert 'Products' table to class 'Product'
            //TODO: allow customized tableName-className mappings from an XML file
            name2 = name2.Singularize();

            return name2;
        }
    }
}

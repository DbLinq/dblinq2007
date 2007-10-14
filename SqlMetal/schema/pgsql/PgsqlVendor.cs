////////////////////////////////////////////////////////////////////
//Initial author: Jiri George Moudry, 2006.
//License: LGPL. (Visit http://www.gnu.org)
////////////////////////////////////////////////////////////////////

//Sources: Extracting META Information from PostgreSQL - Lorenzo Alberton
//http://www.alberton.info/postgresql_meta_info.html

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Npgsql;
using SqlMetal.util;

namespace SqlMetal.schema.mysql { } //namespace only used from other csproj
namespace SqlMetal.schema.mssql { } //namespace only used from other csproj
namespace SqlMetal.schema.oracle { } //namespace only used from other csproj

namespace SqlMetal.schema.pgsql
{
    /// <summary>
    /// this class contains MySql-specific way of retrieving DB schema.
    /// </summary>
    class Vendor : IDBVendor
    {
        public string VendorName(){ return "PostgreSQL"; }

        /// <summary>
        /// main entry point to load schema
        /// </summary>
        public DlinqSchema.Database LoadSchema()
        {
            string connStr = string.Format("server={0};user id={1}; password={2}; database={3}; pooling=false"
                , mmConfig.server, mmConfig.user, mmConfig.password, mmConfig.database);

            NpgsqlConnection conn = new NpgsqlConnection(connStr);
            conn.Open();

            DlinqSchema.Database schema = new DlinqSchema.Database();
            schema.Name = mmConfig.database;
            schema.Class = schema.Name;

            //##################################################################
            //step 1 - load tables
            TableSql tsql = new TableSql();
            List<TableRow> tables = tsql.getTables(conn,mmConfig.database);
            if(tables==null || tables.Count==0){
                Console.WriteLine("No tables found for schema "+mmConfig.database+", exiting");
                return null;
            }

            foreach(TableRow tblRow in tables)
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
            ColumnSql csql = new ColumnSql();
            List<Column> columns = csql.getColumns(conn,mmConfig.database);

            KeyColumnUsageSql ksql = new KeyColumnUsageSql();
            List<KeyColumnUsage> constraints = ksql.getConstraints(conn,mmConfig.database);
            ForeignKeySql fsql = new ForeignKeySql();

            List<ForeignKeyCrossRef> allKeys2 = fsql.getConstraints(conn,mmConfig.database);
            List<ForeignKeyCrossRef> foreignKeys = allKeys2.Where( k => k.constraint_type == "FOREIGN KEY" ).ToList();
            List<ForeignKeyCrossRef> primaryKeys = allKeys2.Where( k => k.constraint_type == "PRIMARY KEY" ).ToList();
            

            foreach(Column columnRow in columns)
            {
                //find which table this column belongs to
                DlinqSchema.Table tableSchema = schema.Tables.FirstOrDefault(tblSchema => columnRow.table_name==tblSchema.Name);
                if(tableSchema==null)
                {
                    Console.WriteLine("ERROR L46: Table '"+columnRow.table_name+"' not found for column "+columnRow.column_name);
                    continue;
                }
                DlinqSchema.Column colSchema = new DlinqSchema.Column();
                colSchema.Name = columnRow.column_name;
                colSchema.DbType = columnRow.datatype; //.column_type ?
                KeyColumnUsage primaryKCU = constraints.FirstOrDefault(c => c.column_name==columnRow.column_name 
                    && c.table_name==columnRow.table_name && c.constraint_name.EndsWith("_pkey"));

                colSchema.IsPrimaryKey = primaryKCU!=null; //columnRow.column_key=="PRI";
                colSchema.IsDbGenerated = columnRow.column_default!=null && columnRow.column_default.StartsWith("nextval(");
                //colSchema.IsVersion = ???
                colSchema.CanBeNull = columnRow.isNullable;
                colSchema.Type = Mappings.mapSqlTypeToCsType(columnRow.datatype, columnRow.column_type);
                
                //this will be the c# field name
                colSchema.Member = CSharp.IsCsharpKeyword(columnRow.column_name) 
                    ? columnRow.column_name+"_" //avoid keyword conflict - append underscore
                    : columnRow.column_name;

                colSchema.Type = Mappings.mapSqlTypeToCsType(columnRow.datatype, columnRow.column_type);
                if(CSharp.IsValueType(colSchema.Type) && columnRow.isNullable)
                colSchema.Type += "?";

                //tableSchema.Types[0].Columns.Add(colSchema);
                tableSchema.Type.Columns.Add(colSchema);
            }

            //##################################################################
            //step 3 - analyse foreign keys etc

            //TableSorter.Sort(tables, constraints); //sort tables - parents first

            foreach(KeyColumnUsage keyColRow in constraints)
            {
                //find my table:
                DlinqSchema.Table table = schema.Tables.FirstOrDefault(t => keyColRow.table_name==t.Name);
                if(table==null)
                {
                    Console.WriteLine("ERROR L46: Table '"+keyColRow.table_name+"' not found for column "+keyColRow.column_name);
                    continue;
                }

                if(keyColRow.constraint_name.EndsWith("_pkey")) //MYSQL reads 'PRIMARY'
                {
                    //A) add primary key
                    DlinqSchema.Column primaryKeyCol = table.Type.Columns.First(c => c.Name == keyColRow.column_name);
                    primaryKeyCol.IsPrimaryKey = true;
                } 
                else 
                {
                    ForeignKeyCrossRef foreignKey = foreignKeys.FirstOrDefault(f=>f.constraint_name==keyColRow.constraint_name);
                    if(foreignKey==null)
                    {
                        string msg = "Missing data from 'constraint_column_usage' for foreign key "+keyColRow.constraint_name;
                        Console.WriteLine(msg);
                        throw new ApplicationException(msg);
                    }

                    //if not PRIMARY, it's a foreign key.
                    //both parent and child table get an [Association]
                    DlinqSchema.Association assoc = new DlinqSchema.Association();
                    assoc.IsForeignKey = true;
                    assoc.Name = keyColRow.constraint_name;
                    //assoc.Type = keyColRow.referenced_table_name; //see below instead
                    assoc.ThisKey = keyColRow.column_name;
                    assoc.Member = FormatTableName(foreignKey.table_name_Parent);
                    table.Type.Associations.Add(assoc);

                    //and insert the reverse association:
                    DlinqSchema.Association assoc2 = new DlinqSchema.Association();
                    assoc2.Name = keyColRow.constraint_name;
                    assoc2.Type = table.Type.Name; //keyColRow.table_name;
                    assoc2.Member = FormatTableName(keyColRow.table_name).Pluralize();
                    assoc2.OtherKey = keyColRow.column_name; //.referenced_column_name;

                    //DlinqSchema.Table parentTable = schema0.Tables.FirstOrDefault(t => keyColRow.referenced_table_name==t.Name);
                    DlinqSchema.Table parentTable = schema.Tables.FirstOrDefault(t => foreignKey.table_name_Parent==t.Name);
                    if (parentTable == null)
                        Console.WriteLine("ERROR L151: parent table not found: " + foreignKey.table_name_Parent);
                    else
                    {
                        parentTable.Type.Associations.Add(assoc2);
                        assoc.Type = parentTable.Type.Name;
                    }

                }

            }

            //##################################################################
            //step 4 - analyse stored procs
            if (mmConfig.sprocs)
            {
                Pg_Proc_Sql procSql = new Pg_Proc_Sql();
                List<Pg_Proc> procs = procSql.getProcs(conn, mmConfig.database);

                //4a. determine unknown types
                Dictionary<long, string> typeOidToName = new Dictionary<long, string>();

                foreach (Pg_Proc proc in procs)
                {
                    typeOidToName[proc.prorettype] = proc.formatted_prorettype;

                    foreach (long argType in proc.progargtypes2)
                    {
                        if (!typeOidToName.ContainsKey(argType))
                            typeOidToName[argType] = null;
                    }

                    //List<long> unknownTypeOids = typeOidToName.Where(kv => kv.Value == null)
                    //    .Select(kv => kv.Key).ToList();
                }

                //4b. get names for unknown types
                procSql.getTypeNames(conn, mmConfig.database, typeOidToName);

                //4c. generate dbml objects
                foreach (Pg_Proc proc in procs)
                {
                    DlinqSchema.Function dbml_fct = ParseFunction(proc, typeOidToName);
                    schema.Functions.Add(dbml_fct);
                }

            }
            return schema;
        }

        static DlinqSchema.Function ParseFunction(Pg_Proc pg_proc, Dictionary<long, string> typeOidToName)
        {
            DlinqSchema.Function dbml_func = new DlinqSchema.Function();
            dbml_func.Name = pg_proc.proname;

            if (pg_proc.formatted_prorettype != null)
            {
                DlinqSchema.Parameter dbml_param = new DlinqSchema.Parameter();
                dbml_param.DbType = pg_proc.formatted_prorettype;
                dbml_param.Type = Mappings.mapSqlTypeToCsType( pg_proc.formatted_prorettype,"");
                dbml_func.Return.Add(dbml_param);
            }

            string paramNames1 = pg_proc.proargnames;
            if (paramNames1 == null || paramNames1.Length < 2)
            {
                //eg. 'hello0' has no arguments
            }
            else
            {
                string[] paramNames = paramNames1.Split(',');
                int len = pg_proc.progargtypes2.Count;
                if (paramNames.Length != len)
                {
                    Console.WriteLine("L238 Mistmatch between typeArr and nameArr for func " + pg_proc.proname);
                    return null;
                }

                List<DlinqSchema.Parameter> paramList = new List<DlinqSchema.Parameter>();
                for (int i = 0; i < len; i++)
                {
                    DlinqSchema.Parameter dbml_param = new DlinqSchema.Parameter();
                    long argTypeOid = pg_proc.progargtypes2[i];
                    dbml_param.DbType = typeOidToName[argTypeOid];
                    dbml_param.Name = paramNames[i];
                    dbml_param.Type = Mappings.mapSqlTypeToCsType(dbml_param.DbType, "");

                    dbml_func.Parameters.Add(dbml_param);
                }
            }

            return dbml_func;
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

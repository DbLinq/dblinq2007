using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using MySql.Data.MySqlClient;
using SqlMetal.util;

namespace SqlMetal.schema.pgsql { } //this namespace is used from other csproj
namespace SqlMetal.schema.mssql { } //this namespace is used from other csproj
namespace SqlMetal.schema.oracle { } //this namespace is used from other csproj

namespace SqlMetal.schema.mysql
{
    /// <summary>
    /// this class contains MySql-specific way of retrieving DB schema.
    /// </summary>
    class Vendor : IDBVendor
    {
        public string VendorName(){ return "MySql"; }

        /// <summary>
        /// main entry point to load schema
        /// </summary>
        public DlinqSchema.Database LoadSchema()
        {
            string connStr = string.Format("server={0};user id={1}; password={2}; database={3}; pooling=false"
                , mmConfig.server, mmConfig.user, mmConfig.password, mmConfig.database);

            MySqlConnection conn = new MySqlConnection(connStr);
            conn.Open();

            DlinqSchema.Database schema = new DlinqSchema.Database();
            schema.Name = mmConfig.database;
            schema.Class = mmConfig.database; // FormatTableName(schema.Name);

            //##################################################################
            //step 1 - load tables
            TableSql tsql = new TableSql();
            List<TableRow> tables = tsql.getTables(conn,mmConfig.database);
            if (tables == null || tables.Count == 0)
            {
                Console.WriteLine("No tables found for schema " + mmConfig.database + ", exiting");
                return null;
            }

            foreach(TableRow tblRow in tables)
            {
                DlinqSchema.Table tblSchema = new DlinqSchema.Table();
                tblSchema.Name = tblRow.table_name;
                tblSchema.Member = FormatTableName(tblRow.table_name).Pluralize();
                tblSchema.Type.Name = FormatTableName(tblRow.table_name);
                schema.Tables.Add( tblSchema );
            }

            //##################################################################
            //step 2 - load columns
            ColumnSql csql = new ColumnSql();
            List<Column> columns = csql.getColumns(conn,mmConfig.database);
            
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
                colSchema.IsPrimaryKey = columnRow.column_key=="PRI";
                colSchema.IsDbGenerated = columnRow.extra=="auto_increment";
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
            //step 3 - load foreign keys etc
            KeyColumnUsageSql ksql = new KeyColumnUsageSql();
            List<KeyColumnUsage> constraints = ksql.getConstraints(conn,mmConfig.database);

            TableSorter.Sort(tables, constraints); //sort tables - parents first

            foreach(KeyColumnUsage keyColRow in constraints)
            {
                //find my table:
                DlinqSchema.Table table = schema.Tables.FirstOrDefault(t => keyColRow.table_name==t.Name);
                if(table==null)
                {
                    Console.WriteLine("ERROR L46: Table '"+keyColRow.table_name+"' not found for column "+keyColRow.column_name);
                    continue;
                }

                if (keyColRow.constraint_name == "PRIMARY")
                {
                }
                else
                {
                    //if not PRIMARY, it's a foreign key.
                    //both parent and child table get an [Association]
                    DlinqSchema.Association assoc = new DlinqSchema.Association();
                    assoc.IsForeignKey = true;
                    assoc.Name = keyColRow.constraint_name;
                    //assoc.Type = keyColRow.referenced_table_name; //see below instead
                    assoc.ThisKey = keyColRow.column_name;
                    assoc.Member = FormatTableName(keyColRow.referenced_table_name);
                    table.Type.Associations.Add(assoc);

                    //and insert the reverse association:
                    DlinqSchema.Association assoc2 = new DlinqSchema.Association();
                    assoc2.Name = keyColRow.constraint_name;
                    assoc2.Type = table.Type.Name; //keyColRow.table_name;
                    assoc2.Member = FormatTableName(keyColRow.table_name).Pluralize();
                    assoc2.OtherKey = keyColRow.referenced_column_name;

                    DlinqSchema.Table parentTable = schema.Tables.FirstOrDefault(t => keyColRow.referenced_table_name == t.Name);
                    if (parentTable == null)
                    {
                        Console.WriteLine("ERROR 148: parent table not found: " + keyColRow.referenced_table_name);
                    }
                    else
                    {
                        parentTable.Type.Associations.Add(assoc2);
                        assoc.Type = parentTable.Type.Name;
                    }

                }

            }


            //##################################################################
            //step 4 - load stored procs
            if (mmConfig.sprocs)
            {
                ProcSql procsql = new ProcSql();
                List<ProcRow> procs = procsql.getProcs(conn, mmConfig.database);

                foreach (ProcRow proc in procs)
                {
                    DlinqSchema.Function func = new DlinqSchema.Function();
                    func.Name = proc.specific_name;
                    ParseProcParams(proc, func);
                    
                    schema.Functions.Add(func);
                }
            }

            return schema;
        }

        /// <summary>
        /// parse bytes 'OUT param1 int, param2 int'.
        /// The newly created DlinqSchema.Parameter objects will be appended to 'outputFunc'.
        /// </summary>
        static void ParseProcParams(ProcRow inputProc, DlinqSchema.Function outputFunc)
        {
            string paramString = inputProc.param_list;
            if(paramString==null || paramString=="")
                return;
            string[] parts = paramString.Split(',');

            char[] SPACES = new char[]{' ','\t','\n'}; //word separators

            foreach (string part in parts) //part='OUT param1 int'
            {
                DlinqSchema.Parameter paramObj = ParseParameterString(part);
                if(paramObj!=null)
                    outputFunc.Parameters.Add(paramObj);

                if (inputProc.returns != null)
                {
                    DlinqSchema.Parameter paramRet = new DlinqSchema.Parameter();
                    paramRet.DbType = inputProc.returns;
                    paramRet.Type = ParseDbType(inputProc.returns);
                    outputFunc.Return.Add(paramRet);
                }
            }
        }

        /// <summary>
        /// parse strings such as 'INOUT param2 INT' or 'param4 varchar ( 32 )'
        /// </summary>
        /// <param name="paramStr"></param>
        /// <returns></returns>
        static DlinqSchema.Parameter ParseParameterString(string param)
        {
            param = param.Trim();
            DlinqSchema.InOutEnum inOut = DlinqSchema.InOutEnum.In;
            
            if (param.StartsWith("IN", StringComparison.CurrentCultureIgnoreCase))
            {
                inOut = DlinqSchema.InOutEnum.In;
                param = param.Substring(2).Trim();
            }
            if (param.StartsWith("INOUT", StringComparison.CurrentCultureIgnoreCase))
            {
                inOut = DlinqSchema.InOutEnum.InOut;
                param = param.Substring(5).Trim();
            }
            if (param.StartsWith("OUT", StringComparison.CurrentCultureIgnoreCase))
            {
                inOut = DlinqSchema.InOutEnum.Out;
                param = param.Substring(3).Trim();
            }

            int indxSpace = param.IndexOfAny(new char[] { ' ', '\t' });
            if (indxSpace == -1)
                return null; //cannot find space between varName and varType

            string varName = param.Substring(0,indxSpace);
            string varType = param.Substring(indxSpace+1);

            DlinqSchema.Parameter paramObj = new DlinqSchema.Parameter();
            paramObj.InOut = inOut;
            paramObj.Name = varName;
            paramObj.DbType = varType;
            paramObj.Type = ParseDbType(varType);

            return paramObj;
        }

        /// <summary>
        /// given 'CHAR(30)', return 'string'
        /// </summary>
        static string ParseDbType(string dbType)
        {
            string varType = dbType.Trim().ToLower();
            string varTypeQualifier = "";
            int indxQuote = varType.IndexOf('(');
            if (indxQuote > -1)
            {
                //split 'CHAR(30)' into 'char' and '(30)'
                varTypeQualifier = varType.Substring(indxQuote);
                varType = varType.Substring(0, indxQuote);
            }
            string dbTypeStr = Mappings.mapSqlTypeToCsType(varType, varTypeQualifier);
            return dbTypeStr;
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

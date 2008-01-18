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
//        Andrey Shchekin
////////////////////////////////////////////////////////////////////
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Data.SQLite;
using SqlMetal.util;

namespace SqlMetal.schema.pgsql { } //this namespace is used from other csproj
namespace SqlMetal.schema.mssql { } //this namespace is used from other csproj
namespace SqlMetal.schema.oracle { } //this namespace is used from other csproj
namespace SqlMetal.schema.mysql { } //this namespace is used from other csproj

namespace SqlMetal.schema.sqlite
{
    /// <summary>
    /// this class contains SQLite-specific way of retrieving DB schema.
    /// </summary>
    class Vendor : IDBVendor
    {
        public Vendor()
        {
            mmConfig.forceUcaseFieldName = false;
        }

        public string VendorName() { return "SQLite"; }

        /// <summary>
        /// main entry point to load schema
        /// </summary>
        public DlinqSchema.Database LoadSchema()
        {
            ///todo: Faire la traduction pour SQLite
            string connStr = string.Format("Data Source={0}", mmConfig.database);

            SQLiteConnection conn = new SQLiteConnection(connStr);
            conn.Open();

            DlinqSchema.Database schema = new DlinqSchema.Database();
            string database = mmConfig.database.Replace("\"", "");
            if (database.Contains('.'))
                database = database.Split('.')[0];

            schema.Name = database;
            schema.Class = database; // FormatTableName(schema.Name);

            //##################################################################
            //step 1 - load tables
            TableSql tsql = new TableSql();
            List<TableRow> tables = tsql.getTables(conn,mmConfig.database);
            if (tables == null || tables.Count == 0)
            {
                Console.WriteLine("No tables found for schema " + mmConfig.database + ", exiting");
                return null;
            }

            foreach (TableRow tblRow in tables)
            {
                DlinqSchema.Table tblSchema = new DlinqSchema.Table();
                tblSchema.Name = tblRow.table_name;

                //defer Pluralize() until SchemaPostprocess
                tblSchema.Member = tblRow.table_name;
                tblSchema.Type.Name = tblRow.table_name;

                schema.Tables.Add(tblSchema);
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

                //sample input: columnRow.column_type="varchar(15)", coloumRow.datatype="varchar"
                //colSchema.DbType = columnRow.datatype;
                string dbType = columnRow.column_type;

                dbType = dbType.Replace("int(11)", "int") //remove some default sizes
                    .Replace("int(10) unsigned", "int unsigned")
                    .Replace("mediumint(8) unsigned", "mediumint unsigned")
                    .Replace("decimal(10,0)", "decimal")
                    ;
                colSchema.DbType = dbType;

                colSchema.IsPrimaryKey = columnRow.column_key=="PRI";
                colSchema.IsDbGenerated = columnRow.extra=="auto_increment";
                
                colSchema.CanBeNull = columnRow.isNullable;

                //determine the C# type
                colSchema.Type = Mappings.mapSqlTypeToCsType(columnRow.datatype, columnRow.column_type);
                if (columnRow.column_name == "DbLinq_EnumTest")
                    colSchema.Type = "DbLinq_EnumTest"; //hadcoded value - used during enum testing
                if (CSharp.IsValueType(colSchema.Type) && columnRow.isNullable)
                    colSchema.Type += "?";
                
                //SQLite always autoincrement PRimary Key integers
                if (!colSchema.IsDbGenerated && colSchema.IsPrimaryKey && (colSchema.Type == "int" || colSchema.Type == "int?"))
                    colSchema.IsDbGenerated = true;


                //determine the c# field name - this may be changed in SchemaPostprocess
                colSchema.Member = mmConfig.forceUcaseFieldName
                    ? Util.Capitalize(columnRow.column_name)
                    : columnRow.column_name;

                //tableSchema.Types[0].Columns.Add(colSchema);
                tableSchema.Type.Columns.Add(colSchema);
            }

            //##################################################################
            //step 3 - load foreign keys etc
            KeyColumnUsageSql ksql = new KeyColumnUsageSql();
            List<KeyColumnUsage> constraints = ksql.getConstraints(conn,mmConfig.database);

            //sort tables - parents first (this is moving to SchemaPostprocess)
            //TableSorter.Sort(tables, constraints); 

			// Deal with non existing foreign key database
            if (constraints != null)
            {
                foreach (KeyColumnUsage keyColRow in constraints)
                {
                    //find my table:
                    DlinqSchema.Table table = schema.Tables.FirstOrDefault(t => keyColRow.table_name == t.Name);
                    if (table == null)
                    {
                        Console.WriteLine("ERROR L46: Table '" + keyColRow.table_name + "' not found for column " + keyColRow.column_name);
                        continue;
                    }

                    bool isForeignKey = keyColRow.constraint_name != "PRIMARY"
                                     && keyColRow.referenced_table_name != null;

                    if (isForeignKey)
                    {
                        //both parent and child table get an [Association]
                        DlinqSchema.Association assoc = new DlinqSchema.Association();
                        assoc.IsForeignKey = true;
                        assoc.Name = keyColRow.constraint_name;
                        assoc.Type = null;
                        assoc.ThisKey = keyColRow.column_name;
                        assoc.Member = keyColRow.referenced_table_name;
                        table.Type.Associations.Add(assoc);

                        //and insert the reverse association:
                        DlinqSchema.Association assoc2 = new DlinqSchema.Association();
                        assoc2.Name = keyColRow.constraint_name;
                        assoc2.Type = table.Type.Name; //keyColRow.table_name;
                        assoc2.Member = keyColRow.table_name;

                        //bool isSelfJoin = keyColRow.table_name == keyColRow.referenced_table_name;
                        //assoc2.OtherKey = isSelfJoin
                        //    ? keyColRow.column_name //in Employees table - "ReportsTo" appears in both [Association]
                        //    : keyColRow.referenced_column_name;
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
                    func.ProcedureOrFunction = proc.type;
                    func.BodyContainsSelectStatement = proc.body != null
                        && proc.body.IndexOf("select", StringComparison.OrdinalIgnoreCase) > -1;
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
            if (paramString == null || paramString == "")
            {
                //nothing to parse
            }
            else
            {
                string[] parts = paramString.Split(',');

                char[] SPACES = new char[] { ' ', '\t', '\n' }; //word separators

                foreach (string part in parts) //part='OUT param1 int'
                {
                    DlinqSchema.Parameter paramObj = ParseParameterString(part);
                    if (paramObj != null)
                        outputFunc.Parameters.Add(paramObj);
                }
            }

            if (inputProc.returns != null && inputProc.returns!="")
            {
                DlinqSchema.Parameter paramRet = new DlinqSchema.Parameter();
                paramRet.DbType = inputProc.returns;
                paramRet.Type = ParseDbType(inputProc.returns);
                paramRet.InOut = System.Data.ParameterDirection.ReturnValue;
                outputFunc.Return.Add(paramRet);
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
            System.Data.ParameterDirection inOut = System.Data.ParameterDirection.Input;
            
            if (param.StartsWith("IN", StringComparison.CurrentCultureIgnoreCase))
            {
                inOut = System.Data.ParameterDirection.Input;
                param = param.Substring(2).Trim();
            }
            if (param.StartsWith("INOUT", StringComparison.CurrentCultureIgnoreCase))
            {
                inOut = System.Data.ParameterDirection.InputOutput;
                param = param.Substring(5).Trim();
            }
            if (param.StartsWith("OUT", StringComparison.CurrentCultureIgnoreCase))
            {
                inOut = System.Data.ParameterDirection.Output;
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

        static System.Text.RegularExpressions.Regex re_CHARSET = new System.Text.RegularExpressions.Regex(@" CHARSET \w+$");
        /// <summary>
        /// given 'CHAR(30)', return 'string'
        /// </summary>
        static string ParseDbType(string dbType1)
        {
            //strip 'CHARSET latin1' from the end
            string dbType2 = re_CHARSET.Replace(dbType1, "");

            string varType = dbType2.Trim().ToLower();
            string varTypeQualifier = "";
            int indxQuote = varType.IndexOf('(');
            if (indxQuote > -1)
            {
                //split 'CHAR(30)' into 'char' and '(30)'
                varTypeQualifier = varType.Substring(indxQuote);
                varType = varType.Substring(0, indxQuote);
            }
            else if (varType.IndexOf("unsigned", StringComparison.OrdinalIgnoreCase) > -1)
            {
                varTypeQualifier = "unsigned";
                varType = varType.Replace("unsigned", "").Trim();
            }
            string dbTypeStr = Mappings.mapSqlTypeToCsType(varType, varTypeQualifier);
            return dbTypeStr;
        }


    }
}

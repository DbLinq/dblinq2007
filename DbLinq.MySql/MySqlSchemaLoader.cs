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
using DbLinq.MySql.Schema;
using DbLinq.Schema;
using DbLinq.Schema.Dbml;
using DbLinq.Util;
using DbLinq.Vendor;
using DbLinq.Vendor.Implementation;
using DbLinq.Logging;

namespace DbLinq.MySql
{
    class MySqlSchemaLoader : SchemaLoader
    {
        private readonly IVendor vendor = new MySqlVendor();
        public override IVendor Vendor { get { return vendor; } }

        public override System.Type DataContextType { get { return typeof(MySqlDataContext); } }

        protected override TableName CreateTableName(string dbTableName, string dbSchema, IDictionary<string, string> tableAliases)
        {
            WordsExtraction extraction = WordsExtraction.FromDictionary;
            if (tableAliases != null && tableAliases.ContainsKey(dbTableName))
            {
                extraction = WordsExtraction.FromCase;
                dbTableName = tableAliases[dbTableName];
            }
            var tableName = NameFormatter.GetTableName(dbTableName, extraction);
            tableName.DbName = GetFullDbName(dbTableName, dbSchema);
            return tableName;
        }

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
                Logger.Write(Level.Warning, "No tables found for schema " + schemaName.DbName + ", exiting");
                return null;
            }

            foreach (TableRow tblRow in tables)
            {
                var tableName = CreateTableName(tblRow.table_name, tblRow.table_schema, tableAliases);
                names.TablesNames[tableName.DbName] = tableName;

                var tblSchema = new Table();
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
                string fullColumnDbName = GetFullDbName(columnRow.table_name, columnRow.table_schema);
                DbLinq.Schema.Dbml.Table tableSchema = schema.Tables.FirstOrDefault(tblSchema => fullColumnDbName == tblSchema.Name);
                if (tableSchema == null)
                {
                    Logger.Write(Level.Error, "ERROR L46: Table '" + columnRow.table_name + "' not found for column " + columnRow.column_name);
                    continue;
                }
                DbLinq.Schema.Dbml.Column colSchema = new DbLinq.Schema.Dbml.Column();
                colSchema.Name = columnName.DbName;
                colSchema.Member = columnName.PropertyName;
                colSchema.Storage = columnName.StorageFieldName;

                //sample input: columnRow.column_type="varchar(15)", coloumRow.datatype="varchar"
                //colSchema.DbType = columnRow.datatype;
                string dbType = columnRow.column_type;

                dbType = dbType.Replace("int(11)", "int") //remove some default sizes
                    .Replace("int(10) unsigned", "int unsigned")
                    .Replace("mediumint(8) unsigned", "mediumint unsigned")
                    .Replace("decimal(10,0)", "decimal")
                    ;
                colSchema.DbType = dbType;

                if (columnRow.column_key == "PRI")
                    colSchema.IsPrimaryKey = true;
                if (columnRow.extra == "auto_increment")
                    colSchema.IsDbGenerated = true;
                colSchema.CanBeNull = columnRow.isNullable;

                //determine the C# type
                colSchema.Type = MapDbType(GetDataType(columnRow)).ToString();
                if (columnRow.column_name == "DbLinq_EnumTest")
                    colSchema.Type = "DbLinq_EnumTest"; //hadcoded value - used during enum testing

                //tableSchema.Types[0].Columns.Add(colSchema);
                tableSchema.Type.Columns.Add(colSchema);
            }

            //##################################################################
            //step 3 - load foreign keys etc
            KeyColumnUsageSql ksql = new KeyColumnUsageSql();
            List<KeyColumnUsage> constraints = ksql.getConstraints(conn, schemaName.DbName);

            //sort tables - parents first (this is moving to SchemaPostprocess)
            //TableSorter.Sort(tables, constraints); 

            foreach (KeyColumnUsage keyColRow in constraints)
            {
                //find my table:
                string fullKeyDbName = GetFullDbName(keyColRow.table_name, keyColRow.table_schema);
                DbLinq.Schema.Dbml.Table table = schema.Tables.FirstOrDefault(t => fullKeyDbName == t.Name);
                if (table == null)
                {
                    Logger.Write(Level.Error, "ERROR L46: Table '" + keyColRow.table_name + "' not found for column " + keyColRow.column_name);
                    continue;
                }

                bool isForeignKey = keyColRow.constraint_name != "PRIMARY"
                                 && keyColRow.referenced_table_name != null;

                if (isForeignKey)
                {
                    var associationName = CreateAssociationName(keyColRow.table_name, keyColRow.table_schema,
                        keyColRow.referenced_table_name, keyColRow.referenced_table_schema,
                        keyColRow.constraint_name);

                    //both parent and child table get an [Association]
                    DbLinq.Schema.Dbml.Association assoc = new DbLinq.Schema.Dbml.Association();
                    assoc.IsForeignKey = true;
                    assoc.Name = keyColRow.constraint_name;
                    assoc.Type = null;
                    assoc.ThisKey = names.ColumnsNames[keyColRow.table_name][keyColRow.column_name].PropertyName; //GetColumnName(keyColRow.column_name);
                    assoc.Member = associationName.ManyToOneMemberName;
                    assoc.Storage = associationName.ForeignKeyStorageFieldName; //GetColumnFieldName(keyColRow.constraint_name);
                    table.Type.Associations.Add(assoc);

                    //and insert the reverse association:
                    DbLinq.Schema.Dbml.Association assoc2 = new DbLinq.Schema.Dbml.Association();
                    assoc2.Name = keyColRow.constraint_name;
                    assoc2.Type = table.Type.Name;
                    assoc2.Member = associationName.OneToManyMemberName;
                    assoc2.OtherKey = names.ColumnsNames[keyColRow.referenced_table_name][keyColRow.referenced_column_name].PropertyName; // GetColumnName(keyColRow.referenced_column_name);

                    string referencedTableFullDbName = GetFullDbName(keyColRow.referenced_table_name, keyColRow.referenced_table_schema);
                    DbLinq.Schema.Dbml.Table parentTable = schema.Tables.FirstOrDefault(t => referencedTableFullDbName == t.Name);
                    if (parentTable == null)
                    {
                        Logger.Write(Level.Error, "ERROR 148: parent table not found: " + keyColRow.referenced_table_name);
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
            if (loadStoredProcedures)
            {
                ProcSql procsql = new ProcSql();
                List<ProcRow> procs = procsql.getProcs(conn, schemaName.DbName);

                foreach (ProcRow proc in procs)
                {
                    var procedureName = CreateProcedureName(proc.specific_name, proc.db);

                    DbLinq.Schema.Dbml.Function func = new DbLinq.Schema.Dbml.Function();
                    func.Name = procedureName.DbName;
                    func.Method = procedureName.MethodName;
                    func.IsComposable = string.Compare(proc.type, "FUNCTION") == 0;
                    func.BodyContainsSelectStatement = proc.body != null
                        && proc.body.IndexOf("select", StringComparison.OrdinalIgnoreCase) > -1;
                    ParseProcParams(proc, func);

                    schema.Functions.Add(func);
                }
            }

            return schema;
        }

        protected void ParseProcParams(ProcRow inputProc, DbLinq.Schema.Dbml.Function outputFunc)
        {
            string paramString = inputProc.param_list;
            if (string.IsNullOrEmpty(paramString))
            {
                //nothing to parse
            }
            else
            {
                string[] parts = paramString.Split(',');

                char[] SPACES = new char[] { ' ', '\t', '\n' }; //word separators

                foreach (string part in parts) //part='OUT param1 int'
                {
                    DbLinq.Schema.Dbml.Parameter paramObj = ParseParameterString(part);
                    if (paramObj != null)
                        outputFunc.Parameters.Add(paramObj);
                }
            }

            if (inputProc.returns != null && inputProc.returns != "")
            {
                var paramRet = new DbLinq.Schema.Dbml.Return();
                paramRet.DbType = inputProc.returns;
                paramRet.Type = ParseDbType(inputProc.returns);
                outputFunc.Return = paramRet;
            }
        }

        /// <summary>
        /// parse strings such as 'INOUT param2 INT' or 'param4 varchar ( 32 )'
        /// </summary>
        /// <param name="paramStr"></param>
        /// <returns></returns>
        protected DbLinq.Schema.Dbml.Parameter ParseParameterString(string param)
        {
            param = param.Trim();
            var inOut = DbLinq.Schema.Dbml.ParameterDirection.In;

            if (param.StartsWith("IN", StringComparison.CurrentCultureIgnoreCase))
            {
                inOut = DbLinq.Schema.Dbml.ParameterDirection.In;
                param = param.Substring(2).Trim();
            }
            if (param.StartsWith("INOUT", StringComparison.CurrentCultureIgnoreCase))
            {
                inOut = DbLinq.Schema.Dbml.ParameterDirection.InOut;
                param = param.Substring(5).Trim();
            }
            if (param.StartsWith("OUT", StringComparison.CurrentCultureIgnoreCase))
            {
                inOut = DbLinq.Schema.Dbml.ParameterDirection.Out;
                param = param.Substring(3).Trim();
            }

            int indxSpace = param.IndexOfAny(new char[] { ' ', '\t' });
            if (indxSpace == -1)
                return null; //cannot find space between varName and varType

            string varName = param.Substring(0, indxSpace);
            string varType = param.Substring(indxSpace + 1);

            DbLinq.Schema.Dbml.Parameter paramObj = new DbLinq.Schema.Dbml.Parameter();
            paramObj.Direction = inOut;
            paramObj.Name = varName;
            paramObj.DbType = varType;
            paramObj.Type = ParseDbType(varType);

            return paramObj;
        }

        static System.Text.RegularExpressions.Regex re_CHARSET = new System.Text.RegularExpressions.Regex(@" CHARSET \w+$");
        /// <summary>
        /// given 'CHAR(30)', return 'string'
        /// </summary>
        protected string ParseDbType(string dbType1)
        {
            //strip 'CHARSET latin1' from the end
            string dbType2 = re_CHARSET.Replace(dbType1, "");
            bool isUnsigned = false;
            int? length = null;
            string varType = dbType2.Trim().ToLower();
            string varTypeQualifier = "";
            int indxQuote = varType.IndexOf('(');
            if (indxQuote > -1)
            {
                //split 'CHAR(30)' into 'char' and '(30)'
                varTypeQualifier = varType.Substring(indxQuote);
                length = int.Parse(varTypeQualifier.TrimStart('(').TrimEnd(')'));
                varType = varType.Substring(0, indxQuote);
            }
            else if (varType.IndexOf("unsigned", StringComparison.OrdinalIgnoreCase) > -1)
            {
                isUnsigned = true;
                varType = varType.Replace("unsigned", "").Trim();
            }
            var dataType = new DataType
            {
                Type = varType,
                Length = length,
                Unsigned = isUnsigned
            };
            string dbTypeStr = MapDbType(dataType).ToString();
            return dbTypeStr;
        }

        protected DataType GetDataType(Schema.Column column)
        {
            return new DataType
            {
                Type = column.datatype,
                Length = column.Length,
                Precision = column.Precision,
                Scale = column.Scale,
                Unsigned = column.Unsigned
            };
        }
    }
}

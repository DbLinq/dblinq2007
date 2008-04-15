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
using System.IO;
using System.Linq;
using DbLinq.Linq;
using DbLinq.Logging;
using DbLinq.Schema;
using DbLinq.Schema.Dbml;
using DbLinq.Sqlite;
using DbLinq.Sqlite.Schema;
using DbLinq.Util;
using DbLinq.Vendor;
using DbLinq.Vendor.Implementation;

namespace DbLinq.Sqlite
{
    partial class SqliteSchemaLoader : SchemaLoader
    {
        private readonly IVendor vendor = new SqliteVendor();
        public override IVendor Vendor { get { return vendor; } }

        public override System.Type DataContextType { get { return typeof(SqliteDataContext); } }

        protected override Database Load(SchemaName schemaName, IDictionary<string, string> tableAliases, NameFormat nameFormat, bool loadStoredProcedures)
        {
            IDbConnection conn = Connection;

            var names = new Names();

            var schema = new Database();

            schema.Name = schemaName.DbName;
            schema.Class = schemaName.ClassName;

            //##################################################################
            //step 1 - load tables
            var tables = LoadTablesSchema(conn, schemaName.DbName);
            if (tables == null || tables.Count == 0)
            {
                Logger.Write(Level.Warning, "No tables found for schema " + schemaName.DbName + ", exiting");
                return null;
            }

            foreach (var tblRow in tables)
            {
                var tableName = CreateTableName(tblRow.Name, tblRow.Schema, tableAliases, nameFormat);
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

                //sample input: columnRow.column_type="varchar(15)", coloumRow.Type="varchar"
                //colSchema.DbType = columnRow.Type;
                string dbType = columnRow.column_type;

                dbType = dbType.Replace("int(11)", "int") //remove some default sizes
                    .Replace("int(10) unsigned", "int unsigned")
                    .Replace("mediumint(8) unsigned", "mediumint unsigned")
                    .Replace("decimal(10,0)", "decimal")
                    ;
                colSchema.DbType = dbType;

                colSchema.IsPrimaryKey = columnRow.isPrimaryKey;
                //if (columnRow.extra == "auto_increment")
                //    colSchema.IsDbGenerated = true;

                colSchema.CanBeNull = columnRow.isNullable;

                //determine the C# type
                colSchema.Type = MapDbType(columnRow).ToString();
                //if (columnRow.column_name == "DbLinq_EnumTest")
                //    colSchema.Type = "DbLinq_EnumTest"; //hadcoded value - used during enum testing

                //SQLite always autoincrement PRimary Key integers
                if (!colSchema.IsDbGenerated && colSchema.IsPrimaryKey && (colSchema.Type == "int" || colSchema.Type == "int?"))
                    colSchema.IsDbGenerated = true;

                //tableSchema.Types[0].Columns.Add(colSchema);
                tableSchema.Type.Columns.Add(colSchema);
            }

            //##################################################################
            //step 3 - load foreign keys etc
            KeyColumnUsageSql ksql = new KeyColumnUsageSql();
            List<KeyColumnUsage> constraints = ksql.getConstraints(conn, schemaName.DbName);

            //sort tables - parents first (this is moving to SchemaPostprocess)
            //TableSorter.Sort(tables, constraints); 

            // Deal with non existing foreign key database
            if (constraints != null)
            {
                foreach (KeyColumnUsage keyColRow in constraints)
                {
                    //find my table:
                    string tableFullDbName = GetFullDbName(keyColRow.table_name, keyColRow.table_schema);
                    DbLinq.Schema.Dbml.Table table = schema.Tables.FirstOrDefault(t => tableFullDbName == t.Name);
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
                            keyColRow.constraint_name, nameFormat);

                        var foreignKey = names.ColumnsNames[keyColRow.table_name][keyColRow.column_name].PropertyName;
                        var reverseForeignKey = names.ColumnsNames[keyColRow.referenced_table_name][keyColRow.referenced_column_name].PropertyName; // GetColumnName(keyColRow.referenced_column_name);

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
                        assoc2.Type = table.Type.Name; //keyColRow.Name;
                        assoc2.Member = associationName.OneToManyMemberName;
                        assoc2.ThisKey = reverseForeignKey;
                        assoc2.OtherKey = foreignKey;
                        //assoc2.Member = keyColRow.Name;

                        //bool isSelfJoin = keyColRow.Name == keyColRow.referenced_table_name;
                        //assoc2.OtherKey = isSelfJoin
                        //    ? keyColRow.column_name //in Employees table - "ReportsTo" appears in both [Association]
                        //    : keyColRow.referenced_column_name;
                        //assoc2.OtherKey = keyColRow.referenced_column_name;

                        string parentFullDbName = GetFullDbName(keyColRow.referenced_table_name, keyColRow.referenced_table_schema);
                        DbLinq.Schema.Dbml.Table parentTable = schema.Tables.FirstOrDefault(t => parentFullDbName == t.Name);
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
            }


            //##################################################################
            //step 4 - load stored procs
            if (loadStoredProcedures)
            {
                ProcSql procsql = new ProcSql();
                List<ProcRow> procs = procsql.getProcs(conn, schemaName.DbName);

                foreach (ProcRow proc in procs)
                {
                    var procedureName = CreateProcedureName(proc.specific_name, proc.db, nameFormat);

                    DbLinq.Schema.Dbml.Function func = new DbLinq.Schema.Dbml.Function();
                    func.Name = proc.specific_name;
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

        /// <summary>
        /// parse bytes 'OUT param1 int, param2 int'.
        /// The newly created DbLinq.Schema.Dbml.Parameter objects will be appended to 'outputFunc'.
        /// </summary>
        protected void ParseProcParams(ProcRow inputProc, DbLinq.Schema.Dbml.Function outputFunc)
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

            var dataType = new DataType();
            dataType.UnpackRawDbType(dbType2);
            return MapDbType(dataType).ToString();
        }
    }
}

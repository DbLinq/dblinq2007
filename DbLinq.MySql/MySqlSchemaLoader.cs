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
    partial class MySqlSchemaLoader : SchemaLoader
    {
        private readonly IVendor vendor = new MySqlVendor();
        public override IVendor Vendor { get { return vendor; } }

        public override System.Type DataContextType { get { return typeof(MySqlDataContext); } }

        protected override TableName CreateTableName(string dbTableName, string dbSchema, IDictionary<string, string> tableAliases, NameFormat nameFormat)
        {
            WordsExtraction extraction = WordsExtraction.FromDictionary;
            if (tableAliases != null && tableAliases.ContainsKey(dbTableName))
            {
                extraction = WordsExtraction.FromCase;
                dbTableName = tableAliases[dbTableName];
            }
            var tableName = NameFormatter.GetTableName(dbTableName, extraction, nameFormat);
            tableName.DbName = GetFullDbName(dbTableName, dbSchema);
            return tableName;
        }

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
                        keyColRow.constraint_name, nameFormat);

                    var foreignKey = names.ColumnsNames[keyColRow.table_name][keyColRow.column_name].PropertyName; //GetColumnName(keyColRow.column_name);
                    var reverseForeignKey = names.ColumnsNames[keyColRow.referenced_table_name][keyColRow.referenced_column_name].PropertyName; // GetColumnName(keyColRow.referenced_column_name);

                    //both parent and child table get an [Association]
                    DbLinq.Schema.Dbml.Association assoc = new DbLinq.Schema.Dbml.Association();
                    assoc.IsForeignKey = true;
                    assoc.Name = keyColRow.constraint_name;
                    assoc.Type = null;
                    assoc.ThisKey = foreignKey;
                    assoc.OtherKey = reverseForeignKey;
                    assoc.Member = associationName.ManyToOneMemberName;
                    assoc.Storage = associationName.ForeignKeyStorageFieldName; //GetColumnFieldName(keyColRow.constraint_name);
                    table.Type.Associations.Add(assoc);

                    //and insert the reverse association:
                    DbLinq.Schema.Dbml.Association assoc2 = new DbLinq.Schema.Dbml.Association();
                    assoc2.Name = keyColRow.constraint_name;
                    assoc2.Type = table.Type.Name;
                    assoc2.Member = associationName.OneToManyMemberName;
                    assoc2.ThisKey = reverseForeignKey;
                    assoc2.OtherKey = foreignKey;

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
                    var procedureName = CreateProcedureName(proc.specific_name, proc.db, nameFormat);

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
            var dataType = new DataType();
            dataType.UnpackRawDbType(dbType2);
            return MapDbType(dataType).ToString();
        }
    }
}

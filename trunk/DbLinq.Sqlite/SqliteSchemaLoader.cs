#region MIT license
// 
// Copyright (c) 2007-2008 Jiri Moudry
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
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

        protected override void LoadStoredProcedures(Database schema, SchemaName schemaName, IDbConnection conn, NameFormat nameFormat)
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

        protected override void LoadConstraints(Database schema, SchemaName schemaName, IDbConnection conn, NameFormat nameFormat, Names names)
        {
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
                    string tableFullDbName = GetFullDbName(keyColRow.TableName, keyColRow.TableSchema);
                    DbLinq.Schema.Dbml.Table table = schema.Tables.FirstOrDefault(t => tableFullDbName == t.Name);
                    if (table == null)
                    {
                        Logger.Write(Level.Error, "ERROR L46: Table '" + keyColRow.TableName + "' not found for column " + keyColRow.column_name);
                        continue;
                    }

                    bool isForeignKey = keyColRow.ConstraintName != "PRIMARY"
                                        && keyColRow.ReferencedTableName != null;

                    if (isForeignKey)
                    {
                        LoadForeignKey(schema, table, keyColRow.column_name, keyColRow.TableName, keyColRow.TableSchema,
                                       keyColRow.referenced_column_name, keyColRow.ReferencedTableName,
                                       keyColRow.ReferencedTableSchema,
                                       keyColRow.ConstraintName, nameFormat, names);

                    }

                }
            }
        }

        /// <summary>
        /// parse bytes 'OUT param1 int, param2 int'.
        /// The newly created DbLinq.Schema.Dbml.Parameter objects will be appended to 'outputFunc'.
        /// </summary>
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

            if (!string.IsNullOrEmpty(inputProc.returns))
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

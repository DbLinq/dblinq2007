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
using System.Linq;
using DbLinq.Linq;
using DbLinq.PostgreSql.Schema;
using DbLinq.Schema;
using DbLinq.Schema.Dbml;
using DbLinq.Util;
using DbLinq.Vendor.Implementation;
using DbLinq.Logging;

namespace DbLinq.PostgreSql
{
    partial class PgsqlSchemaLoader : SchemaLoader
    {
        private readonly Vendor.IVendor vendor = new PgsqlVendor();
        public override Vendor.IVendor Vendor { get { return vendor; } }

        public override System.Type DataContextType { get { return typeof(PgsqlDataContext); } }

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
            //step 3 - analyse foreign keys etc

            //TableSorter.Sort(tables, constraints); //sort tables - parents first

            KeyColumnUsageSql ksql = new KeyColumnUsageSql();
            List<KeyColumnUsage> constraints = ksql.getConstraints(conn, schemaName.DbName);
            ForeignKeySql fsql = new ForeignKeySql();

            List<ForeignKeyCrossRef> allKeys2 = fsql.getConstraints(conn, schemaName.DbName);
            List<ForeignKeyCrossRef> foreignKeys = allKeys2.Where(k => k.constraint_type == "FOREIGN KEY").ToList();
            List<ForeignKeyCrossRef> primaryKeys = allKeys2.Where(k => k.constraint_type == "PRIMARY KEY").ToList();


            foreach (KeyColumnUsage keyColRow in constraints)
            {
                //find my table:
                string constraintFullDbName = GetFullDbName(keyColRow.TableName, keyColRow.TableSchema);
                DbLinq.Schema.Dbml.Table table = schema.Tables.FirstOrDefault(t => constraintFullDbName == t.Name);
                if (table == null)
                {
                    Logger.Write(Level.Error, "ERROR L138: Table '" + keyColRow.TableName + "' not found for column " + keyColRow.ColumnName);
                    continue;
                }

                if (keyColRow.ConstraintName.EndsWith("_pkey")) //MYSQL reads 'PRIMARY'
                {
                    //A) add primary key
                    DbLinq.Schema.Dbml.Column primaryKeyCol = table.Type.Columns.First(c => c.Name == keyColRow.ColumnName);
                    if (!primaryKeyCol.IsPrimaryKey) // picrap: just to check if the case happens
                        primaryKeyCol.IsPrimaryKey = true;
                }
                else
                {
                    ForeignKeyCrossRef foreignKey = foreignKeys.FirstOrDefault(f => f.constraint_name == keyColRow.ConstraintName);
                    if (foreignKey == null)
                    {
                        string msg = "Missing data from 'constraint_column_usage' for foreign key " + keyColRow.ConstraintName;
                        Logger.Write(Level.Error, msg);
                        //throw new ApplicationException(msg);
                        continue; //as per Andrus, do not throw. //putting together an Adnrus_DB test case.
                    }

                    LoadForeignKey(schema, table, keyColRow.ColumnName, keyColRow.TableName, keyColRow.TableSchema,
                                  foreignKey.ColumnName, foreignKey.ReferencedTableName,
                                  foreignKey.ReferencedTableSchema,
                                  keyColRow.ConstraintName, nameFormat, names);

                }

            }

            //##################################################################
            //step 4 - analyse stored procs
            if (loadStoredProcedures)
            {
                Pg_Proc_Sql procSql = new Pg_Proc_Sql();
                List<Pg_Proc> procs = procSql.getProcs(conn, schemaName.DbName);

                //4a. determine unknown types
                Dictionary<long, string> typeOidToName = new Dictionary<long, string>();

                foreach (Pg_Proc proc in procs)
                {
                    if (proc.proallargtypes == null && proc.proargtypes != null && proc.proargtypes != "")
                        proc.proallargtypes = "{" + proc.proargtypes.Replace(' ', ',') + "}"; //work around pgsql weirdness?
                }

                foreach (Pg_Proc proc in procs)
                {
                    typeOidToName[proc.prorettype] = proc.formatted_prorettype;
                    if (proc.proallargtypes == null)
                        continue; //no args, no Oids to resolve, skip

                    string[] argTypes1 = parseCsvString(proc.proallargtypes); //eg. {23,24,1043}
                    var argTypes2 = from t in argTypes1 select long.Parse(t);

                    foreach (long argType in argTypes2)
                    {
                        if (!typeOidToName.ContainsKey(argType))
                            typeOidToName[argType] = null;
                    }
                }

                //4b. get names for unknown types
                procSql.getTypeNames(conn, schemaName.DbName, typeOidToName);

                //4c. generate dbml objects
                foreach (Pg_Proc proc in procs)
                {
                    DbLinq.Schema.Dbml.Function dbml_fct = ParseFunction(proc, typeOidToName, nameFormat);
                    if (!SkipProc(dbml_fct.Name))
                        schema.Functions.Add(dbml_fct);
                }

            }
            return schema;
        }

        #region function parsing

        /// <summary>
        /// parse pg param modes string such as '{i,i,o}'
        /// </summary>
        static string[] parseCsvString(string csvString)
        {
            if (csvString == null || (!csvString.StartsWith("{")) || (!csvString.EndsWith("}")))
                return null;
            List<string> list = new List<string>();
            string middle = csvString.Substring(1, csvString.Length - 2);
            string[] parts = middle.Split(',');
            return parts;
        }

        DbLinq.Schema.Dbml.Function ParseFunction(Pg_Proc pg_proc, Dictionary<long, string> typeOidToName, NameFormat nameFormat)
        {
            var procedureName = CreateProcedureName(pg_proc.proname, null, nameFormat);

            DbLinq.Schema.Dbml.Function dbml_func = new DbLinq.Schema.Dbml.Function();
            dbml_func.Name = procedureName.DbName;
            dbml_func.Method = procedureName.MethodName;

            if (pg_proc.formatted_prorettype != null)
            {
                var dbml_param = new DbLinq.Schema.Dbml.Return();
                dbml_param.DbType = pg_proc.formatted_prorettype;
                dbml_param.Type = MapDbType(new DataType { Type = pg_proc.formatted_prorettype }).ToString();
                dbml_func.Return = dbml_param;
            }

            if (pg_proc.proallargtypes != null)
            {
                string[] argModes = parseCsvString(pg_proc.proargmodes);
                string[] argNames = parseCsvString(pg_proc.proargnames);
                string[] argTypes1 = parseCsvString(pg_proc.proallargtypes); //eg. {23,24,1043}
                List<long> argTypes2 = (from t in argTypes1 select long.Parse(t)).ToList();

                if (argNames == null)
                {
                    //proc was specified as 'FUNCTION doverlaps(IN date)' - names not specified
                    argNames = new string[argTypes1.Length];
                    for (int i = 0; i < argNames.Length; i++) { argNames[i] = ((char)('a' + i)).ToString(); }
                }

                bool doLengthsMatch = (argTypes2.Count != argNames.Length
                    || (argModes != null && argModes.Length != argNames.Length));
                if (doLengthsMatch)
                {
                    Logger.Write(Level.Error, "L238 Mistmatch between modesArr, typeArr and nameArr for func " + pg_proc.proname);
                    return null;
                }

                List<DbLinq.Schema.Dbml.Parameter> paramList = new List<DbLinq.Schema.Dbml.Parameter>();
                for (int i = 0; i < argNames.Length; i++)
                {
                    DbLinq.Schema.Dbml.Parameter dbml_param = new DbLinq.Schema.Dbml.Parameter();
                    long argTypeOid = argTypes2[i];
                    dbml_param.DbType = typeOidToName[argTypeOid];
                    dbml_param.Name = argNames[i];
                    dbml_param.Type = MapDbType(new DataType { Type = dbml_param.DbType }).ToString();
                    string inOut = argModes == null ? "i" : argModes[i];
                    dbml_param.Direction = ParseInOut(inOut);
                    dbml_func.Parameters.Add(dbml_param);
                }
            }

            return dbml_func;
        }

        static DbLinq.Schema.Dbml.ParameterDirection ParseInOut(string inOut)
        {
            switch (inOut)
            {
            case "i": return DbLinq.Schema.Dbml.ParameterDirection.In;
            case "o": return DbLinq.Schema.Dbml.ParameterDirection.Out;
            case "b": return DbLinq.Schema.Dbml.ParameterDirection.InOut;
            default: return DbLinq.Schema.Dbml.ParameterDirection.InOut;
            }
        }

        #endregion


        private bool SkipProc(string name)
        {
            string[] prefixes = System.Configuration.ConfigurationManager.AppSettings["postgresqlSkipProcPrefixes"].Split(',');

            foreach (string s in prefixes)
            {
                if (name.StartsWith(s))
                    return true;
            }
            return false;
        }

    }
}

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
namespace SqlMetal.schema.sqlite { } //this namespace is used from other csproj

namespace SqlMetal.schema.pgsql
{
    /// <summary>
    /// this class contains MySql-specific way of retrieving DB schema.
    /// </summary>
    class Vendor : IDBVendor
    {
        public string VendorName() { return "PostgreSQL"; }
        public string DataContextName() { return "DbLinq.PostgreSql.PgsqlDataContext"; }
        public string ProviderClassName() { return "PgsqlVendor"; }

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
            List<TableRow> tables = tsql.getTables(conn, mmConfig.database);
            if (tables == null || tables.Count == 0)
            {
                Console.WriteLine("No tables found for schema " + mmConfig.database + ", exiting");
                return null;
            }

            foreach (TableRow tblRow in tables)
            {
                DlinqSchema.Table tblSchema = new DlinqSchema.Table();
                tblSchema.Name = tblRow.table_schema + "." + tblRow.table_name;
                //name formatting is deferred till SchemaPostprocess
                tblSchema.Member = tblRow.table_name;
                tblSchema.Type.Name = tblRow.table_name;
                schema.Tables.Add(tblSchema);
            }

            //##################################################################
            //step 2 - load columns
            ColumnSql csql = new ColumnSql();
            List<Column> columns = csql.getColumns(conn, mmConfig.database);

            KeyColumnUsageSql ksql = new KeyColumnUsageSql();
            List<KeyColumnUsage> constraints = ksql.getConstraints(conn, mmConfig.database);
            ForeignKeySql fsql = new ForeignKeySql();

            List<ForeignKeyCrossRef> allKeys2 = fsql.getConstraints(conn, mmConfig.database);
            List<ForeignKeyCrossRef> foreignKeys = allKeys2.Where(k => k.constraint_type == "FOREIGN KEY").ToList();
            List<ForeignKeyCrossRef> primaryKeys = allKeys2.Where(k => k.constraint_type == "PRIMARY KEY").ToList();


            foreach (Column columnRow in columns)
            {
                //find which table this column belongs to
                DlinqSchema.Table tableSchema = schema.Tables.FirstOrDefault(tblSchema => columnRow.TableNameWithSchema == tblSchema.Name);
                if (tableSchema == null)
                {
                    Console.WriteLine("ERROR L46: Table '" + columnRow.table_name + "' not found for column " + columnRow.column_name);
                    continue;
                }
                DlinqSchema.Column colSchema = new DlinqSchema.Column();
                colSchema.Name = columnRow.column_name;
                colSchema.DbType = columnRow.DataTypeWithWidth; //columnRow.datatype;
                KeyColumnUsage primaryKCU = constraints.FirstOrDefault(c => c.column_name == columnRow.column_name
                    && c.table_name == columnRow.table_name && c.constraint_name.EndsWith("_pkey"));

                colSchema.IsPrimaryKey = primaryKCU != null; //columnRow.column_key=="PRI";
                colSchema.IsDbGenerated = columnRow.column_default != null && columnRow.column_default.StartsWith("nextval(");

                //parse sequence name from string such as "nextval('suppliers_supplierid_seq'::regclass)"
                if (colSchema.IsDbGenerated)
                    colSchema.Expression = columnRow.column_default.Replace("::regclass)", ")");

                //colSchema.IsVersion = ???
                colSchema.CanBeNull = columnRow.isNullable;
                colSchema.Type = Mappings.mapSqlTypeToCsType(columnRow.datatype, columnRow.column_type);

                //this will be the c# field name
                colSchema.Member = Util.FieldName(columnRow.column_name);

                colSchema.Type = Mappings.mapSqlTypeToCsType(columnRow.datatype, columnRow.column_type);
                if (CSharp.IsValueType(colSchema.Type) && columnRow.isNullable)
                    colSchema.Type += "?";

                if (columnRow.column_name == "employeetype" && columnRow.table_name == "employee" && mmConfig.database=="Andrus")
                {
                    //Andrus DB - Employee table: hardcoded for testing of vertical Partitioning
                    colSchema.IsDiscriminator = true;
                }

                //tableSchema.Types[0].Columns.Add(colSchema);
                tableSchema.Type.Columns.Add(colSchema);
            }

            //##################################################################
            //step 3 - analyse foreign keys etc

            //TableSorter.Sort(tables, constraints); //sort tables - parents first

            foreach (KeyColumnUsage keyColRow in constraints)
            {
                //find my table:
                DlinqSchema.Table table = schema.Tables.FirstOrDefault(t => keyColRow.TableNameWithSchema == t.Name);
                if (table == null)
                {
                    Console.WriteLine("ERROR L138: Table '" + keyColRow.table_name + "' not found for column " + keyColRow.column_name);
                    continue;
                }

                if (keyColRow.constraint_name.EndsWith("_pkey")) //MYSQL reads 'PRIMARY'
                {
                    //A) add primary key
                    DlinqSchema.Column primaryKeyCol = table.Type.Columns.First(c => c.Name == keyColRow.column_name);
                    primaryKeyCol.IsPrimaryKey = true;
                }
                else
                {
                    ForeignKeyCrossRef foreignKey = foreignKeys.FirstOrDefault(f => f.constraint_name == keyColRow.constraint_name);
                    if (foreignKey == null)
                    {
                        string msg = "Missing data from 'constraint_column_usage' for foreign key " + keyColRow.constraint_name;
                        Console.WriteLine(msg);
                        //throw new ApplicationException(msg);
                        continue; //as per Andrus, do not throw. //putting together an Adnrus_DB test case.
                    }

                    //if not PRIMARY, it's a foreign key.
                    //both parent and child table get an [Association]
                    DlinqSchema.Association assoc = new DlinqSchema.Association();
                    assoc.IsForeignKey = true;
                    assoc.Name = keyColRow.constraint_name;
                    assoc.Type = null;
                    assoc.ThisKey = keyColRow.column_name;
                    assoc.Member = foreignKey.table_name_Parent;
                    table.Type.Associations.Add(assoc);

                    //and insert the reverse association:
                    DlinqSchema.Association assoc2 = new DlinqSchema.Association();
                    assoc2.Name = keyColRow.constraint_name;
                    assoc2.Type = table.Type.Name;
                    assoc2.Member = keyColRow.table_name;
                    assoc2.OtherKey = keyColRow.column_name; //.referenced_column_name;

                    //DlinqSchema.Table parentTable = schema0.Tables.FirstOrDefault(t => keyColRow.referenced_table_name==t.Name);
                    DlinqSchema.Table parentTable = schema.Tables.FirstOrDefault(t => foreignKey.TableNameWithSchema_Parent == t.Name);
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

        static DlinqSchema.Function ParseFunction(Pg_Proc pg_proc, Dictionary<long, string> typeOidToName)
        {
            DlinqSchema.Function dbml_func = new DlinqSchema.Function();
            dbml_func.Name = pg_proc.proname;
            dbml_func.Method = Util.Rename(pg_proc.proname); //getproductcount -> getProductCount

            if (pg_proc.formatted_prorettype != null)
            {
                DlinqSchema.Parameter dbml_param = new DlinqSchema.Parameter();
                dbml_param.DbType = pg_proc.formatted_prorettype;
                dbml_param.Type = Mappings.mapSqlTypeToCsType(pg_proc.formatted_prorettype, "");
                dbml_func.Return.Add(dbml_param);
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
                    Console.WriteLine("L238 Mistmatch between modesArr, typeArr and nameArr for func " + pg_proc.proname);
                    return null;
                }

                List<DlinqSchema.Parameter> paramList = new List<DlinqSchema.Parameter>();
                for (int i = 0; i < argNames.Length; i++)
                {
                    DlinqSchema.Parameter dbml_param = new DlinqSchema.Parameter();
                    long argTypeOid = argTypes2[i];
                    dbml_param.DbType = typeOidToName[argTypeOid];
                    dbml_param.Name = argNames[i];
                    dbml_param.Type = Mappings.mapSqlTypeToCsType(dbml_param.DbType, "");
                    string inOut = argModes == null ? "i" : argModes[i];
                    dbml_param.InOut = ParseInOut(inOut);
                    dbml_func.Parameters.Add(dbml_param);
                }
            }

            return dbml_func;
        }

        static System.Data.ParameterDirection ParseInOut(string inOut)
        {
            switch (inOut)
            {
                case "i": return System.Data.ParameterDirection.Input;
                case "o": return System.Data.ParameterDirection.Output;
                case "b": return System.Data.ParameterDirection.InputOutput;
                default: return System.Data.ParameterDirection.InputOutput;
            }
        }

        #endregion

    }
}

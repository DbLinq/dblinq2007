
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DbLinq.PostgreSql.Schema;
using DbLinq.Schema;
using DbLinq.Util;
using DbLinq.Vendor.Implementation;

namespace DbLinq.PostgreSql
{
    class PgsqlSchemaLoader : SchemaLoader
    {
        public override string VendorName { get { return "PostgreSQL"; } }
        public override Type DataContextType { get { return typeof(PgsqlDataContext); } }
        public override DbLinq.Schema.Dbml.Database Load(string databaseName, IDictionary<string, string> tableAliases,
                                                  bool loadStoredProcedures)
        {
            IDbConnection conn = Connection;
            conn.Open();

            DbLinq.Schema.Dbml.Database schema = new DbLinq.Schema.Dbml.Database();
            schema.Name = databaseName;
            schema.Class = databaseName;

            //##################################################################
            //step 1 - load tables
            TableSql tsql = new TableSql();
            List<TableRow> tables = tsql.getTables(conn, databaseName);
            if (tables == null || tables.Count == 0)
            {
                Console.WriteLine("No tables found for schema " + databaseName + ", exiting");
                return null;
            }

            foreach (TableRow tblRow in tables)
            {
                DbLinq.Schema.Dbml.Table tblSchema = new DbLinq.Schema.Dbml.Table();
                tblSchema.Name = tblRow.table_schema + "." + tblRow.table_name;
                tblSchema.Member = GetColumnName(tblRow.table_name);
                tblSchema.Type.Name = GetTableName(tblRow.table_name, tableAliases);
                schema.Tables.Add(tblSchema);
            }

            //##################################################################
            //step 2 - load columns
            ColumnSql csql = new ColumnSql();
            List<Column> columns = csql.getColumns(conn, databaseName);

            KeyColumnUsageSql ksql = new KeyColumnUsageSql();
            List<KeyColumnUsage> constraints = ksql.getConstraints(conn, databaseName);
            ForeignKeySql fsql = new ForeignKeySql();

            List<ForeignKeyCrossRef> allKeys2 = fsql.getConstraints(conn, databaseName);
            List<ForeignKeyCrossRef> foreignKeys = allKeys2.Where(k => k.constraint_type == "FOREIGN KEY").ToList();
            List<ForeignKeyCrossRef> primaryKeys = allKeys2.Where(k => k.constraint_type == "PRIMARY KEY").ToList();


            foreach (Column columnRow in columns)
            {
                //find which table this column belongs to
                DbLinq.Schema.Dbml.Table tableSchema = schema.Tables.FirstOrDefault(tblSchema => columnRow.TableNameWithSchema == tblSchema.Name);
                if (tableSchema == null)
                {
                    Console.WriteLine("ERROR L46: Table '" + columnRow.table_name + "' not found for column " + columnRow.column_name);
                    continue;
                }
                DbLinq.Schema.Dbml.Column colSchema = new DbLinq.Schema.Dbml.Column();
                colSchema.Name = columnRow.column_name;
                colSchema.Member = GetColumnName(columnRow.column_name);
                colSchema.Storage = GetColumnFieldName(columnRow.column_name);
                colSchema.DbType = columnRow.DataTypeWithWidth; //columnRow.datatype;
                KeyColumnUsage primaryKCU = constraints.FirstOrDefault(c => c.column_name == columnRow.column_name
                    && c.table_name == columnRow.table_name && c.constraint_name.EndsWith("_pkey"));

                if (primaryKCU != null) //columnRow.column_key=="PRI";
                    colSchema.IsPrimaryKey = true;
                if (columnRow.column_default != null && columnRow.column_default.StartsWith("nextval("))
                    colSchema.IsDbGenerated = true;

                //parse sequence name from string such as "nextval('suppliers_supplierid_seq'::regclass)"
                if (colSchema.IsDbGenerated)
                    colSchema.Expression = columnRow.column_default.Replace("::regclass)", ")");

                //colSchema.IsVersion = ???
                colSchema.CanBeNull = columnRow.isNullable;
                colSchema.Type = Mappings.mapSqlTypeToCsType(columnRow.datatype, columnRow.column_type);

                colSchema.Type = Mappings.mapSqlTypeToCsType(columnRow.datatype, columnRow.column_type);
                if (CSharp.IsValueType(colSchema.Type) && columnRow.isNullable)
                    colSchema.Type += "?";

                if (columnRow.column_name == "employeetype" && columnRow.table_name == "employee" && databaseName == "Andrus")
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
                DbLinq.Schema.Dbml.Table table = schema.Tables.FirstOrDefault(t => keyColRow.TableNameWithSchema == t.Name);
                if (table == null)
                {
                    Console.WriteLine("ERROR L138: Table '" + keyColRow.table_name + "' not found for column " + keyColRow.column_name);
                    continue;
                }

                if (keyColRow.constraint_name.EndsWith("_pkey")) //MYSQL reads 'PRIMARY'
                {
                    //A) add primary key
                    DbLinq.Schema.Dbml.Column primaryKeyCol = table.Type.Columns.First(c => c.Name == keyColRow.column_name);
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
                    DbLinq.Schema.Dbml.Association assoc = new DbLinq.Schema.Dbml.Association();
                    assoc.IsForeignKey = true;
                    assoc.Name = keyColRow.constraint_name;
                    assoc.Type = null;
                    assoc.ThisKey = keyColRow.column_name;
                    assoc.Member = GetManyToOneColumnName(foreignKey.table_name_Parent, keyColRow.table_name);
                    assoc.Storage = GetColumnFieldName(keyColRow.constraint_name);
                    table.Type.Associations.Add(assoc);

                    //and insert the reverse association:
                    DbLinq.Schema.Dbml.Association assoc2 = new DbLinq.Schema.Dbml.Association();
                    assoc2.Name = keyColRow.constraint_name;
                    assoc2.Type = table.Type.Name;
                    assoc2.Member = GetOneToManyColumnName(keyColRow.table_name);
                    assoc2.OtherKey = keyColRow.column_name; //.referenced_column_name;

                    //Dbml.Table parentTable = schema0.Tables.FirstOrDefault(t => keyColRow.referenced_table_name==t.Name);
                    DbLinq.Schema.Dbml.Table parentTable = schema.Tables.FirstOrDefault(t => foreignKey.TableNameWithSchema_Parent == t.Name);
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
            if (loadStoredProcedures)
            {
                Pg_Proc_Sql procSql = new Pg_Proc_Sql();
                List<Pg_Proc> procs = procSql.getProcs(conn, databaseName);

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
                procSql.getTypeNames(conn, databaseName, typeOidToName);

                //4c. generate dbml objects
                foreach (Pg_Proc proc in procs)
                {
                    DbLinq.Schema.Dbml.Function dbml_fct = ParseFunction(proc, typeOidToName);
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

        DbLinq.Schema.Dbml.Function ParseFunction(Pg_Proc pg_proc, Dictionary<long, string> typeOidToName)
        {
            DbLinq.Schema.Dbml.Function dbml_func = new DbLinq.Schema.Dbml.Function();
            dbml_func.Name = pg_proc.proname;
            dbml_func.Method = GetMethodName(pg_proc.proname); //getproductcount -> getProductCount

            if (pg_proc.formatted_prorettype != null)
            {
                var dbml_param = new DbLinq.Schema.Dbml.Return();
                dbml_param.DbType = pg_proc.formatted_prorettype;
                dbml_param.Type = Mappings.mapSqlTypeToCsType(pg_proc.formatted_prorettype, "");
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
                    Console.WriteLine("L238 Mistmatch between modesArr, typeArr and nameArr for func " + pg_proc.proname);
                    return null;
                }

                List<DbLinq.Schema.Dbml.Parameter> paramList = new List<DbLinq.Schema.Dbml.Parameter>();
                for (int i = 0; i < argNames.Length; i++)
                {
                    DbLinq.Schema.Dbml.Parameter dbml_param = new DbLinq.Schema.Dbml.Parameter();
                    long argTypeOid = argTypes2[i];
                    dbml_param.DbType = typeOidToName[argTypeOid];
                    dbml_param.Name = argNames[i];
                    dbml_param.Type = Mappings.mapSqlTypeToCsType(dbml_param.DbType, "");
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

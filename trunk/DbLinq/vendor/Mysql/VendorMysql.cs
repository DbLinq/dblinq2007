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

using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Data.Linq.Mapping;
using System.Reflection;
using MySql.Data.MySqlClient;
using DBLinq.Linq.Mapping;
using DBLinq.util;
using DBLinq.Linq;
using DBLinq.Linq.clause;

namespace DBLinq.vendor
{
    public class Vendor
    {
        /// <summary>
        /// Client code needs to specify: 'Vendor.UserBulkInsert[db.Products]=10' to enable bulk insert, 10 rows at a time.
        /// </summary>
        public static readonly Dictionary<DBLinq.Linq.IMTable, int> UseBulkInsert = new Dictionary<DBLinq.Linq.IMTable, int>();

        /// <summary>
        /// Postgres string concatenation, eg 'a||b'
        /// </summary>
        public static string Concat(List<string> parts)
        {
            string[] arr = parts.ToArray();
            return "CONCAT("+string.Join(",",arr)+")";
        }

        /// <summary>
        /// on Postgres or Oracle, return eg. ':P1', on Mysql, '?P1'
        /// </summary>
        public static string ParamName(int index)
        {
            return "?P"+index;
        }

        /// <summary>
        /// given 'int', return '`int`' to prevent a SQL keyword conflict
        /// </summary>
        public static string FieldName_Safe(string name)
        {
            string nameL = name.ToLower();
            switch (nameL)
            {
                case "user":
                case "bit":
                case "int":
                case "smallint":
                case "tinyint":
                case "mediumint":

                case "float":
                case "double":
                case "real":
                case "decimal":
                case "numeric":

                case "blob":
                case "text":
                case "char":
                case "varchar":

                case "date":
                case "time":
                case "datetime":
                case "timestamp":
                case "year":

                    return "`" + name + "`";
                default:
                    return name;
            }
        }

        public static MySqlParameter CreateSqlParameter(string dbTypeName, string paramName)
        {
            MySqlDbType dbType = MySqlTypeConversions.ParseType(dbTypeName);
            MySqlParameter param = new MySqlParameter(paramName, dbType);
            return param;
        }

        /// <summary>
        /// for large number of rows, we want to use BULK INSERT, 
        /// because it does not fill up the translation log.
        /// This is enabled for tables where Vendor.UserBulkInsert[db.Table] is true.
        /// </summary>
        public static void DoBulkInsert<T>(DBLinq.Linq.MTable<T> table, List<T> rows, MySqlConnection conn)
        {
            int pageSize = Vendor.UseBulkInsert[table];
            //ProjectionData projData = ProjectionData.FromReflectedType(typeof(T));
            ProjectionData projData = AttribHelper.GetProjectionData(typeof(T));
            TableAttribute tableAttrib = typeof(T).GetCustomAttributes(false).OfType<TableAttribute>().Single();

            //build "INSERT INTO products (ProductName, SupplierID, CategoryID, QuantityPerUnit)"
            string header = "INSERT INTO " + tableAttrib.Name + " " + InsertClauseBuilder.InsertRowHeader(conn, projData);

            foreach (List<T> page in Util.Paginate(rows, pageSize))
            {
                int numFieldsAdded = 0;
                StringBuilder sbValues = new StringBuilder(" VALUES ");
                List<MySqlParameter> paramList = new List<MySqlParameter>();

                //package up all fields in N rows:
                string separator = "";
                foreach (T row in page)
                {
                    //prepare values = "(?P1, ?P2, ?P3, ?P4)"
                    string values = InsertClauseBuilder.InsertRowFields(row, projData, paramList, ref numFieldsAdded);
                    sbValues.Append(separator).Append(values);
                    separator = ", ";
                }

                string sql = header + sbValues; //'INSET t1 (field1) VALUES (11),(12)'
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                paramList.ForEach(param => cmd.Parameters.Add(param));

                int result = cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// call mysql stored proc or stored function, 
        /// optionally return DataSet, and collect return params.
        /// </summary>
        public static System.Data.Linq.IExecuteResult ExecuteMethodCall(DBLinq.Linq.MContext context, MethodInfo method
            , params object[] inputValues)
        {
            if (method == null)
                throw new ArgumentNullException("L56 Null 'method' parameter");

            object[] attribs1 = method.GetCustomAttributes(false);

            //check to make sure there is exactly one [FunctionEx]? that's below.
            FunctionExAttribute functionAttrib = attribs1.OfType<FunctionExAttribute>().Single();

            ParameterInfo[] paramInfos = method.GetParameters();
            //int numRequiredParams = paramInfos.Count(p => p.IsIn || p.IsRetval);
            //if (numRequiredParams != inputValues.Length)
            //    throw new ArgumentException("L161 Argument count mismatch");

            MySql.Data.MySqlClient.MySqlConnection conn = context.SqlConnection;
            //conn.Open();

            string sp_name = functionAttrib.Name;

            using (MySqlCommand command = new MySqlCommand(sp_name, conn))
            {
                //MySqlCommand command = new MySqlCommand("select hello0()");
                int currInputIndex = 0;

                List<string> paramNames = new List<string>();
                for (int i = 0; i < paramInfos.Length; i++)
                {
                    ParameterInfo paramInfo = paramInfos[i];

                    //TODO: check to make sure there is exactly one [Parameter]?
                    ParameterAttribute paramAttrib = paramInfo.GetCustomAttributes(false).OfType<ParameterAttribute>().Single();

                    string paramName = "?" + paramAttrib.Name; //eg. '?param1'
                    paramNames.Add(paramName);

                    System.Data.ParameterDirection direction = GetDirection(paramInfo, paramAttrib);
                    //MySqlDbType dbType = MySqlTypeConversions.ParseType(paramAttrib.DbType);
                    MySqlParameter cmdParam = null;
                    //cmdParam.Direction = System.Data.ParameterDirection.Input;
                    if (direction == System.Data.ParameterDirection.Input || direction == System.Data.ParameterDirection.InputOutput)
                    {
                        object inputValue = inputValues[currInputIndex++];
                        cmdParam = new MySqlParameter(paramName, inputValue);
                    }
                    else
                    {
                        cmdParam = new MySqlParameter(paramName, null);
                    }
                    cmdParam.Direction = direction;
                    command.Parameters.Add(cmdParam);
                }

                if (functionAttrib.ProcedureOrFunction == "PROCEDURE")
                {
                    //procedures: under the hood, this seems to prepend 'CALL '
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                }
                else
                {
                    //functions: 'SELECT myFunction()' or 'SELECT hello(?s)'
                    string cmdText = "SELECT " + command.CommandText + "($args)";
                    cmdText = cmdText.Replace("$args", string.Join(",", paramNames.ToArray()));
                    command.CommandText = cmdText;
                }

                if (method.ReturnType == typeof(System.Data.DataSet))
                {
                    //unknown shape of resultset:
                    System.Data.DataSet dataSet = new System.Data.DataSet();
                    MySqlDataAdapter adapter = new MySqlDataAdapter();
                    adapter.SelectCommand = command;
                    adapter.Fill(dataSet);
                    List<object> outParamValues = CopyOutParams(paramInfos, command.Parameters);
                    return new ProcResult(dataSet, outParamValues.ToArray());
                }
                else
                {
                    object obj = command.ExecuteScalar();
                    List<object> outParamValues = CopyOutParams(paramInfos, command.Parameters);
                    return new ProcResult(obj, outParamValues.ToArray());
                }
            }
        }

        static System.Data.ParameterDirection GetDirection(ParameterInfo paramInfo, ParameterAttribute paramAttrib)
        {
            //strange hack to determine what's a ref, out parameter:
            //http://lists.ximian.com/pipermain/mono-list/2003-March/012751.html
            bool hasAmpersand = paramInfo.ParameterType.FullName.Contains('&');
            if(paramInfo.IsOut)
                return System.Data.ParameterDirection.Output;
            if (hasAmpersand)
                return System.Data.ParameterDirection.InputOutput;
            return System.Data.ParameterDirection.Input;
        }

        /// <summary>
        /// Collect all Out or InOut param values, casting them to the correct .net type.
        /// </summary>
        static List<object> CopyOutParams(ParameterInfo[] paramInfos, MySqlParameterCollection paramSet)
        {
            List<object> outParamValues = new List<object>();
            //Type type_t = typeof(T);
            int i=-1;
            foreach (MySqlParameter param in paramSet)
            {
                i++;
                if (param.Direction == System.Data.ParameterDirection.Input)
                {
                    outParamValues.Add("unused");
                    continue;
                }

                object val = param.Value;
                Type desired_type = paramInfos[i].ParameterType;

                if (desired_type.Name.EndsWith("&"))
                {
                    //for ref and out parameters, we need to tweak ref types, e.g.
                    // "System.Int32&, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
                    string fullName1 = desired_type.AssemblyQualifiedName;
                    string fullName2 = fullName1.Replace("&", ""); 
                    desired_type = Type.GetType(fullName2);
                }
                try
                {
                    //fi.SetValue(t, val); //fails with 'System.Decimal cannot be converted to Int32'
                    //DBLinq.util.FieldUtils.SetObjectIdField(t, fi, val);
                    object val2 = DBLinq.util.FieldUtils.CastValue(val, desired_type);
                    outParamValues.Add(val2);
                }
                catch (Exception ex)
                {
                    //fails with 'System.Decimal cannot be converted to Int32'
                    Console.WriteLine("CopyOutParams ERROR L245: failed on CastValue(): " + ex.Message);
                }
            }
            return outParamValues;
        }

        public static int ExecuteCommand(DBLinq.Linq.MContext context, string sql, params object[] parameters)
        {
            MySql.Data.MySqlClient.MySqlConnection conn = context.SqlConnection;
            using (MySqlCommand command = new MySqlCommand(sql, conn))
            {
                return command.ExecuteNonQuery();
            }
        }
    }

}

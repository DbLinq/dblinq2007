////////////////////////////////////////////////////////////////////
//Initial author: Jiri George Moudry, 2006.
//License: LGPL. (Visit http://www.gnu.org)
//Commercial code may call into this library, if it's in a different module (DLL).
//All changes to this library must be published.
////////////////////////////////////////////////////////////////////

using System;
using System.Linq;
using System.Data.Linq.Mapping;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using DBLinq.Linq.Mapping;
using DBLinq.util;
using Npgsql;

namespace DBLinq.vendor
{
    /// <summary>
    /// PostgreSQL - specific code.
    /// </summary>
    public class Vendor
    {
        /// <summary>
        /// Postgres string concatenation, eg 'a||b'
        /// </summary>
        public static string Concat(List<string> parts)
        {
            string[] arr = parts.ToArray();
            return string.Join("||",arr);
        }

        /// <summary>
        /// on Postgres or Oracle, return eg. ':P1', on Mysql, '?P1'
        /// </summary>
        public static string ParamName(int index)
        {
            return ":P"+index;
        }

        /// <summary>
        /// given 'User', return '[User]' to prevent a SQL keyword conflict
        /// </summary>
        public static string FieldName_Safe(string name)
        {
            if (name.ToLower() == "user")
                return "[" + name + "]";
            return name;
        }

        public static NpgsqlParameter CreateSqlParameter(string dbTypeName, string paramName)
        {
            //System.Data.SqlDbType dbType = DBLinq.util.SqlTypeConversions.ParseType(dbTypeName);
            //SqlParameter param = new SqlParameter(paramName, dbType);
            NpgsqlTypes.NpgsqlDbType dbType = PgsqlTypeConversions.ParseType(dbTypeName);
            NpgsqlParameter param = new NpgsqlParameter(paramName, dbType);
            return param;
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

            NpgsqlConnection conn = context.SqlConnection;
            //conn.Open();

            string sp_name = functionAttrib.Name;

            using (NpgsqlCommand command = new NpgsqlCommand(sp_name, conn))
            {
                //MySqlCommand command = new MySqlCommand("select hello0()");
                int currInputIndex = 0;

                List<string> paramNames = new List<string>();
                for (int i = 0; i < paramInfos.Length; i++)
                {
                    ParameterInfo paramInfo = paramInfos[i];

                    //TODO: check to make sure there is exactly one [Parameter]?
                    ParameterAttribute paramAttrib = paramInfo.GetCustomAttributes(false).OfType<ParameterAttribute>().Single();

                    //string paramName = "?" + paramAttrib.Name; //eg. '?param1' MYSQL
                    string paramName = ":" + paramAttrib.Name; //eg. '?param1' PostgreSQL
                    paramNames.Add(paramName);

                    System.Data.ParameterDirection direction = GetDirection(paramInfo, paramAttrib);
                    //MySqlDbType dbType = MySqlTypeConversions.ParseType(paramAttrib.DbType);
                    NpgsqlParameter cmdParam = null;
                    //cmdParam.Direction = System.Data.ParameterDirection.Input;
                    if (direction == System.Data.ParameterDirection.Input || direction == System.Data.ParameterDirection.InputOutput)
                    {
                        object inputValue = inputValues[currInputIndex++];
                        cmdParam = new NpgsqlParameter(paramName, inputValue);
                    }
                    else
                    {
                        cmdParam = new NpgsqlParameter(paramName, null);
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
                    NpgsqlDataAdapter adapter = new NpgsqlDataAdapter();
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
            if (paramInfo.IsOut)
                return System.Data.ParameterDirection.Output;
            if (hasAmpersand)
                return System.Data.ParameterDirection.InputOutput;
            return System.Data.ParameterDirection.Input;
        }

        /// <summary>
        /// Collect all Out or InOut param values, casting them to the correct .net type.
        /// </summary>
        static List<object> CopyOutParams(ParameterInfo[] paramInfos, NpgsqlParameterCollection paramSet)
        {
            List<object> outParamValues = new List<object>();
            //Type type_t = typeof(T);
            int i = -1;
            foreach (NpgsqlParameter param in paramSet)
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

    }
}

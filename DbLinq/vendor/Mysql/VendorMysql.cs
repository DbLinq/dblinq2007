////////////////////////////////////////////////////////////////////
//Initial author: Jiri George Moudry, 2006.
//License: LGPL. (Visit http://www.gnu.org)
//Commercial code may call into this library, if it's in a different module (DLL)
////////////////////////////////////////////////////////////////////

using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Data.Linq.Mapping;
using MySql.Data.MySqlClient;
using DBLinq.Linq.Mapping;

namespace DBLinq.vendor
{
    public class Vendor
    {
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
        /// given 'User', return '[User]' to prevent a SQL keyword conflict
        /// </summary>
        public static string FieldName_Safe(string name)
        {
            if (name.ToLower() == "user")
                return "[" + name + "]";
            return name;
        }

        public static MySqlParameter CreateSqlParameter(string dbTypeName, string paramName)
        {
            MySqlDbType dbType = MySqlTypeConversions.ParseType(dbTypeName);
            MySqlParameter param = new MySqlParameter(paramName, dbType);
            return param;
        }

        public static System.Data.Linq.IExecuteResult ExecuteMethodCall(DBLinq.Linq.MContext context, System.Reflection.MethodInfo method
            , params object[] sqlParams)
        {
            //System.Reflection.MethodAttributes attribs = method.Attributes;
            if(method==null)
                throw new ArgumentNullException("L56 Null 'method' parameter");

            object[] attribs1 = method.GetCustomAttributes(false);

            //check to make sure there is exactly one [FunctionEx]? that's below.
            FunctionExAttribute functionAttrib = attribs1.OfType<FunctionExAttribute>().Single();

            //List<FunctionExAttribute> funcAttribs = attribs1.OfType<FunctionExAttribute>().ToList();
            //if(funcAttribs.Count==0)
            //    throw new ArgumentException("L61 The method you passed does not have a [Function] attribute:"+method);
            //if(funcAttribs.Count>1)
            //    throw new ArgumentException("L63 The method you passed has more than one [Function] attribute:"+method);
            //FunctionExAttribute functionAttrib = funcAttribs[0];

            System.Reflection.ParameterInfo[] paramInfos = method.GetParameters();
            if(paramInfos.Length!=sqlParams.Length)
                throw new ArgumentException("L70 Argument count mismatch");

            MySql.Data.MySqlClient.MySqlConnection conn = context.SqlConnection;
            //conn.Open();

            string sp_name = functionAttrib.Name;

            MySqlCommand command = new MySqlCommand(sp_name, conn);
            //MySqlCommand command = new MySqlCommand("select hello0()");

            List<string> paramNames = new List<string>();
            for (int i = 0; i < paramInfos.Length; i++)
            {
                System.Reflection.ParameterInfo paramInfo = paramInfos[i];
                object sqlParam = sqlParams[i];
                
                //TODO: check to make sure there is exactly one [Parameter]?
                ParameterAttribute paramAttrib = paramInfo.GetCustomAttributes(false).OfType<ParameterAttribute>().Single();
                
                string paramName = "?" + paramAttrib.Name; //eg. '?param1'
                paramNames.Add(paramName);
                //MySqlDbType dbType = MySqlTypeConversions.ParseType(paramAttrib.DbType);
                MySqlParameter cmdParam = new MySqlParameter(paramName, sqlParam);
                cmdParam.Direction = System.Data.ParameterDirection.Input;
                //if (paramInfo.IsIn)
                //{
                //    cmdParam.Direction = System.Data.ParameterDirection.Input;
                //}
                //else if (paramInfo.IsOut)
                //{
                //    cmdParam.Direction = System.Data.ParameterDirection.Output;
                //}
                //else 
                //{
                //    cmdParam.Direction = System.Data.ParameterDirection.InputOutput;
                //}
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
                cmdText = cmdText.Replace("$args", string.Join(",",paramNames.ToArray()));
                command.CommandText = cmdText;
            }

            object obj = command.ExecuteScalar();
            return new ProcResult(obj);
            //throw new NotImplementedException("TODO - call stored procs");
        }

    }

    public class ProcResult : System.Data.Linq.IExecuteResult
    {
        #region IExecuteResult Members

        public object GetParameterValue(int parameterIndex)
        {
            throw new NotImplementedException();
        }

        public object ReturnValue { get; set; }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion

        public ProcResult(object retVal)
        {
            ReturnValue = retVal;
        }
    }
}

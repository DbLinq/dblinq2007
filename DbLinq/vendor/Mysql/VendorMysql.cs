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
            List<FunctionAttribute> funcAttribs = attribs1.OfType<FunctionAttribute>().ToList();
            if(funcAttribs.Count==0)
                throw new ArgumentException("L61 The method you passed does not have a [Function] attribute:"+method);
            if(funcAttribs.Count>1)
                throw new ArgumentException("L63 The method you passed has more than one [Function] attribute:"+method);

            FunctionAttribute functionAttrib = funcAttribs[0];

            System.Reflection.ParameterInfo[] paramInfos = method.GetParameters();
            if(paramInfos.Length!=sqlParams.Length)
                throw new ArgumentException("L70 Argument count mismatch");

            MySql.Data.MySqlClient.MySqlConnection conn = context.SqlConnection;
            //conn.Open();

            string sp_name = functionAttrib.Name;
            MySqlCommand command = new MySqlCommand(sp_name);

            for (int i = 0; i < paramInfos.Length; i++)
            {
                System.Reflection.ParameterInfo paramInfo = paramInfos[i];
                object sqlParam = sqlParams[i];
                List<ParameterAttribute> paramAttribs = paramInfo.GetCustomAttributes(false).OfType<ParameterAttribute>().ToList();
                if (paramAttribs.Count == 0)
                    throw new ArgumentException("L83 The method you passed does not have a [Parameter] attribute on param "+i+":" + method);
                if (paramAttribs.Count > 1)
                    throw new ArgumentException("L85 The method you passed has more than one [Parameter] attribute on param " + i + ":" + method);
                
                ParameterAttribute paramAttrib = paramAttribs[0];
                string paramName = "?" + paramAttrib.Name; //eg. '?param1'
                MySqlDbType dbType = MySqlTypeConversions.ParseType(paramAttrib.DbType);
                MySqlParameter cmdParam = new MySqlParameter(paramName, dbType);
                command.Parameters.Add(cmdParam);

                command.Parameters[i].Value = sqlParam;
            }

            command.CommandType = System.Data.CommandType.StoredProcedure;

            using (MySqlDataReader reader = command.ExecuteReader())
            {
                bool hasRows = reader.HasRows;
                while (reader.Read())
                {
                    object obj = reader.GetFieldType(0);

                }
            }

            throw new NotImplementedException("TODO - call stored procs");
        }

    }
}

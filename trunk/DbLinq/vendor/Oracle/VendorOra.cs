using System;
using System.Data;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Data.OracleClient;
using DBLinq.util;
using DBLinq.Linq;

namespace DBLinq.vendor
{
    public class VendorOra : VendorBase, IVendor
    {
        public string VendorName { get { return VendorFactory.ORACLE; } }

        //public const string SQL_PING_COMMAND = "SELECT 11 FROM DUAL";

        public override string SqlPingCommand
        {
            get { return "SELECT 11 FROM DUAL"; }
        }

        public IDbDataParameter CreateSqlParameter(string dbTypeName, string paramName)
        {
            OracleType dbType = OracleTypeConversions.ParseType(dbTypeName);
            OracleParameter param = new OracleParameter(paramName, dbType);
            return param;
        }

        /// <summary>
        /// On Oracle, we have to insert a primary key manually.
        /// On MySql/Pgsql/Mssql, we use the IDENTITY clause to populate it automatically.
        /// </summary>
        public IDbDataParameter ProcessPkField(ProjectionData projData, ColumnAttribute colAtt
            , StringBuilder sb, StringBuilder sbValues, StringBuilder sbIdentity, ref int numFieldsAdded)
        {
            if (numFieldsAdded++ > 0) { sb.Append(", "); sbValues.Append(", "); }

            sb.Insert(0, "BEGIN ");
            sb.Append(colAtt.Name);
            string sequenceName = projData.tableAttribute.Name + "_SEQ";
            sbValues.AppendFormat("{0}.NextVal", sequenceName);

            string outParamName = this.ParamName(numFieldsAdded);
            OracleParameter outParam = new OracleParameter(outParamName, OracleType.Number);
            outParam.Direction = ParameterDirection.Output;

            string nextvalStr = string.Format(";\n SELECT {0}.CurrVal INTO {1} FROM DUAL; END;"
                , sequenceName, outParamName);
            //this semicolon gives error: ORA-00911: invalid character
            //but Oracle docs imply that you can use semicolon and multiple commands?!
            //http://www.oracle.com/technology/sample_code/tech/windows/odpnet/howto/anonyblock/index.html
            sbIdentity.Append(nextvalStr);

            return outParam;
        }

        public void ProcessInsertedId(IDbCommand cmd1, ref object returnedId)
        {
            OracleCommand cmd = (OracleCommand)cmd1;
            //OracleParameter outParam = cmd.Parameters.Where();
            foreach (OracleParameter parm in cmd.Parameters)
            {
                if (parm.Direction == ParameterDirection.Output)
                {
                    object objFromParam = parm.Value;
                    returnedId = objFromParam;
                    return;
                }
            }
        }

        /// <summary>
        /// given 'User', return '[User]' to prevent a SQL keyword conflict
        /// </summary>
        public string FieldName_Safe(string name)
        {
            if (name.ToLower() == "user")
                return "[" + name + "]";
            return name;
        }

        public int ExecuteCommand(DBLinq.Linq.MContext context, string sql, params object[] parameters)
        {
            OracleConnection conn = context.SqlConnection;
            using (OracleCommand command = new OracleCommand(sql, conn))
            {
                //int ret = command.ExecuteNonQuery();
                object obj = command.ExecuteScalar();
                Type t = obj.GetType();
                if (t == typeof(int))
                    return (int)obj;
                else if (t == typeof(decimal))
                    return (int)(decimal)obj;
                return -1;
            }
        }

        public System.Data.Linq.IExecuteResult ExecuteMethodCall(DBLinq.Linq.MContext context, MethodInfo method
            , params object[] inputValues)
        {
            throw new NotImplementedException();
        }

    }
}

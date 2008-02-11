using System;
using System.Data;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using DbLinq.Oracle;
using DBLinq.Linq;
using DBLinq.Vendor;

namespace DbLinq.Oracle
{
    public class OracleVendor : Vendor, IVendor
    {
        public string VendorName { get { return "Oracle"; } }

        //public const string SQL_PING_COMMAND = "SELECT 11 FROM DUAL";

        public override string SqlPingCommand
        {
            get { return "SELECT 11 FROM DUAL"; }
        }

        private IDictionary<string, DbType> extraTypes = new Dictionary<string, DbType>();

        public OracleVendor()
        {
            extraTypes["tinyint"] = DbType.Int16;
            extraTypes["int"] = DbType.Int32;
            extraTypes["varchar2"] = DbType.AnsiString;
        }

        public IDbDataParameter CreateSqlParameter(IDbCommand cmd, string dbTypeName, string paramName)
        {
            //OracleType dbType = OracleTypeConversions.ParseType(dbTypeName);
            //OracleParameter param = new OracleParameter(paramName, dbType);
            IDbDataParameter param = cmd.CreateParameter();
            param.ParameterName = paramName;
            SetDataParameterType(param, "OracleType", dbTypeName, extraTypes);
            return param;
        }

        /// <summary>
        /// On Oracle, we have to insert a primary key manually.
        /// On MySql/Pgsql/Mssql, we use the IDENTITY clause to populate it automatically.
        /// </summary>
        public IDbDataParameter ProcessPkField(IDbCommand cmd, ProjectionData projData, ColumnAttribute colAtt
                                               , StringBuilder sb, StringBuilder sbValues, StringBuilder sbIdentity, ref int numFieldsAdded)
        {
            if (numFieldsAdded++ > 0) { sb.Append(", "); sbValues.Append(", "); }

            sb.Insert(0, "BEGIN ");
            sb.Append(colAtt.Name);
            string sequenceName = projData.tableAttribute.Name + "_SEQ";
            sbValues.AppendFormat("{0}.NextVal", sequenceName);

            string outParamName = this.ParamName(numFieldsAdded);
            IDbDataParameter outParam = cmd.CreateParameter();
            outParam.ParameterName = outParamName;
            outParam.DbType = DbType.Decimal;
            //OracleParameter outParam = new OracleParameter(outParamName, OracleType.Number);
            outParam.Direction = ParameterDirection.Output;

            string nextvalStr = string.Format(";\n SELECT {0}.CurrVal INTO {1} FROM DUAL; END;"
                                              , sequenceName, outParamName);
            //this semicolon gives error: ORA-00911: invalid character
            //but Oracle docs imply that you can use semicolon and multiple commands?!
            //http://www.oracle.com/technology/sample_code/tech/windows/odpnet/howto/anonyblock/index.html
            sbIdentity.Append(nextvalStr);

            return outParam;
        }

        public void ProcessInsertedId(IDbCommand cmd, ref object returnedId)
        {
            //OracleParameter outParam = cmd.Parameters.Where();
            foreach (IDbDataParameter parm in cmd.Parameters)
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

        public System.Data.Linq.IExecuteResult ExecuteMethodCall(DBLinq.Linq.DataContext context, MethodInfo method
                                                                 , params object[] inputValues)
        {
            throw new NotImplementedException();
        }

        public IDataReader2 CreateDataReader2(IDataReader dataReader)
        {
            return new OracleDataReader2(dataReader);
        }
    }
}
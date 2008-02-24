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
using DbLinq.Linq;
using DbLinq.Vendor;

namespace DbLinq.Oracle
{
    public class OracleVendor : Vendor.Implementation.Vendor
    {
        public override string VendorName { get { return "Oracle"; } }

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

        public override IDbDataParameter CreateSqlParameter(IDbCommand cmd, string dbTypeName, string paramName)
        {
            IDbDataParameter param = cmd.CreateParameter();
            param.ParameterName = paramName;
            SetDataParameterType(param, "OracleType", dbTypeName, extraTypes);
            return param;
        }

        /// <summary>
        /// On Oracle, we have to insert a primary key manually.
        /// On MySql/Pgsql/Mssql, we use the IDENTITY clause to populate it automatically.
        /// </summary>
        public override IDbDataParameter ProcessPkField(IDbCommand cmd, ProjectionData projData, ColumnAttribute colAtt
                                               , StringBuilder sb, StringBuilder sbValues, StringBuilder sbIdentity, ref int numFieldsAdded)
        {
            if (numFieldsAdded++ > 0) { sb.Append(", "); sbValues.Append(", "); }

            sb.Insert(0, "BEGIN ");
            sb.Append(colAtt.Name);
            string sequenceName = projData.tableAttribute.Name + "_SEQ";
            sbValues.AppendFormat("{0}.NextVal", sequenceName);

            string outParamName = this.GetParameterName(numFieldsAdded);
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

        public override void ProcessInsertedId(IDbCommand cmd, ref object returnedId)
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
        public override string GetFieldSafeName(string name)
        {
            if (name.ToLower() == "user")
                return "[" + name + "]";
            return name;
        }

        public override IExecuteResult ExecuteMethodCall(DbLinq.Linq.DataContext context, MethodInfo method
                                                                 , params object[] inputValues)
        {
            throw new NotImplementedException();
        }

        public override IDataReader2 CreateDataReader(IDataReader dataReader)
        {
            return new OracleDataReader2(dataReader);
        }

        protected override void AddEarlyLimits(SqlExpressionParts parts, List<string> whereAndjoins)
        {
            // TODO: this doesn't work
            // I think the correct syntax is something like
            // SELECT *,_ROWNUM FROM (SELECT xxx, ROWNUM AS _ROWNUM yyy) WHERE _ROWNUM >= start AND _ROWNUM <= limit
            if (parts.LimitClause != null)
            {
                //http://dotnet.org.za/thea/archive/2005/02/22/14715.aspx
                whereAndjoins.Add("ROWNUM <= " + parts.LimitClause);
                parts.LimitClause = null; //limit clause has now been handled
            }
        }

        protected override void AddLateLimits(StringBuilder sql, SqlExpressionParts parts)
        {
        }
    }
}

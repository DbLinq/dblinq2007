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
using System.Data;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using DbLinq.Linq;
using DbLinq.Util;

namespace DbLinq.Oracle
{
    public class OracleVendor : Vendor.Implementation.Vendor
    {
        public override string VendorName { get { return "Oracle"; } }

        public override string SqlPingCommand
        {
            get { return "SELECT 11 FROM DUAL"; }
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

            string outParamName = this.GetOrderableParameterName(numFieldsAdded);
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

        protected override string MakeFieldSafeName(string name)
        {
            return name.Enquote('\"');
        }

        public override IExecuteResult ExecuteMethodCall(DbLinq.Linq.DataContext context, MethodInfo method
                                                                 , params object[] inputValues)
        {
            throw new NotImplementedException();
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

        // This method workds much better on various environment. But why? Thanks Oracle guys for dry documentation.
        public override string BuildConnectionString(string host, string databaseName, string userName, string password)
        {
            var connectionStringBuilder = new StringBuilder();
            connectionStringBuilder.AppendFormat(
                "Data Source = (DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST = {0})(PORT = 1521)))(CONNECT_DATA = (SERVER = DEDICATED)))",
                host);
            if (!string.IsNullOrEmpty(userName))
            {
                connectionStringBuilder.AppendFormat("; User Id = {0}", userName);
                if (!string.IsNullOrEmpty(password))
                    connectionStringBuilder.AppendFormat("; Password = {0}", password);
            }
            return connectionStringBuilder.ToString();
        }

        protected override string ConnectionStringDatabase { get { return null; } }
        protected override string ConnectionStringServer { get { return "data source"; } }
    }
}

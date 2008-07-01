#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne
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
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Data;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using DbLinq.Factory;
using DbLinq.Linq;
using DbLinq.Logging;
using DbLinq.Util;

#if MONO_STRICT
using DataContext=System.Data.Linq.DataContext;
using Data = System.Data;
#else
using DataContext=DbLinq.Data.Linq.DataContext;
using Data = DbLinq.Data;
#endif

namespace DbLinq.Vendor.Implementation
{
    /// <summary>
    /// some IVendor functionality is the same for many vendors,
    /// implemented here as virtual functions.
    /// </summary>
    public abstract partial class Vendor : IVendor
    {
        public ILogger Logger { get; set; }

        protected Vendor()
            : this(ObjectFactory.Get<ISqlProvider>())
        {
        }

        protected Vendor(ISqlProvider sqlProvider)
        {
            Logger = ObjectFactory.Get<ILogger>();
            this.sqlProvider = sqlProvider;
        }

        public virtual string SqlPingCommand
        {
            get { return "SELECT 11"; }
        }

        private ISqlProvider sqlProvider;
        public ISqlProvider SqlProvider { get { return sqlProvider; } }


        /// <summary>
        /// string concatenation, eg 'a||b' on Oracle.
        /// Customized in Postgres and Ingres to add casting to varchar.
        /// Customized in Mysql to use CONCAT().
        /// </summary>
        /// <param name="parts"></param>
        /// <returns></returns>
        public virtual string GetSqlConcat(List<DbLinq.Util.ExpressionAndType> parts)
        {
            string[] arr = parts.Select(p => p.expression).ToArray();
            return string.Join("||", arr);
        }

        /// <summary>
        /// on Postgres or Oracle, return eg. ':P1'.
        /// Mysql needs to override to return '?P1'
        /// Ingres needs to override to return '?'.
        /// </summary>
        public virtual string GetOrderableParameterName(int index)
        {
            return ":P" + index;
        }

        public virtual string GetFinalParameterName(string orderableName)
        {
            return orderableName;
        }

        public virtual string ReplaceParamNameInSql(string orderableName, string sql)
        {
            return sql;
        }

        public virtual void ProcessInsertedId(IDbCommand cmd1, ref object returnedId)
        {
            //only Oracle does anything
        }

        public virtual string GetSqlStringLengthFunction()
        {
            return "LENGTH";
        }

        public virtual IDbCommand AddParameter(IDbCommand cmd, IDbDataParameter param)
        {
            // only Ingres does something special here...
            cmd.Parameters.Add(param);
            return cmd;
        }

        public virtual int ExecuteCommand(DataContext context, string sql, params object[] parameters)
        {
            using (IDbCommand command = context.DatabaseContext.CreateCommand())
            {
                string sql2 = ExecuteCommand_PrepareParams(command, sql, parameters);
                command.CommandText = sql2;
                //return command.ExecuteNonQuery();
                // picrap TODO optimize and use TypeConvert.ToNumber<>();
                object objResult = command.ExecuteScalar();
                if (objResult is short)
                    return (int)(short)objResult;
                if (objResult is int)
                    return (int)objResult;
                if (objResult is long)
                    return (int)(long)objResult;
                if (objResult is decimal)
                    return (int)(decimal)objResult;
                return 0;
            }
        }

        private string ExecuteCommand_PrepareParams(IDbCommand command, string sql, object[] parameters)
        {
            sql = GetSqlCaseSafeQuery(sql);

            if (parameters.Length == 0)
            {
                Logger.Write(Level.Debug, "SQL1: " + sql);
                return sql; //nothing to do
            }

            int iParam = 0;
            List<string> paramNames = new List<string>();
            foreach (object paramValue in parameters)
            {
                string paramName = GetFinalParameterName(GetOrderableParameterName(iParam++));
                IDbDataParameter sqlParam = command.CreateParameter();
                sqlParam.ParameterName = paramName;
                sqlParam.Value = paramValue;
                command = AddParameter(command, sqlParam);
                paramNames.Add(paramName);
            }

            //replace "SET ProductName={0}" -> "SET ProductName=:P0"
            string sql2 = string.Format(sql, paramNames.ToArray());
            Logger.Write(Level.Debug, "SQL2: " + sql2);
            return sql2;
        }

        public virtual bool CanBulkInsert<T>(Data.Linq.Table<T> table) where T : class
        {
            return false;
        }

        public virtual void SetBulkInsert<T>(Data.Linq.Table<T> table, int pageSize) where T : class
        {
        }

        public virtual void DoBulkInsert<T>(Data.Linq.Table<T> table, List<T> rows, IDbConnection conn) where T : class
        {
            throw new NotImplementedException();
        }

        // TODO: we may split this big function is several parts, so we can override them in specific vendors
        public string BuildSqlString(SqlExpressionParts parts)
        {
            StringBuilder sql = new StringBuilder(500);
            sql.Append("SELECT ");
            if (parts.DistinctClause != null)
            {
                //SELECT DISTINCT(ProductID) FROM ...
                sql.Append(parts.DistinctClause).Append(" ");
            }

            if (parts.CountClause != null)
            {
                //SELECT COUNT(ProductID) FROM ... <-would count non-null ProductIDs, thanks to Andrus
                //SELECT COUNT(*) FROM ...         <-count all
                if (parts.CountClause == "COUNT")
                    sql.Append("COUNT(*)");
                else
                    sql.Append(parts.CountClause)
                        .Append("(").Append(parts.SelectFieldList[0]).Append(")");
            }
            else
            {
                //normal (non-count) select
                string opt_comma = "";
                foreach (string s in parts.SelectFieldList)
                {
                    sql.Append(opt_comma).Append(s);
                    opt_comma = ", ";
                }
            }
            //if(sb.Length>80){ sb.Append("\n"); } //for legibility, append a newline for long expressions
            //AppendList(sql, "\n FROM ", parts.FromTableList, ", ");
            string from_separator = "\n FROM ";
            foreach (TableSpec fromTable in parts.FromTableList)
            {
                if (fromTable.isHidden)
                    continue; //during LEFT JOIN, we have multiple nicknames, keep only one
                sql.Append(from_separator);
                sql.Append(fromTable.ToString());
                from_separator = ", ";
            }

            //MySql docs for JOIN:
            //http://dev.mysql.com/doc/refman/4.1/en/join.html
            //for now, we will not be using the JOIN keyword
            //List<string> whereAndjoins = new List<string>(parts.JoinList);
            foreach (JoinSpec js in parts.JoinList)
            {
                //sql.Append(" JOIN " + js.RightSpec + " ON " + js.LeftField + " = " + js.RightField + " ");
                sql.Append(js.ToString()); //js.ToString handles LEFT JOIN also
            }

            List<string> whereAndjoins = new List<string>();
            whereAndjoins.AddRange(parts.WhereList);

            AddEarlyLimits(parts, whereAndjoins);

            AppendList(sql, " WHERE ", whereAndjoins, " AND ");
            AppendList(sql, " GROUP BY ", parts.GroupByList, ", ");
            AppendList(sql, " HAVING ", parts.HavingList, ", ");

            if (parts.LimitClause == null && parts.OffsetClause != null)
            {
                //Mysql does not allow OFFSET without LIMIT.
                //use a hack:
                //change 'SELECT * FROM customers               OFFSET 2' 
                //into   'SELECT * FROM customers LIMIT 9999999 OFFSET 2' 
                parts.LimitClause = 9999999;
            }

            // PostgreSQL, Ingres cause exception if COUNT(*) is used with ORDER BY
            if (parts.CountClause != "COUNT")
            {
                AppendList(sql, " ORDER BY ", parts.OrderByList, ", ");
                if (parts.OrderDirection != null)
                    sql.Append(' ').Append(parts.OrderDirection).Append(' '); //' DESC '
            }
            AddLateLimits(sql, parts);

            return sql.ToString();
        }

        protected virtual void AddEarlyLimits(SqlExpressionParts parts, List<string> whereAndjoins)
        {
        }

        protected virtual void AddLateLimits(StringBuilder sql, SqlExpressionParts parts)
        {
            if (parts.LimitClause != null)
                sql.Append(" LIMIT " + parts.LimitClause.Value);

            if (parts.OffsetClause != null)
                sql.Append(" OFFSET " + parts.OffsetClause.Value);
        }

        void AppendList(StringBuilder sb, string header, List<string> list, string separator)
        {
            if (list.Count == 0)
                return;
            sb.Append(header);
            string currSeparator = "";
            foreach (string str in list)
            {
                sb.Append(currSeparator).Append(str);
                currSeparator = separator;
            }
        }

        public abstract string VendorName { get; }

        public abstract IDbDataParameter ProcessPkField(IDbCommand cmd, ProjectionData projData, ColumnAttribute colAtt, StringBuilder sb, StringBuilder sbValues, StringBuilder sbIdentity, ref int numFieldsAdded);
        public abstract IExecuteResult ExecuteMethodCall(DataContext context, MethodInfo method, params object[] sqlParams);

        protected virtual IDbDataAdapter CreateDataAdapter(DataContext dataContext)
        {
            return dataContext.CreateDataAdapter();
        }

        public virtual IDbDataParameter CreateDbDataParameter(IDbCommand cmd, string dbTypeName, string paramName)
        {
            IDbDataParameter param = cmd.CreateParameter();
            param.ParameterName = paramName;
            return param;
        }

        public virtual bool SupportsOutputParameter { get { return true; } }

        protected FunctionAttribute GetFunctionAttribute(MethodInfo methodInfo)
        {
            return AttribHelper.GetFunctionAttribute(methodInfo);
        }

        protected virtual string ConnectionStringServer { get { return "server"; } }
        protected virtual string ConnectionStringUser { get { return "user id"; } }
        protected virtual string ConnectionStringPassword { get { return "password"; } }
        protected virtual string ConnectionStringDatabase { get { return "database"; } }

        protected virtual void AddConnectionStringPart(IList<string> parts, string name, string value)
        {
            if (!string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(name))
                parts.Add(string.Format("{0}={1}", name, value));
        }

        public virtual string BuildConnectionString(string host, string databaseName, string userName, string password)
        {
            var connectionStringParts = new List<string>();
            AddConnectionStringPart(connectionStringParts, ConnectionStringServer, host);
            AddConnectionStringPart(connectionStringParts, ConnectionStringDatabase, databaseName);
            AddConnectionStringPart(connectionStringParts, ConnectionStringUser, userName);
            AddConnectionStringPart(connectionStringParts, ConnectionStringPassword, password);
            return string.Join(";", connectionStringParts.ToArray());
        }
    }
}

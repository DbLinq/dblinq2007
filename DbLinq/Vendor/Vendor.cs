using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Data;
using System.Linq.Expressions;
using System.Data.Linq;
using System.Data.Linq.Mapping;
//using System.Data.OracleClient;
using DbLinq.Linq;

namespace DbLinq.Vendor
{
    /// <summary>
    /// some IVendor functionality is the same for many vendors,
    /// implemented here as virtual functions.
    /// </summary>
    public abstract class Vendor: IVendor
    {
        //public abstract string VendorName { get; }
        //{
        //    get { throw new NotImplementedException(); }
        //}

        public virtual string SqlPingCommand
        {
            get { return "SELECT 11"; }
        }

        //public void ProcessPkField(DbLinq.Linq.ProjectionData projData, System.Data.Linq.Mapping.ColumnAttribute colAtt, StringBuilder sb, StringBuilder sbValues, StringBuilder sbIdentity, ref int numFieldsAdded)
        //{
        //    throw new NotImplementedException();
        //}

        /// <summary>
        /// string concatenation, eg 'a||b' on Oracle.
        /// Customized in Postgres to add casting to varchar.
        /// Customized in Mysql to use CONCAT().
        /// </summary>
        /// <param name="parts"></param>
        /// <returns></returns>
        public virtual string Concat(List<DbLinq.Util.ExpressionAndType> parts)
        {
            string[] arr = parts.Select(p => p.expression).ToArray();
            return string.Join("||", arr);
        }

        /// <summary>
        /// on Postgres or Oracle, return eg. ':P1'.
        /// Mysql needs to override to return '?P1'
        /// </summary>
        public virtual string GetParameterName(int index)
        {
            return ":P" + index;
        }

        public virtual void ProcessInsertedId(IDbCommand cmd1, ref object returnedId)
        {
            //only Oracle does anything
        }

        public virtual string GetStringLengthFunction()
        {
            return "LENGTH";
        }

        public virtual int ExecuteCommand(DbLinq.Linq.DataContext context, string sql, params object[] parameters)
        {
            IDbConnection conn = context.Connection;
            using (IDbCommand command = conn.CreateCommand())
            {
                string sql2 = ExecuteCommand_PrepareParams(command, sql, parameters);
                command.CommandText = sql2;
                //return command.ExecuteNonQuery();
                object objResult = command.ExecuteScalar();
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
            if (parameters.Length == 0)
                return sql; //nothing to do

            int iParam = 0;
            List<string> paramNames = new List<string>();
            foreach (object paramValue in parameters)
            {
                string paramName = GetParameterName(iParam++);
                IDbDataParameter sqlParam = command.CreateParameter();
                sqlParam.ParameterName = paramName;
                sqlParam.Value = paramValue;
                command.Parameters.Add(sqlParam);
                paramNames.Add(paramName);
            }

            //replace "SET ProductName={0}" -> "SET ProductName=:P0"
            string sql2 = string.Format(sql, paramNames.ToArray());
            return sql2;
        }

        public virtual bool CanBulkInsert<T>(DbLinq.Linq.Table<T> table)
        {
            return false;
        }

        public virtual void SetBulkInsert<T>(DbLinq.Linq.Table<T> table, int pageSize)
        {
        }

        public virtual void DoBulkInsert<T>(DbLinq.Linq.Table<T> table, List<T> rows, IDbConnection conn)
        {
            throw new NotImplementedException();
        }

        protected IDictionary<string, IDictionary<string, Enum>> typeMapsByProperty = new Dictionary<string, IDictionary<string, Enum>>();

        /// <summary>
        /// Sets the database property specific type by reflection, if we have a match
        /// If not, sets the database generic type
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="propertyName"></param>
        /// <param name="dbTypeName"></param>
        /// <param name="extraTypes"></param>
        public void SetDataParameterType(IDbDataParameter parameter, string propertyName, string dbTypeName, IDictionary<string, DbType> extraTypes)
        {
            PropertyInfo propertyInfo = parameter.GetType().GetProperty(propertyName);
            IDictionary<string, Enum> typeMaps = GetTypeMaps(propertyInfo);
            string dbTypeKey = dbTypeName.ToLower();
            if (typeMaps.ContainsKey(dbTypeKey))
            {
                // specific type (called from inherited CreateSqlParameter)
                propertyInfo.GetSetMethod().Invoke(parameter, new object[] {typeMaps[dbTypeKey]});
            }
            else if (extraTypes.ContainsKey(dbTypeKey))
            {
                // generic type
                parameter.DbType = extraTypes[dbTypeKey];
            }
        }

        /// <summary>
        /// Returns a dictionary of literal to database specific type enum
        /// this is used to set a parameter type with specific database type (as an enum value)
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        private IDictionary<string, Enum> GetTypeMaps(PropertyInfo propertyInfo)
        {
            IDictionary<string, Enum> typeMaps;
            if (typeMapsByProperty.ContainsKey(propertyInfo.Name))
                typeMaps = typeMapsByProperty[propertyInfo.Name];
            else
            {
                typeMaps = new Dictionary<string, Enum>();
                typeMapsByProperty[propertyInfo.Name] = typeMaps;
                // now, we use reflection to enumerate the type possible values
                if (propertyInfo.PropertyType.IsEnum)
                {
                    foreach (Enum value in Enum.GetValues(propertyInfo.PropertyType))
                    {
                        typeMaps[value.ToString().ToLower()] = value;
                    }
                }
            }
            return typeMaps;
        }

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
            AppendList(sql, "\n FROM ", parts.FromTableList, ", ");

            //MySql docs for JOIN:
            //http://dev.mysql.com/doc/refman/4.1/en/join.html
            //for now, we will not be using the JOIN keyword
            List<string> whereAndjoins = new List<string>(parts.JoinList);
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

            AppendList(sql, " ORDER BY ", parts.OrderByList, ", ");
            if (parts.OrderDirection != null)
                sql.Append(' ').Append(parts.OrderDirection).Append(' '); //' DESC '

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
        public abstract string GetFieldSafeName(string name);
        public abstract IExecuteResult ExecuteMethodCall(DbLinq.Linq.DataContext context, MethodInfo method, params object[] sqlParams);
        public abstract IDbDataParameter CreateSqlParameter(IDbCommand cmd, string dbTypeName, string paramName);
        public abstract IDataReader2 CreateDataReader(IDataReader dataReader);
    }
}

#region MIT license
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
using System.ComponentModel;
using System.Linq.Expressions;

namespace DbLinq.Vendor.Implementation
{
    /// <summary>
    /// some IVendor functionality is the same for many vendors,
    /// implemented here as virtual functions.
    /// </summary>
    public abstract class Vendor : IVendor
    {
        public ILogger Logger { get; set; }

        public Vendor()
        {
            Logger = ObjectFactory.Get<ILogger>();
        }

        public virtual string SqlPingCommand
        {
            get { return "SELECT 11"; }
        }

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
            using (IDbCommand command = context.DatabaseContext.CreateCommand())
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

    /// <summary>
    /// Executes query. Stores matching columns in instance properties and fields.
    /// Based on Marc Gravell code published in microsoft.public.dotnet.languages.csharp newsgroup
    /// </summary>
    /// <typeparam name="TResult">Entity type whose instances are returned</typeparam>
    /// <param name="context">Database to use</param>
    /// <param name="sql">Server query returning table</param>
    /// <param name="parameters">query parameters</param>
    /// <returns>Entity with matching properties and fields filled</returns>
    // TODO: make this and RowEnumeratorCompiler to use same code. This
    // enabled to use object tracking and property mapping attributes.
    // TODO: consider nulls, perhaps return TResult? as a fully intialized TResult
    public virtual IEnumerable<TResult> ExecuteQuery<TResult>(DbLinq.Linq.DataContext context, string sql, params object[] parameters) {
      using (IDbCommand command = context.DatabaseContext.CreateCommand()) {
        command.CommandText = ExecuteCommand_PrepareParams(command, sql, parameters);
        command.Connection.Open();
        using (IDataReader reader = command.ExecuteReader(
            CommandBehavior.CloseConnection | CommandBehavior.SingleResult)) {
          if (reader.Read())   {
            Func<IDataReader, TResult> objInit = CreateInitializer<TResult>(reader);
            do
              { // walk the data
               yield return objInit(reader);
              } while (reader.Read());
            }
          while (reader.NextResult()) { } // ensure any trailing errors caught
          }
        }
    }
          
      /// <summary>
      /// Compiles function which creates and initializes entity.
      /// </summary>
      /// <typeparam name="T">Entity to create</typeparam>
      /// <param name="template">returned columns from database</param>
      /// <returns>Pointer to compiled code</returns>
    static Func<IDataReader, T> CreateInitializer<T>(IDataReader template)
    {
        if (template == null) throw new ArgumentNullException("template");
        var readerParam = Expression.Parameter(typeof(IDataReader), "reader");
        Type entityType = typeof(T), readerType = typeof(IDataRecord);
        List<MemberBinding> bindings = new List<MemberBinding>();

        Type[] byOrdinal = {typeof(int)};
        MethodInfo defaultMethod = readerType.GetMethod("GetValue", byOrdinal);
        NewExpression ctor = Expression.New(entityType); // try this first...
        for (int ordinal = 0; ordinal < template.FieldCount; ordinal++)
        {
            string name = template.GetName(ordinal);
            // TODO: apply mapping here via attribute

            // get the lhs of a binding
            const BindingFlags FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase ;
            MemberInfo member = (MemberInfo) entityType.GetProperty(name, FLAGS) ??
                (MemberInfo)entityType.GetField(name, FLAGS);
            if (member == null) continue; // doesn't exist
            Type valueType;
            switch (member.MemberType) {
                case MemberTypes.Field:
                    valueType = ((FieldInfo)member).FieldType;
                    break;
                case MemberTypes.Property:
                    if (!((PropertyInfo)member).CanWrite) continue; // read only
                    valueType = ((PropertyInfo)member).PropertyType;
                    break;
                default:
                    throw new NotSupportedException(string.Format("Unexpected member-type: {0}",
                        member.MemberType));
            }

            // get the rhs of a binding
            MethodInfo method = readerType.GetMethod("Get" + valueType.Name, byOrdinal);
            Expression rhs;
            // TODO: add invoking conversion event for CHAR columns
            if (method != null && method.ReturnType == valueType)
            {
                rhs = Expression.Call(readerParam, method,
                      Expression.Constant(ordinal, typeof(int)));
            }
            else
            {
                rhs = Expression.Convert(Expression.Call(readerParam,
defaultMethod, Expression.Constant(ordinal, typeof(int))), valueType);
            }
            bindings.Add(Expression.Bind(member, rhs));
        }
        return Expression.Lambda<Func<IDataReader, T>>(
            Expression.MemberInit(ctor, bindings), readerParam).Compile();
    }
     
#if ExecuteQueryNET2Version
// Alternate implementation which works in MONO and .NET 2. Uses optional HyperDescriptor for speed.
public virtual IEnumerable<TResult> 
ExecuteQuery<TResult>(DbLinq.Linq.DataContext context, string sql, params 
object[] parameters)
                                                                where 
TResult : new() {
          using (IDbCommand command = 
context.DatabaseContext.CreateCommand()) {
            string sql2 = ExecuteCommand_PrepareParams(command, sql, 
parameters);
            command.CommandText = sql2;
            command.Connection.Open();
            using (IDataReader reader = command.ExecuteReader(
                     CommandBehavior.CloseConnection | 
CommandBehavior.SingleResult)) {
              if (reader.Read()) {
                // prepare a buffer and look at the properties
                object[] values = new object[reader.FieldCount];
                PropertyDescriptor[] props = new 
PropertyDescriptor[values.Length];
#if HyperDescriptor
            // Using Marc Gravell HyperDescriptor gets significantly better 
reflection performance (~100 x faster)
            // http://www.codeproject.com/KB/cs/HyperPropertyDescriptor.aspx
            PropertyDescriptorCollection allProps = 
PropertyHelper<TResult>.GetProperties();
#else
                PropertyDescriptorCollection allProps = 
TypeDescriptor.GetProperties(typeof(TResult));
#endif
                for (int i = 0; i < props.Length; i++) {
                  string name = reader.GetName(i);
                  props[i] = allProps.Find(name, true);
                }
                  do { // walk the data
                    reader.GetValues(values);
                    TResult t = new TResult();
                    for (int i = 0; i < props.Length; i++) {
                      // TODO: use char type conversion delegate.
                      if (props[i] != null) props[i].SetValue(t, values[i]);
                    }
                    yield return t;
                  } while (reader.Read());
                }
                while (reader.NextResult()) { } // ensure any trailing 
errors caught
              }
            }
          }

#if HyperDescriptor
         static class PropertyHelper<T>
    {
        private static readonly PropertyDescriptorCollection
properties;
        public static PropertyDescriptorCollection GetProperties()
        {
            return properties;
        }
        static PropertyHelper()
        {
            // add HyperDescriptor (optional) and get the properties
            HyperTypeDescriptionProvider.Add(typeof(T));
            properties = TypeDescriptor.GetProperties(typeof(T));
            // ensure we have a readonly collection
            PropertyDescriptor[] propArray = new
PropertyDescriptor[properties.Count];
            properties.CopyTo(propArray, 0);
            properties = new PropertyDescriptorCollection(propArray,
true);
        }
    }
#endif

#endif

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

        public virtual string GetFieldSafeName(string name)
        {
            if (IsFieldNameSafe(name))
                return name;
            return MakeFieldSafeName(name);
        }

        public virtual bool IsFieldNameSafe(string name)
        {
            switch (name.ToLower())
            {
            case "user":
                return false;
            default:
                return !name.Contains(' ');
            }
        }

        public abstract string MakeFieldSafeName(string name);
        public abstract IDbDataParameter ProcessPkField(IDbCommand cmd, ProjectionData projData, ColumnAttribute colAtt, StringBuilder sb, StringBuilder sbValues, StringBuilder sbIdentity, ref int numFieldsAdded);
        public abstract IExecuteResult ExecuteMethodCall(Linq.DataContext context, MethodInfo method, params object[] sqlParams);

        public virtual IDataReader2 CreateDataReader(IDataReader dataReader)
        {
            return new DataReader2(dataReader);
        }

        public virtual IDbDataParameter CreateSqlParameter(IDbCommand cmd, string dbTypeName, string paramName)
        {
            IDbDataParameter param = cmd.CreateParameter();
            param.ParameterName = paramName;
            return param;
        }

        protected FunctionAttribute GetFunctionAttribute(MethodInfo methodInfo)
        {
            return AttribHelper.GetFunctionAttribute(methodInfo);
        }

        /// <summary>
        /// By default, the name is case sensitive if not in full uppercase
        /// </summary>
        /// <param name="dbName"></param>
        /// <returns></returns>
        public virtual bool IsCaseSensitiveName(string dbName)
        {
            return dbName != dbName.ToUpper();
        }
    }
}

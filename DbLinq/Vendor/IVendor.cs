using System;
using System.Data;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Data.Linq;
using System.Data.Linq.Mapping;
//using System.Data.OracleClient;
using DbLinq.Util;
using DbLinq.Linq;

namespace DbLinq.Vendor
{

    public class ValueConversionEventArgs : EventArgs
    {
        internal void Init(int ordinal, IDataRecord record, object value)
        {
            Ordinal = ordinal;
            Record = record;
            Value = value;
        }

        internal ValueConversionEventArgs() { }
        public ValueConversionEventArgs(int ordinal, IDataRecord record, object value)
        {
            Init(ordinal, record, value);
        }

        public int Ordinal { get; private set; }
        public object Value { get; set; }
        public IDataRecord Record { get; private set; }
    }

    /// <summary>
    /// Vendor - specific part of DbLinq.
    /// </summary>
    public interface IVendor
    {
        /// <summary>
        /// return 'Oracle' or 'Mysql'
        /// </summary>
        string VendorName { get; }

        /// <summary>
        /// simple command to test round-trip functionality against DB:
        /// 'SELECT 11' or
        /// 'SELECT 11 FROM DUAL'
        /// </summary>
        string SqlPingCommand { get; }

        /// <summary>
        /// On Oracle, we have to insert a primary key manually.
        /// On MySql/Pgsql/Mssql, we use the IDENTITY clause to populate it automatically.
        /// </summary>
        IDbDataParameter ProcessPkField(IDbCommand cmd, ProjectionData projData, ColumnAttribute colAtt
            , StringBuilder sb, StringBuilder sbValues, StringBuilder sbIdentity, ref int numFieldsAdded);

        void ProcessInsertedId(IDbCommand cmd1, ref object returnedId);

        /// <summary>
        /// string concatenation, eg 'a||b' on Postgres 
        /// </summary>
        string Concat(List<ExpressionAndType> parts);

        /// <summary>
        /// GetParameterName: on Postgres or Oracle, return ':P1', on Mysql, return '?P1'
        /// </summary>
        string GetParameterName(int index);

        /// <summary>
        /// given 'User', return '[User]' to prevent a SQL keyword conflict
        /// </summary>
        string GetFieldSafeName(string name);

        /// <summary>
        /// return 'LENGTH' on Oracle,Mysql,PostgreSql, return 'LEN' on MSSql
        /// </summary>
        string GetStringLengthFunction();

        int ExecuteCommand(DbLinq.Linq.DataContext context, string sql, params object[] parameters);

        IExecuteResult ExecuteMethodCall(DbLinq.Linq.DataContext context, MethodInfo method, params object[] sqlParams);

        IDbDataParameter CreateSqlParameter(IDbCommand cmd, string dbTypeName, string paramName);

        bool CanBulkInsert<T>(DbLinq.Linq.Table<T> table);
        void SetBulkInsert<T>(DbLinq.Linq.Table<T> table, int pageSize);
        void DoBulkInsert<T>(DbLinq.Linq.Table<T> table, List<T> rows, IDbConnection connection);

        string BuildSqlString(SqlExpressionParts parts);

        bool IsCaseSensitiveName(string dbName);

        /// <summary>
        /// Executes query. Stores matching columns in instance fields and properties.
        /// Does 2-pass (case sensitive then insensitive) match. 
        /// Handles null reference-type values (string, byte[])
        /// Handles null Nullable<T> value-type values (int? etc)
        /// Handles (for entity TResult) class, struct and Nullable<struct>
        /// Caches and re-uses compiled delegates (thread-safe)
        /// </summary>
        /// <typeparam name="TResult">Entity type whose instances are returned</typeparam>
        /// <param name="context">Database to use</param>
        /// <param name="sql">Server query returning table</param>
        /// <param name="parameters">query parameters</param>
        /// <returns>Entity with matching properties and fields filled</returns>
        IEnumerable<TResult> ExecuteQuery<TResult>(DbLinq.Linq.DataContext dataContext, string command, object[] parameters);

        /// <summary>
        /// Custom conversion of retrieved values.
        /// Sample:   
        /// class MyNorthwind : Northwind {
        ///   public DefaultContext() : base() {
        ///     Vendor.ConvertValue += (sender, args) => {
        ///       if (args.Value != null && args.Value is string) {
        ///         args.Value = ((string)args.Value).TrimEnd();
        ///       }
        ///    };
        ///   }
        /// </summary>
        event EventHandler<ValueConversionEventArgs> ConvertValue;

        // builds a connection string from given parameters
        string BuildConnectionString(string host, string databaseName, string userName, string password);
    }
}

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
        #region Database access and generic methods

        /// <summary>
        /// VendorName represents the database being handled by this vendor 'Oracle' or 'MySql'
        /// </summary>
        string VendorName { get; }

        /// <summary>
        /// Builds a connection string given the input parameters
        /// </summary>
        /// <param name="host">Server host</param>
        /// <param name="databaseName">Database (or schema) name</param>
        /// <param name="userName">Login user name</param>
        /// <param name="password">Login password</param>
        /// <returns></returns>
        string BuildConnectionString(string host, string databaseName, string userName, string password);

        /// <summary>
        /// Executes an int-returning simple command
        /// </summary>
        /// <param name="context"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        int ExecuteCommand(Linq.DataContext context, string sql, params object[] parameters);

        /// <summary>
        /// Executes a stored procedure/function call
        /// </summary>
        /// <param name="context"></param>
        /// <param name="method"></param>
        /// <param name="sqlParams"></param>
        /// <returns></returns>
        IExecuteResult ExecuteMethodCall(Linq.DataContext context, MethodInfo method, params object[] sqlParams);

        /// <summary>
        /// Executes query. Stores matching columns in instance fields and properties.
        /// Does 2-pass (case sensitive then insensitive) match. 
        /// Handles null reference-type values (string, byte[])
        /// Handles null Nullable<T> value-type values (int? etc)
        /// Handles (for entity TResult) class, struct and Nullable<struct>
        /// Caches and re-uses compiled delegates (thread-safe)
        /// </summary>
        /// <typeparam name="TResult">Entity type whose instances are returned</typeparam>
        /// <param name="dataContext">Database to use</param>
        /// <param name="command">Server query returning table</param>
        /// <param name="parameters">query parameters</param>
        /// <returns>Entity with matching properties and fields filled</returns>
        IEnumerable<TResult> ExecuteQuery<TResult>(Linq.DataContext dataContext, string command, object[] parameters);

        /// <summary>
        /// Creates a parameter for use with IDbCommand.
        /// To be removed (should be useless)
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="dbTypeName"></param>
        /// <param name="paramName"></param>
        /// <returns></returns>
        IDbDataParameter CreateDbDataParameter(IDbCommand cmd, string dbTypeName, string paramName);

        #endregion

        #region SQL generator

        /// <summary>
        /// simple command to test round-trip functionality against DB:
        /// 'SELECT 11' or
        /// 'SELECT 11 FROM DUAL'
        /// </summary>
        string SqlPingCommand { get; }

        /// <summary>
        /// string concatenation, eg 'a||b' on Postgres 
        /// </summary>
        string GetSqlConcat(List<ExpressionAndType> parts);

        /// <summary>
        /// Returns a named parameter based on a given index
        /// This has to be an alphabetically orderable name
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        string GetOrderableParameterName(int index);

        /// <summary>
        /// Transform the name into the final parameter name
        /// and patch the SQL accordingly
        /// </summary>
        /// <param name="orderableName"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        string GetFinalParameterName(string orderableName);

        /// <summary>
        /// Patch the SQL according to the name parameter name
        /// </summary>
        /// <param name="orderableName"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        string ReplaceParamNameInSql(string orderableName, string sql);

        /// <summary>
        /// given 'User', return '[User]' to prevent a SQL keyword conflict
        /// </summary>
        string GetSqlFieldSafeName(string name);

        /// <summary>
        /// Returns a table/column in case safe expression, if required
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        string GetSqlCaseSafeName(string name);

        /// <summary>
        /// return 'LENGTH' on Oracle,Mysql,PostgreSql, return 'LEN' on MSSql
        /// </summary>
        string GetSqlStringLengthFunction();

        /// <summary>
        /// Generates the SQL request, based on provided parts
        /// </summary>
        /// <param name="parts">The request, expressed as objects</param>
        /// <returns></returns>
        string BuildSqlString(SqlExpressionParts parts);

        #endregion

        #region Insert / PK processor

        /// <summary>
        /// Determines if the current vendor/table can do bulk insert
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <returns></returns>
        bool CanBulkInsert<T>(Linq.Table<T> table);
        /// <summary>
        /// Sets the bulk insert capability for a given table
        /// If the vendor doesn't support bulk insert, then this method is ignored and the CanBulkInsert() method always return false.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <param name="pageSize"></param>
        void SetBulkInsert<T>(Linq.Table<T> table, int pageSize);
        /// <summary>
        /// Performs bulk insert.
        /// Please note that PKs may not be updated
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <param name="rows"></param>
        /// <param name="connection"></param>
        void DoBulkInsert<T>(Linq.Table<T> table, List<T> rows, IDbConnection connection);

        /// <summary>
        /// On Oracle, we have to insert a primary key manually.
        /// On MySql/Pgsql/Mssql, we use the IDENTITY clause to populate it automatically.
        /// </summary>
        IDbDataParameter ProcessPkField(IDbCommand cmd, ProjectionData projData, ColumnAttribute colAtt
            , StringBuilder sb, StringBuilder sbValues, StringBuilder sbIdentity, ref int numFieldsAdded);

        /// <summary>
        /// Gets generated PK from an insert command
        /// </summary>
        /// <param name="cmd1"></param>
        /// <param name="returnedId"></param>
        void ProcessInsertedId(IDbCommand cmd1, ref object returnedId);

        #endregion

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
        [Obsolete("Use MappingContext")]
        event EventHandler<ValueConversionEventArgs> ConvertValue;
    }
}

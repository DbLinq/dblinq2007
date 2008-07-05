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

#if MONO_STRICT
using DataContext=System.Data.Linq.DataContext;
using Data = System.Data;
#else
using DataContext = DbLinq.Data.Linq.DataContext;
using Data = DbLinq.Data;
#endif

namespace DbLinq.Vendor
{

#if MONO_STRICT
    internal
#else
    public
#endif
    class ValueConversionEventArgs : EventArgs
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
#if MONO_STRICT
    internal
#else
    public
#endif
 interface IVendor
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
        /// Executes a stored procedure/function call
        /// </summary>
        /// <param name="context"></param>
        /// <param name="method"></param>
        /// <param name="sqlParams"></param>
        /// <returns></returns>
        IExecuteResult ExecuteMethodCall(DataContext context, MethodInfo method, params object[] sqlParams);

        /// <summary>
        /// Creates a parameter for use with IDbCommand.
        /// To be removed (should be useless)
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="dbTypeName"></param>
        /// <param name="paramName"></param>
        /// <returns></returns>
        IDbDataParameter CreateDbDataParameter(IDbCommand cmd, string dbTypeName, string paramName);

        bool SupportsOutputParameter { get; }

        #endregion

        ISqlProvider SqlProvider { get; }

        #region SQL generator

        /// <summary>
        /// simple command to test round-trip functionality against DB:
        /// 'SELECT 11' or
        /// 'SELECT 11 FROM DUAL'
        /// </summary>
        string SqlPingCommand { get; }

        /// <summary>
        /// Returns a named parameter based on a given index
        /// This has to be an alphabetically orderable name
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        string GetOrderableParameterName(int index);

        /// <summary>
        /// given 'User', return '[User]' to prevent a SQL keyword conflict
        /// </summary>
        string GetSqlFieldSafeName(string name);

        /// <summary>
        /// Returns a case safe query, converting quoted names &lt;&ltMixedCaseName>> to "MixedCaseName"
        /// </summary>
        /// <param name="sqlString"></param>
        /// <returns></returns>
        string GetSqlCaseSafeQuery(string sqlString);

        #endregion

        #region Bulk Insert

        /// <summary>
        /// Determines if the current vendor/table can do bulk insert
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <returns></returns>
        bool CanBulkInsert<T>(Data.Linq.Table<T> table) where T : class;
        /// <summary>
        /// Sets the bulk insert capability for a given table
        /// If the vendor doesn't support bulk insert, then this method is ignored and the CanBulkInsert() method always return false.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <param name="pageSize"></param>
        void SetBulkInsert<T>(Data.Linq.Table<T> table, int pageSize) where T : class;
        /// <summary>
        /// Performs bulk insert.
        /// Please note that PKs may not be updated
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <param name="rows"></param>
        /// <param name="connection"></param>
        void DoBulkInsert<T>(Data.Linq.Table<T> table, List<T> rows, IDbConnection connection) where T : class;

        #endregion
    }
}

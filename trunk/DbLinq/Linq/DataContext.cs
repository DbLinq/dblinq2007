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
using System.Data;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using DBLinq.Linq.Connection;
using DBLinq.vendor;
using DBLinq.util;

namespace DBLinq.Linq
{
    public abstract class DataContext : IDisposable
    {
        //readonly List<MTable> tableList = new List<MTable>();//MTable requires 1 type arg
        private readonly List<IMTable> _tableList = new List<IMTable>();
        private System.IO.TextWriter _log;
        protected IVendor _vendor;

        readonly Dictionary<string, IMTable> _tableMap = new Dictionary<string, IMTable>();

        protected IConnectionProvider _connectionProvider;
        public IConnectionProvider ConnectionProvider { get { return _connectionProvider; } }

        public IDbConnection Connection { get { return _connectionProvider.Connection; } }
        public IVendor Vendor { get { return _vendor; } }

            // picrap: commented out this feature: we're going to be db independant
        //public DataContext(string sqlConnString)
        //{
        //    _sqlConnString = sqlConnString;
        //    _conn = new XSqlConnection(sqlConnString);
        //    _conn.Open();
        //}

        /// <summary>
        /// A DataContext opens and closes a database connection as needed 
        /// if you provide a closed connection or a connection string. 
        /// In general, you should never have to call Dispose on a DataContext. 
        /// If you provide an open connection, the DataContext will not close it
        /// source: http://msdn2.microsoft.com/en-us/library/bb292288.aspx
        /// </summary>
        /// <param name="dbConnection"></param>
        public DataContext(System.Data.IDbConnection dbConnection, IVendor vendor)
            : this(new DefaultConnectionProvider(dbConnection), vendor)
        {
            // picrap: removed this, we fall from the most specific cases to the most generic ones (IConnectionProvider)
            //if (dbConnection == null)
            //    throw new ArgumentNullException("Null db connection");
            //_conn = dbConnection as XSqlConnection;
            //_sqlConnString = dbConnection.ConnectionString;
            //try
            //{
            //    _conn.Open();
            //}
            //catch (Exception)
            //{
            //}
        }

        public DataContext(IConnectionProvider connectionProvider, IVendor vendor)
        {
            if (connectionProvider == null)
                throw new ArgumentNullException("Null connectionProvider");
            _connectionProvider = connectionProvider;
            _vendor = vendor;
        }

        //public XSqlConnection SqlConnection
        //{
        //    [DebuggerStepThrough]
        //    get { return _conn; }
        //}

        //public string SqlConnString { get { return _sqlConnString; } }

        public bool DatabaseExists()
        {
            try
            {
                using (new ConnectionManager(_connectionProvider.Connection))
                {
                    //command: "SELECT 11" (Oracle: "SELECT 11 FROM DUAL")
                    string SQL = _vendor.SqlPingCommand;
                    int result = _vendor.ExecuteCommand(this, SQL);
                    return result == 11;
                }
            }
            catch (Exception ex)
            {
                if (true)
                    Trace.WriteLine("DatabaseExists failed:" + ex);
                return false;
            }
        }

        public Table<T> GetTable<T>(string tableName) where T : IModified
        {
            IMTable tableExisting;
            lock (this)
            {
                if (_tableMap.TryGetValue(tableName, out tableExisting))
                    return tableExisting as Table<T>; //return existing
                Table<T> tableNew = new Table<T>(this); //create new and store it
                _tableMap[tableName] = tableNew;
                return tableNew;
            }
        }

        public void RegisterChild(IMTable table)
        {
            _tableList.Add(table);
        }

        public void SubmitChanges()
        {
            SubmitChanges(System.Data.Linq.ConflictMode.FailOnFirstConflict);
        }

        public virtual List<Exception> SubmitChanges(System.Data.Linq.ConflictMode failureMode)
        {
            List<Exception> exceptions = new List<Exception>();
            //TODO: perform all queued up operations - INSERT,DELETE,UPDATE
            //TODO: insert order must be: first parent records, then child records

            using (new ConnectionManager(_connectionProvider.Connection)) //ConnMgr will close connection for us

            //TranMgr may start transaction /  commit transaction
            using (TransactionManager transactionMgr = new TransactionManager(_connectionProvider.Connection, failureMode)) 
            {
                foreach (IMTable tbl in _tableList)
                {
                    try
                    {
                        List<Exception> innerExceptions = tbl.SaveAll(failureMode);
                        exceptions.AddRange(innerExceptions);
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine("Context.SubmitChanges failed: " + ex.Message);
                        switch (failureMode)
                        {
                            case System.Data.Linq.ConflictMode.ContinueOnConflict:
                                exceptions.Add(ex);
                                break;
                            case System.Data.Linq.ConflictMode.FailOnFirstConflict:
                                throw ex;
                        }
                    }
                }
                bool doCommit = failureMode == System.Data.Linq.ConflictMode.FailOnFirstConflict 
                    && exceptions.Count == 0;
                if(doCommit)
                    transactionMgr.Commit();
            }
            return exceptions;
        }

        #region Debugging Support
        /// <summary>
        /// Dlinq spec: Returns the query text of the query without of executing it
        /// </summary>
        /// <returns></returns>
        public string GetQueryText(IQueryable query)
        {
            if (query == null)
                return "GetQueryText: null query";
            IQueryText queryText1 = query as IQueryText;
            if (queryText1 != null)
                return queryText1.GetQueryText(); //so far, MTable_Projected has been updated to use this path

            return "ERROR L78 Unexpected type:" + query;
        }

        /// <summary>
        /// FA: Returns the text of SQL commands for insert/update/delete without executing them
        /// </summary>
        public string GetChangeText()
        {
            return "TODO L56 GetChangeText()";
        }

        /// <summary>
        /// debugging output
        /// </summary>
        public System.IO.TextWriter Log
        {
            get { return _log; }
            set { _log = value; }
        }

        #endregion

#if MYSQL || POSTGRES || SQLITE
        /// <summary>
        /// TODO - allow generated methods to call into stored procedures
        /// </summary>
        protected System.Data.Linq.IExecuteResult ExecuteMethodCall(DataContext context, System.Reflection.MethodInfo method, params object[] sqlParams)
        {
            using (new ConnectionManager(_connectionProvider.Connection))
            {
                System.Data.Linq.IExecuteResult result = _vendor.ExecuteMethodCall(context, method, sqlParams);
                return result;
            }
        }

#else
        //ExecuteMethodCall for Postgres, Oracle: coming later
#endif
        /// <summary>
        /// TODO: conflict detection is not implemented!
        /// </summary>
        [Obsolete("NOT IMPLEMENTED YET")]
        public System.Data.Linq.ChangeConflictCollection ChangeConflicts
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// use ExecuteCommand to call raw SQL
        /// </summary>
        public int ExecuteCommand(string command, params object[] parameters)
        {
            using (new ConnectionManager(_connectionProvider.Connection))
            {
                return _vendor.ExecuteCommand(this, command, parameters);
            }
        }

        /// <summary>
        /// Execute raw SQL query and return object
        /// </summary>
        [Obsolete("NOT IMPLEMENTED YET")]
        public System.Collections.Generic.IEnumerable<TResult> ExecuteQuery<TResult>(string command, params object[] parameters)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// TODO: DataLoadOptions ds = new DataLoadOptions(); ds.LoadWith<Customer>(p => p.Orders);
        /// </summary>
        [Obsolete("NOT IMPLEMENTED YET")]
        public System.Data.Linq.DataLoadOptions LoadOptions
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        [Obsolete("NOT IMPLEMENTED YET")]
        public System.Data.Common.DbTransaction Transaction
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public void Dispose()
        {
            //connection closing should not be done here.
            //read: http://msdn2.microsoft.com/en-us/library/bb292288.aspx
        }
    }

    /// <summary>
    /// Table has a SaveAll() method that Context needs to call
    /// </summary>
    public interface IMTable
    {
        List<Exception> SaveAll(System.Data.Linq.ConflictMode failureMode);
        void SaveAll();
    }

    /// <summary>
    /// TODO: can we retrieve _sqlString without requiring an interface?
    /// </summary>
    public interface IQueryText
    {
        string GetQueryText();
    }

}

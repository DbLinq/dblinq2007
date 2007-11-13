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

using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;

#if ORACLE
using System.Data.OracleClient;
using XSqlConnection = System.Data.OracleClient.OracleConnection;
using XSqlCommand = System.Data.OracleClient.OracleCommand;
#elif POSTGRES
using XSqlConnection = Npgsql.NpgsqlConnection;
using XSqlCommand = Npgsql.NpgsqlCommand;
#elif MICROSOFT
using System.Data.SqlClient;
using XSqlConnection = System.Data.SqlClient.SqlConnection;
using XSqlCommand = System.Data.SqlClient.SqlCommand;
#else
using MySql.Data.MySqlClient;
using XSqlConnection = MySql.Data.MySqlClient.MySqlConnection;
using XSqlCommand = MySql.Data.MySqlClient.MySqlCommand;
#endif

namespace DBLinq.Linq
{
    public abstract class MContext
    {
        //readonly List<MTable> tableList = new List<MTable>();//MTable requires 1 type arg
        readonly List<IMTable> _tableList = new List<IMTable>();
        System.IO.TextWriter _log;

        readonly string _sqlConnString;
        XSqlConnection _conn;

        readonly Dictionary<string, IMTable> _tableMap = new Dictionary<string, IMTable>();

        public MContext(string sqlConnString)
        {
            _sqlConnString = sqlConnString;
            _conn = new XSqlConnection(sqlConnString);
            _conn.Open();
        }

        public MContext(System.Data.IDbConnection dbConnection)
        {
            if (dbConnection == null)
                throw new ArgumentNullException("Null db connection");
            _conn = dbConnection as XSqlConnection;
            _sqlConnString = dbConnection.ConnectionString;
            try
            {
                _conn.Open();
            }
            catch (Exception)
            {
            }
        }

        public XSqlConnection SqlConnection
        {
            [DebuggerStepThrough]
            get { return _conn; }
        }

        public string SqlConnString { get { return _sqlConnString; } }

        [Obsolete("NOT IMPLEMENTED YET")]
        public bool DatabaseExists()
        {
            throw new NotImplementedException();
        }

        public MTable<T> GetTable<T>(string tableName) where T : IModified
        {
            IMTable tableExisting;
            if (_tableMap.TryGetValue(tableName, out tableExisting))
                return tableExisting as MTable<T>; //return existing
            MTable<T> tableNew = new MTable<T>(this); //create new and store it
            _tableMap[tableName] = tableNew;
            return tableNew;
        }

        public void RegisterChild(IMTable table)
        {
            _tableList.Add(table);
        }

        public void SubmitChanges()
        {
            //TODO: perform all queued up operations - INSERT,DELETE,UPDATE
            //TODO: insert order must be: first parent records, then child records
            foreach(IMTable tbl in _tableList)
            {
                tbl.SaveAll();
            }
        }

        /// <summary>
        /// TODO: Not implemented
        /// </summary>
        /// <param name="failureMode"></param>
        [Obsolete("NOT IMPLEMENTED YET")]
        public virtual void SubmitChanges(System.Data.Linq.ConflictMode failureMode)
        {
            throw new NotImplementedException();
        }

        #region Debugging Support
        /// <summary>
        /// Dlinq spec: Returns the query text of the query without of executing it
        /// </summary>
        /// <returns></returns>
        public string GetQueryText(IQueryable query)
        {
            if(query==null)
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

#if MYSQL || POSTGRES
        /// <summary>
        /// TODO - allow generated methods to call into stored procedures
        /// </summary>
        protected System.Data.Linq.IExecuteResult ExecuteMethodCall(MContext context, System.Reflection.MethodInfo method, params object[] sqlParams)
        {
            System.Data.Linq.IExecuteResult result = vendor.Vendor.ExecuteMethodCall(context, method, sqlParams);
            return result;
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
            return vendor.Vendor.ExecuteCommand(this, command, parameters);
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
    }

    /// <summary>
    /// MTable has a SaveAll() method that MContext needs to call
    /// </summary>
    public interface IMTable
    {
        void SaveAll();
    }

    /// <summary>
    /// TODO: can we retrieve _sqlString without requiring an interface?
    /// </summary>
    public interface IQueryText
    {
        string GetQueryText();
    }

    /// <summary>
    /// a callback that alows an outside method such as Max() or Count() modify SQL statement just before being executed
    /// </summary>
    public delegate void CustomExpressionHandler(SessionVars vars);

    public interface IGetModifiedEnumerator<T>
    {
        DBLinq.util.RowEnumerator<T> GetModifiedEnumerator(CustomExpressionHandler callback);
    }
}

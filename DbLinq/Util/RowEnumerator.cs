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
using System.Diagnostics;
using System.Reflection;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using DbLinq.Util;
using DbLinq.Linq;
using DbLinq.Linq.Clause;
using DbLinq.Vendor;

namespace DbLinq.Util
{
    /// <summary>
    /// class to read a row of data from MySqlDataReader, and package it into type T.
    /// It creates a SqlCommand and MySqlDataReader.
    /// </summary>
    /// <typeparam name="T">the type of the row object</typeparam>
    public class RowEnumerator<T> : IEnumerable<T>, IDisposable //IEnumerator<T>
        , IQueryText
    {
        protected SessionVarsParsed _vars;
        protected IDbConnection _conn;

        //while the FatalExecuteEngineError persists, we use a wrapper class to retrieve data
        protected Func<IDataRecord,T> _objFromRow2;

        //Type _sourceType;
        protected ProjectionData _projectionData;
        Dictionary<T,T> _liveObjectMap;
        internal string _sqlString;
        ConnectionManager _connectionManager;
        IDataReader _rdr;

        public RowEnumerator(SessionVarsParsed vars, Dictionary<T,T> liveObjectMap)
        {
            _vars = vars;

            //for [Table] objects only: keep objects (to save them if they get modified)
            _liveObjectMap = liveObjectMap; 

            _conn = vars.Context.Connection;
            _projectionData = vars.projectionData;
            if (_conn == null)
                throw new ApplicationException("Connection is null");

            //ConnectionManager remembers whether we need to close connection at the end
            _connectionManager = new ConnectionManager(_conn);

            CompileReaderFct();

            _sqlString = vars.sqlString;
        }

        protected virtual void CompileReaderFct()
        {
            _objFromRow2 = _vars.Context.DataMapper.GetMapper<T>(_vars);
        }

        public string GetQueryText(){ return _sqlString; }

        protected IDbCommand ExecuteSqlCommand(IDbConnection newConn, out IDataReader rdr2)
        {
            //prepend user prolog string, if any
            string sqlFull = DbLinq.Vendor.Settings.sqlStatementProlog + _sqlString;


            IDbCommand cmd = newConn.CreateCommand();
            cmd.CommandText = sqlFull;

            if (_vars._sqlParts != null)
            {
                foreach (string paramName in _vars._sqlParts.ParametersMap.Keys)
                {
                    object value = _vars._sqlParts.ParametersMap[paramName];
                    Trace.WriteLine("SQL PARAM: " + paramName + " = " + value);

                    IDbDataParameter parameter = cmd.CreateParameter();
                    parameter.ParameterName = paramName;
                    parameter.Value = value;

                    cmd.Parameters.Add(parameter);
                }
            }

            //Console.WriteLine("cmd.ExecuteCommand()");
            //XSqlDataReader _rdr = cmd.ExecuteReader();
            // picrap: right we should remove IDataReader2 (even if this this is hard work :))
            _rdr = cmd.ExecuteReader();
            // picrap: and also this
            //rdr2 = new DataReader2(_rdr);
            rdr2 = _vars.Context.Vendor.CreateDataReader(_rdr);

            // picrap: need to solve the HasRows mystery
            /*            if (_vars.context.Log != null)
                        {
                            int fields = _rdr.FieldCount;
                            string hasRows = _rdr.HasRows ? "rows: yes" : "rows: no";
                            _vars.context.Log.WriteLine("ExecuteSqlCommand numFields=" + fields + " " + hasRows);
                        }*/
            return cmd;
        }

        #region Dispose()
        public void Dispose()
        {
            //Dispose logic moved into the "yield return" loop
            Console.WriteLine("RowEnum.Dispose()");

            _connectionManager.Dispose();

            if (_rdr != null)
            {
                _rdr.Dispose();
                _rdr = null;
            }

            //if(cmd!=null){ 
            //    cmd.Dispose();
            //    cmd = null;
            //}
        }
        #endregion



        /// <summary>
        /// this is called during foreach
        /// </summary>
        public virtual IEnumerator<T> GetEnumerator()
        {
            if(_objFromRow2==null)
            {
                throw new ApplicationException("Internal error, missing _objFromRow compiled func");
            }

            IDataReader rdr2;

#if CREATE_NEW_CONNECTION
            //string origConnString = _conn.ConnectionString; //for MySql, cannot retrieve prev ConnStr
            //create a new connection to prevent error "SqlConnection already has SqlDataReader associated with it"
            XSqlConnection newConn = new XSqlConnection(_vars.context.SqlConnString);
            newConn.Open();
            //TODO: use connection pool instead of always opening a new one

            using( newConn )
#else
            //use this if you are not worried about "SqlConnection already has SqlDataReader associated with it"
            IDbConnection newConn = _conn; 
#endif

            using( IDbCommand cmd = ExecuteSqlCommand(newConn, out rdr2) )
            using( rdr2 )
            {
                //_current = default(T);
                while(rdr2.Read())
                {
                    if (rdr2.FieldCount == 0) // note to the below code author: could you check this modification validity?
                        continue;
//#if SQLITE
//                    // if not this might crash with SQLite
//                    //(only SqlLite implements HasRow?!)
//                    if (!rdr2.HasRow)
//                        continue;
//#endif

                    T current = _objFromRow2(rdr2);

                    //live object cache:
                    if(_liveObjectMap!=null && current!=null)
                    {
                        //TODO: given object's ID, try to retrieve an existing cached object
                        //_rowCache.Add(_current); //store so we can save if modified
                        T previousObj;
                        //rowCache uses Order.OrderId as key (uses Order.GetHashCode and .Equals)
                        bool contains = _liveObjectMap.TryGetValue(current,out previousObj);
                        if(contains)
                        {
                            //discard data from DB, return previously loaded instance
                            current = previousObj; 
                        }
                        _liveObjectMap[current] = current;
                    }

                    //Error: Cannot yield a value in the body of a try block with a catch clause
                    yield return current;
                }
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #region IsBuiltinType(), IsColumnType(), IsProjection()

        //IsGroupBy: derived class returns true
        public virtual bool IsGroupBy(){ return false; }

        #endregion
    }
}

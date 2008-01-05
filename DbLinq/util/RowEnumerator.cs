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
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Data.Linq;
using System.Data.Linq.Mapping;

#if ORACLE
using System.Data.OracleClient;
using XSqlCommand = System.Data.OracleClient.OracleCommand;
using XSqlDataReader = System.Data.OracleClient.OracleDataReader;
using XSqlConnection = System.Data.OracleClient.OracleConnection;
#elif POSTGRES
using Npgsql;
using XSqlCommand = Npgsql.NpgsqlCommand;
using XSqlDataReader = Npgsql.NpgsqlDataReader;
using XSqlConnection = Npgsql.NpgsqlConnection;
#elif MICROSOFT
using System.Data.SqlClient;
using XSqlConnection = System.Data.SqlClient.SqlConnection;
using XSqlCommand = System.Data.SqlClient.SqlCommand;
using XSqlDataReader = System.Data.SqlClient.SqlDataReader;
#else
using MySql.Data.MySqlClient;
using XSqlCommand = MySql.Data.MySqlClient.MySqlCommand;
using XSqlDataReader = MySql.Data.MySqlClient.MySqlDataReader;
using XSqlConnection = MySql.Data.MySqlClient.MySqlConnection;
#endif
using DBLinq.util;
using DBLinq.Linq;
using DBLinq.Linq.clause;

namespace DBLinq.util
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
        protected XSqlConnection _conn;

        //while the FatalExecuteEngineError persists, we use a wrapper class to retrieve data
        protected Func<DataReader2,T> _objFromRow2;

        //Type _sourceType;
        protected ProjectionData _projectionData;
        Dictionary<T,T> _liveObjectMap;
        internal string _sqlString;


        public RowEnumerator(SessionVarsParsed vars, Dictionary<T,T> liveObjectMap)
        {
            _vars = vars;

            //for [Table] objects only: keep objects (to save them if they get modified)
            _liveObjectMap = liveObjectMap; 

            _conn = vars.context.SqlConnection;
            _projectionData = vars.projectionData;
            if(_conn==null || _conn.State!=ConnectionState.Open)
                throw new ApplicationException("Connection is not open");

            CompileReaderFct();

            _sqlString = vars.sqlString;
        }

        protected virtual void CompileReaderFct()
        {
            int fieldID = 0;
            _objFromRow2 = RowEnumeratorCompiler<T>.CompileRowDelegate(_vars, ref fieldID);
        }

        public string GetQueryText(){ return _sqlString; }

        protected XSqlCommand ExecuteSqlCommand(XSqlConnection newConn, out DataReader2 rdr2)
        {
            //prepend user prolog string, if any
            string sqlFull = DBLinq.vendor.Settings.sqlStatementProlog + _sqlString;

            XSqlCommand cmd = new XSqlCommand(sqlFull, newConn);

            if(_vars._sqlParts!=null)
            {
                foreach(string paramName in _vars._sqlParts.paramMap.Keys){
                    object value = _vars._sqlParts.paramMap[paramName];
                    Console.WriteLine("SQL PARAM: "+paramName+" = "+value);
#if MICROSOFT
                    cmd.Parameters.AddWithValue(paramName, value);
#else
                    cmd.Parameters.Add(paramName, value); //warning CS0618: Add is obsolete:
#endif
                }
            }

            //Console.WriteLine("cmd.ExecuteCommand()");
            XSqlDataReader _rdr = cmd.ExecuteReader();
            rdr2 = new DataReader2(_rdr);

            if (_vars.context.Log != null)
            {
                int fields = _rdr.FieldCount;
                string hasRows = _rdr.HasRows ? "rows: yes" : "rows: no";
                _vars.context.Log.WriteLine("ExecuteSqlCommand numFields=" + fields + " " + hasRows);
            }
            return cmd;
        }

        #region Dispose()
        public void Dispose()
        {
            //Dispose logic moved into the "yield return" loop
            Console.WriteLine("RowEnum.Dispose()");
            //if(_rdr!=null){ 
            //    _rdr.Close();
            //    _rdr = null;
            //}
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

            //string origConnString = _conn.ConnectionString; //for MySql, cannot retrieve prev ConnStr
            //create a new connection to prevent error "SqlConnection already has SqlDataReader associated with it"
            XSqlConnection newConn = new XSqlConnection(_vars.context.SqlConnString);
            newConn.Open();
            //TODO: use connection pool instead of always opening a new one

            DataReader2 rdr2;
            using( newConn )
            using( XSqlCommand cmd = ExecuteSqlCommand(newConn, out rdr2) )
            using( rdr2 )
            {
                //_current = default(T);
                while(rdr2.Read())
                {
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

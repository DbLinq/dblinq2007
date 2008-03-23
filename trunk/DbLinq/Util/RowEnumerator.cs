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
using System.Data;
using System.Collections.Generic;
using DbLinq.Factory;
using DbLinq.Logging;
using DbLinq.Util;
using DbLinq.Linq;

namespace DbLinq.Util
{
    /// <summary>
    /// class to read a row of data from MySqlDataReader, and package it into type T.
    /// It creates a SqlCommand and MySqlDataReader.
    /// </summary>
    /// <typeparam name="T">the type of the row object</typeparam>
    public class RowEnumerator<T> : IEnumerable<T>, IQueryText
    {
        public ILogger Logger { get; set; }

        protected SessionVarsParsed _vars;

        //while the FatalExecuteEngineError persists, we use a wrapper class to retrieve data
        protected Func<IDataRecord, T> _objFromRow2;

        private Dictionary<T, T> _liveObjectMap;
        private string _sqlString;

        public RowEnumerator(SessionVarsParsed vars, Dictionary<T, T> liveObjectMap)
        {
            Logger = ObjectFactory.Get<ILogger>();

            _vars = vars;

            //for [Table] objects only: keep objects (to save them if they get modified)
            _liveObjectMap = liveObjectMap;

            CompileReaderFct();

            _sqlString = vars.SqlString;
        }

        protected virtual void CompileReaderFct()
        {
            _objFromRow2 = _vars.Context.ResultMapper.GetMapper<T>(_vars);
        }

        public string GetQueryText() { return _sqlString; }

        protected IDbCommand ExecuteSqlCommand(out IDataReader dataReader)
        {
            //prepend user prolog string, if any
            string sqlFull = _vars.sqlProlog + _sqlString;

            IDbCommand cmd = _vars.Context.DatabaseContext.CreateCommand();
            cmd.CommandText = sqlFull;

            if (_vars.SqlParts != null)
            {
                foreach (string paramName in _vars.SqlParts.ParametersMap.Keys)
                {
                    object value = _vars.SqlParts.ParametersMap[paramName];
                    Trace.WriteLine("SQL PARAM: " + paramName + " = " + value);

                    IDbDataParameter parameter = cmd.CreateParameter();
                    parameter.ParameterName = paramName;
                    parameter.Value = value;

                    cmd.Parameters.Add(parameter);
                }
            }

            //toncho11: http://code.google.com/p/dblinq2007/issues/detail?id=24
            QuotesHelper.AddQuotesToQuery(cmd);

            dataReader = cmd.ExecuteReader();

            return cmd;
        }

        /// <summary>
        /// this is called during foreach
        /// </summary>
        public virtual IEnumerator<T> GetEnumerator()
        {
            if (_objFromRow2 == null)
            {
                throw new ApplicationException("Internal error, missing _objFromRow compiled func");
            }

            IDataReader dataReader;
            using (_vars.Context.DatabaseContext.OpenConnection())

            using (IDbCommand cmd = ExecuteSqlCommand(out dataReader))
            using (dataReader)
            {
                //_current = default(T);
                while (dataReader.Read())
                {
                    if (dataReader.FieldCount == 0)
                        // note to the below code author: could you check this modification validity?
                        continue;
                    //#if SQLITE
                    //                    // if not this might crash with SQLite
                    //                    //(only SqlLite implements HasRow?!)
                    //                    if (!rdr2.HasRow)
                    //                        continue;
                    //#endif

                    T current = _objFromRow2(dataReader);

                    //live object cache:
                    if (_liveObjectMap != null && current != null)
                    {
                        //TODO: given object's ID, try to retrieve an existing cached object
                        //_rowCache.Add(_current); //store so we can save if modified
                        T previousObj;
                        //rowCache uses Order.OrderId as key (uses Order.GetHashCode and .Equals)
                        bool contains = _liveObjectMap.TryGetValue(current, out previousObj);
                        if (contains)
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
        public virtual bool IsGroupBy() { return false; }

        #endregion
    }
}

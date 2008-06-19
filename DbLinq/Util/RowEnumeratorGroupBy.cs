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
using System.Collections.Generic;
using System.Linq;
using DbLinq.Logging;
using DbLinq.Util;
using DbLinq.Linq;

namespace DbLinq.Util
{
    /// <summary>
    /// class to read a row of data from MySqlDataReader, and package it into type T.
    /// It creates a SqlCommand and MySqlDataReader.
    /// 
    /// Differs from simple RowEnumerator:
    /// a single call to GetEnumerator reads all rows into a Lookup structure.
    /// </summary>
    /// <typeparam name="Key">the type of the key object</typeparam>
    /// <typeparam name="Grp">the type of the IGrouping object (Lookup)</typeparam>
    /// <typeparam name="T">the return type, containing an IGrouping</typeparam>
    public class RowEnumeratorGroupBy<T, Key, Val> : RowEnumerator<T>
    {
        Func<IDataRecord, MappingContext, Key> _keyReadFunc = null;
        Func<IDataRecord, MappingContext, Val> _valReadFunc = null;

        public RowEnumeratorGroupBy(SessionVarsParsed vars)
            : base(vars)
        {
        }

        protected override void CompileReaderFct()
        {
            //bool doSelectKey = false;
            //if(_vars.groupByNewExpr==null && _vars.selectExpr==null)
            //    doSelectKey = true;

            try
            {
                int fieldID = 0;
                //always load key from SQL:
                _keyReadFunc = RowEnumeratorCompiler<Key>.CompilePrimitiveRowDelegate(ref fieldID);
                _valReadFunc = RowEnumeratorCompiler<Val>.CompileRowDelegate(_vars, ref fieldID);
            }
            catch (Exception ex)
            {
                Logger.Write(Level.Error, "CompileRowDelegate failed: " + ex);
                throw;
            }
        }


        /// <summary>
        /// this is called during foreach
        /// </summary>
        public override IEnumerator<T> GetEnumerator()
        {
            //string origConnString = _conn.ConnectionString;
            //create a new connection to prevent error "SqlConnection already has SqlDataReader associated with it"

            //XSqlConnection newConn = new XSqlConnection(_vars.context.SqlConnString);
            //newConn.Open();
            //TODO: use connection pool instead of always opening a new one

            IDataReader dataReader;
            using (_vars.Context.DatabaseContext.OpenConnection())
            using (IDbCommand cmd = ExecuteSqlCommand(out dataReader))
            using (dataReader)
            {

                //rowObjFunc: given current lookup, produce return obj
                Func<IGrouping<Key, Val>, T> rowObjFunc = null;
                if (typeof(T) == typeof(IGrouping<Key, Val>))
                {
                    //rowObjFunc = l => l; //for simplest GroupBy, just return the Lookup obj.
                    Func<IGrouping<Key, Val>, IGrouping<Key, Val>> rowFuncXXX = l => l;
                    object objXXX = rowFuncXXX;
                    rowObjFunc = (Func<IGrouping<Key, Val>, T>)objXXX;
                }

                //assumption: there is no Read() loop around this code

                Key prevKey = default(Key); //keyReadFunc(rdr);
                Lookup<Key, Val> lookup = null;

                while (dataReader.Read())
                {
                    if (lookup == null)
                    {
                        prevKey = _keyReadFunc(dataReader, _vars.Context.MappingContext);
                        Val firstVal = _valReadFunc(dataReader, _vars.Context.MappingContext); // valueReadFunc(rdr2);
                        lookup = new Lookup<Key, Val>(prevKey, firstVal);
                        continue;
                    }

                    Key currKey = _keyReadFunc(dataReader, _vars.Context.MappingContext);
                    Val currVal = _valReadFunc(dataReader, _vars.Context.MappingContext);
                    if (currKey.Equals(prevKey))
                    {
                        lookup._elements.Add(currVal);
                    }
                    else
                    {
                        yield return rowObjFunc(lookup);
                        lookup = new Lookup<Key, Val>(currKey, currVal);
                    }
                }

                if (lookup != null)
                {
                    yield return rowObjFunc(lookup);
                    //yield return lookup;
                }

            } //Dispose reader, sqlCommand
        }

        //IsGroupBy: base class returns false, derived class returns true
        public override bool IsGroupBy() { return true; }

    }
}

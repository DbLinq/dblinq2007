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

using DBLinq.Util;
using DBLinq.Linq;
using DBLinq.Linq.Clause;
using DBLinq.Vendor;

namespace DBLinq.Util
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
        Func<IDataReader2,Key> _keyReadFunc = null;
        Func<IDataReader2,Val> _valReadFunc = null;

        public RowEnumeratorGroupBy(SessionVarsParsed vars)
            :base(vars,null)
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
            catch(Exception ex)
            {
                Console.WriteLine("CompileRowDelegate failed: "+ex);
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

            IDataReader2 rdr2;
            using( new ConnectionManager(_conn) )
            using( IDbCommand cmd = ExecuteSqlCommand(_conn, out rdr2) )
            using( rdr2 )
            {

                //rowObjFunc: given current lookup, produce return obj
                Func<IGrouping<Key,Val> ,T> rowObjFunc = null;
                if( typeof(T)==typeof(IGrouping<Key,Val>) )
                {
                    //rowObjFunc = l => l; //for simplest GroupBy, just return the Lookup obj.
                    Func<IGrouping<Key,Val>,IGrouping<Key,Val>> rowFuncXXX = l => l;
                    object objXXX = rowFuncXXX;
                    rowObjFunc = (Func<IGrouping<Key,Val> ,T>)objXXX;
                }

                //assumption: there is no Read() loop around this code

                Key prevKey = default(Key); //keyReadFunc(rdr);
                Lookup<Key,Val> lookup = null;

                while(rdr2.Read())
                {
                    if(lookup==null)
                    {
                        prevKey = _keyReadFunc(rdr2);
                        Val firstVal = _valReadFunc(rdr2); // valueReadFunc(rdr2);
                        lookup=new Lookup<Key,Val>(prevKey, firstVal);
                        continue;
                    }

                    Key currKey = _keyReadFunc(rdr2);
                    Val currVal = _valReadFunc(rdr2);
                    if(currKey.Equals(prevKey)){
                        lookup._elements.Add(currVal);
                    } else {
                        yield return rowObjFunc(lookup);
                        lookup = new Lookup<Key,Val>(currKey,currVal);
                    }
                }

                if(lookup!=null){
                    yield return rowObjFunc(lookup);
                    //yield return lookup;
                }

            } //Dispose reader, sqlCommand

        }

        //IsGroupBy: base class returns false, derived class returns true
        public override bool IsGroupBy(){ return true; }

    }
}

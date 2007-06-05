////////////////////////////////////////////////////////////////////
//Initial author: Jiri George Moudry, 2006.
//License: LGPL. (Visit http://www.gnu.org)
//Commercial code may call into this library, if it's in a different module (DLL)
////////////////////////////////////////////////////////////////////

using System;
#if LINQ_PREVIEW_2006
//Visual Studio 2005 with Linq Preview May 2006 - can run on Win2000
using System.Query;
using System.Expressions;
#else
//Visual Studio Orcas - requires WinXP
using System.Linq;
using System.Linq.Expressions;
#endif

using System.Reflection;
using System.Data;
//using System.Data.DLinq;
using System.Collections.Generic;
using System.Text;

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
    /// <typeparam name="Key">the type of the key object</typeparam>
    /// <typeparam name="Grp">the type of the IGrouping object (Lookup)</typeparam>
    /// <typeparam name="T">the return type, containing an IGrouping</typeparam>
    public class RowEnumeratorGroupBy<T, Key, Val> : RowEnumerator<T>
    {
        Func<DataReader2,Key> _keyReadFunc = null;
        Func<DataReader2,Val> _valReadFunc = null;

        public RowEnumeratorGroupBy(SessionVars vars)
            :base(vars,null)
        {
            //try
            //{
            //    init(vars);
            //}
            //catch(Exception ex)
            //{
            //    Console.WriteLine("Failed:"+ex);
            //    throw;
            //}
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
            XSqlConnection newConn = new XSqlConnection(_vars.context.SqlConnString);
            newConn.Open();
            //TODO: use connection pool instead of always opening a new one

            DataReader2 rdr2;
            using( newConn )
            using( XSqlCommand cmd = ExecuteSqlCommand(newConn, out rdr2) )
            using( rdr2 )
            {

                //rowObjFunc: given current lookup, prouced return obj
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

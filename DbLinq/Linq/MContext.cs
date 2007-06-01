////////////////////////////////////////////////////////////////////
//Initial author: Jiri George Moudry, 2006.
//License: LGPL. (Visit http://www.gnu.org)
//Commercial code may call into this library, if it's in a different module (DLL)
////////////////////////////////////////////////////////////////////

using System;
using System.Query;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Text;

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
        //internal static bool s_suppressSqlExecute = false;
        System.IO.TextWriter _log;

        readonly string _sqlConnString;
        XSqlConnection _conn;
        public MContext(string sqlConnString)
        {
            _sqlConnString = sqlConnString;
            _conn = new XSqlConnection(sqlConnString);
            _conn.Open();
        }

        public XSqlConnection SqlConnection
        {
            [DebuggerStepThrough]
            get { return _conn; }
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

            //s_suppressSqlExecute = true; //TODO: get rid of this boolean flag
            //IEnumerator rowEnum1 = query.GetEnumerator();
            //s_suppressSqlExecute = false;
            ////MySql.util.RowEnumerator<T> rowEnum2 = rowEnum1 as MySql.util.RowEnumerator<T>;
            //IQueryText rowEnum2 = rowEnum1 as IQueryText;
            //if(rowEnum2==null)
            //    return "ERROR L78 Unexpected type:"+rowEnum1;

            //string queryText = rowEnum2.GetQueryText();
            //return queryText;
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

////////////////////////////////////////////////////////////////////
//Initial author: Jiri George Moudry, 2006.
//License: LGPL. (Visit http://www.gnu.org)
//Commercial code may call into this library, if it's in a different module (DLL)
////////////////////////////////////////////////////////////////////

using System;
using System.Query;
using System.Reflection;
using System.Expressions;
using System.Data;
using System.Data.DLinq;
using System.Collections.Generic;
using System.Text;
#if ORACLE
using System.Data.OracleClient;
using XSqlCommand = System.Data.OracleClient.OracleCommand;
#elif POSTGRES
using Npgsql;
using XSqlCommand = Npgsql.NpgsqlCommand;
#else
using MySql.Data.MySqlClient;
using XSqlCommand = MySql.Data.MySqlClient.MySqlCommand;
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
    public class RowEnumerator<T> : IEnumerator<T>, IQueryText
    {
        SessionVars _vars;
#if ORACLE
        OracleConnection _conn;
        //OracleCommand _cmd;
        OracleDataReader _rdr;
#elif POSTGRES
        NpgsqlConnection _conn;
        //NpgsqlCommand _cmd;
        NpgsqlDataReader _rdr;
#else
        MySqlConnection _conn;
        //MySqlCommand _cmd;
        MySqlDataReader _rdr;
#endif
        XSqlCommand _cmd;

        T _current;

#if USE_MYSQLREADER_DIRECTLY
        //when Microsoft fixes FatalExecuteEngineError (crash when assembling nullables), we can use MySqlDataReader directly
        Func<MySqlDataReader,T> _objFromRow2;
        MySqlDataReader _rdr2;
#else
        //while the FatalExecuteEngineError persists, we use a wrapper class to retrieve data
        Func<DataReader2,T> _objFromRow2;
        DataReader2 _rdr2;
#endif
        Type _sourceType;
        ProjectionData _projectionData;
        //ICollection<T> _rowCache;
        Dictionary<T,T> _rowCache;
        internal readonly string _sqlString;


        public RowEnumerator(SessionVars vars, Dictionary<T,T> rowCache)
        {
            _vars = vars;
            _rowCache = rowCache; //for [Table] objects only: keep objects (to save them if they get modified)
            _sourceType = vars.sourceType;
            _conn = vars.context.SqlConnection;
            _projectionData = vars.projectionData;
            if(_conn==null || _conn.State!=ConnectionState.Open)
                throw new ApplicationException("Connection is not open");


            //three categories to handle:
            //A) extract object of primitive / builtin type (eg. string or int)
            //B) extract column object, which will be 'newed' and then tracked for changes
            //C) extract a projection object, using default ctor and bindings, no tracking needed.
            bool isBuiltinType = CSharp.IsPrimitiveType(typeof(T));
            bool isColumnType  = IsColumnType();
            bool isProjectedType = IsProjection();

            if(_projectionData==null && !isBuiltinType)
            {
                //for Table types, use attributes to determine fields
                //for projection types, return projData with only ctor assigned
                _projectionData = ProjectionData.FromDbType(typeof(T));
            }

            if(isBuiltinType)
            {
                _objFromRow2 = RowEnumeratorCompiler<T>.CompilePrimitiveRowDelegate();
            }
            else if(isColumnType)
            {
                _objFromRow2 = RowEnumeratorCompiler<T>.CompileColumnRowDelegate(_projectionData);
            }
            else if(isProjectedType && vars.groupByExpr!=null)
            {
                //now we know what the GroupBy object is, 
                //and what method to use with grouping (eg Count())
                //_projectionData.type = typeof(T);
                //vars._sqlParts.selectFieldList.Add("Count(*)");

                ProjectionData projData2 = ProjectionData.FromReflectedType(typeof(T));

                //and compile the sucker
                _objFromRow2 = RowEnumeratorCompiler<T>.CompileProjectedRowDelegate(vars, projData2);
            }
            else if(isProjectedType)
            {
                _objFromRow2 = RowEnumeratorCompiler<T>.CompileProjectedRowDelegate(vars, _projectionData);
            }
            else
            {
                throw new ApplicationException("L176: RowEnumerator can handle basic types or projected types, but not "+typeof(T));
            }

            //eg. '$p' for user query "from p in db.products"
            if(vars._sqlParts.IsEmpty())
            {
                //occurs when there no Where or Select expression, eg. 'from p in Products select p'
                //select all fields of target type:
                string varName = _vars.GetDefaultVarName(); //'$x'
                FromClauseBuilder.SelectAllFields(vars, vars._sqlParts,typeof(T),varName);
                //PS. Should _sqlParts not be empty here? Debug Clone() and AnalyzeLambda()
            }

            string sql = vars._sqlParts.ToString();

            if(vars.orderByExpr!=null)
            {
                //TODO: don't look at C# field name, retrieve SQL field name from attrib
                Expression body = vars.orderByExpr.Body;
                MemberExpression member = body as MemberExpression;
                if(member!=null)
                {
                    sql += " ORDER BY " + member.Member.Name;
                    if(vars.orderBy_desc!=null)
                    {
                        sql += " " + vars.orderBy_desc;
                    }
                }
            }

            if(vars.limitClause!=null)
            {
                sql += " " + vars.limitClause;
            }

            _sqlString = sql;
            Console.WriteLine("SQL: "+_sqlString);
        }

        public string GetQueryText(){ return _sqlString; }

        public void ExecuteSqlCommand()
        {
            _cmd = new XSqlCommand(_sqlString,_conn);

            if(_vars._sqlParts!=null)
            {
                foreach(string paramName in _vars._sqlParts.paramMap.Keys){
                    object value = _vars._sqlParts.paramMap[paramName];
                    Console.WriteLine("SQL PARAM: "+paramName+" = "+value);
                    _cmd.Parameters.Add(paramName,value);
                }
            }

            Console.WriteLine("_cmd.ExecuteCommand()");
            _rdr = _cmd.ExecuteReader();
#if USE_MYSQLREADER_DIRECTLY
            _rdr2 = _rdr;
#else
            _rdr2 = new DataReader2(_rdr);
#endif
            int fields = _rdr.FieldCount;
            string hasRows = _rdr.HasRows ? "rows: yes" : "rows: no";
            Console.WriteLine("ExecuteSqlCommand numFields="+fields+ " "+hasRows);
        }

        #region Dispose()
        public void Dispose()
        {
            if(_rdr!=null){ 
                _rdr.Close();
                _rdr = null;
            }
            if(_cmd!=null){ 
                _cmd.Dispose();
                _cmd = null;
            }
        }
        #endregion

        public object Current 
        { 
            get { return _current; } 
        }

        /// <summary>
        /// this is called during foreach
        /// </summary>
        T IEnumerator<T>.Current 
        { 
            get { return _current; }
        }

        public void Reset()
        {
            throw new ApplicationException("TODO: reset MySqlDataReader");
        }

        /// <summary>
        /// this is called during foreach
        /// </summary>
        public bool MoveNext()
        {
            _current = default(T);
            bool hasNext = _rdr.Read();
            //Console.WriteLine("RowEnumerator.MoveNext returned "+hasNext);
            if(hasNext){
                try {
                    if(_objFromRow2!=null){
                        _current = _objFromRow2(_rdr2);

                        if(_rowCache!=null && _current!=null)
                        {
                            //TODO: given object's ID, try to retrieve an existing cached object
                            //_rowCache.Add(_current); //store so we can save if modified
                            T previousObj;
                            //rowCache uses Order.OrderId as key (uses Order.GetHashCode and .Equals)
                            bool contains = _rowCache.TryGetValue(_current,out previousObj);
                            if(contains)
                            {
                                //discard data from DB, return previously loaded instance
                                _current = previousObj; 
                            }
                            _rowCache[_current] = _current;
                        }
                    }
                    else
                        Console.WriteLine("L149 Error: Null _objFromRow");
                } catch(Exception ex){
                    Console.WriteLine("RowEnumerator.ObjFromRow failed:"+ex);
                }
            }
            return hasNext;
        }

        #region IsBuiltinType(), IsColumnType(), IsProjection()


        /// <summary>
        /// if our type has the [Table] attribute, it's a column type
        /// </summary>
        bool IsColumnType()
        {
            if(_vars.projectionData==null)
            {
                TableAttribute tAttrib1 = AttribHelper.GetTableAttrib(typeof(T));
                return (tAttrib1!=null);
            }
            
            if(_vars.groupByExpr!=null)
                return false;

            TableAttribute tAttrib = _vars.projectionData.tableAttribute;
            return tAttrib!=null; //
        }

        /// <summary>
        /// a projection has only a default ctor and some fields.
        /// A projected class is generated by the compiler and has a name like $__proj4.
        /// </summary>
        bool IsProjection()
        {
            Type t = typeof(T);
            ConstructorInfo[] cinfo = t.GetConstructors();
            if(cinfo.Length!=1)
                return false;
            if(cinfo[0].GetParameters().Length!=0)
                return false;
            return true;
        }
        #endregion
    }
}

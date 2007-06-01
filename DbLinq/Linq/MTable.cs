////////////////////////////////////////////////////////////////////
//Initial author: Jiri George Moudry, 2006.
//License: LGPL. (Visit http://www.gnu.org)
//Commercial code may call into this library, if it's in a different module (DLL)
////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Query;
using System.Data.DLinq;
using System.Expressions;
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
using XSqlParameter = System.Data.SqlClient.SqlParameter;
#else
using MySql.Data.MySqlClient;
using XSqlConnection = MySql.Data.MySqlClient.MySqlConnection;
using XSqlCommand = MySql.Data.MySqlClient.MySqlCommand;
#endif
using DBLinq.Linq.clause;
using DBLinq.util;

namespace DBLinq.Linq
{
    /// <summary>
    /// T may be eg. class Employee or string - the output
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MTable<T> : 
        IQueryable<T>
        , IOrderedQueryable<T> //this is cheating ... we pretend to be always ordered
        , IMTable
        , IGetModifiedEnumerator<T>
        , IQueryText
    {
        /// <summary>
        /// the parent MContext holds our connection etc
        /// </summary>
        MContext _parentDB;
        readonly List<T> _insertList = new List<T>();
        readonly Dictionary<T,T> _liveObjectMap = new Dictionary<T,T>();
        readonly List<T> _deleteList = new List<T>();

        //SqlExpressionParts _sqlParts = new SqlExpressionParts();
        SessionVars _vars = new SessionVars();

        public MTable(MContext parent)
        {
            _parentDB = parent;
            _parentDB.RegisterChild(this);
            _vars.context = this._parentDB;
            _vars.sourceType = typeof(T);
            _vars.log = parent.Log;
            //_vars._sqlParts = _sqlParts;
        }

        /// <summary>
        /// this is used when we call CreateQuery to create a copy of orig table object
        /// </summary>
        public MTable(MTable<T> parent, SessionVars vars)
        {
            _insertList = parent._insertList;
            _liveObjectMap = parent._liveObjectMap;
            _deleteList = parent._deleteList;
            _parentDB = parent._parentDB;
            _vars = vars;
        }

        /// <summary>
        /// 'S' is the projected type. If you say 'from e in Employees select e.ID', then type S will be int.
        /// If you say 'select new {e.ID}', then type S will be something like Projection.f__1
        /// </summary>
        public IQueryable<S> CreateQuery<S>(Expression expr)
        {
            //Log1.Info("MTable.CreateQuery: "+expr);
            if (_parentDB.Log != null)
            {
                _vars.log = _parentDB.Log;
                _parentDB.Log.WriteLine("MTable.CreateQuery: " + expr);
            }

            SessionVars vars = _vars.Clone();
            vars.StoreQuery(expr);
            
            if(this is IQueryable<S>)
            {
                //this occurs if we are not projecting
                //(meaning that we are selecting entire row object)
                //IQueryable<S> this_S = (IQueryable<S>)this; 
                //return this_S;
                MTable<T> clonedThis = new MTable<T>(this, vars);
                IQueryable<S> this_S = (IQueryable<S>)clonedThis; 
                return this_S;
            } else {
                //if we get here, we are projecting.
                //(eg. you select only a few fields: "select name from employee")
                vars.createQueryExpr = expr;
                MTable_Projected<S> projectedQ = new MTable_Projected<S>(vars);
                return projectedQ;
            }
        }

        /// <summary>
        /// the query '(from o in Orders select o).First()' enters here
        /// </summary>
        public S Execute<S>(Expression expression)
        {
            Log1.Info("MTable.Execute<"+typeof(S)+">: "+expression);
            return new RowScalar<T>(_vars, this).GetScalar<S>(expression);
        }

        /// <summary>
        /// entry point for 'foreach' statement.
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            QueryProcessor.ProcessLambdas(_vars, typeof(T));
            RowEnumerator<T> rowEnumerator = new RowEnumerator<T>(_vars, _liveObjectMap);
            //if(MContext.s_suppressSqlExecute)
            //{
            //    //we are doing GetQueryText
            //} else {
            //    //rowEnumerator.ExecuteSqlCommand();
            //}
            return rowEnumerator.GetEnumerator();
        }

        /// <summary>
        /// GetEnumerator where you can inject an extra clause, eg. "LIMIT 2" or an extra where
        /// </summary>
        /// <param name="fct"></param>
        public RowEnumerator<T> GetModifiedEnumerator(CustomExpressionHandler fct)
        {
            SessionVars vars2 = _vars.Clone();
            fct(vars2);
            QueryProcessor.ProcessLambdas(vars2, typeof(T));
            RowEnumerator<T> rowEnumerator = new RowEnumerator<T>(vars2, _liveObjectMap);
            //rowEnumerator.ExecuteSqlCommand();
            return rowEnumerator;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            IEnumerator<T> enumT = GetEnumerator();
            return enumT;
        }

        public Type ElementType { 
            get {
                throw new ApplicationException("Not implemented");
            }
        }
        public Expression Expression { 
            //copied from RdfProvider
            get { return Expression.Constant(this); }
        }
 

        public IQueryable CreateQuery(Expression expression)
        {
            throw new ApplicationException("Not implemented");
        }

        public object Execute(Expression expression)
        {
            throw new ApplicationException("Not implemented");
        }

        public void Add(T newObject)
        {
            _insertList.Add(newObject);
        }
        public void Remove(T objectToDelete)
        {
            //TODO: queue an object for SQL DELETE
            _deleteList.Add(objectToDelete);
        }
        public void SaveAll()
        {
            //TODO: process deleteList, insertList, liveObjectList
            if(_insertList.Count==0 && _liveObjectMap.Count==0 && _deleteList.Count==0)
                return; //nothing to do
            //object[] indices = new object[0];
            ProjectionData proj = ProjectionData.FromDbType(typeof(T));
            XSqlConnection conn = _parentDB.SqlConnection;
            foreach(T obj in _insertList)
            {
                //INSERT EMPLOYEES (Name, DateStarted) VALUES (?p1,?p2)
                using(XSqlCommand cmd = InsertClauseBuilder.GetClause(conn,obj,proj))
                {
                    object objID = null;
#if POSTGRES
                    objID = cmd.ExecuteScalar();
#else
                    //Mysql:
                    cmd.ExecuteNonQuery();
                    if(proj.autoGenField!=null)
                    {
                        try
                        {
                            cmd.CommandText = "SELECT @@Identity";
			                objID = cmd.ExecuteScalar();

                        }
                        catch(Exception ex)
                        {
                            Console.WriteLine("MTable.SaveAll: Failed on retrieving @@ID/assigning ID:"+ex);
                        }
                    }
#endif
                    try
                    {
                        //set the object's ID:
                        FieldUtils.SetObjectIdField(obj, proj.autoGenField, objID);

                        IModified imod = obj as IModified;
                        if(imod!=null){
                            imod.IsModified = false; //we just saved it - it's not 'dirty'
                        }
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine("Failed on SetObjectIdField: "+ex);
                    }
                    //cmd.CommandText = 
                    //TODO: use reflection to assign the field ID - that way the _isModified flag will not get set

                    //Console.WriteLine("MTable insert TODO: populate ID field ");
                }

            }

            Func<T, string> getObjectID = RowEnumeratorCompiler<T>.CompileIDRetrieval(proj);

            //todo: check object is not in two lists
            foreach(T obj in _liveObjectMap.Values)
            {
                IModified iMod = obj as IModified;
                if(iMod==null || !iMod.IsModified)
                    continue;
                Console.WriteLine("MTable SaveAll TODO: save modified object");
                string ID_to_update = getObjectID(obj);

                XSqlCommand cmd = InsertClauseBuilder.GetUpdateCommand(conn, iMod, proj, ID_to_update);
                int result = cmd.ExecuteNonQuery();
                Console.WriteLine("MTable SaveAll.Update returned:" + result);
            }

            if(_deleteList.Count>0)
            {
                //Func<T,string> getObjectID = RowEnumeratorCompiler<T>.CompileIDRetrieval(proj);
                StringBuilder sbDeleteIDs = new StringBuilder();
                int indx2=0;
                foreach(T obj in _deleteList)
                {
                    string ID_to_delete = getObjectID(obj);
                    if(indx2++ >0 ){ sbDeleteIDs.Append(","); }
                    sbDeleteIDs.Append(ID_to_delete);
                }
                string tableName = proj.tableAttribute.Name;
                string sql = "DELETE FROM "+tableName+" WHERE "+proj.keyColumnName+" in ("+sbDeleteIDs+")";
                Console.WriteLine("MTable SaveAll.Delete: "+sql);
                XSqlCommand cmd = new XSqlCommand(sql, _parentDB.SqlConnection);
                int result = cmd.ExecuteNonQuery();
                Console.WriteLine("MTable SaveAll.Delete returned:"+result);
            }

        }
        public string GetQueryText()
        {
            QueryProcessor.ProcessLambdas(_vars, typeof(T));
            return _vars.sqlString;
        }
    }
}

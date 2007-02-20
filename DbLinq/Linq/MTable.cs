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
    {
        /// <summary>
        /// the parent MContext holds our connection etc
        /// </summary>
        MContext _parent;
        readonly List<T> _insertList = new List<T>();
        //readonly List<T> _liveObjectList = new List<T>();
        readonly Dictionary<T,T> _liveObjectMap = new Dictionary<T,T>();
        readonly List<T> _deleteList = new List<T>();

        //WhereClauses _whereClauses;
        SqlExpressionParts _sqlParts = new SqlExpressionParts();
        //LambdaExpression _whereExpr;
        //LambdaExpression _selectExpr;
        //LambdaExpression _orderByExpr;
        SessionVars _vars = new SessionVars();
        //static int s_serial = 1;
        //Expression _expr;

        public MTable(MContext parent)
        {
            _parent = parent;
            _parent.RegisterChild(this);
            _vars.context = this._parent;
            _vars.sourceType = typeof(T);
            _vars._sqlParts = _sqlParts;
        }

        /// <summary>
        /// 'S' is the projected type. If you say 'from e in Employees select e.ID', then type S will be int.
        /// If you say 'select new {e.ID}', then type S will be something like Projection.f__1
        /// </summary>
        public IQueryable<S> CreateQuery<S>(Expression expr)
        {
            Console.WriteLine("MTable.CreateQuery: "+expr);
            //DumpExpressionXml<S> formatter = new DumpExpressionXml<S>();
            //string q = formatter.FormatExpression(0,expr);
            //File.WriteAllText("../expr_tree"+(s_serial++)+".xml", q);

            string methodName;
            LambdaExpression lambda = WhereClauseBuilder.FindLambda(expr,out methodName);

            //From the C# spec: Specifically, query expressions are translated 
            //into invocations of methods named 
            //Where, Select, SelectMany, Join, GroupJoin, OrderBy, OrderByDescending, ThenBy, ThenByDescending, GroupBy, and Cast
            switch(methodName)
            {
                case "Where":  
                    {
                        Console.WriteLine("Disabled whereBuilder.Main_AnalyzeLambda");
                        //WhereClauses whereClauses = whereBuilder.Main_AnalyzeLambda(lambda);
                        //whereClauses.CopyInto(_sqlParts);
                        this._vars.whereExpr.Add(lambda);
                    }
                    break;
                case "Select": 
                    {
                        _vars.selectExpr = lambda;
                        //FromClauseBuilder.Main_AnalyzeLambda(_sqlParts,lambda);
                        Console.WriteLine("Disabled FromClauseBuilder.Main_AnalyzeLambda");
                    }
                    break;
                case "SelectMany":
                    //dig deeper to find the where clause
                    _vars.selectExpr = lambda; 
                    lambda = WhereClauseBuilder.FindSelectManyLambda(lambda,out methodName);
                    if(methodName=="Where")
                    {
                        _vars.whereExpr.Add(lambda);
                        //WhereClauses whereClauses = whereBuilder.Main_AnalyzeLambda(lambda);
                        //whereClauses.CopyInto(_sqlParts);
                        //_vars.whereExpr.Add(lambda);
                    }
                    break;
                case "OrderBy": 
                    _vars.orderByExpr = lambda; 
                    break;
                default: 
                    throw new ApplicationException("L45: Unprepared for method "+methodName);
            }
            
            if(this is IQueryable<S>){
                //this occurs if we are not projecting
                //meaning that we are selecting entire object
                IQueryable<S> this_S = (IQueryable<S>)this; 
                return this_S;
            } else {
                //if we get here, we are projecting.
                //(eg. you select only a few fields: "select name from employee")
                _vars.createQueryExpr = expr;
                MTable_Projected<S> projectedQ = new MTable_Projected<S>(_vars);
                projectedQ._sqlParts = _sqlParts;
                return projectedQ;
            }
        }

        /// <summary>
        /// the query '(from o in Orders select o).First()' enters here
        /// </summary>
        public S Execute<S>(Expression expression)
        {
            Console.WriteLine("MTable.Execute<"+typeof(S)+">: "+expression);
            return new RowScalar<T>(_vars, this).GetScalar<S>(expression);
        }

        /// <summary>
        /// entry point for 'foreach' statement.
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            QueryProcessor.ProcessLambdas(_vars);
            RowEnumerator<T> rowEnumerator = new RowEnumerator<T>(_vars, _liveObjectMap);
            if(MContext.s_suppressSqlExecute){
                //we are doing GetQueryText
            } else {
                rowEnumerator.ExecuteSqlCommand();
            }
            return rowEnumerator;
        }

        /// <summary>
        /// GetEnumerator where you can inject an extra clause, eg. "LIMIT 2" or an extra where
        /// </summary>
        /// <param name="fct"></param>
        public RowEnumerator<T> GetModifiedEnumerator(CustomExpressionHandler fct)
        {
            SessionVars vars2 = _vars.Clone();
            fct(vars2);
            QueryProcessor.ProcessLambdas(vars2);
            RowEnumerator<T> rowEnumerator = new RowEnumerator<T>(vars2, _liveObjectMap);
            rowEnumerator.ExecuteSqlCommand();
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
            //TODO: queue an object for SQL INSERT
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
            object[] indices = new object[0];
            ProjectionData proj = ProjectionData.FromType(typeof(T));
            XSqlConnection conn = _parent.SqlConnection;
            foreach(T obj in _insertList)
            {
                //INSERT EMPLOYEES (Name, DateStarted) VALUES (?p1,?p2)
                using(XSqlCommand cmd = InsertClauseBuilder.GetClause(conn,obj,proj))
                {
                    //object oResult = cmd.ExecuteScalar();
                    cmd.ExecuteNonQuery();
                    if(proj.autoGenField!=null)
                    {
                        try
                        {
                            cmd.CommandText = "SELECT @@Identity";
			                object id = cmd.ExecuteScalar();
                            if(id is long){
                                //prevent {"Object of type 'System.Int64' cannot be converted to type 'System.UInt32'."}
                                long longID = (long)id;
                                bool assignable1 = proj.autoGenField.FieldType.IsAssignableFrom(typeof(long));
                                bool assignable2 = proj.autoGenField.FieldType.IsAssignableFrom(typeof(int));
                                if(proj.autoGenField.FieldType==typeof(uint))
                                {
                                    uint uintID = (uint) (long) id;
                                    id = uintID;
                                }
                            }
                            proj.autoGenField.SetValue(obj, id);
                            IModified imod = obj as IModified;
                            if(imod!=null){
                                imod.IsModified = false; //we just saved it - it's not 'dirty'
                            }
                        }
                        catch(Exception ex)
                        {
                            Console.WriteLine("MTable.SaveAll: Failed on retrieving @@ID/assigning ID:"+ex);
                        }
                    }
                    //cmd.CommandText = 
                    //TODO: use reflection to assign the field ID - that way the _isModified flag will not get set

                    //Console.WriteLine("MTable insert TODO: populate ID field ");
                }

            }

            //todo: check object is not in two lists
            foreach(T obj in _liveObjectMap.Values)
            {
                IModified iMod = obj as IModified;
                if(iMod==null || !iMod.IsModified)
                    continue;
                Console.WriteLine("MTable SaveAll TODO: save modified object");
            }

            if(_deleteList.Count>0)
            {
                Func<T,string> getObjectID = RowEnumeratorCompiler<T>.CompileIDRetrieval(proj);
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
                XSqlCommand cmd = new XSqlCommand(sql, _parent.SqlConnection);
                int result = cmd.ExecuteNonQuery();
                Console.WriteLine("MTable SaveAll.Delete returned:"+result);
            }

        }

    }
}

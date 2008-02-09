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
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;

//using System.Data.DLinq;

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
#elif SQLITE
using System.Data.SQLite;
using XSqlConnection = System.Data.SQLite.SQLiteConnection;
using XSqlCommand = System.Data.SQLite.SQLiteCommand;
using XSqlParameter = System.Data.SQLite.SQLiteParameter;
#else
using MySql.Data.MySqlClient;
using XSqlConnection = MySql.Data.MySqlClient.MySqlConnection;
using XSqlCommand = MySql.Data.MySqlClient.MySqlCommand;
#endif
using DBLinq.Linq.clause;
using DBLinq.util;
using DBLinq.vendor;

namespace DBLinq.Linq
{
    /// <summary>
    /// T may be eg. class Employee or string - the output
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Table<T> :
        IQueryable<T>
        , IOrderedQueryable<T> //this is cheating ... we pretend to be always ordered
        , IMTable
        , IQueryText
        , IQueryProvider //new as of Beta2
    {
        static IVendor s_vendor = VendorFactory.Make();

        /// <summary>
        /// the parent MContext holds our connection etc
        /// </summary>
        DataContext _parentDB;
        readonly List<T> _insertList = new List<T>();
        readonly Dictionary<T, T> _liveObjectMap = new Dictionary<T, T>();
        readonly List<T> _deleteList = new List<T>();

        readonly SessionVars _vars;

        public Table(DataContext parent)
        {
            _parentDB = parent;
            _parentDB.RegisterChild(this);
            _vars = new SessionVars(parent);
        }

        /// <summary>
        /// this is used when we call CreateQuery to create a copy of orig table object
        /// </summary>
        public Table(Table<T> parent, SessionVars vars)
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
            if (_parentDB.Log != null)
            {
                _parentDB.Log.WriteLine("MTable.CreateQuery: " + expr);
            }

            SessionVars vars = new SessionVars(_vars).Add(expr);

            //if (this is IQueryable<S>)
            if (typeof(S) == typeof(T))
            {
                //this occurs if we are not projecting
                //(meaning that we are selecting entire row object)
                Table<T> clonedThis = new Table<T>(this, vars);
                IQueryable<S> this_S = (IQueryable<S>)clonedThis;
                return this_S;
            }
            else
            {
                //if we get here, we are projecting.
                //(eg. you select only a few fields: "select name from employee")
                MTable_Projected<S> projectedQ = new MTable_Projected<S>(vars);
                return projectedQ;
            }
        }

        /// <summary>
        /// the query '(from o in Orders select o).First()' enters here
        /// </summary>
        public S Execute<S>(Expression expression)
        {
            Log1.Info("MTable.Execute<" + typeof(S) + ">: " + expression);
            SessionVars vars2 = new SessionVars(_vars).AddScalar(expression); //clone and append Expr
            SessionVarsParsed varsFin = QueryProcessor.ProcessLambdas(vars2, typeof(T)); //parse all
            return new RowScalar<T>(varsFin, this, _liveObjectMap).GetScalar<S>(expression);
        }

        /// <summary>
        /// entry point for 'foreach' statement.
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            SessionVarsParsed varsFin = QueryProcessor.ProcessLambdas(_vars, typeof(T));
            RowEnumerator<T> rowEnumerator = new RowEnumerator<T>(varsFin, _liveObjectMap);
            return rowEnumerator.GetEnumerator();
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            IEnumerator<T> enumT = GetEnumerator();
            return enumT;
        }

        [Obsolete("NOT IMPLEMENTED YET")]
        public Type ElementType
        {
            get { throw new ApplicationException("Not implemented"); }
        }

        public Expression Expression
        {
            //copied from RdfProvider
            get { return Expression.Constant(this); }
        }


        [Obsolete("NOT IMPLEMENTED YET - Use CreateQuery<S>")]
        public IQueryable CreateQuery(Expression expression)
        {
            throw new ApplicationException("Not implemented");
        }

        [Obsolete("NOT IMPLEMENTED YET - Use Execute<S>")]
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

        /// <summary>
        /// Attaches an entity from another Context to a table,
        /// with the intention to perform an update or delete operation
        /// </summary>
        /// <param name="entity">table row object to attach</param>
        /// <param name="asModified">true if object will be updated on Submit, false if not</param>
        public void Attach(T entity, bool asModified)
        {
            if (_liveObjectMap.ContainsKey(entity))
                throw new System.Data.Linq.DuplicateKeyException(entity);

            _liveObjectMap[entity] = entity;
            //todo: add where T: IsModified to class definition and use
            //entity.IsModified = asModified
            (entity as IModified).IsModified = asModified;
        }

        public void SaveAll()
        {
            SaveAll(System.Data.Linq.ConflictMode.FailOnFirstConflict);
        }

        public List<Exception> SaveAll(System.Data.Linq.ConflictMode failureMode)
        {
            if (_insertList.Count == 0 && _liveObjectMap.Count == 0 && _deleteList.Count == 0)
                return new List<Exception>(); //nothing to do

            XSqlConnection conn = _parentDB.SqlConnection;

            using(new ConnectionManager(conn))
            {
                return SaveAll_unsafe(conn, failureMode);
            } //Dispose(): close connection, if it was initally closed
        }

        private List<Exception> SaveAll_unsafe(XSqlConnection conn, System.Data.Linq.ConflictMode failureMode)
        {
            List<Exception> excepts = new List<Exception>();
            //TODO: process deleteList, insertList, liveObjectList
            //object[] indices = new object[0];
            ProjectionData proj = ProjectionData.FromDbType(typeof(T));

#if MYSQL //bulk insert code
            if (vendor.mysql.VendorMysql.UseBulkInsert.ContainsKey(this))
            {
                vendor.mysql.VendorMysql.DoBulkInsert(this, _insertList, conn);
                _insertList.Clear();
            }
#elif MICROSOFT //bulk insert code
            if (vendor.mssql.VendorMssql.UseBulkInsert.ContainsKey(this))
            {
                vendor.mssql.VendorMssql.DoBulkInsert(this, _insertList, conn);
                _insertList.Clear();
            }
#endif

            foreach (T obj in _insertList)
            {
                //build command similar to:
                //INSERT INTO EMPLOYEES (Name, DateStarted) VALUES (?p1,?p2); SELECT @@IDENTITY
                //INSERT INTO EMPLOYEES (EmpId, Name, DateStarted) VALUES (EmpID_SEQ.NextVal,?p1,?p2); SELECT EmpID_SEQ.CurrVal
                try
                {
                    using (XSqlCommand cmd = InsertClauseBuilder.GetClause(conn, obj, proj))
                    {
                        object objID = null;
                        objID = cmd.ExecuteScalar();

                        if (proj.autoGenField == null)
                            continue; //ID was already assigned by user, not from a DB sequence.

                        //Oracle unpacks objID from an out-param:
                        s_vendor.ProcessInsertedId(cmd, ref objID);

                        try
                        {
                            //set the object's ID:
                            FieldUtils.SetObjectIdField(obj, proj.autoGenField, objID);

                            IModified imod = obj as IModified;
                            if (imod != null)
                            {
                                imod.IsModified = false; //we just saved it - it's not 'dirty'
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("L227 Failed on SetObjectIdField: " + ex);
                        }
                        //cmd.CommandText = 
                        //TODO: use reflection to assign the field ID - that way the _isModified flag will not get set

                        //Console.WriteLine("MTable insert TODO: populate ID field ");
                    }
                }
                catch (Exception ex)
                {
                    switch (failureMode)
                    {
                        case System.Data.Linq.ConflictMode.ContinueOnConflict:
                            excepts.Add(ex);
                            break;
                        case System.Data.Linq.ConflictMode.FailOnFirstConflict:
                            throw ex;
                    }
                }

            }

            Func<T, string> getObjectID = RowEnumeratorCompiler<T>.CompileIDRetrieval(proj);

            //todo: check object is not in two lists
            foreach (T obj in _liveObjectMap.Values)
            {
                try
                {
                    IModified iMod = obj as IModified;
                    if (iMod == null || !iMod.IsModified)
                        continue;
                    Trace.WriteLine("MTable SaveAll: saving modified object");
                    string ID_to_update = getObjectID(obj);

                    XSqlCommand cmd = InsertClauseBuilder.GetUpdateCommand(conn, iMod, proj, ID_to_update);
                    int result = cmd.ExecuteNonQuery();
                    Trace.WriteLine("MTable SaveAll.Update returned:" + result);

                    iMod.IsModified = false; //mark as saved, thanks to Martin Rauscher
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("Table.SubmitChanges failed: " + ex);
                    switch (failureMode)
                    {
                        case System.Data.Linq.ConflictMode.ContinueOnConflict:
                            excepts.Add(ex);
                            break;
                        case System.Data.Linq.ConflictMode.FailOnFirstConflict:
                            throw ex;
                    }
                }
            }

            foreach (T insertedT in _insertList)
            {
                //inserted objects are now live:
                _liveObjectMap[insertedT] = insertedT;
            }
            //thanks to Martin Rauscher for spotting that I forgot to clear the list:
            _insertList.Clear();

            if (_deleteList.Count > 0)
            {
                //Func<T,string> getObjectID = RowEnumeratorCompiler<T>.CompileIDRetrieval(proj);

                KeyValuePair<PropertyInfo, System.Data.Linq.Mapping.ColumnAttribute>[] primaryKeys
                    = AttribHelper.FindPrimaryKeys(typeof(T));
                if (primaryKeys.Length != 1)
                    throw new ApplicationException("L329: Composite PKs are not yet supported");
                Type primaryKeyType = primaryKeys[0].Key.PropertyType;
                bool mustQuoteIds = primaryKeyType == typeof(string) || primaryKeyType == typeof(char);

                StringBuilder sbDeleteIDs = new StringBuilder();
                int indx2 = 0;
                foreach (T obj in _deleteList)
                {
                    try
                    {
                        string ID_to_delete = getObjectID(obj);
                        if (indx2++ > 0) { sbDeleteIDs.Append(","); }

                        // turn PK ALFKI into 'ALFKI'
                        string ID_quoted = mustQuoteIds
                            ? "'" + ID_to_delete + "'"
                            : ID_to_delete;

                        sbDeleteIDs.Append(ID_quoted);
                    }
                    catch (Exception ex)
                    {
                        switch (failureMode)
                        {
                            case System.Data.Linq.ConflictMode.ContinueOnConflict:
                                excepts.Add(ex);
                                break;
                            case System.Data.Linq.ConflictMode.FailOnFirstConflict:
                                throw ex;
                        }
                    }
                }
                string tableName = proj.tableAttribute.Name;
                string sql = "DELETE FROM " + tableName + " WHERE " + proj.keyColumnName + " in (" + sbDeleteIDs + ")";
                Trace.WriteLine("MTable SaveAll.Delete: " + sql);
                XSqlCommand cmd = new XSqlCommand(sql, _parentDB.SqlConnection);
                int result = cmd.ExecuteNonQuery();
                Trace.WriteLine("MTable SaveAll.Delete returned:" + result);
            }

            _deleteList.Clear();

            return excepts;
        }

        public string GetQueryText()
        {
            SessionVarsParsed varsFin = QueryProcessor.ProcessLambdas(_vars, typeof(T));
            return varsFin.sqlString;
        }

        //New as of Orcas Beta2 - what does it do?
        public IQueryProvider Provider
        {
            get { return this; }
        }

        /// <summary>
        /// TODO: RemoveAll(where_clause)
        /// </summary>
        [Obsolete("NOT IMPLEMENTED YET")]
        public void RemoveAll<TSubEntity>(IEnumerable<TSubEntity> entities)
            where TSubEntity : T
        {
            throw new NotImplementedException();
        }

    }
}

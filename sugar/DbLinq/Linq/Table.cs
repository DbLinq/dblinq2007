#region MIT license
// 
// Copyright (c) 2007-2008 Jiri Moudry
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
using System.Reflection;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;

//using System.Data.DLinq;

using DbLinq.Linq.Clause;
using DbLinq.Linq.Implementation;
using DbLinq.Logging;
using DbLinq.Util;
using DbLinq.Vendor;

namespace DbLinq.Linq
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
        /// <summary>
        /// the parent MContext holds our connection etc
        /// </summary>
        public DataContext DataContext { get; private set; }
        private readonly List<T> _insertList = new List<T>();
        private readonly List<T> _deleteList = new List<T>();

        private readonly SessionVars _vars;

        public ILogger Logger { get; set; }

        private IModificationHandler _modificationHandler { get { return DataContext.ModificationHandler; } }

        public Table(DataContext parent)
        {
            DataContext = parent;
            DataContext.RegisterChild(this);
            _vars = new SessionVars(parent, this);
        }

        /// <summary>
        /// this is used when we call CreateQuery to create a copy of orig table object
        /// </summary>
        public Table(Table<T> parent, SessionVars vars)
        {
            _insertList = parent._insertList;
            _deleteList = parent._deleteList;
            DataContext = parent.DataContext;
            _vars = vars;
        }

        /// <summary>
        /// 'S' is the projected type. If you say 'from e in Employees select e.ID', then type S will be int.
        /// If you say 'select new {e.ID}', then type S will be something like Projection.f__1
        /// </summary>
        public IQueryable<S> CreateQuery<S>(Expression expr)
        {
            DataContext.Logger.Write(Level.Debug, "MTable.CreateQuery: {0}", expr);

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
        /// this is only called during Dynamic Linq
        /// </summary>
        [Obsolete("COMPLETELY UNTESTED - Use CreateQuery<S>")]
        public IQueryable CreateQuery(Expression expression)
        {
            DataContext.Logger.Write(Level.Debug, "MTable.CreateQuery: {0}", expression);

            Type S = expression.Type;
            SessionVars vars = new SessionVars(_vars).Add(expression);

            //if (this is IQueryable<S>)
            bool sameType = (S == typeof(IQueryable<T>)) || (S == typeof(IOrderedQueryable<T>));
            if (sameType)
            {
                //this occurs if we are not projecting
                //(meaning that we are selecting entire row object)
                Table<T> clonedThis = new Table<T>(this, vars);
                //IQueryable<S> this_S = (IQueryable<S>)clonedThis;
                IQueryable this_S = (IQueryable)clonedThis;
                return this_S;
            }
            else
            {
                //if we get here, we are projecting.
                //(eg. you select only a few fields: "select name from employee")
                //MTable_Projected<S> projectedQ = new MTable_Projected<S>(vars);
                Type TArg2 = S;
                Type mtableProjectedType2 = typeof(MTable_Projected<>).MakeGenericType(TArg2);
                object projectedQ = Activator.CreateInstance(mtableProjectedType2, vars);
                return projectedQ as IQueryable;
            }
        }

        /// <summary>
        /// the query '(from o in Orders select o).First()' enters here
        /// </summary>
        public S Execute<S>(Expression expression)
        {
            _vars.Context.Logger.Write(Level.Debug, "MTable.Execute<{0}>: {1}", typeof(S) , expression);
            SessionVars vars2 = new SessionVars(_vars).AddScalar(expression); //clone and append Expr
            SessionVarsParsed varsFin = _vars.Context.QueryGenerator.GenerateQuery(vars2, typeof(T)); //parse all
            return new RowScalar<T>(varsFin, this).GetScalar<S>(expression);
        }

        /// <summary>
        /// entry point for 'foreach' statement.
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            SessionVarsParsed varsFin = _vars.Context.QueryGenerator.GenerateQuery(_vars, typeof(T));
            RowEnumerator<T> rowEnumerator = new RowEnumerator<T>(varsFin);
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
            //we have no clue what to return here ...
            get { return typeof(T); }
        }

        public Expression Expression
        {
            //copied from RdfProvider
            get { return Expression.Constant(this); }
        }

        [Obsolete("NOT IMPLEMENTED YET - Use Execute<S>")]
        public object Execute(Expression expression)
        {
            throw new ApplicationException("L205 Not implemented");
        }

        [Obsolete("Replace with InsertOnSubmit")]
        public void Add(T newObject)
        {
            InsertOnSubmit(newObject);
        }

        public void InsertOnSubmit(T newObject)
        {
            _insertList.Add(newObject);
        }

        public void InsertAllOnSubmit<TSubEntity>(IEnumerable<TSubEntity> entities) where TSubEntity : T
        {
            foreach (TSubEntity newObject in entities)
                _insertList.Add(newObject);
        }

        [Obsolete("Replace with DeleteOnSubmit")]
        public void Remove(T objectToDelete)
        {
            DeleteOnSubmit(objectToDelete);
        }

        public void DeleteOnSubmit(T objectToDelete)
        {
            if (_insertList.Contains(objectToDelete))
                _insertList.Remove(objectToDelete);
            else
                //TODO: queue an object for SQL DELETE
                _deleteList.Add(objectToDelete);
        }

        public void DeleteAllOnSubmit<TSubEntity>(IEnumerable<TSubEntity> entities) where TSubEntity : T
        {
            foreach (TSubEntity row in entities)
                DeleteOnSubmit(row);
        }

        /// <summary>
        /// Attaches an entity from another Context to a table,
        /// with the intention to perform an update or delete operation
        /// </summary>
        /// <param name="entity">table row object to attach</param>
        public void Attach(T entity)
        {
            if (DataContext.GetRegisteredEntity(entity) != null)
                throw new System.Data.Linq.DuplicateKeyException(entity);

            DataContext.RegisterEntity(entity);
            _modificationHandler.Register(entity);
        }

        public void AttachAll<TSubEntity>(IEnumerable<TSubEntity> entities) where TSubEntity : T
        {
            foreach (TSubEntity row in entities)
                Attach(row);
        }

        /// <summary>
        /// Attaches existing entity with original state
        /// </summary>
        /// <param name="entity">live entity added to change tracking</param>
        /// <param name="original">original unchanged property values</param>
        public void Attach(T entity, T original) 
        {
            throw new NotImplementedException();
        }

        public void CheckAttachment(object entity)
        {
            DataContext.GetOrRegisterEntity(entity);
        }

        public void SaveAll()
        {
            SaveAll(System.Data.Linq.ConflictMode.FailOnFirstConflict);
        }

        public List<Exception> SaveAll(System.Data.Linq.ConflictMode failureMode)
        {
            if (_insertList.Count == 0 && _deleteList.Count == 0 && !DataContext.HasRegisteredEntities<T>())
                return new List<Exception>(); //nothing to do

            using (DataContext.DatabaseContext.OpenConnection())
            {
                return SaveAll_unsafe(failureMode);
            } //Dispose(): close connection, if it was initally closed
        }

        private List<Exception> SaveAll_unsafe(System.Data.Linq.ConflictMode failureMode)
        {
            List<Exception> excepts = new List<Exception>();
            //TODO: process deleteList, insertList, liveObjectList
            //object[] indices = new object[0];
            ProjectionData proj = ProjectionData.FromDbType(typeof(T));

            if (_vars.Context.Vendor.CanBulkInsert(this))
            {
                _vars.Context.Vendor.DoBulkInsert(this, _insertList, _vars.Context.DatabaseContext.Connection);
                _insertList.Clear();
            }

            foreach (T obj in _insertList)
            {
                //build command similar to:
                //INSERT INTO EMPLOYEES (Name, DateStarted) VALUES (?p1,?p2); SELECT @@IDENTITY
                //INSERT INTO EMPLOYEES (EmpId, Name, DateStarted) VALUES (EmpID_SEQ.NextVal,?p1,?p2); SELECT EmpID_SEQ.CurrVal
                try
                {
                    using (IDbCommand cmd = InsertClauseBuilder.GetClause(_vars.Context.Vendor, _vars.Context.DatabaseContext, obj, proj))
                    {
                        object objID = cmd.ExecuteScalar();

                        if (!proj.AutoGen)
                            continue; //ID was already assigned by user, not from a DB sequence.

                        //Oracle unpacks objID from an out-param:
                        _vars.Context.Vendor.ProcessInsertedId(cmd, ref objID);

                        try
                        {
                            //set the object's ID:
                            if (!proj.IsAutoGenSpecified(obj))
                                proj.UpdateAutoGen(obj, objID);

                            _modificationHandler.ClearModified(obj); //we just saved it - it's not 'dirty'
                        }
                        catch (Exception ex)
                        {
                            Logger.Write(Level.Error, "L227 Failed on SetObjectIdField: " + ex);
                        }
                        //cmd.CommandText = 
                        //TODO: use reflection to assign the field ID - that way the _isModified flag will not get set

                        //Logger.Write("MTable insert TODO: populate ID field ");
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

            Func<T, string[]> getObjectID = RowEnumeratorCompiler<T>.CompileIDRetrieval(proj);

            //todo: check object is not in two lists
            foreach (T obj in DataContext.GetRegisteredEntities<T>())
            {
                try
                {
                    if (!_modificationHandler.IsModified(obj))
                        continue;

                    Trace.WriteLine("MTable SaveAll: saving modified object");
                    string[] ID_to_update = getObjectID(obj);

                    IList<PropertyInfo> modifiedProperties = _modificationHandler.GetModifiedProperties(obj);

                    using (IDbCommand cmd = InsertClauseBuilder.GetUpdateCommand(_vars, obj, proj, ID_to_update, modifiedProperties))
                    {
                        int result = cmd.ExecuteNonQuery();
                        Trace.WriteLine("MTable SaveAll.Update returned:" + result);
                    }

                    _modificationHandler.ClearModified(obj); //mark as saved, thanks to Martin Rauscher
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
                DataContext.RegisterEntity(insertedT);
            }
            //thanks to Martin Rauscher for spotting that I forgot to clear the list:
            _insertList.Clear();

            if (_deleteList.Count > 0)
            {
                //Func<T,string> getObjectID = RowEnumeratorCompiler<T>.CompileIDRetrieval(proj);

                KeyValuePair<PropertyInfo, System.Data.Linq.Mapping.ColumnAttribute>[] primaryKeys
                    = AttribHelper.FindPrimaryKeys(typeof(T));

                //Type primaryKeyType = primaryKeys[0].Key.PropertyType;
                //bool mustQuoteIds = primaryKeyType == typeof(string) || primaryKeyType == typeof(char);

                List<string> idsToDelete = new List<string>();
                foreach (T obj in _deleteList)
                {
                    try
                    {
                        string[] ID_to_delete = getObjectID(obj);

                        string whereClause = InsertClauseBuilder.GetPrimaryKeyWhereClause(_vars, obj, proj, ID_to_delete);

                        idsToDelete.Add(whereClause);
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
                string tableName = _vars.Context.Vendor.GetSqlFieldSafeName(proj.tableAttribute.Name);

                //this does not work with CompositePKs:
                //string sql = "DELETE FROM " + tableName + " WHERE " + proj.keyColumnName + " in (" + sbDeleteIDs + ")";

                //this should work with CompositePKs:
                string sql = "DELETE FROM " + tableName + " WHERE " + string.Join(" OR ", idsToDelete.ToArray());

                Trace.WriteLine("MTable SaveAll.Delete: " + sql);
                using (IDbCommand cmd = DataContext.DatabaseContext.CreateCommand())
                {
                    cmd.CommandText = sql;
                    int result = cmd.ExecuteNonQuery();
                    Trace.WriteLine("MTable SaveAll.Delete returned:" + result);
                }
            }

            _deleteList.Clear();

            return excepts;
        }

        public string GetQueryText()
        {
            SessionVarsParsed varsFin = _vars.Context.QueryGenerator.GenerateQuery(_vars, typeof(T));
            return varsFin.SqlString;
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

        public IEnumerable<object> Inserts
        {
            get { return _insertList.Cast<object>(); }
        }

        public IEnumerable<object> Updates
        {
            get
            {
                List<object> list = new List<object>();

                foreach (T obj in DataContext.GetRegisteredEntities<T>())
                {
                    if (_modificationHandler.IsModified(obj))
                        list.Add(obj);
                }
                return list;
            }
        }

        public IEnumerable<object> Deletes
        {
            get
            {
                return _deleteList.Cast<object>();
            }
        }

    }
}

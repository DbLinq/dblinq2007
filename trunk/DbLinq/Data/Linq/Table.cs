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
using System.Data.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

//using System.Data.DLinq;
using DbLinq.Data.Linq.Implementation;
using DbLinq.Data.Linq.Sugar;
using DbLinq.Linq;
using DbLinq.Linq.Clause;
using DbLinq.Logging;
using DbLinq.Util;
using ITable = System.Data.Linq.ITable;

#if MONO_STRICT
namespace System.Data.Linq
#else
namespace DbLinq.Data.Linq
#endif
{
    /// <summary>
    /// T may be eg. class Employee or string - the output
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Table<T> :
        IQueryable<T>
        , IOrderedQueryable<T> //this is cheating ... we pretend to be always ordered
        , ITable
        , IMTable
        , IQueryProvider //new as of Beta2
        where T : class
    {
        /// <summary>
        /// the parent MContext holds our connection etc
        /// </summary>
        public DataContext DataContext { get; private set; }

        [Obsolete("NOT IMPLEMENTED - Use DataContext instead of Context")]
        public System.Data.Linq.DataContext Context { get { throw new NotImplementedException("TODO L63"); } }

        private readonly List<T> _insertList = new List<T>();
        private readonly List<T> _deleteList = new List<T>();
        private QueryProvider<T> _queryProvider;

        public ILogger Logger { get; set; }

        private IModificationHandler _modificationHandler { get { return DataContext.ModificationHandler; } }

        public Table(DataContext parent)
        {
            DataContext = parent;
            DataContext.RegisterChild(this);
            _queryProvider = new QueryProvider<T>(parent);
        }

        /// <summary>
        /// 'S' is the projected type. If you say 'from e in Employees select e.ID', then type S will be int.
        /// If you say 'select new {e.ID}', then type S will be something like Projection.f__1
        /// </summary>
        public IQueryable<S> CreateQuery<S>(Expression expr)
        {
            return _queryProvider.CreateQuery<S>(expr);
        }

        /// <summary>
        /// this is only called during Dynamic Linq
        /// </summary>
        [Obsolete("COMPLETELY UNTESTED - Use CreateQuery<S>")]
        public IQueryable CreateQuery(Expression expression)
        {
            return _queryProvider.CreateQuery(expression);
        }

        /// <summary>
        /// the query '(from o in Orders select o).First()' enters here
        /// </summary>
        public S Execute<S>(Expression expression)
        {
            return _queryProvider.Execute<S>(expression);
        }

        public object Execute(Expression expression)
        {
            return _queryProvider.Execute(expression);
        }

        /// <summary>
        /// entry point for 'foreach' statement.
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            return _queryProvider.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            IEnumerator<T> enumT = GetEnumerator();
            return enumT;
        }

        public Type ElementType
        {
            get { return _queryProvider.ElementType; }
        }

        public Expression Expression
        {
            get { return Expression.Constant(this); } // do not change this to _queryProvider.Expression, Sugar doesn't fully handle QueryProviders by now
        }

        public IQueryProvider Provider
        {
            get { return _queryProvider.Provider; }
        }

        #region Insert functions
        void ITable.InsertOnSubmit(object entity)
        {
            T te = entity as T;
            if (te == null)
                throw new ArgumentException("Cannot insert this object");
            InsertOnSubmit(te);
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

        void ITable.InsertAllOnSubmit(IEnumerable entities)
        {
            foreach (object entity in entities)
            {
                T te = entity as T;
                if (te == null)
                    throw new ArgumentException("Cannot insert this object");
                _insertList.Add(te);
            }
        }

        public void InsertAllOnSubmit<TSubEntity>(IEnumerable<TSubEntity> entities) where TSubEntity : T
        {
            foreach (TSubEntity newObject in entities)
                _insertList.Add(newObject);
        }
        #endregion

        #region Delete functions
        public void DeleteAllOnSubmit(IEnumerable entities)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// required by ITable interface
        /// </summary>
        /// <param name="entity"></param>
        void ITable.DeleteOnSubmit(object entity)
        {
            throw new NotImplementedException();
        }

        //[Obsolete("Replace with DeleteOnSubmit")]
        //public void Remove(T objectToDelete)
        //{
        //    DeleteOnSubmit(objectToDelete);
        //}

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
        #endregion

        #region Attach functions

        /// <summary>
        /// required for ITable
        /// </summary>
        /// <param name="entity"></param>
        void ITable.Attach(object entity)
        {
            T te = entity as T;
            if (te == null)
                throw new ArgumentException("Cannot attach this object");
            Attach(te);
        }
        void ITable.Attach(object entity, object original)
        {
            T te = entity as T;
            T to = original as T;
            if (te == null || to == null)
                throw new ArgumentException("Cannot attach this object");
            Attach(te, to);
        }
        void ITable.Attach(object entity, bool asModified)
        {
            throw new NotImplementedException();
        }
        void ITable.AttachAll(IEnumerable entities)
        {
            throw new NotImplementedException();
        }
        void ITable.AttachAll(IEnumerable entities, bool asModified)
        {
            throw new NotImplementedException();
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
        #endregion

        #region Save functions
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
            var exceptions = new List<Exception>();
            //TODO: process deleteList, insertList, liveObjectList
            //object[] indices = new object[0];
            ProjectionData proj = ProjectionData.FromDbType(typeof(T));
            Func<T, string[]> getObjectID = RowEnumeratorCompiler<T>.CompileIDRetrieval(proj);

            ProcessInsert(proj, failureMode, exceptions);
            ProcessUpdate(proj, failureMode, exceptions, getObjectID);
            ProcessDelete(proj, failureMode, exceptions, getObjectID);

            return exceptions;
        }

        private void ProcessInsert(ProjectionData proj, ConflictMode failureMode, List<Exception> excepts)
        {
            if (DataContext.Vendor.CanBulkInsert<T>(this))
            {
                DataContext.Vendor.DoBulkInsert(this, _insertList, DataContext.DatabaseContext.Connection);
                _insertList.Clear();
            }

            foreach (T obj in _insertList)
            {
                //build command similar to:
                //INSERT INTO EMPLOYEES (Name, DateStarted) VALUES (?p1,?p2); SELECT @@IDENTITY
                //INSERT INTO EMPLOYEES (EmpId, Name, DateStarted) VALUES (EmpID_SEQ.NextVal,?p1,?p2); SELECT EmpID_SEQ.CurrVal
                try
                {
                    using (IDbCommand cmd = InsertClauseBuilder.GetClause(DataContext.Vendor, DataContext.DatabaseContext, obj, proj))
                    {
                        object objID = cmd.ExecuteScalar();

                        if (!proj.AutoGen)
                        {
                            _modificationHandler.ClearModified(obj); //we just saved it - it's not 'dirty'
                            continue; //ID was already assigned by user, not from a DB sequence.
                        }

                        //Oracle unpacks objID from an out-param:
                        DataContext.Vendor.ProcessInsertedId(cmd, ref objID);

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

            foreach (T insertedT in _insertList)
            {
                //inserted objects are now live:
                DataContext.RegisterEntity(insertedT);
            }
            //thanks to Martin Rauscher for spotting that I forgot to clear the list:
            _insertList.Clear();
        }

        private void ProcessUpdate(ProjectionData proj, ConflictMode failureMode, List<Exception> excepts, Func<T, string[]> getObjectID)
        {
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

                    using (IDbCommand cmd = InsertClauseBuilder.GetUpdateCommand(DataContext, obj, proj, ID_to_update, modifiedProperties))
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
        }

        private void ProcessDelete(ProjectionData proj, ConflictMode failureMode, List<Exception> excepts, Func<T, string[]> getObjectID)
        {
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

                        string whereClause = InsertClauseBuilder.GetPrimaryKeyWhereClause(DataContext, obj, proj, ID_to_delete);

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
                string tableName = DataContext.Vendor.GetSqlFieldSafeName(proj.tableAttribute.Name);

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
        }

        #endregion

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

        public bool IsReadOnly { get { return false; } }

        [Obsolete("NOT IMPLEMENTED YET")]
        public System.Data.Linq.ModifiedMemberInfo[] GetModifiedMembers(object entity)
        {
            throw new ApplicationException("L579 Not implemented");
        }

        [Obsolete("NOT IMPLEMENTED YET")]
        object ITable.GetOriginalEntityState(object entity)
        {
            throw new ApplicationException("L585 Not implemented");
        }

    }
}

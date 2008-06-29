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

#if MONO_STRICT
using System.Data.Linq.Implementation;
using System.Data.Linq.Sugar;
using ITable = System.Data.Linq.ITable;
#else
using DbLinq.Data.Linq.Implementation;
using DbLinq.Data.Linq.Sugar;
using ITable = DbLinq.Data.Linq.ITable;
#endif

using DbLinq.Logging;
using DbLinq;

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
    public partial class Table<T> :
        IQueryable<T>,
        IOrderedQueryable<T>, //this is cheating ... we pretend to be always ordered
        ITable,
        IQueryProvider,
        IManagedTable // internal helper. One day, all data will be processed from DataContext
        where T : class
    {
        /// <summary>
        /// the parent DataContext holds our connection etc
        /// </summary>
        public DataContext Context { get { return _context; } }
        private readonly DataContext _context;

        // QueryProvider is the running entity, running through nested Expressions
        private readonly QueryProvider<T> _queryProvider;

        public ILogger Logger { get; set; }

        internal Table(DataContext parentContext)
        {
            _context = parentContext;
            _queryProvider = new QueryProvider<T>(parentContext);
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

        /// <summary>
        /// IQueryable.Provider: represents the Table as a IQueryable provider (hence the name)
        /// </summary>
        public IQueryProvider Provider
        {
            get { return _queryProvider.Provider; }
        }

        #region Insert functions

        void ITable.InsertOnSubmit(object entity)
        {
            Context.RegisterInsert(entity, typeof(T));
        }

        public void InsertOnSubmit(T newObject)
        {
            Context.RegisterInsert(newObject, typeof(T));
        }

        void ITable.InsertAllOnSubmit(IEnumerable entities)
        {
            foreach (var entity in entities)
                Context.RegisterInsert(entity, typeof(T));
        }

        public void InsertAllOnSubmit<TSubEntity>(IEnumerable<TSubEntity> entities) where TSubEntity : T
        {
            foreach (var entity in entities)
                Context.RegisterInsert(entity, typeof(T));
        }

        #endregion

        #region Delete functions

        void ITable.DeleteAllOnSubmit(IEnumerable entities)
        {
            foreach (var entity in entities)
                Context.RegisterDelete(entity, typeof(T));
        }

        /// <summary>
        /// required by ITable interface
        /// </summary>
        /// <param name="entity"></param>
        void ITable.DeleteOnSubmit(object entity)
        {
            Context.RegisterDelete(entity, typeof(T));
        }

        public void DeleteOnSubmit(T objectToDelete)
        {
            Context.RegisterDelete(objectToDelete, typeof(T));
        }

        public void DeleteAllOnSubmit<TSubEntity>(IEnumerable<TSubEntity> entities) where TSubEntity : T
        {
            foreach (var row in entities)
                Context.RegisterDelete(row, typeof(T));
        }

        #endregion

        #region Attach functions

        /// <summary>
        /// required for ITable
        /// </summary>
        /// <param name="entity"></param>
        void ITable.Attach(object entity)
        {
            Context.RegisterUpdate(entity, typeof(T));
        }

        void ITable.Attach(object entity, object original)
        {
            Context.RegisterUpdate(entity, original, typeof(T));
        }

        void ITable.Attach(object entity, bool asModified)
        {
            Context.RegisterUpdate(entity, asModified ? null : entity, typeof(T));
        }

        void ITable.AttachAll(IEnumerable entities)
        {
            foreach (var entity in entities)
                Context.RegisterUpdate(entity, typeof(T));
        }
        void ITable.AttachAll(IEnumerable entities, bool asModified)
        {
            foreach (var entity in entities)
                Context.RegisterUpdate(entity, typeof(T));
        }

        /// <summary>
        /// Attaches an entity from another Context to a table,
        /// with the intention to perform an update or delete operation
        /// </summary>
        /// <param name="entity">table row object to attach</param>
        public void Attach(T entity)
        {
            Context.RegisterUpdate(entity, typeof(T));
        }

        public void AttachAll<TSubEntity>(IEnumerable<TSubEntity> entities) where TSubEntity : T
        {
            foreach (var entity in entities)
                Context.RegisterUpdate(entity, typeof(T));
        }

        /// <summary>
        /// Attaches existing entity with original state
        /// </summary>
        /// <param name="entity">live entity added to change tracking</param>
        /// <param name="original">original unchanged property values</param>
        public void Attach(T entity, T original)
        {
            Context.RegisterUpdate(entity, original, typeof(T));
        }

        #endregion

        #region Save functions

        /// <summary>
        /// Saves all contained entities
        /// This has to move to DataContext (which is the real data pool)
        /// </summary>
        /// <param name="failureMode"></param>
        /// <returns></returns>
        List<Exception> IManagedTable.SaveAll(ConflictMode failureMode)
        {
            if (Context.InsertList.Count<T>() == 0
                && Context.DeleteList.Count<T>() == 0
                && !Context.HasRegisteredEntities<T>())
                return new List<Exception>(); //nothing to do

            var exceptions = new List<Exception>();
            using (Context.DatabaseContext.OpenConnection())
            {
                ProcessInsert(failureMode, exceptions);
                ProcessUpdate(failureMode, exceptions);
                ProcessDelete(failureMode, exceptions);
            }
            return exceptions;
        }

        protected virtual void Process(IEnumerable<T> ts, Action<T, QueryContext> process, ConflictMode failureMode,
            IList<Exception> exceptions)
        {
            var queryContext = new QueryContext(Context);
            foreach (var t in ts)
            {
                try
                {
                    process(t, queryContext);
                }
                catch (Exception e)
                {
                    switch (failureMode)
                    {
                    case ConflictMode.ContinueOnConflict:
                        Trace.WriteLine("Table.SubmitChanges failed: " + e);
                        exceptions.Add(e);
                        break;
                    case ConflictMode.FailOnFirstConflict:
                        throw;
                    }
                }
            }
        }

        protected virtual void ProcessInsert(ConflictMode failureMode, IList<Exception> exceptions)
        {
            var toInsert = new List<T>(Context.InsertList.Enumerate<T>());
            if (Context.Vendor.CanBulkInsert(this))
            {
                Context.Vendor.DoBulkInsert(this, toInsert, Context.Connection);
                Context.InsertList.RemoveRange(toInsert);
            }
            else
            {
                Process(toInsert,
                    delegate(T t, QueryContext queryContext)
                    {
                        var insertQuery = Context.QueryBuilder.GetInsertQuery(t, queryContext);
                        Context.QueryRunner.Insert(t, insertQuery);

                        Context.UnregisterInsert(t, typeof(T));
                        Context.RegisterUpdate(t, typeof(T));
                    }, failureMode, exceptions);
            }
        }

        protected virtual void ProcessUpdate(ConflictMode failureMode, List<Exception> exceptions)
        {
            Process(Context.GetRegisteredEntities<T>(),
                    delegate(T t, QueryContext queryContext)
                    {
                        if (Context.MemberModificationHandler.IsModified(t, Context.Mapping))
                        {
                            var modifiedMembers = Context.MemberModificationHandler.GetModifiedProperties(t, Context.Mapping);
                            var updateQuery = Context.QueryBuilder.GetUpdateQuery(t, modifiedMembers, queryContext);
                            Context.QueryRunner.Update(t, updateQuery, modifiedMembers);

                            Context.RegisterUpdateAgain(t, typeof(T));
                        }
                    }, failureMode, exceptions);
        }

        protected virtual void ProcessDelete(ConflictMode failureMode, List<Exception> exceptions)
        {
            var toDelete = new List<T>(Context.DeleteList.Enumerate<T>());
            Process(toDelete,
                    delegate(T t, QueryContext queryContext)
                    {
                        var deleteQuery = Context.QueryBuilder.GetDeleteQuery(t, queryContext);
                        Context.QueryRunner.Delete(t, deleteQuery);

                        Context.UnregisterDelete(t, typeof(T));
                    }, failureMode, exceptions);
        }

        #endregion

        public bool IsReadOnly { get { return false; } }

        // PC: this will probably required to recreate a new object instance with all original values
        //     (that we currently do not always store, so we may need to make a differential copy
        [Obsolete("NOT IMPLEMENTED YET")]
        [DbLinqToDo]
        public ModifiedMemberInfo[] GetModifiedMembers(object entity)
        {
            throw new ApplicationException("L579 Not implemented");
        }

        // PC: complementary to GetModifiedMembers(), we probably need a few changes to the IMemberModificationHandler,
        //     to recall original values
        [Obsolete("NOT IMPLEMENTED YET")]
        [DbLinqToDo]
        object ITable.GetOriginalEntityState(object entity)
        {
            throw new ApplicationException("L585 Not implemented");
        }

    }
}

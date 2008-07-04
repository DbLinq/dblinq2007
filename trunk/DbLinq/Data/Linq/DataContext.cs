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
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

#if MONO_STRICT
using System.Data.Linq.Sugar;
using System.Data.Linq.Identity;
using AttributeMappingSource = System.Data.Linq.Mapping.AttributeMappingSource;
using MappingContext = System.Data.Linq.Mapping.MappingContext;
using DbLinq;
#else
using DbLinq.Data.Linq;
using DbLinq.Data.Linq.Sugar;
using DbLinq.Data.Linq.Identity;
using AttributeMappingSource = DbLinq.Data.Linq.Mapping.AttributeMappingSource;
using MappingContext = DbLinq.Data.Linq.Mapping.MappingContext;
#endif

using DbLinq.Factory;
using DbLinq.Linq;
using DbLinq.Linq.Database;
using DbLinq.Linq.Database.Implementation;
using DbLinq.Logging;
using DbLinq.Vendor;

#if MONO_STRICT
namespace System.Data.Linq
#else
namespace DbLinq.Data.Linq
#endif
{
    public partial class DataContext : IDisposable
    {
        private readonly Dictionary<string, ITable> _tableMap = new Dictionary<string, ITable>();

        public MetaModel Mapping { get; private set; }
        // PC question: at ctor, we get a IDbConnection and the Connection property exposes a DbConnection
        //              WTF?
        public DbConnection Connection { get { return DatabaseContext.Connection as DbConnection; } }

        // all properties below are set public to optionally be injected
        internal IVendor Vendor { get; set; }
        internal IQueryBuilder QueryBuilder { get; set; }
        internal IQueryRunner QueryRunner { get; set; }
        internal IMemberModificationHandler MemberModificationHandler { get; set; }
        internal IDatabaseContext DatabaseContext { get; private set; }
        internal ILogger Logger { get; set; }
        internal IEntityMap EntityMap { get; set; }
        // /all properties...

        // entities may be registered in 3 sets: InsertList, EntityMap and DeleteList
        // InsertList is for new entities
        // DeleteList is for entities to be deleted
        // EntityMap is the cache: entities are alive in the DataContext, identified by their PK (IdentityKey)
        // an entity can only live in one of the three caches, so the DataContext will provide 6 methods:
        // 3 to register in each list, 3 to unregister
        internal readonly EntityList InsertList = new EntityList();
        internal readonly EntityList DeleteList = new EntityList();

        private IIdentityReaderFactory identityReaderFactory;
        private IDictionary<Type, IIdentityReader> identityReaders = new Dictionary<Type, IIdentityReader>();

        /// <summary>
        /// The default behavior creates one MappingContext.
        /// </summary>
        internal virtual MappingContext _MappingContext { get; set; }

        /// <summary>
        /// A DataContext opens and closes a database connection as needed 
        /// if you provide a closed connection or a connection string. 
        /// In general, you should never have to call Dispose on a DataContext. 
        /// If you provide an open connection, the DataContext will not close it
        /// source: http://msdn2.microsoft.com/en-us/library/bb292288.aspx
        /// </summary>

        private void Init(IDatabaseContext databaseContext, MappingSource mappingSource, IVendor vendor)
        {
            if (databaseContext == null || vendor == null)
                throw new ArgumentNullException("Null arguments");

            Logger = ObjectFactory.Get<ILogger>();

            DatabaseContext = databaseContext;
            Vendor = vendor;

            MemberModificationHandler = ObjectFactory.Create<IMemberModificationHandler>(); // not a singleton: object is stateful
            QueryBuilder = ObjectFactory.Get<IQueryBuilder>();
            QueryRunner = ObjectFactory.Get<IQueryRunner>();

            EntityMap = ObjectFactory.Create<IEntityMap>();
            identityReaderFactory = ObjectFactory.Get<IIdentityReaderFactory>();

            _MappingContext = new MappingContext();

            // initialize the mapping information
            if (mappingSource == null)
                mappingSource = new AttributeMappingSource();
            Mapping = mappingSource.GetModel(GetType());
        }
      

        public Table<T> GetTable<T>(string tableName) where T : class
        {
            lock (this)
            {
                ITable tableExisting;
                if (_tableMap.TryGetValue(tableName, out tableExisting))
                    return tableExisting as Table<T>; //return existing
                Table<T> tableNew = new Table<T>(this); //create new and store it
                _tableMap[tableName] = tableNew;
                return tableNew;
            }
        }

        public Table<T> GetTable<T>() where T : class
        {
            return GetTable<T>(typeof(T).FullName);
        }

        public void SubmitChanges()
        {
            SubmitChanges(ConflictMode.FailOnFirstConflict);
        }

        public virtual List<Exception> SubmitChanges(ConflictMode failureMode)
        {
            List<Exception> exceptions = new List<Exception>();
            //TODO: perform all queued up operations - INSERT,DELETE,UPDATE
            //TODO: insert order must be: first parent records, then child records

            using (DatabaseContext.OpenConnection()) //ConnMgr will close connection for us
            using (IDatabaseTransaction transactionMgr = DatabaseContext.Transaction())
            {
                foreach (IManagedTable table in _tableMap.Values)
                {
                    try
                    {
                        List<Exception> innerExceptions = table.SaveAll(failureMode);
                        exceptions.AddRange(innerExceptions);
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine("Context.SubmitChanges failed: " + ex.Message);
                        switch (failureMode)
                        {
                            case ConflictMode.ContinueOnConflict:
                                exceptions.Add(ex);
                                break;
                            case ConflictMode.FailOnFirstConflict:
                                throw;
                        }
                    }
                }
                bool doCommit = failureMode == ConflictMode.FailOnFirstConflict
                                && exceptions.Count == 0;
                if (doCommit)
                    transactionMgr.Commit();
            }
            return exceptions;
        }

        /// <summary>
        /// TODO - allow generated methods to call into stored procedures
        /// </summary>
        protected IExecuteResult ExecuteMethodCall(DataContext context, System.Reflection.MethodInfo method, params object[] sqlParams)
        {
            using (DatabaseContext.OpenConnection())
            {
                System.Data.Linq.IExecuteResult result = Vendor.ExecuteMethodCall(context, method, sqlParams);
                return result;
            }
        }

        #region Identity management

        internal IIdentityReader _GetIdentityReader(Type t)
        {
            IIdentityReader identityReader;
            if (!identityReaders.TryGetValue(t, out identityReader))
            {
                identityReader = identityReaderFactory.GetReader(t);
                identityReaders[t] = identityReader;
            }
            return identityReader;
        }

        protected void RegisterEntity(object entity)
        {
            var identityReader = _GetIdentityReader(entity.GetType());
            var identityKey = identityReader.GetIdentityKey(entity);
            if (identityKey == null)
                return;
            EntityMap[identityKey] = entity;
        }

        protected object GetRegisteredEntity(object entity)
        {
            var identityReader = _GetIdentityReader(entity.GetType());
            var identityKey = identityReader.GetIdentityKey(entity);
            if (identityKey == null) // if we don't have an entitykey here, it means that the entity has no PK
                return entity;
            var registeredEntity = EntityMap[identityKey];
            return registeredEntity;
        }

        internal object GetRegisteredEntityByKey(IdentityKey identityKey)
        {
            return EntityMap[identityKey];
        }

        protected object GetOrRegisterEntity(object entity)
        {
            var identityReader = _GetIdentityReader(entity.GetType());
            var identityKey = identityReader.GetIdentityKey(entity);
            if (identityKey == null)
                return entity;
            var registeredEntity = EntityMap[identityKey];
            if (registeredEntity == null)
            {
                registeredEntity = entity;
                EntityMap[identityKey] = entity;
            }
            return registeredEntity;
        }

        internal IEnumerable<T> GetRegisteredEntities<T>()
        {
            foreach (IdentityKey key in EntityMap.Keys)
            {
                if (key.Type == typeof(T))
                    yield return (T)EntityMap[key];
            }
        }

        // TODO: remove this
        internal bool HasRegisteredEntities<T>()
        {
            foreach (IdentityKey key in EntityMap.Keys)
            {
                if (key.Type == typeof(T))
                    return true;
            }
            return false;
        }

        #endregion

        #region Insert/Update/Delete management

        protected virtual void CheckNotRegisteredForInsert(object entity, Type asType)
        {
            if (InsertList.Contains(entity, asType))
                throw new ArgumentException("Object already registered for insertion");
        }

        protected virtual void CheckNotRegisteredForUpdate(object entity, Type asType)
        {
            if (GetRegisteredEntity(entity) != null)
                throw new ArgumentException("Object already attached");
        }

        protected virtual void CheckRegisteredForUpdate(object entity, Type asType)
        {
            if (GetRegisteredEntity(entity) == null)
                throw new ArgumentException("Object not attached");
        }

        protected virtual void CheckNotRegisteredForDelete(object entity, Type asType)
        {
            if (DeleteList.Contains(entity, asType))
                throw new ArgumentException("Object already registered for deletion");
        }

        /// <summary>
        /// Checks if the entity is not already registered somewhere in some way
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="asType"></param>
        protected virtual void CheckNotRegistered(object entity, Type asType)
        {
            CheckNotRegisteredForInsert(entity, asType);
            CheckNotRegisteredForUpdate(entity, asType);
            CheckNotRegisteredForDelete(entity, asType);
        }

        /// <summary>
        /// Registers an entity for insert
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="asType"></param>
        internal void RegisterInsert(object entity, Type asType)
        {
            CheckNotRegistered(entity, asType);
            InsertList.Add(entity, entity.GetType());
        }

        /// <summary>
        /// Registers an entity for update
        /// The entity will be updated only if some of its members have changed after the registration
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="asType"></param>
        internal void RegisterUpdate(object entity, Type asType)
        {
            CheckNotRegistered(entity, asType);
            Register(entity, asType);
        }

        /// <summary>
        /// Registers or re-registers an entity and clears its state
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="asType"></param>
        /// <returns></returns>
        internal object Register(object entity, Type asType)
        {
            var registeredEntity = GetOrRegisterEntity(entity);
            // the fact of registering again clears the modified state, so we're... clear with that
            MemberModificationHandler.Register(registeredEntity, Mapping);
            return registeredEntity;
        }

        /// <summary>
        /// Registers an entity for update
        /// The entity will be updated only if some of its members have changed after the registration
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="entityOriginalState"></param>
        /// <param name="asType"></param>
        internal void RegisterUpdate(object entity, object entityOriginalState, Type asType)
        {
            CheckNotRegistered(entity, asType);
            RegisterEntity(entity);
            MemberModificationHandler.Register(entity, entityOriginalState, Mapping);
        }

        /// <summary>
        /// Clears the current state, and marks the object as clean
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="asType"></param>
        internal void RegisterUpdateAgain(object entity, Type asType)
        {
            MemberModificationHandler.ClearModified(entity, Mapping);
        }

        /// <summary>
        /// Registers an entity for delete
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="asType"></param>
        internal void RegisterDelete(object entity, Type asType)
        {
            CheckNotRegisteredForInsert(entity, asType);
            CheckRegisteredForUpdate(entity, asType);
            CheckNotRegisteredForDelete(entity, asType);
            DeleteList.Add(entity, asType);
        }

        internal void UnregisterInsert(object entity, Type asType)
        {
            if (!InsertList.Contains(entity, asType))
                throw new ArgumentException("Object not registered for insertion");
            InsertList.Remove(entity, asType);
        }

        internal void UnregisterUpdate(object entity, Type asType)
        {
            var identityReader = _GetIdentityReader(entity.GetType());
            var identityKey = identityReader.GetIdentityKey(entity);
            if (EntityMap[identityKey] == null)
                throw new ArgumentException("Object not attached");
            EntityMap.Remove(identityKey);
            MemberModificationHandler.Unregister(entity);
        }

        internal void UnregisterDelete(object entity, Type asType)
        {
            if (!DeleteList.Contains(entity, asType))
                throw new ArgumentException("Object not registered for deletion");
            DeleteList.Remove(entity, asType);
        }

        #endregion

        /// <summary>
        /// Changed object determine 
        /// </summary>
        /// <returns>Lists of inserted, updated, deleted objects</returns>
        public ChangeSet GetChangeSet()
        {
            var inserts = InsertList.EnumerateAll().ToList();
            var updates =
                (from k in EntityMap.Keys
                 let e = EntityMap[k]
                 where MemberModificationHandler.IsModified(e, Mapping)
                 select e).ToList();
            var deletes = DeleteList.EnumerateAll().ToList();
            return new ChangeSet(inserts, updates, deletes);
        }

        /// <summary>
        /// TODO: conflict detection is not implemented!
        /// </summary>
        [Obsolete("NOT IMPLEMENTED YET")]
        public ChangeConflictCollection ChangeConflicts
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// use ExecuteCommand to call raw SQL
        /// </summary>
        public int ExecuteCommand(string command, params object[] parameters)
        {
            using (DatabaseContext.OpenConnection())
            {
                return Vendor.ExecuteCommand(this, command, parameters);
            }
        }

        /// <summary>
        /// Execute raw SQL query and return object
        /// </summary>
        public IEnumerable<TResult> ExecuteQuery<TResult>(string command,
                                                          params object[] parameters) where TResult : new()
        {

            using (DatabaseContext.OpenConnection())
            {
                IEnumerable<TResult> res = Vendor.ExecuteQuery<TResult>(this, command, parameters);
                return res;
            }
        }

        /// <summary>
        /// TODO: DataLoadOptions ds = new DataLoadOptions(); ds.LoadWith<Customer>(p => p.Orders);
        /// </summary>
        [Obsolete("NOT IMPLEMENTED YET")]
        [DbLinqToDo]
        public DataLoadOptions LoadOptions
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        [Obsolete("NOT IMPLEMENTED YET")]
        [DbLinqToDo]
        public DbTransaction Transaction
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public IEnumerable<TResult> Translate<TResult>(DbDataReader reader)
        {
            throw new NotImplementedException();
        }

        public IMultipleResults Translate(DbDataReader reader)
        {
            throw new NotImplementedException();
        }


        public IEnumerable Translate(Type elementType, DbDataReader reader)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            //connection closing should not be done here.
            //read: http://msdn2.microsoft.com/en-us/library/bb292288.aspx
        }

        /// <summary>
        /// Creates a IDbDataAdapter. Used internally by Vendors
        /// </summary>
        /// <returns></returns>
        internal IDbDataAdapter CreateDataAdapter()
        {
            return DatabaseContext.CreateDataAdapter();
        }
    }
}

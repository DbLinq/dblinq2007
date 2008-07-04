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
using System.Data.Linq;
using DataContext = DbLinq.Data.Linq.DataContext;
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
        [DBLinqExtended]
        internal virtual MappingContext _MappingContext { get; set; }


        [DbLinqToDo]
        public DataContext(System.Data.IDbConnection connection, System.Data.Linq.Mapping.MappingSource mapping)
        {
            throw new NotImplementedException();
        }
        [DbLinqToDo]
        public DataContext(System.Data.IDbConnection connection)
        {
            throw new NotImplementedException();
        }
        [DbLinqToDo]
        public DataContext(string fileOrServerOrConnection, System.Data.Linq.Mapping.MappingSource mapping)
        {
            throw new NotImplementedException();
        }
        [DbLinqToDo]
        public DataContext(string fileOrServerOrConnection)
        {
            throw new NotImplementedException();
        }
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

        [DBLinqExtended]
        internal ITable _GetTable(System.Type type)
        {
            lock (this)
            {
                string tableName = type.FullName;
                ITable tableExisting;
                if (_tableMap.TryGetValue(tableName, out tableExisting))
                    return tableExisting as ITable; //return existing

                ITable tableNew = Activator.CreateInstance(
                                  typeof(Table<>).MakeGenericType(type)
                                  , System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
                                  , null
                                  , new object[] { this }
                                  , System.Globalization.CultureInfo.CurrentCulture) as ITable;

                _tableMap[tableName] = tableNew;
                return tableNew;
            }
        }

        public Table<TEntity> GetTable<TEntity>() where TEntity : class
        {
            return _GetTable(typeof(TEntity)) as Table<TEntity>;
        }

        public ITable GetTable(System.Type type)
        {
            return _GetTable(type);
        }

        public void SubmitChanges()
        {
            SubmitChanges(ConflictMode.FailOnFirstConflict);
        }

        /// <summary>
        /// Pings database
        /// </summary>
        /// <returns></returns>
        public bool DatabaseExists()
        {
            try
            {
                using (DatabaseContext.OpenConnection())
                {
                    //command: "SELECT 11" (Oracle: "SELECT 11 FROM DUAL")
                    string SQL = Vendor.SqlPingCommand;
                    int result = Vendor.ExecuteCommand(this, SQL);
                    return result == 11;
                }
            }
            catch (Exception ex)
            {
                if (true)
                    Trace.WriteLine("DatabaseExists failed:" + ex);
                return false;
            }
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

        [DbLinqToDo]
        protected IExecuteResult ExecuteMethodCall(object instance, System.Reflection.MethodInfo methodInfo, params object[] parameters)
        {
            throw new NotImplementedException();
        }

        #region Identity management

        [DBLinqExtended]
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

        [DBLinqExtended]
        internal void _RegisterEntity(object entity)
        {
            var identityReader = _GetIdentityReader(entity.GetType());
            var identityKey = identityReader.GetIdentityKey(entity);
            if (identityKey == null)
                return;
            EntityMap[identityKey] = entity;
        }

        [DBLinqExtended]
        internal object _GetRegisteredEntity(object entity)
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

        [DBLinqExtended]
        internal object _GetOrRegisterEntity(object entity)
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

        [DBLinqExtended]
        internal void _CheckNotRegisteredForInsert(object entity, Type asType)
        {
            if (InsertList.Contains(entity, asType))
                throw new ArgumentException("Object already registered for insertion");
        }

        [DBLinqExtended]
        internal void _CheckNotRegisteredForUpdate(object entity, Type asType)
        {
            if (_GetRegisteredEntity(entity) != null)
                throw new ArgumentException("Object already attached");
        }

        [DBLinqExtended]
        internal void _CheckRegisteredForUpdate(object entity, Type asType)
        {
            if (_GetRegisteredEntity(entity) == null)
                throw new ArgumentException("Object not attached");
        }

        [DBLinqExtended]
        internal void _CheckNotRegisteredForDelete(object entity, Type asType)
        {
            if (DeleteList.Contains(entity, asType))
                throw new ArgumentException("Object already registered for deletion");
        }

        /// <summary>
        /// Checks if the entity is not already registered somewhere in some way
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="asType"></param>
        [DBLinqExtended]
        internal void _CheckNotRegistered(object entity, Type asType)
        {
            _CheckNotRegisteredForInsert(entity, asType);
            _CheckNotRegisteredForUpdate(entity, asType);
            _CheckNotRegisteredForDelete(entity, asType);
        }

        /// <summary>
        /// Registers an entity for insert
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="asType"></param>
        internal void RegisterInsert(object entity, Type asType)
        {
            _CheckNotRegistered(entity, asType);
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
            _CheckNotRegistered(entity, asType);
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
            var registeredEntity = _GetOrRegisterEntity(entity);
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
            _CheckNotRegistered(entity, asType);
            _RegisterEntity(entity);
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
            _CheckNotRegisteredForInsert(entity, asType);
            _CheckRegisteredForUpdate(entity, asType);
            _CheckNotRegisteredForDelete(entity, asType);
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
        public IEnumerable<TResult> ExecuteQuery<TResult>(string query,
                                                          params object[] parameters) where TResult : new()
        {

            using (DatabaseContext.OpenConnection())
            {
                IEnumerable<TResult> res = Vendor.ExecuteQuery<TResult>(this, query, parameters);
                return res;
            }
        }

        [DbLinqToDo]
        public IEnumerable ExecuteQuery(System.Type elementType, string query, params object[] parameters)
        {
            throw new NotImplementedException();
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
            internal set { throw new NotImplementedException(); }
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

        [DbLinqToDo]
        protected virtual void Dispose(bool disposing)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a IDbDataAdapter. Used internally by Vendors
        /// </summary>
        /// <returns></returns>
        internal IDbDataAdapter CreateDataAdapter()
        {
            return DatabaseContext.CreateDataAdapter();
        }

        [DbLinqToDo]
        public System.IO.TextWriter Log
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        [DbLinqToDo]
        public bool ObjectTrackingEnabled
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        [DbLinqToDo]
        public int CommandTimeout
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        [DbLinqToDo]
        public bool DeferredLoadingEnabled
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        [DbLinqToDo]
        public ChangeConflictCollection ChangeConflicts
        {
            get { throw new NotImplementedException(); }
        }

        [DbLinqToDo]
        public DbCommand GetCommand(IQueryable query)
        {
            throw new NotImplementedException();
        }

        [DbLinqToDo]
        public void Refresh(System.Data.Linq.RefreshMode mode, System.Collections.IEnumerable entities)
        {
            throw new NotImplementedException();
        }

        [DbLinqToDo]
        public void Refresh(System.Data.Linq.RefreshMode mode, params object[] entities)
        {
            throw new NotImplementedException();
        }

        [DbLinqToDo]
        public void Refresh(System.Data.Linq.RefreshMode mode, object entity)
        {
            throw new NotImplementedException();
        }

        [DbLinqToDo]
        public void DeleteDatabase()
        {
            throw new NotImplementedException();
        }

        [DbLinqToDo]
        public void CreateDatabase()
        {
            throw new NotImplementedException();
        }

        [DbLinqToDo]
        protected internal IQueryable<TResult> CreateMethodCallQuery<TResult>(object instance, System.Reflection.MethodInfo methodInfo, params object[] parameters)
        {
            throw new NotImplementedException();
        }

        [DbLinqToDo]
        protected internal void ExecuteDynamicDelete(object entity)
        {
            throw new NotImplementedException();
        }

        [DbLinqToDo]
        protected internal void ExecuteDynamicInsert(object entity)
        {
            throw new NotImplementedException();
        }

        [DbLinqToDo]
        protected internal void ExecuteDynamicUpdate(object entity)
        {
            throw new NotImplementedException();
        }
    }
}

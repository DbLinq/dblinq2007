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
using System.Data.Common;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
#if MONO_STRICT
using System.Data.Linq;
#else
using DbLinq.Data.Linq;
#endif
#if MONO_STRICT
using System.Data.Linq.Identity;
#else
using DbLinq.Data.Linq.Identity;
#endif
#if MONO_STRICT
using System.Data.Linq.Sugar;
#else
using DbLinq.Data.Linq.Sugar;
#endif
using DbLinq.Factory;
using DbLinq.Linq;
using DbLinq.Linq.Database;
using DbLinq.Linq.Database.Implementation;
using DbLinq.Logging;
using DbLinq.Vendor;
#if MONO_STRICT
using System.Data.Linq.Sugar;
using AttributeMappingSource = System.Data.Linq.Mapping.AttributeMappingSource;
#else
using AttributeMappingSource = DbLinq.Data.Linq.Mapping.AttributeMappingSource;
#endif
#if MONO_STRICT
using MappingContext = System.Data.Linq.Mapping.MappingContext;
#else
using MappingContext = DbLinq.Data.Linq.Mapping.MappingContext;
#endif

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
        internal IModificationHandler ModificationHandler { get; set; }
        internal IMemberModificationHandler MemberModificationHandler { get; set; }
        internal IDatabaseContext DatabaseContext { get; private set; }
        internal ILogger Logger { get; set; }
        internal IEntityMap EntityMap { get; set; }
        // /all properties...

        internal readonly EntityList InsertList = new EntityList();
        internal readonly EntityList DeleteList = new EntityList();

        private IIdentityReaderFactory identityReaderFactory;
        private IDictionary<Type, IIdentityReader> identityReaders = new Dictionary<Type, IIdentityReader>();

        /// <summary>
        /// The default behavior creates one MappingContext.
        /// </summary>
        public virtual MappingContext MappingContext { get; set; }

        /// <summary>
        /// A DataContext opens and closes a database connection as needed 
        /// if you provide a closed connection or a connection string. 
        /// In general, you should never have to call Dispose on a DataContext. 
        /// If you provide an open connection, the DataContext will not close it
        /// source: http://msdn2.microsoft.com/en-us/library/bb292288.aspx
        /// </summary>
        public DataContext(IDatabaseContext databaseContext, MappingSource mappingSource, IVendor vendor)
        {
            if (databaseContext == null || vendor == null)
                throw new ArgumentNullException("Null arguments");

            Logger = ObjectFactory.Get<ILogger>();

            DatabaseContext = databaseContext;
            Vendor = vendor;

            ModificationHandler = ObjectFactory.Create<IModificationHandler>(); // not a singleton: object is stateful
            MemberModificationHandler = ObjectFactory.Create<IMemberModificationHandler>(); // not a singleton: object is stateful
            QueryBuilder = ObjectFactory.Get<IQueryBuilder>();
            QueryRunner = ObjectFactory.Get<IQueryRunner>();

            EntityMap = ObjectFactory.Create<IEntityMap>();
            identityReaderFactory = ObjectFactory.Get<IIdentityReaderFactory>();

            MappingContext = new MappingContext();

            // initialize the mapping information
            if (mappingSource == null)
                mappingSource = new AttributeMappingSource();
            Mapping = mappingSource.GetModel(GetType());
        }

        public DataContext(IDbConnection dbConnection, MappingSource mappingSource, IVendor vendor)
            : this(new DatabaseContext(dbConnection), mappingSource, vendor)
        {
        }


        public DataContext(IDatabaseContext databaseContext, IVendor vendor)
            : this(databaseContext, null, vendor)
        {
        }

        public DataContext(IDbConnection dbConnection, IVendor vendor)
            : this(new DatabaseContext(dbConnection), vendor)
        {
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

        protected IIdentityReader GetIdentityReader(Type t)
        {
            IIdentityReader identityReader;
            if (!identityReaders.TryGetValue(t, out identityReader))
            {
                identityReader = identityReaderFactory.GetReader(t);
                identityReaders[t] = identityReader;
            }
            return identityReader;
        }

        internal void RegisterEntity(object entity)
        {
            var identityReader = GetIdentityReader(entity.GetType());
            var identityKey = identityReader.GetIdentityKey(entity);
            if (identityKey == null)
                return;
            EntityMap[identityKey] = entity;
        }

        internal object GetRegisteredEntityByKey(IdentityKey identityKey)
        {
            return EntityMap[identityKey];
        }

        internal object GetRegisteredEntity(object entity)
        {
            var identityReader = GetIdentityReader(entity.GetType());
            var identityKey = identityReader.GetIdentityKey(entity);
            if (identityKey == null) // if we don't have an entitykey here, it means that the entity has no PK
                return entity;
            var registeredEntity = EntityMap[identityKey];
            return registeredEntity;
        }

        internal object GetOrRegisterEntity(object entity)
        {
            var identityReader = GetIdentityReader(entity.GetType());
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

        /// <summary>
        /// Changed object determine 
        /// </summary>
        /// <returns>Lists of inserted, updated, deleted objects</returns>
        public ChangeSet GetChangeSet()
        {
            var inserts = InsertList.EnumerateAll().ToList();
            var updates = (from k in EntityMap.Keys let e = EntityMap[k] where ModificationHandler.IsModified(e) select e).ToList();
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
        public DataLoadOptions LoadOptions
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        [Obsolete("NOT IMPLEMENTED YET")]
        public DbTransaction Transaction
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
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

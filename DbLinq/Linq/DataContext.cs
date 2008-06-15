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
using System.Data.Linq.Mapping;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using DbLinq.Factory;
using DbLinq.Linq.Database;
using DbLinq.Linq.Database.Implementation;
using DbLinq.Linq.Identity;
using DbLinq.Logging;
using DbLinq.Vendor;
using QueryGenerator=DbLinq.Data.Linq.Sugar.Implementation.QueryGenerator;

namespace DbLinq.Linq
{
    public class DataContext : IDisposable
    {
        internal /*private*/ readonly List<IMTable> _tableList = new List<IMTable>();
        private readonly Dictionary<string, IMTable> _tableMap = new Dictionary<string, IMTable>();

        public MetaModel Mapping { get; private set; }

        // all properties below are set public to optionally be injected
        // TODO check if 'internal' works
        public IVendor Vendor { get; set; }
        public IQueryGenerator QueryGenerator { get; set; }
        public IResultMapper ResultMapper { get; set; }
        public IModificationHandler ModificationHandler { get; set; }
        public IDatabaseContext DatabaseContext { get; private set; }
        public ILogger Logger { get; set; }
        public IEntityMap EntityMap { get; set; }
        // /all properties...

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

            ResultMapper = ObjectFactory.Get<IResultMapper>();
            ModificationHandler = ObjectFactory.Create<IModificationHandler>(); // not a singleton: object is stateful
            //QueryGenerator = ObjectFactory.Get<QueryGenerator>();
            QueryGenerator = ObjectFactory.Get<QueryGenerator>();

            EntityMap = ObjectFactory.Create<IEntityMap>();
            identityReaderFactory = ObjectFactory.Get<IIdentityReaderFactory>();

            MappingContext = new MappingContext();

            // initialize the mapping information
            if (mappingSource == null)
                mappingSource = new Mapping.AttributeMappingSource();
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

        public Table<T> GetTable<T>(string tableName) where T : class
        {
            lock (this)
            {
                IMTable tableExisting;
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

        public void RegisterChild(IMTable table)
        {
            _tableList.Add(table);
        }

        public void SubmitChanges()
        {
            SubmitChanges(System.Data.Linq.ConflictMode.FailOnFirstConflict);
        }

        public virtual List<Exception> SubmitChanges(System.Data.Linq.ConflictMode failureMode)
        {
            List<Exception> exceptions = new List<Exception>();
            //TODO: perform all queued up operations - INSERT,DELETE,UPDATE
            //TODO: insert order must be: first parent records, then child records

            using (DatabaseContext.OpenConnection()) //ConnMgr will close connection for us

            //TranMgr may start transaction /  commit transaction
            using (IDatabaseTransaction transactionMgr = DatabaseContext.Transaction())
            {
                foreach (IMTable tbl in _tableList)
                {
                    try
                    {
                        List<Exception> innerExceptions = tbl.SaveAll(failureMode);
                        exceptions.AddRange(innerExceptions);
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine("Context.SubmitChanges failed: " + ex.Message);
                        switch (failureMode)
                        {
                        case System.Data.Linq.ConflictMode.ContinueOnConflict:
                            exceptions.Add(ex);
                            break;
                        case System.Data.Linq.ConflictMode.FailOnFirstConflict:
                            throw ex;
                        }
                    }
                }
                bool doCommit = failureMode == System.Data.Linq.ConflictMode.FailOnFirstConflict
                    && exceptions.Count == 0;
                if (doCommit)
                    transactionMgr.Commit();
            }
            return exceptions;
        }

        #region Debugging Support
        /// <summary>
        /// Dlinq spec: Returns the query text of the query without of executing it
        /// </summary>
        /// <returns></returns>
        public string GetQueryText(IQueryable query)
        {
            if (query == null)
                return "GetQueryText: null query";
            IQueryText queryText1 = query as IQueryText;
            if (queryText1 != null)
                return queryText1.GetQueryText(); //so far, MTable_Projected has been updated to use this path

            return "ERROR L78 Unexpected type:" + query;
        }

        /// <summary>
        /// FA: Returns the text of SQL commands for insert/update/delete without executing them
        /// </summary>
        public string GetChangeText()
        {
            return "TODO L56 GetChangeText()";
        }

        #endregion

        /// <summary>
        /// TODO - allow generated methods to call into stored procedures
        /// </summary>
        protected System.Data.Linq.IExecuteResult ExecuteMethodCall(DataContext context, System.Reflection.MethodInfo method, params object[] sqlParams)
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
            return new ChangeSet(this);
        }

        /// <summary>
        /// TODO: conflict detection is not implemented!
        /// </summary>
        [Obsolete("NOT IMPLEMENTED YET")]
        public System.Data.Linq.ChangeConflictCollection ChangeConflicts
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
        public System.Data.Linq.DataLoadOptions LoadOptions
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        [Obsolete("NOT IMPLEMENTED YET")]
        public System.Data.Common.DbTransaction Transaction
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public void Dispose()
        {
            //connection closing should not be done here.
            //read: http://msdn2.microsoft.com/en-us/library/bb292288.aspx
        }
    }

    /// <summary>
    /// Table has a SaveAll() method that Context needs to call
    /// </summary>
    public interface IMTable
    {
        void CheckAttachment(object entity);
        List<Exception> SaveAll(System.Data.Linq.ConflictMode failureMode);
        void SaveAll();

        IEnumerable<object> Inserts { get; }
        IEnumerable<object> Updates { get; }
        IEnumerable<object> Deletes { get; }
    }

    /// <summary>
    /// TODO: can we retrieve _sqlString without requiring an interface?
    /// </summary>
    public interface IQueryText
    {
        string GetQueryText();
    }

}

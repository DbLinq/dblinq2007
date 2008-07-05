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
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Data;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using DbLinq.Factory;
using DbLinq.Linq;
using DbLinq.Logging;
using DbLinq.Util;

#if MONO_STRICT
using DataContext=System.Data.Linq.DataContext;
using Data = System.Data;
#else
using DataContext = DbLinq.Data.Linq.DataContext;
using Data = DbLinq.Data;
#endif

namespace DbLinq.Vendor.Implementation
{
    /// <summary>
    /// some IVendor functionality is the same for many vendors,
    /// implemented here as virtual functions.
    /// </summary>
#if MONO_STRICT
    internal
#else
    public
#endif
    abstract partial class Vendor : IVendor
    {
        public ILogger Logger { get; set; }

        protected Vendor()
            : this(ObjectFactory.Get<ISqlProvider>())
        {
        }

        protected Vendor(ISqlProvider sqlProvider)
        {
            Logger = ObjectFactory.Get<ILogger>();
            this.sqlProvider = sqlProvider;
        }

        public virtual string SqlPingCommand
        {
            get { return "SELECT 11"; }
        }

        private ISqlProvider sqlProvider;
        public ISqlProvider SqlProvider { get { return sqlProvider; } }


        /// <summary>
        /// on Postgres or Oracle, return eg. ':P1'.
        /// Mysql needs to override to return '?P1'
        /// Ingres needs to override to return '?'.
        /// </summary>
        public virtual string GetOrderableParameterName(int index)
        {
            return ":P" + index;
        }

        public virtual bool CanBulkInsert<T>(Data.Linq.Table<T> table) where T : class
        {
            return false;
        }

        public virtual void SetBulkInsert<T>(Data.Linq.Table<T> table, int pageSize) where T : class
        {
        }

        public virtual void DoBulkInsert<T>(Data.Linq.Table<T> table, List<T> rows, IDbConnection conn) where T : class
        {
            throw new NotImplementedException();
        }

        // TODO: we may split this big function is several parts, so we can override them in specific vendors

        public abstract string VendorName { get; }

        public abstract IExecuteResult ExecuteMethodCall(DataContext context, MethodInfo method, params object[] sqlParams);

        protected virtual IDbDataAdapter CreateDataAdapter(DataContext dataContext)
        {
            return dataContext.CreateDataAdapter();
        }

        public virtual IDbDataParameter CreateDbDataParameter(IDbCommand cmd, string dbTypeName, string paramName)
        {
            IDbDataParameter param = cmd.CreateParameter();
            param.ParameterName = paramName;
            return param;
        }

        public virtual bool SupportsOutputParameter { get { return true; } }

        protected FunctionAttribute GetFunctionAttribute(MethodInfo methodInfo)
        {
            return AttribHelper.GetFunctionAttribute(methodInfo);
        }

        protected virtual string ConnectionStringServer { get { return "server"; } }
        protected virtual string ConnectionStringUser { get { return "user id"; } }
        protected virtual string ConnectionStringPassword { get { return "password"; } }
        protected virtual string ConnectionStringDatabase { get { return "database"; } }

        protected virtual void AddConnectionStringPart(IList<string> parts, string name, string value)
        {
            if (!string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(name))
                parts.Add(string.Format("{0}={1}", name, value));
        }

        public virtual string BuildConnectionString(string host, string databaseName, string userName, string password)
        {
            var connectionStringParts = new List<string>();
            AddConnectionStringPart(connectionStringParts, ConnectionStringServer, host);
            AddConnectionStringPart(connectionStringParts, ConnectionStringDatabase, databaseName);
            AddConnectionStringPart(connectionStringParts, ConnectionStringUser, userName);
            AddConnectionStringPart(connectionStringParts, ConnectionStringPassword, password);
            return string.Join(";", connectionStringParts.ToArray());
        }
    }
}

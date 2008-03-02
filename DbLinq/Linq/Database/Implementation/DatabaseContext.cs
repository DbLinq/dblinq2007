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
using System.Data;
using System.Data.Common;

namespace DbLinq.Linq.Database.Implementation
{
    public class DatabaseContext: IDatabaseContext
    {
        private bool _connectionOwner;
        private IDbConnection _connection;
        public IDbConnection Connection
        {
            get { return _connection; }
            set { ChangeConnection(value, false); }
        }

        private DbProviderFactory _providerFactory;
        protected DbProviderFactory ProviderFactory
        {
            get 
            {
                if (_providerFactory == null)
                    throw new Exception("In order to use this method, a DbProviderFactory must be provided");
                return _providerFactory;
            }
        }

        public void Connect(string connectionString)
        {
            IDbConnection connection = ProviderFactory.CreateConnection();
            if (connectionString != null)
                connection.ConnectionString = connectionString;
            ChangeConnection(connection, true);
        }

        public void Disconnect()
        {
            if (Connection != null && _connectionOwner)
                Connection.Close();
        }

        public IDisposable OpenConnection()
        {
            // if we don't own the connection, then it is not our business
            if (_connectionOwner)
                return null;

            return new DatabaseConnection(_connection);
        }

        public IDatabaseTransaction Transaction()
        {
            return new DatabaseTransaction(Connection);
        }

        public IDbCommand CreateCommand()
        {
            IDbCommand command = Connection.CreateCommand();
            if (command.Transaction == null)
                command.Transaction = DatabaseTransaction.CurrentDbTransaction;
            return command;
        }

        public IDbDataAdapter CreateDataAdapter()
        {
            return ProviderFactory.CreateDataAdapter();
        }

        public void Dispose()
        {
            ClearConnection();
        }

        protected void SetConnection(IDbConnection connection, bool owner)
        {
            if (connection == null)
                return;

            _connectionOwner = owner;
            _connection = connection;
            if (owner)
                _connection.Open();
        }

        protected void ClearConnection()
        {
            if (_connectionOwner && _connection != null)
                _connection.Dispose();
        }

        protected void ChangeConnection(IDbConnection connection, bool owner)
        {
            ClearConnection();
            SetConnection(connection, owner);
        }

        public DatabaseContext(DbProviderFactory providerFactory)
            : this(providerFactory, null)
        {
        }

        public DatabaseContext(DbProviderFactory providerFactory, string connectionString)
        {
            _providerFactory = providerFactory;
            Connect(connectionString);
        }

        public DatabaseContext(IDbConnection connection)
        {
            Connection = connection;
        }
    }
}

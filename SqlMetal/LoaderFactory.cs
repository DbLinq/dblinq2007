#region MIT License
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
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Reflection;
using DbLinq.Vendor;

namespace SqlMetal
{
    public class LoaderFactory
    {
        public ISchemaLoader Load(string connectionString, Type dbLinqSchemaLoaderType, Type databaseConnectionType)
        {
            ISchemaLoader loader = (ISchemaLoader)Activator.CreateInstance(dbLinqSchemaLoaderType);
            IDbConnection connection = (IDbConnection)Activator.CreateInstance(databaseConnectionType);
            connection.ConnectionString = connectionString;
            loader.Connection = connection;
            return loader;
        }

        public ISchemaLoader Load(string connectionString, string dbLinqSchemaLoaderType, string databaseConnectionType)
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            ISchemaLoader loader = Load(connectionString, Type.GetType(dbLinqSchemaLoaderType), Type.GetType(databaseConnectionType));
            AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
            return loader;
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            IList<Assembly> assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                if (assembly.GetName().Name == args.Name)
                    return assembly;
            }
            string fileName = args.Name + ".dll";
            if (File.Exists(fileName))
                return Assembly.LoadFile(fileName);
            fileName = args.Name + ".exe";
            if (File.Exists(fileName))
                return Assembly.LoadFile(fileName);
            return null;
        }

        protected string GetConnectionString(SqlMetalParameters mmConfig)
        {
            if (mmConfig.Conn != null)
                return mmConfig.Conn;
            string connectionString = string.Format("server={0};user id={1}; password={2}; database={3}; pooling=false",
                                                    mmConfig.Server, mmConfig.User, mmConfig.Password, mmConfig.Database);
            return connectionString;
        }

        protected void GetLoaderAndConnection(out string dbLinqSchemaLoaderType, out string databaseConnectionType, SqlMetalParameters mmConfig)
        {
            if (mmConfig.Provider != null)
            {
                ProvidersSection configuration = (ProvidersSection)ConfigurationManager.GetSection("providers");
                ProvidersSection.ProviderElement element = configuration.Providers.GetProvider(mmConfig.Provider);
                //databaseConnectionType = types[1].Trim();
                dbLinqSchemaLoaderType = element.DbLinqSchemaLoader;
                databaseConnectionType = element.DatabaseConnection;
            }
            else
            {
                dbLinqSchemaLoaderType = mmConfig.DbLinqSchemaLoaderProvider;
                databaseConnectionType = mmConfig.DatabaseConnectionProvider;
            }
        }

        public ISchemaLoader Load(SqlMetalParameters parameters)
        {
            string dbLinqSchemaLoaderType;
            string databaseConnectionType;
            GetLoaderAndConnection(out dbLinqSchemaLoaderType, out databaseConnectionType, parameters);
            if (dbLinqSchemaLoaderType == null)
                throw new ApplicationException("Please provide -Provider=MySql (or Oracle, OracleODP, PostgreSql, Sqlite - see app.config for provider listing)");
            return Load(GetConnectionString(parameters), dbLinqSchemaLoaderType, databaseConnectionType);
        }
    }
}

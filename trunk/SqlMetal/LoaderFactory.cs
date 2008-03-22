﻿#region MIT License
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
using System.Text;
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
            // try to load from within the current AppDomain
            IList<Assembly> assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                if (assembly.GetName().Name == args.Name)
                    return assembly;
            }
            // try to load the files
            string fileName = args.Name + ".dll";
            if (File.Exists(fileName))
                return Assembly.LoadFile(fileName);
            fileName = args.Name + ".exe";
            if (File.Exists(fileName))
                return Assembly.LoadFile(fileName);
            // try to load from the GAC
            var gacAssembly = GacLoad(args.Name);
            if (gacAssembly != null)
                return gacAssembly;
            return null;
        }

        /// <summary>
        /// This is dirty, and must not be used in production environment
        /// </summary>
        /// <param name="shortName"></param>
        /// <returns></returns>
        private Assembly GacLoad(string shortName)
        {
            string assemblyDirectory = Path.Combine(Environment.GetEnvironmentVariable("SystemRoot"), "Assembly");
            return GacLoad(shortName, Path.Combine(assemblyDirectory, "GAC"));
        }

        private Assembly GacLoad(string shortName, string directory)
        {
            string assemblyDirectory = Path.Combine(directory, shortName);
            if (Directory.Exists(assemblyDirectory))
            {
                Version latestVersion = null;
                string latestVersionDirectory = null;
                foreach (string versionDirectory in Directory.GetDirectories(assemblyDirectory))
                {
                    var testVersion = new Version(Path.GetFileName(versionDirectory).Split('_')[0]);
                    if (latestVersion == null || testVersion.CompareTo(latestVersion) > 0)
                    {
                        latestVersion = testVersion;
                        latestVersionDirectory = versionDirectory;
                    }
                }
                if (latestVersionDirectory != null)
                {
                    string assemblyPath = Path.Combine(latestVersionDirectory, shortName + ".dll");
                    if (File.Exists(assemblyPath))
                        return Assembly.LoadFile(assemblyPath);
                }
            }
            return null;
        }

        protected string GetConnectionString(SqlMetalParameters parameters)
        {
            if (parameters.Conn != null)
                return parameters.Conn;
            var connectionString = new StringBuilder();
            if (!string.IsNullOrEmpty(parameters.Server))
                connectionString.AppendFormat("server={0};", parameters.Server);
            if (!string.IsNullOrEmpty(parameters.User))
                connectionString.AppendFormat("user id={0};", parameters.User);
            if (!string.IsNullOrEmpty(parameters.Password))
                connectionString.AppendFormat("password={0};", parameters.Password);
            if (!string.IsNullOrEmpty(parameters.Database))
                connectionString.AppendFormat("database={0};", parameters.Database);
            return connectionString.ToString();
        }

        protected void GetLoaderAndConnection(out string dbLinqSchemaLoaderType, out string databaseConnectionType, SqlMetalParameters parameters)
        {
            if (parameters.Provider != null)
            {
                ProvidersSection configuration = (ProvidersSection)ConfigurationManager.GetSection("providers");
                ProvidersSection.ProviderElement element = configuration.Providers.GetProvider(parameters.Provider);
                //databaseConnectionType = types[1].Trim();
                dbLinqSchemaLoaderType = element.DbLinqSchemaLoader;
                databaseConnectionType = element.DatabaseConnection;
            }
            else
            {
                dbLinqSchemaLoaderType = parameters.DbLinqSchemaLoaderProvider;
                databaseConnectionType = parameters.DatabaseConnectionProvider;
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

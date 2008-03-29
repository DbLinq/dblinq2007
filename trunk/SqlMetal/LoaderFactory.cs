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
using System.Text;
using DbLinq.Vendor;

namespace SqlMetal
{
    public class LoaderFactory
    {
        /// <summary>
        /// the 'main entry point' into this class
        /// </summary>
        public ISchemaLoader Load(SqlMetalParameters parameters)
        {
            string dbLinqSchemaLoaderType;
            string databaseConnectionType;
            GetLoaderAndConnection(out dbLinqSchemaLoaderType, out databaseConnectionType, parameters);
            if (dbLinqSchemaLoaderType == null)
                throw new ApplicationException("Please provide -Provider=MySql (or Oracle, OracleODP, PostgreSql, Sqlite - see app.config for provider listing)");
            return Load(parameters, dbLinqSchemaLoaderType, databaseConnectionType);
        }

        /// <summary>
        /// given a schemaLoaderType and dbConnType 
        /// (e.g. DbLinq.Oracle.OracleSchemaLoader and System.Data.OracleClient.OracleConnection),
        /// return an instance of the OracleSchemaLoader.
        /// </summary>
        public ISchemaLoader Load(SqlMetalParameters parameters, Type dbLinqSchemaLoaderType, Type databaseConnectionType)
        {
            if (dbLinqSchemaLoaderType == null)
                throw new ArgumentNullException("Null dbLinqSchemaLoaderType");
            if (databaseConnectionType == null)
                throw new ArgumentNullException("Null databaseConnectionType");

            string errorMsg = "";
            try
            {
                errorMsg = "Failed on Activator.CreateInstance(" + dbLinqSchemaLoaderType.Name + ")";
                ISchemaLoader loader = (ISchemaLoader)Activator.CreateInstance(dbLinqSchemaLoaderType);

                errorMsg = "Failed on Activator.CreateInstance(" + databaseConnectionType.Name + ")";
                IDbConnection connection = (IDbConnection)Activator.CreateInstance(databaseConnectionType);

                string connectionString = parameters.Conn;
                if (string.IsNullOrEmpty(connectionString))
                    connectionString = loader.Vendor.BuildConnectionString(parameters.Server, parameters.Database, parameters.User, parameters.Password);
                errorMsg = "Failed on setting ConnectionString=" + connectionString;
                connection.ConnectionString = connectionString;

                errorMsg = "";
                loader.Connection = connection;
                return loader;
            }
            catch (Exception ex)
            {
                //see Pascal's comment on this failure:
                //http://groups.google.com/group/dblinq/browse_thread/thread/b7a29138435b0678
                Console.Error.WriteLine("LoaderFactory.Load(schemaType=" + dbLinqSchemaLoaderType.Name + ", dbConnType=" + databaseConnectionType.Name + ")");
                if (errorMsg != "")
                    Console.Error.WriteLine(errorMsg);
                Console.Error.WriteLine("LoaderFactory.Load() failed: " + ex.Message);
                throw ex;
            }
        }

        public ISchemaLoader Load(SqlMetalParameters parameters, string dbLinqSchemaLoaderTypeName, string databaseConnectionTypeName)
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            Type dbLinqSchemaLoaderType = Type.GetType(dbLinqSchemaLoaderTypeName);
            Type databaseConnectionType = Type.GetType(databaseConnectionTypeName);

            if (dbLinqSchemaLoaderType == null)
                throw new ArgumentException("Unable to resolve dbLinqSchemaLoaderType: " + dbLinqSchemaLoaderTypeName);
            if (databaseConnectionType == null)
                throw new ArgumentException("Unable to resolve databaseConnectionType: " + databaseConnectionTypeName);

            ISchemaLoader loader = Load(parameters, dbLinqSchemaLoaderType, databaseConnectionType);
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
/*
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
            
            if (parameters.Provider == "Oracle")
            {
                //Oracle does not allow specifying DB
            }
            else
            {
                if (!string.IsNullOrEmpty(parameters.Database))
                    connectionString.AppendFormat("database={0};", parameters.Database);
            }
            return connectionString.ToString();
        }
        */
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

    }
}

#region MIT license
// 
// MIT license
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
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Reflection;
using System.Text;
using DbLinq.Vendor;
using DbMetal;
using DbMetal.Configuration;
using DbMetal.Generator;
using DbLinq.Logging;

namespace DbMetal
{
    public class SchemaLoaderFactory : ISchemaLoaderFactory
    {
        ILogger Logger = null;

        /// <summary>
        /// the 'main entry point' into this class
        /// </summary>
        public ISchemaLoader Load(Parameters parameters)
        {
            if (Logger == null)
            {
                Logger = DbLinq.Factory.ObjectFactory.Get<ILogger>();
            }

            string dbLinqSchemaLoaderType;
            string databaseConnectionType;
            GetLoaderAndConnection(out dbLinqSchemaLoaderType, out databaseConnectionType, parameters);
            if (dbLinqSchemaLoaderType == null)
                throw new ApplicationException("Please provide -Provider=MySql (or Oracle, OracleODP, PostgreSql, Sqlite - see app.config for provider listing)");
            return Load(parameters, dbLinqSchemaLoaderType, databaseConnectionType);
        }

        /// <summary>
        /// loads a ISchemaLoader from a provider id string (used by schema loader)
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public ISchemaLoader Load(string provider)
        {
            string dbLinqSchemaLoaderType;
            string databaseConnectionType;
            GetLoaderAndConnection(out dbLinqSchemaLoaderType, out databaseConnectionType, provider);
            if (dbLinqSchemaLoaderType == null)
                return null;
            return Load(null, dbLinqSchemaLoaderType, databaseConnectionType);
        }

        /// <summary>
        /// given a schemaLoaderType and dbConnType 
        /// (e.g. DbLinq.Oracle.OracleSchemaLoader and System.Data.OracleClient.OracleConnection),
        /// return an instance of the OracleSchemaLoader.
        /// </summary>
        public ISchemaLoader Load(Parameters parameters, Type dbLinqSchemaLoaderType, Type databaseConnectionType)
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

                if (parameters != null)
                {
                    string connectionString = parameters.Conn;
                    if (string.IsNullOrEmpty(connectionString))
                        connectionString = loader.Vendor.BuildConnectionString(parameters.Server, parameters.Database,
                                                                               parameters.User, parameters.Password);
                    errorMsg = "Failed on setting ConnectionString=" + connectionString;
                    connection.ConnectionString = connectionString;
                }

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

        public ISchemaLoader Load(Parameters parameters, string dbLinqSchemaLoaderTypeName, string databaseConnectionTypeName)
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

        private Assembly LoadAssembly(string path)
        {
            if (File.Exists(path))
                return Assembly.LoadFile(path);
            return null;
        }

        private Assembly LoadAssembly(string baseName, string path)
        {
            string basePath = Path.Combine(path, baseName);
            Assembly assembly = LoadAssembly(basePath + ".dll");
            if (assembly == null)
                assembly = LoadAssembly(basePath + ".exe");
            return assembly;
        }

        private Assembly LocalLoadAssembly(string baseName)
        {
            Assembly assembly = LoadAssembly(baseName, Directory.GetCurrentDirectory());
            if (assembly == null)
                assembly = LoadAssembly(baseName, new Uri(Path.GetDirectoryName(Assembly.GetEntryAssembly().CodeBase)).LocalPath);
            return assembly;
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            // try to load from within the current AppDomain
            IList<Assembly> assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly loadedAssembly in assemblies)
            {
                if (loadedAssembly.GetName().Name == args.Name)
                    return loadedAssembly;
            }
            var assembly = LocalLoadAssembly(args.Name);
            if (assembly == null)
                assembly = GacLoadAssembly(args.Name);
            return assembly;
        }

        /// <summary>
        /// This is dirty, and must not be used in production environment
        /// </summary>
        /// <param name="shortName"></param>
        /// <returns></returns>
        private Assembly GacLoadAssembly(string shortName)
        {
            string assemblyDirectory = Path.Combine(Environment.GetEnvironmentVariable("SystemRoot"), "Assembly");
            var assembly = GacLoadAssembly(shortName, Path.Combine(assemblyDirectory, "GAC_MSIL"));
            if (assembly == null)
                return GacLoadAssembly(shortName, Path.Combine(assemblyDirectory, "GAC_32"));
            if (assembly == null)
                return GacLoadAssembly(shortName, Path.Combine(assemblyDirectory, "GAC"));
            return assembly;
        }

        private Assembly GacLoadAssembly(string shortName, string directory)
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

        protected void GetLoaderAndConnection(out string dbLinqSchemaLoaderType, out string databaseConnectionType, string provider)
        {
            var configuration = (ProvidersSection)ConfigurationManager.GetSection("providers");

            ProvidersSection.ProviderElement element;
            string errorMsg;
            if (!configuration.Providers.TryGetProvider(provider, out element, out errorMsg))
            {
                Logger.Write(Level.Error, "Failed to load provider " + provider + ": " + errorMsg);
                throw new ApplicationException("Failed to load provider " + provider);
            }

            //var element = configuration.Providers.GetProvider(provider);
            //databaseConnectionType = types[1].Trim();
            dbLinqSchemaLoaderType = element.DbLinqSchemaLoader;
            databaseConnectionType = element.DatabaseConnection;
        }

        protected void GetLoaderAndConnection(out string dbLinqSchemaLoaderType, out string databaseConnectionType, Parameters parameters)
        {
            if (!string.IsNullOrEmpty(parameters.Provider))
            {
                GetLoaderAndConnection(out dbLinqSchemaLoaderType, out databaseConnectionType, parameters.Provider);
            }
            else
            {
                dbLinqSchemaLoaderType = parameters.DbLinqSchemaLoaderProvider;
                databaseConnectionType = parameters.DatabaseConnectionProvider;
            }
        }

    }
}


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

        public ISchemaLoader Load(SqlMetalParameters mmConfig)
        {
            string dbLinqSchemaLoaderType;
            string databaseConnectionType;
            GetLoaderAndConnection(out dbLinqSchemaLoaderType, out databaseConnectionType, mmConfig);
            if (dbLinqSchemaLoaderType == null)
                throw new ApplicationException("Please provide -Provider=MySql (or Oracle, OracleODP, PostgreSql, Sqlite - see app.config for provider listing)");
            return Load(GetConnectionString(mmConfig), dbLinqSchemaLoaderType, databaseConnectionType);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlMetal
{
    public class SqlMetalParameters: Parameters
    {
        /// <summary>
        /// user name for database access
        /// SQLMetal compatible
        /// </summary>
        public string User;
        /// <summary>
        /// user password for database access
        /// SQLMetal compatible
        /// </summary>
        public string Password;
        /// <summary>
        /// server host name
        /// SQLMetal compatible
        /// </summary>
        public string Server;
        /// <summary>
        /// database name
        /// SQLMetal compatible
        /// </summary>
        public string Database;

        /// <summary>
        /// This connection string if present overrides User, Password, Server.
        /// Database is always used to generate the specific DataContext name
        /// SQLMetal compatible
        /// </summary>
        public string Conn;

        /// <summary>
        /// the namespace to put our classes into
        /// SQLMetal compatible
        /// </summary>
        public string Namespace;

        /// <summary>
        /// If present, write out C# code
        /// SQLMetal compatible
        /// </summary>
        public string Code;

        /// <summary>
        /// If present, write out DBML XML representing the DB
        /// SQLMetal compatible
        /// </summary>
        public string Dbml;

        #region Capitalization commands - obsolete

        /// <summary>
        /// convert table name 'products' to class 'Products'
        /// DbLinq specific
        /// </summary>
        public bool ForceUcaseTableName;

        /// <summary>
        /// for mysql, we want to keep case as specified in DB.
        /// DbLinq specific
        /// </summary>
        public bool ForceUcaseFieldName;

        /// <summary>
        /// rename object 'productid' to 'productID'?
        /// DbLinq specific
        /// </summary>
        public bool ForceUcaseID = true;

        /// <summary>
        /// load object renamings from an xml file?
        /// picrap: since we setup a word recognition engine, this may no longer be useful for common cases
        ///         however, it must be kept for fine-tuning
        /// DbLinq specific
        /// </summary>
        public string RenamesFile;

        #endregion

        /// <summary>
        /// this is the "input file" parameter
        /// </summary>
        public string SchemaXmlFile
        {
            get 
            {
                return Extra.Count > 0 ? Extra[0] : null;
            }
        }

        /// <summary>
        /// base class from which all generated entities will inherit
        /// SQLMetal compatible
        /// </summary>
        public string EntityBase;

        /// <summary>
        /// 
        /// </summary>
        public string EntityInterfaces = "IModified";//"INotifyPropertyChanging,INotifyPropertyChanged";
        public IList<string> Interfaces
        {
            get 
            {
                return new List<string>(from entityInterface in EntityInterfaces.Split(',') select entityInterface.Trim());
            }
        }

        /// <summary>
        /// export stored procedures
        /// SQLMetal compatible
        /// </summary>
        public bool SProcs;

        /// <summary>
        /// ??
        /// DbLinq specific
        /// </summary>
        public bool VerboseForeignKeys;

        /// <summary>
        /// when true, we will call Singularize()/Pluralize() functions.
        /// SQLMetal compatible
        /// </summary>
        public bool Pluralize = true;

        /// <summary>
        /// if true, and PostgreSql database contains DOMAINS (typedefs), 
        /// we will generate code DbType='DerivedType'.
        /// if false, generate code DbType='BaseType'.
        /// DbLinq specific
        /// </summary>
        public bool UseDomainTypes = true;

        /// <summary>
        /// force a Console.ReadKey at end of program.
        /// Useful when running from Studio, so the output window does not disappear
        /// picrap comment: you may use the tool to write output to Visual Studio output window instead of a console window
        /// DbLinq specific
        /// </summary>
        public bool ReadLineAtExit;

        /// <summary>
        /// specifies a provider (which here is a pair or ISchemaLoader and IDbConnection implementors)
        /// SQLMetal compatible
        /// </summary>
        public string Provider;

        /// <summary>
        /// For fine tuning, we allow to specifiy an ISchemaLoader
        /// DbLinq specific
        /// </summary>
        public string DbLinqSchemaLoaderProvider;

        /// <summary>
        /// For fine tuning, we allow to specifiy an IDbConnection
        /// DbLinq specific
        /// </summary>
        public string DatabaseConnectionProvider;

        public SqlMetalParameters(string[] args)
            : base(args)
        {
        }
    }
}

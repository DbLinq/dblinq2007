﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlMetal
{
	public class SqlMetalParameters : Parameters
	{
		string user;

		/// <summary>
		/// user name for database access
		/// SQLMetal compatible
		/// </summary>
		public string User
		{
			get { return user; }
			set { user = value; }
		}

		string password;

		/// <summary>
		/// user password for database access
		/// SQLMetal compatible
		/// </summary>
		public string Password
		{
			get { return password; }
			set { password = value; }
		}

		string server = "localhost";

		/// <summary>
		/// server host name
		/// SQLMetal compatible
		/// </summary>
		public string Server
		{
			get { return server; }
			set { server = value; }
		}

		string database;

		/// <summary>
		/// database name
		/// SQLMetal compatible
		/// </summary>
		public string Database
		{
			get { return database; }
			set { database = value; }
		}

		string conn;

		/// <summary>
		/// This connection string if present overrides User, Password, Server.
		/// Database is always used to generate the specific DataContext name
		/// SQLMetal compatible
		/// </summary>
		public string Conn
		{
			get { return conn; }
			set { conn = value; }
		}

		string _namespace;

		/// <summary>
		/// the namespace to put our classes into
		/// SQLMetal compatible
		/// </summary>
		public string Namespace
		{
			get { return _namespace; }
			set { _namespace = value; }
		}

		string code;

		/// <summary>
		/// If present, write out C# code
		/// SQLMetal compatible
		/// </summary>
		public string Code
		{
			get { return code; }
			set { code = value; }
		}

		string dbml;

		/// <summary>
		/// If present, write out DBML XML representing the DB
		/// SQLMetal compatible
		/// </summary>
		public string Dbml
		{
			get { return dbml; }
			set { dbml = value; }
		}

		//#region Capitalization commands - obsolete
		///// <summary>
		///// convert table name 'products' to class 'Products'
		///// DbLinq specific
		///// </summary>
		//public bool ForceUcaseTableName;

		///// <summary>
		///// for mysql, we want to keep case as specified in DB.
		///// DbLinq specific
		///// </summary>
		//public bool ForceUcaseFieldName;

		///// <summary>
		///// rename object 'productid' to 'productID'?
		///// DbLinq specific
		///// </summary>
		//public bool ForceUcaseID = true;
		//#endregion

		/// <summary>
		/// Load object renamings from an xml file
		/// DbLinq specific
		/// </summary>
		public string RenamesFile { get; set; }

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
		public string EntityBase { get; set; }

		/// <summary>
		/// Interfaces to be implemented
		/// </summary>
		public string EntityInterfaces = "IModified";//"INotifyPropertyChanging,INotifyPropertyChanged";
		public string[] Interfaces
		{
			get
			{
				return new List<string>(from entityInterface in EntityInterfaces.Split(',') select entityInterface.Trim()).ToArray();
			}
		}

		/// <summary>
		/// export stored procedures
		/// SQLMetal compatible
		/// </summary>
		public bool SProcs { get; set; }

		/// <summary>
		/// ??
		/// DbLinq specific
		/// </summary>
		public bool VerboseForeignKeys { get; set; }


		bool pluralize = true;

		/// <summary>
		/// when true, we will call Singularize()/Pluralize() functions.
		/// SQLMetal compatible
		/// </summary>
		public bool Pluralize
		{
			get { return pluralize; }
			set { pluralize = value; }
		}

		bool useDomainTypes = true;

		/// <summary>
		/// if true, and PostgreSql database contains DOMAINS (typedefs), 
		/// we will generate code DbType='DerivedType'.
		/// if false, generate code DbType='BaseType'.
		/// DbLinq specific
		/// </summary>
		public bool UseDomainTypes
		{
			get { return useDomainTypes; }
			set { useDomainTypes = value; }
		}

		/// <summary>
		/// force a Console.ReadKey at end of program.
		/// Useful when running from Studio, so the output window does not disappear
		/// picrap comment: you may use the tool to write output to Visual Studio output window instead of a console window
		/// DbLinq specific
		/// </summary>
		public bool ReadLineAtExit { get; set; }

		string provider = "MySql";

		/// <summary>
		/// specifies a provider (which here is a pair or ISchemaLoader and IDbConnection implementors)
		/// SQLMetal compatible
		/// </summary>
		public string Provider
		{
			get { return provider; }
			set { provider = value; }
		}

		/// <summary>
		/// For fine tuning, we allow to specifiy an ISchemaLoader
		/// DbLinq specific
		/// </summary>
		public string DbLinqSchemaLoaderProvider { get; set; }

		/// <summary>
		/// For fine tuning, we allow to specifiy an IDbConnection
		/// DbLinq specific
		/// </summary>
		public string DatabaseConnectionProvider { get; set; }

		public SqlMetalParameters()
		{
		}

		public SqlMetalParameters(string[] args)
			: base(args)
		{
		}
	}
}

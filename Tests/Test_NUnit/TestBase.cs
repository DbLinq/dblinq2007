using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using DbLinq.Factory;
using DbLinq.Logging;
using NUnit.Framework;
using nwind;

#if ORACLE
#if ODP
using xint = System.Int32;
using XSqlConnection = Oracle.DataAccess.Client.OracleConnection;
using XSqlCommand = Oracle.DataAccess.Client.OracleCommand;
#else
using xint = System.Int32;
using XSqlConnection = System.Data.OracleClient.OracleConnection;
using XSqlCommand = System.Data.OracleClient.OracleCommand;
#endif
#elif POSTGRES
using xint = System.Int32;
using XSqlConnection = Npgsql.NpgsqlConnection;
using XSqlCommand = Npgsql.NpgsqlCommand;
#elif SQLITE
using System.Data.SQLite;
using XSqlConnection = System.Data.SQLite.SQLiteConnection;
using XSqlCommand = System.Data.SQLite.SQLiteCommand;
#elif MSSQL
using XSqlConnection = System.Data.SqlClient.SqlConnection;
using XSqlCommand = System.Data.SqlClient.SqlCommand;
using xint = System.UInt32;
#elif INGRES
using XSqlConnection = Ingres.Client.IngresConnection;
using XSqlCommand = Ingres.Client.IngresCommand;
using xint = System.UInt32;
#else
using XSqlConnection = MySql.Data.MySqlClient.MySqlConnection;
using XSqlCommand = MySql.Data.MySqlClient.MySqlCommand;
using xint = System.UInt32;
#endif

namespace Test_NUnit
{
    /// <summary>
    /// base class for ReadTest and WriteTest. 
    /// Provides CreateDB(), Conn, and stringComparisonType.
    /// </summary>
    public abstract class TestBase
    {
        public ILogger Logger { get; set; }

        static bool doRecreate = true;

        public TestBase()
        {
            Logger = ObjectFactory.Get<ILogger>();
        }

        public string DbServer
        {
            get 
            {
                return Environment.GetEnvironmentVariable("DbLinqServer") ?? "localhost";
            }
        }
#if ORACLE
        public string connStr
        {
            get
            {
                return string.Format("data source={0};user id=Northwind; password=linq2", DbServer);
            }
        }
#elif SQLITE
        const string connStr = "Data Source=Northwind.db3";
#elif MSSQL
        const string connStr = "Data Source=.\\SQLExpress;Integrated Security=True;Initial Catalog=Northwind";
#elif POSTGRES

        //Postgres
        public string connStr
        {
            get
            {
                return string.Format("server={0};user id=LinqUser; password=linq2; database=northwind", DbServer);
            }
        }
#else      
        //Mysql
        public string connStr
        {
            get
            {
                return string.Format("server={0};user id=LinqUser; password=linq2; database=Northwind", DbServer);
            }
        }

#endif
        XSqlConnection _conn;
        public XSqlConnection Conn
        {
            get
            {
                if (_conn == null) { _conn = new XSqlConnection(connStr); _conn.Open(); }
                return _conn;
            }
        }

#if POSTGRES || ORACLE
        //Postgres sorting: A,B,C,X,d
        public const StringComparison stringComparisonType = StringComparison.Ordinal; 
#else
        //Mysql,Oracle sorting: A,B,C,d,X
        public const StringComparison stringComparisonType = StringComparison.InvariantCulture;
#endif

        //public Northwind CreateDB()
        //{
        //    return CreateDB(System.Data.ConnectionState.Closed);
        //}

        public Northwind CreateDB()
        {
            return CreateDB(System.Data.ConnectionState.Closed);
            Northwind db = new Northwind(new XSqlConnection(connStr));
            db.Log = Console.Out;
#if SQLITE
            if (doRecreate)
            {
                db.ExecuteCommand(System.IO.File.ReadAllText(@"..\..\..\Example\DbLinq.SQLite.Example\sql\create_Northwind.sql"), new object[] { });
                doRecreate = false;
            }
#endif
            return db;
        }

        public Northwind CreateDB(System.Data.ConnectionState state)
        {
            XSqlConnection conn = new XSqlConnection(connStr);
            if (state==System.Data.ConnectionState.Open)
                conn.Open();
            Northwind db = new Northwind(conn);
            db.Log = Console.Out;
#if SQLITE
            if (doRecreate)
            {
                db.ExecuteCommand(System.IO.File.ReadAllText(@"..\..\..\Example\DbLinq.SQLite.Example\sql\create_Northwind.sql"), new object[] { });
                doRecreate = false;
            }
#endif
            return db;
        }

        /// <summary>
        /// execute a sql statement, return an Int64.
        /// </summary>
        public long ExecuteScalar(string sql)
        {
            using (XSqlCommand cmd = new XSqlCommand(sql, Conn))
            {
                object oResult = cmd.ExecuteScalar();
                Assert.IsNotNull("Expecting result, instead got null. (sql=" + sql + ")");
                Assert.IsInstanceOfType(typeof(long), oResult, "Expecting 'long' result from query " + sql + ", instead got type " + oResult.GetType());
                return (long)oResult;
            }
        }

        /// <summary>
        /// execute a sql statement
        /// </summary>
        public void ExecuteNonQuery(string sql)
        {
            using (XSqlCommand cmd = new XSqlCommand(sql, Conn))
            {
                int iResult = cmd.ExecuteNonQuery();
            }
        }

        public static Product NewProduct(string productName)
        {
            Product p = new Product()
            {
                ProductName = productName,
                SupplierID = 1,
                CategoryID = 1,
                QuantityPerUnit = "11",
#if ORACLE
                UnitPrice = 11, //type "int?"
#else
                UnitPrice = 11m,
#endif
                UnitsInStock = 23,
                UnitsOnOrder = 0,
            };
            return p;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NUnit.Framework;
using nwind;

#if ORACLE
using xint = System.Int32;
using XSqlConnection = System.Data.OracleClient.OracleConnection;
using XSqlCommand = System.Data.OracleClient.OracleCommand;
#elif POSTGRES
using xint = System.Int32;
using XSqlConnection = Npgsql.NpgsqlConnection;
using XSqlCommand = Npgsql.NpgsqlCommand;
#elif SQLITE
using System.Data.SQLite;
using XSqlConnection = System.Data.SQLite.SQLiteConnection;
using XSqlCommand = System.Data.SQLite.SQLiteCommand;
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
        static bool doRecreate = true;
#if ORACLE
        const string connStr = "server=localhost;user id=Northwind; password=linq2";
#elif SQLITE
        const string connStr = "Data Source=Northwind.db3";
#else //Mysql, Postgres
        public const string connStr = "server=localhost;user id=LinqUser; password=linq2; database=Northwind";

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
            Northwind db = new Northwind(connStr);
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

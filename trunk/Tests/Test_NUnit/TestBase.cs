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
#if ORACLE
        const string connStr = "server=localhost;user id=Northwind; password=linq2";
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

#if POSTGRES
        //Postgres sorting: A,B,C,X,d
        public const StringComparison stringComparisonType = StringComparison.Ordinal; 
#else
        //Mysql,Oracle sorting: A,B,C,d,X
        public const StringComparison stringComparisonType = StringComparison.InvariantCulture;
#endif
        public Northwind CreateDB()
        {
            Northwind db = new Northwind(connStr);
            db.Log = Console.Out;
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
                Assert.IsNotNull("Expecting count of Products, instead got null. (sql=" + sql + ")");
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

    }
}

#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne, Pascal Craponne, Pascal Craponne, Pascal Craponne, Pascal Craponne, Pascal Craponne, Pascal Craponne, Pascal Craponne
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
using System.IO;
using System.Xml;
using System.Reflection;
using NUnit.Framework;

#if !MONO_STRICT
using nwind;
using DbLinq.Factory;
#else
using MsNorthwind;
using System.Data.Linq;
#endif

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
#elif FIREBIRD
using XSqlConnection = FirebirdSql.Data.FirebirdClient.FbConnection;
using XSqlCommand = FirebirdSql.Data.FirebirdClient.FbCommand;
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
#if SQLITE
        static bool doRecreate = true;
#endif

        public string DbServer
        {
            get
            {
                return Environment.GetEnvironmentVariable("DbLinqServer") ?? "localhost";
            }
        }
        public string connStr
        {
            get
            {
                var xConnectionStringsDoc = new XmlDocument();
                xConnectionStringsDoc.Load("../ConnectionStrings.xml");
                XmlNode currentAssemblyNode = xConnectionStringsDoc.SelectSingleNode(string.Format("//Connection[@assembly=\"{0}\"]", Assembly.GetCallingAssembly().GetName().Name));
                string stringConnection = currentAssemblyNode.FirstChild.Value.Replace(@"\\", @"\");
                if (stringConnection.Contains("{0}"))
                    stringConnection = string.Format(stringConnection, DbServer);
                return stringConnection;
            }
        }
        XSqlConnection _conn;
        public XSqlConnection Conn
        {
            get
            {
                if (_conn == null) { _conn = new XSqlConnection(connStr); _conn.Open(); }
                return _conn;
            }
        }

#if POSTGRES || ORACLE || INGRES
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

        private static void CheckRecreateSqlite()
        {
#if SQLITE
            if (doRecreate)
            {
                File.Copy(@"..\..\..\src\Northwind.db3", "Northwind.db3", true);
                doRecreate = false;
            }
#endif
        }

        public Northwind CreateDB()
        {
            return CreateDB(System.Data.ConnectionState.Closed);
        }

        public Northwind CreateDB(System.Data.ConnectionState state)
        {
            CheckRecreateSqlite();
            var conn = new XSqlConnection(connStr);
            if (state == System.Data.ConnectionState.Open)
                conn.Open();
            var db = new Northwind(conn) { Log = Console.Out };
            return db;
        }

#if !MONO_STRICT
        public DbLinq.Vendor.IVendor CreateVendor()
        {
            var vendor =
#if MYSQL
    new DbLinq.MySql.MySqlVendor()
#elif ORACLE
    new DbLinq.Oracle.OracleVendor()
#elif POSTGRES
    new DbLinq.PostgreSql.PgsqlVendor()
#elif SQLITE
    new DbLinq.Sqlite.SqliteVendor()
#elif INGRES
    new DbLinq.Ingres.IngresVendor()
#elif MSSQL
    new DbLinq.SqlServer.SqlServerVendor()
#elif FIREBIRD
    new DbLinq.Firebird.FirebirdVendor()
#else
    #error unknown target
#endif
            ;
            return vendor;
        }
#endif

        /// <summary>
        /// execute a sql statement, return an Int64.
        /// </summary>
        public long ExecuteScalar(string sql)
        {
            using (var cmd = new XSqlCommand(sql, Conn))
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
            using (var cmd = new XSqlCommand(sql, Conn))
            {
                int iResult = cmd.ExecuteNonQuery();
            }
        }

        public static Product NewProduct(string productName)
        {
            var p = new Product
            {
                ProductName = productName,
                SupplierID = 1,
                CategoryID = 1,
                QuantityPerUnit = "11",
#if ORACLE || FIREBIRD
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

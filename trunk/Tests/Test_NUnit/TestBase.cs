#region MIT license
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
using System.IO;
using System.Text;
using System.Xml;
using System.Reflection;
using System.Linq;
using DbLinq.Factory;
using DbLinq.Logging;
using NUnit.Framework;

#if !MONO_STRICT
using nwind;
using DbLinq.Linq;
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
        public string connStr
        {
            get
            {
                XmlDocument xConnectionStringsDoc = new XmlDocument();
                xConnectionStringsDoc.Load("../ConnectionStrings.xml");
                XmlNode currentAssemblyNode = xConnectionStringsDoc.SelectSingleNode(string.Format("//Connection[@assembly=\"{0}\"]", Assembly.GetCallingAssembly().GetName().Name));
                string stringConnection = currentAssemblyNode.FirstChild.Value.Replace(@"\\",@"\");
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

        private void CheckRecreateSqlite()
        {
#if SQLITE
            if (doRecreate)
            {
                File.Copy(@"..\..\..\Northwind.db3", "Northwind.db3", true);
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
            XSqlConnection conn = new XSqlConnection(connStr);
            if (state == System.Data.ConnectionState.Open)
                conn.Open();
            Northwind db = new Northwind(conn);
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

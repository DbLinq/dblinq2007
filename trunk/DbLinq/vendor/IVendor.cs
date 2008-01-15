using System;
using System.Data;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Data.Linq;
using System.Data.Linq.Mapping;
//using System.Data.OracleClient;
using DBLinq.util;
using DBLinq.Linq;

namespace DBLinq.vendor
{
    /// <summary>
    /// Vendor - specific part of DBLinq.
    /// </summary>
    public interface IVendor
    {
        /// <summary>
        /// return 'Oracle' or 'Mysql'
        /// </summary>
        string VendorName { get; }

        /// <summary>
        /// simple command to test round-trip functionality against DB:
        /// 'SELECT 11' or
        /// 'SELECT 11 FROM DUAL'
        /// </summary>
        string SqlPingCommand { get; }

        /// <summary>
        /// On Oracle, we have to insert a primary key manually.
        /// On MySql/Pgsql/Mssql, we use the IDENTITY clause to populate it automatically.
        /// </summary>
        IDbDataParameter ProcessPkField(ProjectionData projData, ColumnAttribute colAtt
            , StringBuilder sb, StringBuilder sbValues, StringBuilder sbIdentity, ref int numFieldsAdded);

        void ProcessInsertedId(IDbCommand cmd1, ref object returnedId);

        /// <summary>
        /// string concatenation, eg 'a||b' on Postgres 
        /// </summary>
        string Concat(List<ExpressionAndType> parts);

        /// <summary>
        /// ParamName: on Postgres or Oracle, return ':P1', on Mysql, return '?P1'
        /// </summary>
        string ParamName(int index);

        /// <summary>
        /// given 'User', return '[User]' to prevent a SQL keyword conflict
        /// </summary>
        string FieldName_Safe(string name);

        int ExecuteCommand(DBLinq.Linq.Context context, string sql, params object[] parameters);

        System.Data.Linq.IExecuteResult ExecuteMethodCall(Context context, System.Reflection.MethodInfo method, params object[] sqlParams);

        IDbDataParameter CreateSqlParameter(string dbTypeName, string paramName);


    }

    //############################################################################

    /// <summary>
    /// the only class that instantiates IVendor.
    /// </summary>
    public class VendorFactory
    {
        /// <summary>
        /// Name returned by VendorOra - exact spelling.
        /// </summary>
        public const string ORACLE = "Oracle";

        public const string MYSQL = "Mysql";

        public const string POSTGRESQL = "PostgreSql";

        public const string MSSQLSERVER = "MsSqlServer";

        public const string SQLITE = "SQLite";

#if ORACLE
        public static VendorOra Make()
        {
            return new VendorOra();
        }
#elif MYSQL
        public static mysql.VendorMysql Make()
        {
            return new mysql.VendorMysql();
        }
#elif POSTGRES
        public static pgsql.VendorPgsql Make()
        {
            return new pgsql.VendorPgsql();
        }
#elif MICROSOFT
        public static mssql.VendorMssql Make()
        {
            return new mssql.VendorMssql();
        }
#elif SQLITE
        public static sqlite.VendorSqlite Make()
        {
            return new sqlite.VendorSqlite();
        }
#else
#endif
    }
}

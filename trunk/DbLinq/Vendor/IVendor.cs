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
using DBLinq.Util;
using DBLinq.Linq;

namespace DBLinq.Vendor
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
        IDbDataParameter ProcessPkField(IDbCommand cmd, ProjectionData projData, ColumnAttribute colAtt
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

        /// <summary>
        /// return 'LENGTH' on Oracle,Mysql,PostgreSql, return 'LEN' on MSSql
        /// </summary>
        string String_Length_Function();

        int ExecuteCommand(DBLinq.Linq.DataContext context, string sql, params object[] parameters);

        System.Data.Linq.IExecuteResult ExecuteMethodCall(DBLinq.Linq.DataContext context, System.Reflection.MethodInfo method, params object[] sqlParams);

        IDbDataParameter CreateSqlParameter(IDbCommand cmd, string dbTypeName, string paramName);

        IDataReader2 CreateDataReader2(IDataReader dataReader);

        bool CanBulkInsert<T>(DBLinq.Linq.Table<T> table);
        void SetBulkInsert<T>(DBLinq.Linq.Table<T> table, int pageSize);
        void DoBulkInsert<T>(DBLinq.Linq.Table<T> table, List<T> rows, IDbConnection conn);
    }
}

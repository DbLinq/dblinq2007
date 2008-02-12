using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Data;
using System.Linq.Expressions;
using System.Data.Linq;
using System.Data.Linq.Mapping;
//using System.Data.OracleClient;
using DBLinq.Linq;

namespace DBLinq.Vendor
{
    /// <summary>
    /// some IVendor functionality is the same for many vendors,
    /// implemented here as virtual functions.
    /// </summary>
    public abstract class Vendor //: IVendor
    {
        //public abstract string VendorName { get; }
        //{
        //    get { throw new NotImplementedException(); }
        //}

        public virtual string SqlPingCommand
        {
            get { return "SELECT 11"; }
        }

        //public void ProcessPkField(DBLinq.Linq.ProjectionData projData, System.Data.Linq.Mapping.ColumnAttribute colAtt, StringBuilder sb, StringBuilder sbValues, StringBuilder sbIdentity, ref int numFieldsAdded)
        //{
        //    throw new NotImplementedException();
        //}

        /// <summary>
        /// string concatenation, eg 'a||b' on Oracle.
        /// Customized in Postgres to add casting to varchar.
        /// Customized in Mysql to use CONCAT().
        /// </summary>
        /// <param name="parts"></param>
        /// <returns></returns>
        public virtual string Concat(List<DBLinq.Util.ExpressionAndType> parts)
        {
            string[] arr = parts.Select(p => p.expression).ToArray();
            return string.Join("||", arr);
        }

        /// <summary>
        /// on Postgres or Oracle, return eg. ':P1'.
        /// Mysql needs to override to return '?P1'
        /// </summary>
        public virtual string ParamName(int index)
        {
            return ":P" + index;
        }

        public virtual void ProcessInsertedId(IDbCommand cmd1, ref object returnedId)
        {
            //only Oracle does anything
        }

        public virtual string String_Length_Function()
        {
            return "LENGTH";
        }

        public virtual int ExecuteCommand(DBLinq.Linq.DataContext context, string sql, params object[] parameters)
        {
            IDbConnection conn = context.Connection;
            using (IDbCommand command = conn.CreateCommand())
            {
                string sql2 = ExecuteCommand_PrepareParams(command, sql, parameters);
                command.CommandText = sql2;
                //return command.ExecuteNonQuery();
                object objResult = command.ExecuteScalar();
                if (objResult is int)
                    return (int)objResult;
                if (objResult is long)
                    return (int)(long)objResult;
                if (objResult is decimal)
                    return (int)(decimal)objResult;
                return 0;
            }
        }

        private string ExecuteCommand_PrepareParams(IDbCommand command, string sql, object[] parameters)
        {
            if (parameters.Length == 0)
                return sql; //nothing to do

            int iParam = 0;
            List<string> paramNames = new List<string>();
            foreach (object paramValue in parameters)
            {
                string paramName = ParamName(iParam++);
                IDbDataParameter sqlParam = command.CreateParameter();
                sqlParam.ParameterName = paramName;
                sqlParam.Value = paramValue;
                command.Parameters.Add(sqlParam);
                paramNames.Add(paramName);
            }

            //replace "SET ProductName={0}" -> "SET ProductName=:P0"
            string sql2 = string.Format(sql, paramNames.ToArray());
            return sql2;
        }

        public virtual bool CanBulkInsert<T>(DBLinq.Linq.Table<T> table)
        {
            return false;
        }

        public virtual void SetBulkInsert<T>(DBLinq.Linq.Table<T> table, int pageSize)
        {
        }

        public virtual void DoBulkInsert<T>(DBLinq.Linq.Table<T> table, List<T> rows, IDbConnection conn)
        {
            throw new NotImplementedException();
        }

        protected IDictionary<string, IDictionary<string, Enum>> typeMapsByProperty = new Dictionary<string, IDictionary<string, Enum>>();

        public void SetDataParameterType(IDbDataParameter parameter, string propertyName, string dbTypeName, IDictionary<string, DbType> extraTypes)
        {
            PropertyInfo propertyInfo = parameter.GetType().GetProperty(propertyName);
            IDictionary<string, Enum> typeMaps = GetTypeMaps(propertyInfo);
            string dbTypeKey = dbTypeName.ToLower();
            if (typeMaps.ContainsKey(dbTypeKey))
            {
                propertyInfo.GetSetMethod().Invoke(parameter, new object[] {typeMaps[dbTypeKey]});
            }
            else if (extraTypes.ContainsKey(dbTypeKey))
            {
                parameter.DbType = extraTypes[dbTypeKey];
            }
        }

        private IDictionary<string, Enum> GetTypeMaps(PropertyInfo propertyInfo)
        {
            IDictionary<string, Enum> typeMaps;
            if (typeMapsByProperty.ContainsKey(propertyInfo.Name))
                typeMaps = typeMapsByProperty[propertyInfo.Name];
            else
            {
                typeMaps = new Dictionary<string, Enum>();
                typeMapsByProperty[propertyInfo.Name] = typeMaps;
                // now, we use reflection to enumerate the type possible values
                if (propertyInfo.PropertyType.IsEnum)
                {
                    foreach (Enum value in Enum.GetValues(propertyInfo.PropertyType))
                    {
                        typeMaps[value.ToString().ToLower()] = value;
                    }
                }
            }
            return typeMaps;
        }
    }
}

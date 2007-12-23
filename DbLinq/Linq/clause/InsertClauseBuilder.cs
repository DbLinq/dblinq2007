#region MIT License
////////////////////////////////////////////////////////////////////
// MIT license:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//
// Authors:
//        Jiri George Moudry
////////////////////////////////////////////////////////////////////
#endregion

using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Data.Linq;
using System.Data.Linq.Mapping;

#if ORACLE
using System.Data.OracleClient;
using XSqlConnection = System.Data.OracleClient.OracleConnection;
using XSqlCommand = System.Data.OracleClient.OracleCommand;
using XSqlParameter = System.Data.OracleClient.OracleParameter;
#elif POSTGRES
using XSqlConnection = Npgsql.NpgsqlConnection;
using XSqlCommand = Npgsql.NpgsqlCommand;
using XSqlParameter = Npgsql.NpgsqlParameter;
#elif MICROSOFT
using System.Data.SqlClient;
using XSqlConnection = System.Data.SqlClient.SqlConnection;
using XSqlCommand = System.Data.SqlClient.SqlCommand;
using XSqlParameter = System.Data.SqlClient.SqlParameter;
#else
using MySql.Data.MySqlClient;
using XSqlConnection = MySql.Data.MySqlClient.MySqlConnection;
using XSqlCommand = MySql.Data.MySqlClient.MySqlCommand;
using XSqlParameter = MySql.Data.MySqlClient.MySqlParameter;
#endif
using DBLinq.Linq;
using DBLinq.vendor;
using DBLinq.util;

namespace DBLinq.Linq.clause
{
    public class InsertClauseBuilder
    {
        //static object[] s_emptyIndices = new object[0];
        static Dictionary<string,string> s_logOnceMap = new Dictionary<string,string>();

        /// <summary>
        /// given type Employee, return 'INSERT Employee (ID, Name) VALUES (?p1,?p2)'
        /// (by examining [Table] and [Column] attribs)
        /// </summary>
        public static XSqlCommand GetClause(XSqlConnection conn, object objectToInsert, ProjectionData projData)
        {
            if(conn==null || objectToInsert==null || projData==null)
                throw new ArgumentNullException("InsertClauseBuilder has null args");
            if(projData.fields.Count<1 || projData.fields[0].columnAttribute==null)
                throw new ApplicationException("InsertClauseBuilder need to receive types that have ColumnAttributes");

            StringBuilder sb = new StringBuilder(DBLinq.vendor.Settings.sqlStatementProlog)
                .Append("INSERT INTO ");
            StringBuilder sbValues = new StringBuilder("VALUES (");
            StringBuilder sbIdentity = new StringBuilder();

            sb.Append(projData.tableAttribute.Name).Append(" (");
            List<XSqlParameter> paramList = new List<XSqlParameter>();

            int numFieldsAdded = 0;
            foreach(ProjectionData.ProjectionField projFld in projData.fields)
            {
                ColumnAttribute colAtt = projFld.columnAttribute;

                if (colAtt.IsPrimaryKey) //(colAtt.Id)
                {
                    //on Oracle, populate PK field from associated sequence
                    Vendor.ProcessPkField(projData, colAtt, sb, sbValues, sbIdentity, ref numFieldsAdded);

                    continue; //if field is ID , don't send field
                }

                object paramValue = projFld.GetFieldValue(objectToInsert);
                if (paramValue == null)
                    continue; //don't set null fields

                //append string, eg. ",Name"
                if(numFieldsAdded++> 0){ sb.Append(", "); sbValues.Append(", "); }
                sb.Append(colAtt.Name);

                //get either ":p0" or "?p0"
                string paramName = vendor.Vendor.ParamName(numFieldsAdded);
                sbValues.Append(paramName);

                XSqlParameter param = vendor.Vendor.CreateSqlParameter(colAtt.DbType, paramName);

                param.Value = paramValue;
                paramList.Add(param);
            }
            sb.Append(") ");
            sbValues.Append(") ");
            //" FROM Employee e"
            sb.Append(sbValues.ToString());

            sb.Append(sbIdentity.ToString());

            string sql = sb.ToString();

            if(! s_logOnceMap.ContainsKey(sql))
            {
                Console.WriteLine("SQL INSERT L60: "+sql);
                s_logOnceMap[sql] = "unused"; //log once only
            }

            XSqlCommand cmd = new XSqlCommand(sql,conn);
            foreach(XSqlParameter param in paramList)
            {
                cmd.Parameters.Add(param);
            }
            return cmd;
        }

        /// <summary>
        /// given projData constructed from type Product, 
        /// return string '(ProductName, SupplierID, CategoryID, QuantityPerUnit)'
        /// (suitable for use in INSERT statement)
        /// </summary>
        public static string InsertRowHeader(XSqlConnection conn, ProjectionData projData)
        {
            StringBuilder sbNames = new StringBuilder("(");
            int numFieldsAdded = 0;
            foreach (ProjectionData.ProjectionField projFld in projData.fields)
            {
                ColumnAttribute colAtt = projFld.columnAttribute;

                if (colAtt.IsPrimaryKey) //(colAtt.Id)
                    continue; //if field is ID , don't send field

                //object paramValue = projFld.GetFieldValue(objectToInsert);
                //if (paramValue == null)
                //    continue; //don't set null fields

                //append string, eg. ",Name"
                if (numFieldsAdded++ > 0)
                {
                    sbNames.Append(", "); 
                }
                sbNames.Append(colAtt.Name);
            }
            return sbNames.Append(")").ToString();
        }

        /// <summary>
        /// given an object we want to insert into a table, build the '(?p1,?p2,P3)' 
        /// and package up a list of SqlParameter objects.
        /// 
        /// In Mysql, called multiple times in a row to do a 'bulk insert'.
        /// </summary>
        public static string InsertRowFields(object objectToInsert, ProjectionData projData
            ,List<XSqlParameter> paramList, ref int numFieldsAdded)
        {
            StringBuilder sbVals = new StringBuilder("(");
            string separator = "";
            //int numFieldsAdded = 0;
            foreach (ProjectionData.ProjectionField projFld in projData.fields)
            {
                ColumnAttribute colAtt = projFld.columnAttribute;
                if (colAtt.IsPrimaryKey) //(colAtt.Id)
                    continue; //if field is ID , don't send field

                object paramValue = projFld.GetFieldValue(objectToInsert);
                //if (paramValue == null)
                //    continue; //don't set null fields

                //get either ":p0" or "?p0"
                string paramName = vendor.Vendor.ParamName(numFieldsAdded++);
                sbVals.Append(separator).Append(paramName);
                separator = ", ";

                XSqlParameter param = vendor.Vendor.CreateSqlParameter(colAtt.DbType, paramName);

                param.Value = paramValue;
                paramList.Add(param);
            }
            return sbVals.Append(")").ToString();
        }

        /// <summary>
        /// given type Employee, return 'INSERT Employee (ID, Name) VALUES (?p1,?p2)'
        /// (by examining [Table] and [Column] attribs)
        /// </summary>
        public static XSqlCommand GetUpdateCommand(XSqlConnection conn, object objectToInsert
            ,ProjectionData projData, string ID_to_update)
        {
            if (conn == null || objectToInsert == null || projData == null)
                throw new ArgumentNullException("InsertClauseBuilder has null args");
            if (projData.fields.Count < 1 || projData.fields[0].columnAttribute == null)
                throw new ApplicationException("InsertClauseBuilder need to receive types that have ColumnAttributes");

            StringBuilder sb = new StringBuilder("UPDATE ");
            sb.Append(projData.tableAttribute.Name).Append(" SET ");
            List<XSqlParameter> paramList = new List<XSqlParameter>();

            string primaryKeyName = null;
            int paramIndex=0;
            string separator = "";
            foreach (ProjectionData.ProjectionField projFld in projData.fields)
            {
                ColumnAttribute colAtt = projFld.columnAttribute;

                string columnName_safe = vendor.Vendor.FieldName_Safe(colAtt.Name); //turn 'User' into '[User]'

                if (colAtt.IsPrimaryKey) //colAtt.Id
                {
                    primaryKeyName = columnName_safe;
                    continue; //if field is ID , don't send field
                }

                //PropertyInfo propInfo = projFld.propInfo;
                //object paramValue = propInfo.GetValue(objectToInsert, s_emptyIndices);
                object paramValue = projFld.GetFieldValue(objectToInsert);
                string paramName;
                if (paramValue == null)
                {
                    paramName = "NULL";
                    continue; //don't set null fields
                }
                else
                {
                    paramName = vendor.Vendor.ParamName(paramIndex++);
                }

                //append string, eg. ",Name=:p0"
                sb.Append(separator).Append(columnName_safe).Append("=").Append(paramName);

                separator = ", ";

                XSqlParameter param = vendor.Vendor.CreateSqlParameter(colAtt.DbType, paramName);
                param.Value = paramValue;
                paramList.Add(param);
            }
            // "WHERE myID=11"
            sb.Append(" WHERE ").Append(primaryKeyName).Append("=").Append(ID_to_update);


            string sql = sb.ToString();

            if (!s_logOnceMap.ContainsKey(sql))
            {
                Console.WriteLine("SQL UPDATE L175: " + sql);
                s_logOnceMap[sql] = "unused"; //log once only
            }

            XSqlCommand cmd = new XSqlCommand(sql, conn);
            foreach (XSqlParameter param in paramList)
            {
                cmd.Parameters.Add(param);
            }
            return cmd;
        }


    }
}

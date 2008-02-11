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
using System.Data;

using DBLinq.Linq;
using DBLinq.Vendor;
using DBLinq.Util;

namespace DBLinq.Linq.Clause
{
    public class InsertClauseBuilder
    {
        //static object[] s_emptyIndices = new object[0];
        static Dictionary<string, string> s_logOnceMap = new Dictionary<string, string>();

        /// <summary>
        /// given type Employee, return 'INSERT Employee (ID, Name) VALUES (?p1,?p2)'
        /// (by examining [Table] and [Column] attribs)
        /// </summary>
        public static IDbCommand GetClause(IVendor vendor, IDbConnection conn, object objectToInsert, ProjectionData projData)
        {
            if (vendor == null || conn == null || objectToInsert == null || projData == null)
                throw new ArgumentNullException("InsertClauseBuilder has null args");
            if (projData.fields.Count < 1 || projData.fields[0].columnAttribute == null)
                throw new ApplicationException("InsertClauseBuilder need to receive types that have ColumnAttributes");

            IDbCommand cmd = conn.CreateCommand(); // picrap: moved the command up: it's used by the vendors

            StringBuilder sb = new StringBuilder(DBLinq.Vendor.Settings.sqlStatementProlog)
                .Append("INSERT INTO ");
            StringBuilder sbValues = new StringBuilder("VALUES (");
            StringBuilder sbIdentity = new StringBuilder();

            sb.Append(projData.tableAttribute.Name).Append(" (");
            List<IDbDataParameter> paramList = new List<IDbDataParameter>();

            int numFieldsAdded = 0;
            foreach (ProjectionData.ProjectionField projFld in projData.fields)
            {
                ColumnAttribute colAtt = projFld.columnAttribute;

                if (colAtt.IsPrimaryKey && projData.autoGenField != null)
                {
                    //Note: not every ID is autogen
                    //on Oracle, populate PK field from associated sequence
                    IDbDataParameter outParam = vendor.ProcessPkField(cmd, projData, colAtt, sb, sbValues, sbIdentity, ref numFieldsAdded);
                    if (outParam != null)
                    {
                        paramList.Add(outParam); //only Oracle adds an outParam
                    }
                    continue; //if field is ID , don't send field
                }

                object paramValue = projFld.GetFieldValue(objectToInsert);
                if (paramValue == null)
                    continue; //don't set null fields

                //append string, eg. ",Name"
                if (numFieldsAdded++ > 0) { sb.Append(", "); sbValues.Append(", "); }
                sb.Append(colAtt.Name);

                //get either ":p0" or "?p0"
                string paramName = vendor.ParamName(numFieldsAdded);
                sbValues.Append(paramName);

                IDbDataParameter param = vendor.CreateSqlParameter(cmd, colAtt.DbType, paramName);

                param.Value = paramValue;
                paramList.Add(param);
            }
            sb.Append(") ");
            sbValues.Append(") ");
            //" FROM Employee e"
            sb.Append(sbValues.ToString());

            sb.Append(sbIdentity.ToString());

            string sql = sb.ToString();

            if (!s_logOnceMap.ContainsKey(sql))
            {
                Console.WriteLine("SQL INSERT L60: " + sql);
                s_logOnceMap[sql] = "unused"; //log once only
            }

            cmd.CommandText=sql;
            foreach (IDbDataParameter param in paramList)
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
        public static string InsertRowHeader(IDbConnection conn, ProjectionData projData)
        {
            StringBuilder sbNames = new StringBuilder("(");
            int numFieldsAdded = 0;
            foreach (ProjectionData.ProjectionField projFld in projData.fields)
            {
                ColumnAttribute colAtt = projFld.columnAttribute;

                if (colAtt.IsDbGenerated)
                    continue; //if field is auto-generated ID , don't send field

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
        public static string InsertRowFields(IVendor vendor, IDbCommand cmd, object objectToInsert, ProjectionData projData
            , List<IDbDataParameter> paramList, ref int numFieldsAdded)
        {
            StringBuilder sbVals = new StringBuilder("(");
            string separator = "";
            //int numFieldsAdded = 0;
            foreach (ProjectionData.ProjectionField projFld in projData.fields)
            {
                ColumnAttribute colAtt = projFld.columnAttribute;
                if (colAtt.IsDbGenerated)
                    continue; //if field is auto-generated ID , don't send field

                object paramValue = projFld.GetFieldValue(objectToInsert);

                //get either ":p0" or "?p0"
                string paramName = vendor.ParamName(numFieldsAdded++);
                sbVals.Append(separator).Append(paramName);
                separator = ", ";

                IDbDataParameter param = vendor.CreateSqlParameter(cmd, colAtt.DbType, paramName);

                param.Value = paramValue;
                paramList.Add(param);
            }
            return sbVals.Append(")").ToString();
        }

        /// <summary>
        /// given type Employee, return 'INSERT Employee (ID, Name) VALUES (?p1,?p2)'
        /// (by examining [Table] and [Column] attribs)
        /// </summary>
        public static IDbCommand GetUpdateCommand(SessionVars vars, object objectToInsert
            , ProjectionData projData, string ID_to_update)
        {
            if (vars == null || objectToInsert == null || projData == null)
                throw new ArgumentNullException("InsertClauseBuilder has null args");
            if (projData.fields.Count < 1 || projData.fields[0].columnAttribute == null)
                throw new ApplicationException("InsertClauseBuilder need to receive types that have ColumnAttributes");

            IDbCommand cmd = vars.Context.Connection.CreateCommand();

            StringBuilder sb = new StringBuilder("UPDATE ");
            sb.Append(projData.tableAttribute.Name).Append(" SET ");
            List<IDbDataParameter> paramList = new List<IDbDataParameter>();

            string primaryKeyName = null;
            int paramIndex = 0;
            string separator = "";
            Type pk_type = null;
            foreach (ProjectionData.ProjectionField projFld in projData.fields)
            {
                ColumnAttribute colAtt = projFld.columnAttribute;

                string columnName_safe = vars.Context.Vendor.FieldName_Safe(colAtt.Name); //turn 'User' into '[User]'

                if (colAtt.IsPrimaryKey) 
                {
                    primaryKeyName = columnName_safe;
                    pk_type = projFld.FieldType;
                    continue; //if field is ID , don't send field
                }

                object paramValue = projFld.GetFieldValue(objectToInsert);
                string paramName;
                if (paramValue == null)
                {
                    paramName = "NULL";
                }
                else
                {
                    paramName = vars.Context.Vendor.ParamName(paramIndex++);
                }

                //append string, eg. ",Name=:p0"
                sb.Append(separator).Append(columnName_safe).Append("=").Append(paramName);
                separator = ", ";

                if (paramValue == null)
                {
                    //don't create SqlParameter
                }
                else
                {
                    IDbDataParameter param = vars.Context.Vendor.CreateSqlParameter(cmd, colAtt.DbType, paramName);
                    param.Value = paramValue;
                    paramList.Add(param);
                }
            }

            //append WHERE clause in one of two forms:
            // "WHERE myID=11"
            // "WHERE myID='a'"
            bool mustQuote = pk_type == typeof(char) || pk_type == typeof(string);

            string ID_to_update__quoted = mustQuote
                ? "'" + ID_to_update + "'"
                : ID_to_update;

            sb.Append(" WHERE ")
                .Append(primaryKeyName)
                .Append("=")
                .Append(ID_to_update__quoted);


            string sql = sb.ToString();

            if (!s_logOnceMap.ContainsKey(sql))
            {
                Console.WriteLine("SQL UPDATE L175: " + sql);
                s_logOnceMap[sql] = "unused"; //log once only
            }

            cmd.CommandText=sql;
            foreach (IDbDataParameter param in paramList)
            {
                cmd.Parameters.Add(param);
            }
            return cmd;
        }


    }
}

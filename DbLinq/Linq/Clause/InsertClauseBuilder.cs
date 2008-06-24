#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne
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
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Data.Linq.Mapping;
using System.Data;
#if MONO_STRICT
using System.Data.Linq;
#else
using DbLinq.Data.Linq;
#endif
using DbLinq.Linq;
using DbLinq.Linq.Database;
using DbLinq.Vendor;
using DbLinq.Util;
using System.Diagnostics;

namespace DbLinq.Linq.Clause
{
    public class InsertClauseBuilder
    {
        /// <summary>
        /// given type Employee, return 'INSERT Employee (ID, Name) VALUES (?p1,?p2)'
        /// (by examining [Table] and [Column] attribs)
        /// </summary>
        public static IDbCommand GetClause(IVendor vendor, IDatabaseContext context, object objectToInsert, ProjectionData projData)
        {
            if (vendor == null || context == null || objectToInsert == null || projData == null)
                throw new ArgumentNullException("InsertClauseBuilder has null args");
            if (projData.fields.Count < 1 || projData.fields[0].columnAttribute == null)
                throw new ApplicationException("InsertClauseBuilder need to receive types that have ColumnAttributes");

            IDbCommand cmd = context.CreateCommand();

            StringBuilder sb = new StringBuilder(/*DbLinq.Vendor.Settings.sqlStatementProlog*/)
                .Append("INSERT INTO ");
            StringBuilder sbValues = new StringBuilder("VALUES (");
            StringBuilder sbIdentity = new StringBuilder();

            string tableName_safe = vendor.GetSqlFieldSafeName(projData.tableAttribute.Name); //eg. "[Order Details]"
            sb.Append(tableName_safe).Append(" (");
            List<IDbDataParameter> paramList = new List<IDbDataParameter>();

            int numFieldsAdded = 0;
            foreach (ProjectionData.ProjectionField projFld in projData.fields)
            {
                ColumnAttribute colAtt = projFld.columnAttribute;

                if (colAtt.IsPrimaryKey && projData.AutoGen && !projData.IsAutoGenSpecified(objectToInsert))
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
                string paramName = vendor.GetOrderableParameterName(numFieldsAdded);
                sbValues.Append(paramName);

                IDbDataParameter param = vendor.CreateDbDataParameter(cmd, colAtt.DbType, paramName);

                param.Value = paramValue;
                paramList.Add(param);
            }
            sb.Append(") ");
            sbValues.Append(") ");

            //append " FROM Employee e"
            sb.Append(sbValues.ToString());

            sb.Append(sbIdentity.ToString());

            string sql = sb.ToString();

            //if (!s_logOnceMap.ContainsKey(sql))
            //{
            //    Console.WriteLine("SQL INSERT L60: " + sql);
            //    s_logOnceMap[sql] = "unused"; //log once only
            //}
            Debug.WriteLine("SQL INSERT: " + sql);

            cmd.CommandText = sql;
            foreach (IDbDataParameter param in paramList)
            {
                cmd.CommandText = vendor.ReplaceParamNameInSql(param.ParameterName, cmd.CommandText);
                param.ParameterName = vendor.GetFinalParameterName(param.ParameterName);
                cmd = vendor.AddParameter(cmd, param);
                Debug.Write(", " + param.ParameterName + " = " + param.Value.ToString());
            }
            Debug.WriteLine("");
            return cmd;
        }

        /// <summary>
        /// given projData constructed from type Product, 
        /// return string '(ProductName, SupplierID, CategoryID, QuantityPerUnit)'
        /// (suitable for use in INSERT statement)
        /// </summary>
        public static string InsertRowHeader(ProjectionData projData)
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
                string paramName = vendor.GetOrderableParameterName(numFieldsAdded++);
                sbVals.Append(separator).Append(paramName);
                separator = ", ";

                IDbDataParameter param = vendor.CreateDbDataParameter(cmd, colAtt.DbType, paramName);

                param.Value = paramValue;
                paramList.Add(param);
            }
            return sbVals.Append(")").ToString();
        }

        /// <summary>
        /// given type Employee, return 'UPDATE Employee (Name) VALUES (?p1) WHERE ID='ALFKI' '
        /// (by examining [Table] and [Column] attribs)
        /// </summary>
        public static IDbCommand GetUpdateCommand(DataContext dataContext, object objectToUpdate
            , ProjectionData projData, string[] IDs_to_update, IList<PropertyInfo> modifiedProperties)
        {
            if (dataContext == null || objectToUpdate == null || projData == null)
                throw new ArgumentNullException("InsertClauseBuilder has null args");
            if (projData.fields.Count < 1 || projData.fields[0].columnAttribute == null)
                throw new ApplicationException("InsertClauseBuilder need to receive types that have ColumnAttributes");

            IDbCommand cmd = dataContext.DatabaseContext.CreateCommand();

            string tableName_safe = dataContext.Vendor.GetSqlFieldSafeName(projData.tableAttribute.Name); //eg. "[Order Details]"

            StringBuilder sb = new StringBuilder("UPDATE ");
            sb.Append(tableName_safe).Append(" SET ");

            List<IDbDataParameter> paramList = new List<IDbDataParameter>();

            int paramIndex = 0;
            string separator = "";

            string whereClause = GetPrimaryKeyWhereClause(dataContext, objectToUpdate, projData, IDs_to_update);

            foreach (ProjectionData.ProjectionField projFld in projData.fields)
            {
                // check here if the property was modified
                if (modifiedProperties.FirstOrDefault(modifiedProperty => modifiedProperty.Name == projFld.MemberInfo.Name) == null)
                    continue;

                ColumnAttribute colAtt = projFld.columnAttribute;

                string columnName_safe = dataContext.Vendor.GetSqlFieldSafeName(colAtt.Name); //turn 'User' into '[User]'

                //Toncho, this edit is PostgreSql-specific and breaks MySql,
                //please move this into Vendor.IsFieldNameSafe / MakeFieldNameSafe
                //if (columnName_safe != columnName_safe.ToLower()) columnName_safe = "\"" + columnName_safe + "\""; //toncho11: http://code.google.com/p/dblinq2007/issues/detail?id=24 

                if (colAtt.IsPrimaryKey)
                {
                    //Primary Key field: skip, WHERE clause is already prepared
                    string errorMsg = "PrimaryKey field " + projFld.MemberInfo.Name + " was modified. Instead, DELETE previous row and INSERT another one";
                    throw new InvalidOperationException(errorMsg);
                }
                else
                {
                    //non-primary key field: "Payload"
                    object paramValue = projFld.GetFieldValue(objectToUpdate);
                    string paramName;
                    if (paramValue == null)
                    {
                        paramName = "NULL";
                    }
                    else
                    {
                        paramName = dataContext.Vendor.GetFinalParameterName(dataContext.Vendor.GetOrderableParameterName(paramIndex++));
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
                        IDbDataParameter param = dataContext.Vendor.CreateDbDataParameter(cmd, colAtt.DbType, paramName);
                        param.Value = paramValue;
                        paramList.Add(param);
                    }
                }
            }

            sb.Append(" WHERE ").Append(whereClause);

            string sql = sb.ToString();

            Debug.WriteLine("SQL UPDATE: " + sql);

            //if (!s_logOnceMap.ContainsKey(sql))
            //{
            //    Console.WriteLine("SQL UPDATE L175: " + sql);
            //    s_logOnceMap[sql] = "unused"; //log once only
            //}

            cmd.CommandText = sql;
            foreach (IDbDataParameter param in paramList)
            {
                cmd = dataContext.Vendor.AddParameter(cmd, param);
                Debug.Write(", " + param.ParameterName + " = " + param.Value.ToString());
            }
            Debug.WriteLine("");
            return cmd;
        }

        /// <summary>
        /// given object of type OrderDetails, return WHERE clause which uniquely identifies the object:
        /// ' (OrderID=1 AND ProductID=3) '
        /// (by examining [Table] and [Column] attribs)
        /// </summary>
        public static string GetPrimaryKeyWhereClause(DataContext dataContext, object objectToUpdate
            , ProjectionData projData, string[] IDs_to_update)
        {
            if (dataContext == null || objectToUpdate == null || projData == null)
                throw new ArgumentNullException("InsertClauseBuilder has null args");
            if (projData.fields.Count < 1 || projData.fields[0].columnAttribute == null)
                throw new ApplicationException("InsertClauseBuilder need to receive types that have ColumnAttributes");

            //string separator = "";
            string pkSeparator = " ("; //first WHERE, afterwards AND
            StringBuilder sbPrimaryKeys = new StringBuilder();
            int indexOfIdToUpdate = 0;

            foreach (ProjectionData.ProjectionField projFld in projData.fields)
            {
                ColumnAttribute colAtt = projFld.columnAttribute;

                if (colAtt.IsPrimaryKey)
                {
                    string columnName_safe = dataContext.Vendor.GetSqlFieldSafeName(colAtt.Name); //turn 'User' into '[User]'

                    //Primary Key field: build WHERE clause
                    string primaryKeyName = columnName_safe;
                    Type pk_type = projFld.FieldType;
                    bool mustQuote = pk_type == typeof(char) || pk_type == typeof(string);

                    string ID_to_update = IDs_to_update[indexOfIdToUpdate++];
                    //append WHERE clause in one of two forms:
                    // "WHERE myID=11"
                    // "WHERE myID='a'"
                    string ID_to_update__quoted = mustQuote
                        ? "'" + ID_to_update + "'"
                        : ID_to_update;

                    sbPrimaryKeys.Append(pkSeparator) //WHERE or AND
                        .Append(primaryKeyName)
                        .Append("=")
                        .Append(ID_to_update__quoted);

                    pkSeparator = " AND ";
                }
            }
            sbPrimaryKeys.Append(")");
            return sbPrimaryKeys.ToString();
        }
    }
}

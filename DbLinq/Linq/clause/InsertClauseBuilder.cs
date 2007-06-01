////////////////////////////////////////////////////////////////////
//Initial author: Jiri George Moudry, 2006.
//License: LGPL. (Visit http://www.gnu.org)
//Commercial code may call into this library, if it's in a different module (DLL)
////////////////////////////////////////////////////////////////////

using System;
using System.Query;
using System.Reflection;
using System.Data.DLinq;
using System.Collections.Generic;
using System.Text;
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

            StringBuilder sb = new StringBuilder("INSERT INTO ");
            StringBuilder sbVals = new StringBuilder("VALUES (");
            sb.Append(projData.tableAttribute.Name).Append(" (");
            List<XSqlParameter> paramList = new List<XSqlParameter>();

            int numFieldsAdded = 0;
            foreach(ProjectionData.ProjectionField projFld in projData.fields)
            {
                ColumnAttribute colAtt = projFld.columnAttribute;

                if(colAtt.Id)
                    continue; //if field is ID , don't send field

                //PropertyInfo propInfo = projFld.propInfo;
                //object paramValue = propInfo.GetValue(objectToInsert, s_emptyIndices);
                object paramValue = projFld.GetFieldValue(objectToInsert);
                if (paramValue == null)
                    continue; //don't set null fields

                //append string, eg. ",Name"
                if(numFieldsAdded++> 0){ sb.Append(", "); sbVals.Append(", "); }
                sb.Append(colAtt.Name);

                //get either ":p0" or "?p0"
                string paramName = vendor.Vendor.ParamName(numFieldsAdded);
                sbVals.Append(paramName);

                //OracleType dbType = OracleTypeConversions.ParseType(colAtt.DBType);
                //OracleParameter param = new OracleParameter(paramName, dbType);
                //NpgsqlTypes.NpgsqlDbType dbType = PgsqlTypeConversions.ParseType(colAtt.DBType);
                //XSqlParameter param = new XSqlParameter(paramName, dbType);
                //MySqlDbType dbType = MySqlTypeConversions.ParseType(colAtt.DBType);
                //MySqlParameter param = new MySqlParameter(paramName, dbType);
                XSqlParameter param = vendor.Vendor.CreateSqlParameter(colAtt.DBType, paramName);

                param.Value = paramValue;
                paramList.Add(param);
            }
            sb.Append(") ");
            sbVals.Append(") ");
            //" FROM Employee e"
            sb.Append(sbVals.ToString());
            //sb.Append(";\n");
            //sb.Append("SELECT @@IDENTITY"); //must be on separate line

#if POSTGRES //Postgres needs 'SELECT currval(sequenceName)'
            ColumnAttribute[] colAttribs = AttribHelper.GetColumnAttribs(projData.type);
            ColumnAttribute idColAttrib = colAttribs.FirstOrDefault(c => c.Id);
            string idColName = idColAttrib==null ? "ERROR_L93_MissingIdCol" : idColAttrib.Name;
            string sequenceName = projData.tableAttribute.Name+"_"+idColName+"_seq";
            sb.Append(";SELECT currval('"+sequenceName+"')"); 
#endif

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

                if (colAtt.Id)
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

                XSqlParameter param = vendor.Vendor.CreateSqlParameter(colAtt.DBType, paramName);
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

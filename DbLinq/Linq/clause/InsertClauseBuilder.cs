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
        static object[] s_emptyIndices = new object[0];
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

                PropertyInfo propInfo = projFld.propInfo;
                object paramValue = propInfo.GetValue(objectToInsert, s_emptyIndices);
                if(paramValue==null)
                    continue; //don't set null fields

                //append string, eg. ",Name"
                if(numFieldsAdded++> 0){ sb.Append(", "); sbVals.Append(", "); }
                sb.Append(colAtt.Name);

#if ORACLE || POSTGRES
                string paramName = ":p"+numFieldsAdded;
#else
                string paramName = "?p"+numFieldsAdded;
#endif

                sbVals.Append(paramName);
#if ORACLE
                OracleType dbType = OracleTypeConversions.ParseType(colAtt.DBType);
                OracleParameter param = new OracleParameter(paramName,dbType);
#elif POSTGRES
                NpgsqlTypes.NpgsqlDbType dbType = PgsqlTypeConversions.ParseType(colAtt.DBType);
                XSqlParameter param = new XSqlParameter(paramName,dbType);
#else
                MySqlDbType dbType = MySqlTypeConversions.ParseType(colAtt.DBType);
                MySqlParameter param = new MySqlParameter(paramName,dbType);
#endif
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


    }
}

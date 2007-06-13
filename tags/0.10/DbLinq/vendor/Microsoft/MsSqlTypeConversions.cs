////////////////////////////////////////////////////////////////////
//Initial author: Jiri George Moudry, 2006.
//License: LGPL. (Visit http://www.gnu.org)
//Commercial code may call into this library, if it's in a different module (DLL)
////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;

namespace DBLinq.util
{
    /// <summary>
    /// helper class which help to convert Microsoft Sql's types to SqlClient .NET types,
    /// eg. 'smalldatetime' to SqlDbType.Date.
    /// </summary>
    public static class SqlTypeConversions
    {
        static Dictionary<string, SqlDbType> s_typeMap = new Dictionary<string, SqlDbType>();

        static SqlTypeConversions()
        {
            foreach (SqlDbType dbType in Enum.GetValues(typeof(SqlDbType)))
            {
                s_typeMap[dbType.ToString().ToLower()] = dbType;
            }
        }

        /// <summary>
        /// given name of MySqlType, return it's MySqlDbType enum.
        /// </summary>
        public static SqlDbType ParseType(string typeStr)
        {
            string typeStrL = typeStr.ToLower();

            //convert "DateTime NOT NULL" to "DateTime"
            if (typeStrL.EndsWith(" not null"))
                typeStrL = typeStrL.Substring(0, typeStrL.Length - " NOT NULL".Length);
            
            //shorten "VarChar(50)" to "VarChar"
            int bracket = typeStrL.IndexOf("(");
            if (bracket > 0)
                typeStrL = typeStrL.Substring(0, bracket);


            if(!s_typeMap.ContainsKey(typeStrL))
            {
                switch(typeStrL){
                    case "tinyint":
                        return SqlDbType.Int;
                    case "int":
                        return SqlDbType.Int;
                }
                string msg = "TODO L24: add parsing of type "+typeStr;
                Console.WriteLine(msg);
                throw new ApplicationException(msg);
            }
            return s_typeMap[typeStrL];
        }
    }
}

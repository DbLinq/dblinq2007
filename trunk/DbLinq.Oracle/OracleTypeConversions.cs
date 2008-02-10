using System;
using System.Collections.Generic;
using System.Text;
//using MySql.Data.MySqlClient;
#if NOMORE
using System.Data.OracleClient;
#endif

namespace DBLinq.vendor
{
    /// <summary>
    /// helper class which help to convert MySql's types to MySqlClient .NET types,
    /// eg. 'tinyint' to OracleType.Int16.
    /// </summary>
    public static class OracleTypeConversions
    {
#if NOMORE
        static Dictionary<string,OracleType> s_typeMap = new Dictionary<string,OracleType>();

        static OracleTypeConversions()
        {
            foreach(OracleType dbType in Enum.GetValues(typeof(OracleType)) )
            {
                s_typeMap[dbType.ToString().ToLower()] = dbType;
            }
        }

        /// <summary>
        /// given name of OracleType, return it's OracleType enum.
        /// </summary>
        public static OracleType ParseType(string typeStr)
        {
            string typeStrL = typeStr.ToLower();
            if(!s_typeMap.ContainsKey(typeStrL))
            {
                switch(typeStrL){
                    case "tinyint":
                        return OracleType.Int16;
                    case "int":
                        return OracleType.Int32;
                    case "varchar2":
                        return OracleType.VarChar;
                }
                string msg = "TODO L24: add parsing of type "+typeStr;
                Console.WriteLine(msg);
                throw new ApplicationException(msg);
            }
            return s_typeMap[typeStrL];
        }
#endif
    }
}

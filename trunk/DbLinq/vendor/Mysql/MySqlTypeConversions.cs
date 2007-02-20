using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;

namespace DBLinq.util
{
    /// <summary>
    /// helper class which help to convert MySql's types to MySqlClient .NET types,
    /// eg. 'tinyint' to MySqlDbType.Int16.
    /// </summary>
    public static class MySqlTypeConversions
    {
        static Dictionary<string,MySqlDbType> s_typeMap = new Dictionary<string,MySqlDbType>();

        static MySqlTypeConversions()
        {
            foreach(MySqlDbType dbType in Enum.GetValues(typeof(MySqlDbType)) )
            {
                s_typeMap[dbType.ToString().ToLower()] = dbType;
            }
        }

        /// <summary>
        /// given name of MySqlType, return it's MySqlDbType enum.
        /// </summary>
        public static MySqlDbType ParseType(string typeStr)
        {
            string typeStrL = typeStr.ToLower();
            if(!s_typeMap.ContainsKey(typeStrL))
            {
                switch(typeStrL){
                    case "tinyint":
                        return MySqlDbType.Int16;
                    case "int":
                        return MySqlDbType.Int32;
                }
                string msg = "TODO L24: add parsing of type "+typeStr;
                Console.WriteLine(msg);
                throw new ApplicationException(msg);
            }
            return s_typeMap[typeStrL];
        }
    }
}

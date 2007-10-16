////////////////////////////////////////////////////////////////////
//Initial author: Jiri George Moudry, 2006.
//License: LGPL. (Visit http://www.gnu.org)
//Commercial code may call into this library, if it's in a different module (DLL)
////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;

namespace DBLinq.vendor
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
            if (!s_typeMap.ContainsKey(typeStrL))
            {
                int indxBraket = typeStrL.IndexOf('(');
                if (indxBraket > -1)
                {
                    typeStrL = typeStrL.Substring(0, indxBraket);
                }

                switch (typeStrL)
                {
                    case "char":
                    case "text":
                        return MySqlDbType.String;

                    case "int":
                        return MySqlDbType.Int32;
                    case "mediumint":
                        return MySqlDbType.Int16; //go figure - MEDIUMINT is 3 bytes 
                    case "smallint":
                        return MySqlDbType.Int16;
                    case "tinyint":
                        return MySqlDbType.Byte;
                }
                string msg = "TODO L24: add parsing of type " + typeStr;
                Console.WriteLine(msg);
                throw new ApplicationException(msg);
            }
            return s_typeMap[typeStrL];
        }
    }
}

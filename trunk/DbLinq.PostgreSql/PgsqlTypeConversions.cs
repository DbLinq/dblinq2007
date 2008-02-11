using System;
using System.Collections.Generic;
using System.Text;
using Npgsql;
using NpgsqlTypes;

namespace DbLinq.PostgreSql
{
    /// <summary>
    /// helper class which help to convert MySql's types to MySqlClient .NET types,
    /// eg. 'tinyint' to MySqlDbType.Int16.
    /// </summary>
    public static class PgsqlTypeConversions
    {
        static Dictionary<string, NpgsqlDbType> s_typeMap = new Dictionary<string,NpgsqlDbType>();

        static PgsqlTypeConversions()
        {
            foreach(NpgsqlDbType dbType in Enum.GetValues(typeof(NpgsqlDbType)) )
            {
                s_typeMap[dbType.ToString().ToLower()] = dbType;
            }
        }

        /// <summary>
        /// given name of NpgsqlType, return it's NpgsqlDbType enum.
        /// </summary>
        public static NpgsqlDbType ParseType(string typeStr)
        {
            string typeStrL = typeStr.ToLower();
            int bracket = typeStr.IndexOf('(');
            if (bracket > -1)
            {
                //chop "integer(32,0)" -> "integer"
                typeStrL = typeStrL.Substring(0, bracket);
            }

            if(!s_typeMap.ContainsKey(typeStrL))
            {
                switch(typeStrL){
                    case "tinyint":
                        return NpgsqlDbType.Integer;
                    case "int":
                        return NpgsqlDbType.Integer;
                    case "character":
                        return NpgsqlDbType.Char;

                    case "interval": //: //TODO: really add support for the interval type (after it is supported in Npgsql) 
                    case "character varying":
                        return NpgsqlDbType.Varchar;
                }
                string msg = "TODO L24: add parsing of type "+typeStr;
                Console.WriteLine(msg);
                throw new ApplicationException(msg);
            }
            return s_typeMap[typeStrL];
        }
    }
}
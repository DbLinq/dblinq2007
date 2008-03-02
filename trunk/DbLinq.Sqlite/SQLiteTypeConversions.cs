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

using System;
using System.Collections.Generic;
using System.Text;

namespace DbLinq.Sqlite
{
    /// <summary>
    /// helper class which help to convert SQLite's types to SQLiteClient .NET types,
    /// eg. 'tinyint' to SqlDbType.Int16.
    /// </summary>
    public static class SqliteTypeConversions
    {
        static Dictionary<string, System.Data.DbType> s_typeMap = new Dictionary<string, System.Data.DbType>();

        static SqliteTypeConversions()
        {
            foreach (System.Data.DbType dbType in Enum.GetValues(typeof(System.Data.DbType)))
            {
                s_typeMap[dbType.ToString().ToLower()] = dbType;
            }
        }

        /// <summary>
        /// given name of SQLiteType, return it's SqlDbType enum.
        /// </summary>
        public static System.Data.DbType ParseType(string typeStr)
        {
            string typeStrL = typeStr.ToLower();
            if (!s_typeMap.ContainsKey(typeStrL))
            {
                int indxBraket = typeStrL.IndexOf('(');
                if (indxBraket > -1)
                {
                    typeStrL = typeStrL.Substring(0, indxBraket);
                }

                switch (typeStrL.ToLower())
                {
                    case "char":
                    case "varchar":
                    case "text":
                    case "string":
                        return System.Data.DbType.String;

                    case "real":
                        return System.Data.DbType.Decimal;

                    case "bit":
                        return System.Data.DbType.Byte;
                    case "int":
                    case "integer":
                    case "mediumint":
                        return System.Data.DbType.Int32; //go figure - MEDIUMINT is 3 bytes 
                    case "smallint":
                    case "tinyint":
                        return System.Data.DbType.Int16;
                }
                string msg = "TODO L24: add parsing of type " + typeStr;
                Console.WriteLine(msg);
                throw new ApplicationException(msg);
            }
            return s_typeMap[typeStrL];
        }
    }
}
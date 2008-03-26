#region MIT license
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
//        Thomas Glaser
////////////////////////////////////////////////////////////////////
#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace DbLinq.Ingres.Schema
{
    static class Mappings
    {
        /// <summary>
        /// map 'varchar' to string
        /// </summary>
        /// <param name="mysqlType"></param>
        /// <param name="column_type">e.g. 'unsigned'</param>
        public static string mapSqlTypeToCsType(string mysqlType, string column_type)
        {
            #region map "varchar" to "string"
            switch (mysqlType.Trim().ToLower())
            {
                case "nvarchar":
                case "nchar":
                case "varchar": 
                case "longtext": 
                case "text":
                case "char":
                case "character":
                case "character varying":
                case "inet":
                    return "string";

                case "int": 
                    if(column_type.Contains("unsigned"))
                        return "uint";
                    return "int";

                case "bit":
                case "boolean":
                    //case "ebool": //this is Adrus' domain type. TODO: handle domain types.
                    return "bool";

                case "tinyint": 
                    if(column_type=="tinyint(1)")
                        return "bool";
                    return "char";
                case "smallint": return "short";
                case "mediumint": return "short";
                case "bigint": return "long";

                case "interval":
                    return "TimeSpan";
                case "date":
                case "datetime":
                case "timestamp":
                case "timestamp without time zone":
                case "time without time zone": //reported by twain_bu...@msn.com,
                case "time with time zone":
                    return "DateTime";

                case "enum": 
                    return "Enum";
                case "float": 
                    return "float";

                case "double":
                case "double precision":
                    return "double";

                case "decimal":
                case "numeric":
                    return "decimal";

                case "blob":
                case "oid":
                case "bytea":
                    return "byte[]";

                    //TODO: blob,longblob,set, ...
                case "integer": return "int";
                case "void": 
                    return "void";
                default:
                    return "L52_mapCsType_unprepared_for_ingrestype_"+mysqlType;
            }
            #endregion
        }

    }
}
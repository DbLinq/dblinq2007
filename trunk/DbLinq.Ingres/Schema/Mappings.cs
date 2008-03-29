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
        public static string mapSqlTypeToCsType(string sqlType, string column_type, int length)
        {
            switch (sqlType.Trim())
            {
                case "C":
                case "CHAR":
                case "NCHAR":
                case "VARCHAR":
                case "NVARCHAR":
                case "LONG VARCHAR":
                case "TEXT":
                    return "System.String";

                case "DATE":
                case "INGRESDATE":
                    return "System.DateTime";

                case "INTEGER":
                    switch (length)
                    {
                        case 1:
                        case 2:
                            return "System.Int16";
                        case 4:
                            return "System.Int32";
                        case 8:
                            return "System.Int64";
                        default:
                            return "L52_mapCsType_unprepared_for_ingrestype_"+sqlType+"_of_length_" + length.ToString();
                    }

                case "FLOAT":
                    return "System.Double";

                case "DECIMAL":
                    return "System.Decimal";

                case "BLOB":
                case "BYTE VARYING":
                case "LONG BYTE":
                    return "byte[]";

                default:
                    return "L52_mapCsType_unprepared_for_ingrestype_"+sqlType;
            }
        }

    }
}
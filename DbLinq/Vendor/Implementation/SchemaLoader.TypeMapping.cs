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
////////////////////////////////////////////////////////////////////
#endregion

using System;
using System.Collections.Generic;

namespace DbLinq.Vendor.Implementation
{
    public partial class SchemaLoader
    {
        protected class DataType
        {
            public string Type;
            public int? Length;
            public int? Precision;
            public int? Scale;
            public bool? Unsigned;
        }

        protected virtual Type MapDbType(DataType dataType)
        {
            switch (dataType.Type.ToLower())
            {
            case "varchar":
            case "longtext":
            case "text":
            case "char":
                return typeof(string);

            case "int":
                if (dataType.Unsigned ?? false)
                    return typeof(uint);
                return typeof(int);
            case "bit":
                return typeof(bool);
            case "tinyint":
                if (dataType.Length == 1)
                    return typeof(bool);
                return typeof(char);
            case "smallint":
                return typeof(short);
            case "mediumint":
                return typeof(int);
            case "bigint":
                return typeof(long);

            case "date":
            case "datetime":
            case "timestamp":
                return typeof(DateTime);

            case "enum":
                return typeof(Enum);
            case "float":
                return typeof(float);
            case "double":
                return typeof(double);
            case "decimal":
                return typeof(decimal);

            case "blob":
            case "longblob":
                return typeof(byte[]);
            //TODO: blob,longblob,set, ...
            default:
                //return "L80_mapCsType_unprepared_for_mysqltype_" + mysqlType;
                return null;
            }
        }
    }
}

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
        public class UnknownType
        {
        }

        public class DataType
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
            // string
            case "char":
            case "character":
            case "character varying":
            case "inet":
            case "longtext":
            case "text":
            case "varchar":
                return typeof(String);

            // bool
            case "bit":
            case "boolean":
                return typeof(Boolean);

            // int8
            case "tinyint":
                if (dataType.Length == 1)
                    return typeof(Boolean);
                return typeof(Byte);

            // int16
            case "smallint":
                return typeof(Int16);

            // int32
            case "int":
            case "integer":
            case "mediumint":
                if (dataType.Unsigned ?? false)
                    return typeof(uint);
                return typeof(Int32);

            // int64
            case "bigint":
                return typeof(Int64);

            // single
            case "float":
                return typeof(Single);

            // double
            case "double":
            case "double precision":
                return typeof(Double);

            // decimal
            case "decimal":
            case "numeric":
                return typeof(Decimal);

            // time interval
            case "interval":
                return typeof(TimeSpan);

            // date
            case "date":
            case "datetime":
            case "timestamp":
            case "timestamp without time zone":
            case "time without time zone": //reported by twain_bu...@msn.com,
            case "time with time zone":
                return typeof(DateTime);

            // enum
            case "enum":
                return typeof(Enum);

            // byte[]
            case "blob":
            case "bytea":
            case "longblob":
            case "oid":
            case "sytea":
                return typeof(Byte[]);

            case "void":
                return null;

            // if we fall to this case, we must handle the type
            default:
                return typeof(UnknownType);
            }
        }
    }
}

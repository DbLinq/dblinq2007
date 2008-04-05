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
using System.Text.RegularExpressions;

namespace DbLinq.Vendor.Implementation
{
    public partial class SchemaLoader
    {
        public class UnknownType
        {
        }

        public class DataType : IDataType
        {
            public virtual string Type { get; set; }
            public virtual int? Length { get; set; }
            public virtual int? Precision { get; set; }
            public virtual int? Scale { get; set; }
            public virtual bool? Unsigned { get; set; }
        }

        protected virtual Type MapDbType(IDataType dataType)
        {
            if (dataType == null)
                return typeof(UnknownType);

            switch (dataType.Type.ToLower())
            {
            // string
            case "c":
            case "char":
            case "character":
            case "character varying":
            case "inet":
            case "long":
            case "longtext":
            case "long varchar":
            case "nchar":
            case "nvarchar":
            case "nvarchar2":
            case "string":
            case "text":
            case "varchar":
            case "varchar2":
                return typeof(String);

            // bool
            case "bit":
            case "bool":
            case "boolean":
                return typeof(Boolean);

            // int8
            case "tinyint":
                if (dataType.Length == 1)
                    return typeof(Boolean);
                return typeof(Byte);

            // int16
            case "short":
            case "smallint":
                if (dataType.Unsigned ?? false)
                    return typeof(UInt16);
                return typeof(Int16);

            // int32
            case "int":
            case "integer":
            case "mediumint":
                if (dataType.Unsigned ?? false)
                    return typeof(UInt32);
                return typeof(Int32);

            // int64
            case "bigint":
                return typeof(Int64);

            // single
            case "float":
            case "float4":
            case "real":
                return typeof(Single);

            // double
            case "double":
            case "double precision":
                return typeof(Double);

            // decimal
            case "decimal":
            case "numeric":
                return typeof(Decimal);
            case "number": // special oracle type
                if (dataType.Precision.HasValue && (dataType.Scale ?? 0) == 0)
                {
                    if (dataType.Precision.Value == 1)
                        return typeof(Boolean);
                    if (dataType.Precision.Value <= 4)
                        return typeof(Int16);
                    if (dataType.Precision.Value <= 9)
                        return typeof(Int32);
                    if (dataType.Precision.Value <= 19)
                        return typeof(Int64);
                }
                return typeof(Decimal);

            // time interval
            case "interval":
                return typeof(TimeSpan);

            // date
            case "date":
            case "datetime":
            case "ingresdate":
            case "timestamp":
            case "timestamp without time zone":
            case "time":
            case "time without time zone": //reported by twain_bu...@msn.com,
            case "time with time zone":
                return typeof(DateTime);

            // enum
            case "enum":
                return typeof(Enum);

            // byte[]
            case "blob":
            case "bytea":
            case "byte varying":
            case "longblob":
            case "long byte":
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

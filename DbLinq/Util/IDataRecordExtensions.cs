#region MIT license
// 
// Copyright (c) 2007-2008 Jiri Moudry
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion
using System;
using System.Data;
using System.Reflection;
using DbLinq.Linq;

namespace DbLinq.Util
{
    public static class IDataRecordExtensions
    {
        // please note that sometimes (depending on driver), GetValue() returns DBNull instead of null
        // so at this level, we handle both

        public static string GetAsString(this IDataRecord dataRecord, int index)
        {
            if (dataRecord.IsDBNull(index))
                return null;
            object o = dataRecord.GetValue(index);
            if (o == null) // this is not supposed to happen
                return null;
            return o.ToString();
        }

        public static bool GetAsBool(this IDataRecord dataRecord, int index)
        {
            object b = dataRecord.GetValue(index);
            // first check: this may be a boolean
            if (b is bool)
                return (bool)b;
            // if it is a string, we may have "T"/"F" or "True"/"False"
            if (b is string)
            {
                // regular literals
                var lb = (string)b;
                bool ob;
                if (bool.TryParse(lb, out ob))
                    return ob;
                // alternative literals
                if (lb == "T" || lb == "F")
                    return lb == "T";
                if (lb == "Y" || lb == "N")
                    return lb == "Y";
            }
            return dataRecord.GetAsNumeric<int>(index) != 0;
        }

        public static char GetAsChar(this IDataRecord dataRecord, int index)
        {
            object c = dataRecord.GetValue(index);
            if (c is char)
                return (char)c;
            if (c is string)
            {
                string sc = (string)c;
                if (sc.Length == 1)
                    return sc[0];
            }
            if (c == null || c is DBNull)
                return '\0';
            throw new InvalidCastException(string.Format("Can't convert type {0} in GetAsChar()", c.GetType().Name));
        }

        public static U GetAsNumeric<U>(this IDataRecord dataRecord, int index)
        {
            if (dataRecord.IsDBNull(index))
                return default(U);
            return GetAsNumeric<U>(dataRecord.GetValue(index));
        }

        public static U? GetAsNullableNumeric<U>(this IDataRecord dataRecord, int index)
            where U: struct
        {
            if (dataRecord.IsDBNull(index))
                return null;
            return GetAsNumeric<U>(dataRecord.GetValue(index));
        }

        private static U GetAsNumeric<U>(object o)
        {
            if (o == null || o is DBNull)
                return default(U);
            return TypeConvert.ToNumber<U>(o);
        }

        public static int GetAsEnum(this IDataRecord dataRecord, Type enumType, int index)
        {
            int enumAsInt = dataRecord.GetAsNumeric<int>(index);
            return enumAsInt;
        }

        public static byte[] GetAsBytes(this IDataRecord dataRecord, int index)
        {
            if (dataRecord.IsDBNull(index))
                return null;
            object obj = dataRecord.GetValue(index);
            if (obj == null)
                return null; //nullable blob?
            byte[] bytes = obj as byte[];
            if (bytes != null)
                return bytes; //works for BLOB field
            Console.WriteLine("GetBytes: received unexpected type:" + obj);
            //return _rdr.GetInt32(index);
            return new byte[0];
        }

        public static object GetAsObject(this IDataRecord dataRecord, int index)
        {
            if (dataRecord.IsDBNull(index))
                return null;
            object obj = dataRecord.GetValue(index);
            return obj;
        }
    }
}

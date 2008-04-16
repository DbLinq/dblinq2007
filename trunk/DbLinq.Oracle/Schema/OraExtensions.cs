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
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DbLinq.Oracle.Schema
{
    public static class OraExtensions
    {
        /// <summary>
        /// read a (possibly null) string from DataReader
        /// </summary>
        public static string GetNString(this IDataReader rdr, int fieldID)
        {
            if (rdr.IsDBNull(fieldID)) return null;
            Type t = rdr.GetFieldType(fieldID);
            if (t != typeof(string))
                throw new ApplicationException("Cannot call GetNString for field " + fieldID + " actual type:" + t);
            return rdr.GetString(fieldID);
        }

        public static int? GetNInt(this IDataReader rdr, int fieldID)
        {
            if (rdr.IsDBNull(fieldID))
                return null;
            object d = rdr.GetValue(fieldID);
            return Convert.ToInt32(d);
        }

        public static decimal? GetNDecimal(this IDataReader rdr, int fieldID)
        {
            if (rdr.IsDBNull(fieldID))
                return null;
            return rdr.GetDecimal(fieldID);
        }

    }
}
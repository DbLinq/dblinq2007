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

namespace DBLinq.Vendor
{
    /// <summary>
    /// This interface lists all the functions required on top of regular IDataReader.
    /// 
    /// IDataReader2 is something that is no longer necessary.
    /// In 2006 version of Linq, the compiled expression 'reader.IsDbNull(0)?null:reader.GetInt32(0)' would crash the VM.
    /// Instead, we had to call into hand-written functions - such as GetInt32N().
    /// I haven't checked, but I assume that bug is fixed - 
    /// but I found it very useful to be able to set breakpoints in  the reader function.
    /// </summary>
    public interface IDataReader2: IDisposable
        //: System.Data.IDataReader //causes too many errors: missing NextResult(), Depth, ...
    {
        short? GetInt16N(int index);
        char? GetCharN(int index);
        bool? GetBooleanN(int index);
        int? GetInt32N(int index);
        uint? GetUInt32N(int index);
        double? GetDoubleN(int index);
        float? GetFloatN(int index);
        DateTime? GetDateTimeN(int index);
        long? GetInt64N(int index);
        ulong? GetUInt64N(int index);
        decimal? GetDecimalN(int index);
        T2 GetEnum<T2>(int index) where T2 : new();

        /// <summary>
        /// method to read a blob
        /// </summary>
        byte[] GetBytes(int index);

        #region regular IDataReader functions that we used in compiled code

        bool Read();
        int FieldCount { get; }

        bool GetBoolean(int index);
        byte GetByte(int index);
        char GetChar(int index);
        DateTime GetDateTime(int index);
        decimal GetDecimal(int index);
        double GetDouble(int index);
        float GetFloat(int index);
        short GetInt16(int index);
        int GetInt32(int index);
        long GetInt64(int index);
        uint GetUInt32(int index);
        ulong GetUInt64(int index);

        string GetString(int index);

        #endregion

    }
}

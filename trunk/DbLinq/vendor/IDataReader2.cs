using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DBLinq.vendor
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
        decimal? GetDecimalN(int index);

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

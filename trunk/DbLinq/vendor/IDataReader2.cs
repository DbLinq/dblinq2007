using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DBLinq.vendor
{
    public interface IDataReader2
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
        //why not a GetByte()?
        /// <summary>
        /// method to read a blob
        /// </summary>
        byte[] GetBytes(int index);
    }
}

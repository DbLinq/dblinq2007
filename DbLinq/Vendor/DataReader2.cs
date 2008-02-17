using System;
using System.Data;

namespace DbLinq.Vendor
{
    public class DataReader2 : IDataReader2
    {
        protected IDataReader _rdr;

        public DataReader2(IDataReader rdr)
        {
            _rdr = rdr;
        }

        /// <summary>
        /// Read added to support groupBy clauses, with more than one row returned at a time
        /// </summary>
        public virtual bool Read() { return _rdr.Read(); }
        public virtual void Dispose() { _rdr.Dispose(); }

        public virtual int FieldCount { get { return _rdr.FieldCount; } }
        public virtual string GetName(int index) { return _rdr.GetName(index); }
        public virtual string GetDataTypeName(int index) { return _rdr.GetDataTypeName(index); }
        public virtual Type GetFieldType(int index) { return _rdr.GetFieldType(index); }
        public virtual object GetValue(int index) { return _rdr.GetValue(index); }
        public virtual int GetValues(object[] values) { return _rdr.GetValues(values); }

        public virtual bool IsDBNull(int index) { return _rdr.IsDBNull(index); }

        protected virtual T? GetAsNullable<T>(int index, Func<int, T> getter)
            where T : struct
        {
            if (IsDBNull(index))
                return null;
            return getter(index);
        }

        public virtual bool GetBoolean(int index)
        {
            object asObject = _rdr.GetValue(index);
            if (asObject is bool)
                return (bool) asObject;
            if (asObject is decimal)
                return (decimal) asObject != 0;
            throw new ArgumentException("Can't cast to boolean");
        }

        public virtual bool? GetBooleanN(int index) { return GetAsNullable<bool>(index, GetBoolean); }

        public virtual byte GetByte(int index) { return _rdr.GetByte(index); }
        public virtual byte? GetByteN(int index) { return GetAsNullable<byte>(index,_rdr.GetByte); }

        public virtual char GetChar(int index) { return _rdr.GetChar(index); }
        public virtual char? GetCharN(int index) { return GetAsNullable<char>(index, GetChar); }

        public virtual Int16 GetInt16(int index) { return _rdr.GetInt16(index); }
        public virtual Int16? GetInt16N(int index) { return GetAsNullable<Int16>(index, GetInt16); }

        public virtual int GetInt32(int index) { return _rdr.GetInt32(index); }
        public virtual int? GetInt32N(int index) { return GetAsNullable<Int32>(index, GetInt32); }
        public virtual uint GetUInt32(int index) { return (uint)_rdr.GetInt32(index); }
        public virtual uint? GetUInt32N(int index) { return GetAsNullable<UInt32>(index, GetUInt32); }

        public virtual Int64 GetInt64(int index) { return _rdr.GetInt32(index); }
        public virtual Int64? GetInt64N(int index) { return GetAsNullable<Int64>(index, GetInt64); }
        public virtual UInt64 GetUInt64(int index) { return (UInt64)_rdr.GetInt64(index); }
        public virtual UInt64? GetUInt64N(int index) { return GetAsNullable<UInt64>(index, GetUInt64); }

        public virtual float GetFloat(int index) { return _rdr.GetFloat(index); }
        public virtual float? GetFloatN(int index) { return GetAsNullable<float>(index, GetFloat); }

        public virtual double GetDouble(int index) { return _rdr.GetDouble(index); }
        public virtual double? GetDoubleN(int index) { return GetAsNullable<double>(index, GetDouble); }

        public virtual decimal GetDecimal(int index) { return _rdr.GetDecimal(index); }
        public virtual decimal? GetDecimalN(int index) { return GetAsNullable<decimal>(index, GetDecimal); }

        public virtual DateTime GetDateTime(int index) { return _rdr.GetDateTime(index); }
        public virtual DateTime? GetDateTimeN(int index) { return GetAsNullable<DateTime>(index, GetDateTime); }

        public virtual string GetString(int index) { if(IsDBNull(index)) return null; return _rdr.GetString(index); }

        public virtual byte[] GetBytes(int index)
        {
            object obj = _rdr.GetValue(index);
            if (obj == null)
                return null; //nullable blob?
            byte[] bytes = obj as byte[];
            if (bytes != null)
                return bytes; //works for BLOB field
            Console.WriteLine("GetBytes: received unexpected type:" + obj);
            //return _rdr.GetInt32(index);
            return new byte[0];
        }

        /// <summary>
        /// helper method for reading an integer, and casting it to enum
        /// </summary>
        public virtual T2 GetEnum<T2>(int index)
            where T2 : new()
        {
            int value = (_rdr.IsDBNull(index))
                            ? 0
                            : _rdr.GetInt32(index);
            return (T2) Enum.ToObject(typeof (T2), value);
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            return _rdr.GetBytes(i, fieldOffset, buffer, bufferoffset, length);
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            return _rdr.GetChars(i, fieldoffset, buffer, bufferoffset, length);
        }

        public IDataReader GetData(int i)
        {
            return _rdr.GetData(i);
        }

        public Guid GetGuid(int i)
        {
            return _rdr.GetGuid(i);
        }

        public int GetOrdinal(string name)
        {
            return _rdr.GetOrdinal(name);
        }

        public object this[string name]
        {
            get
            {
                return _rdr[name];
            }
        }

        public object this[int i]
        {
            get
            {
                return _rdr[i];
            }
        }
    }
}

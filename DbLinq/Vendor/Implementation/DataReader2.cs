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
using System.Data;
using DbLinq.Logging;

namespace DbLinq.Vendor.Implementation
{
    public class DataReader2 : IDataReader2
    {
        protected IDataReader _reader;

        public ILogger Logger { get; set; }

        public DataReader2(IDataReader reader, ILogger logger)
        {
            _reader = reader;
            Logger = logger;
        }

        /// <summary>
        /// Read added to support groupBy clauses, with more than one row returned at a time
        /// </summary>
        public virtual bool Read() { return _reader.Read(); }
        public virtual void Close() { _reader.Close(); }
        public virtual DataTable GetSchemaTable() { return _reader.GetSchemaTable(); }
        public virtual bool NextResult() { return _reader.NextResult(); }
        public virtual int Depth { get { return _reader.Depth; } }
        public virtual bool IsClosed { get { return _reader.IsClosed; } }
        public virtual int RecordsAffected { get { return _reader.RecordsAffected; } }

        public virtual void Dispose() { _reader.Dispose(); }

        public virtual int FieldCount { get { return _reader.FieldCount; } }
        public virtual string GetName(int index) { return _reader.GetName(index); }
        public virtual string GetDataTypeName(int index) { return _reader.GetDataTypeName(index); }
        public virtual Type GetFieldType(int index) { return _reader.GetFieldType(index); }
        public virtual object GetValue(int index) { return _reader.GetValue(index); }
        public virtual int GetValues(object[] values) { return _reader.GetValues(values); }

        public virtual bool IsDBNull(int index) { return _reader.IsDBNull(index); }

        protected virtual T? GetAsNullable<T>(int index, Func<int, T> getter)
            where T : struct
        {
            if (IsDBNull(index))
                return null;
            return getter(index);
        }

        public virtual bool GetBoolean(int index)
        {
            object asObject = _reader.GetValue(index);
            if (asObject is bool)
                return (bool) asObject;
            if (asObject is decimal)
                return (decimal) asObject != 0;
            throw new ArgumentException("Can't cast to boolean");
        }

        public virtual bool? GetBooleanN(int index) { return GetAsNullable<bool>(index, GetBoolean); }

        public virtual byte GetByte(int index) { return _reader.GetByte(index); }
        public virtual byte? GetByteN(int index) { return GetAsNullable<byte>(index,_reader.GetByte); }

        public virtual char GetChar(int index) { return _reader.GetChar(index); }
        public virtual char? GetCharN(int index) { return GetAsNullable<char>(index, GetChar); }

        public virtual Int16 GetInt16(int index) { return _reader.GetInt16(index); }
        public virtual Int16? GetInt16N(int index) { return GetAsNullable<Int16>(index, GetInt16); }

        public virtual int GetInt32(int index) { return _reader.GetInt32(index); }
        public virtual int? GetInt32N(int index) { return GetAsNullable<Int32>(index, GetInt32); }
        public virtual uint GetUInt32(int index) { return (uint)_reader.GetInt32(index); }
        public virtual uint? GetUInt32N(int index) { return GetAsNullable<UInt32>(index, GetUInt32); }

        public virtual Int64 GetInt64(int index) { return _reader.GetInt32(index); }
        public virtual Int64? GetInt64N(int index) { return GetAsNullable<Int64>(index, GetInt64); }
        public virtual UInt64 GetUInt64(int index) { return (UInt64)_reader.GetInt64(index); }
        public virtual UInt64? GetUInt64N(int index) { return GetAsNullable<UInt64>(index, GetUInt64); }

        public virtual float GetFloat(int index) { return _reader.GetFloat(index); }
        public virtual float? GetFloatN(int index) { return GetAsNullable<float>(index, GetFloat); }

        public virtual double GetDouble(int index) { return _reader.GetDouble(index); }
        public virtual double? GetDoubleN(int index) { return GetAsNullable<double>(index, GetDouble); }

        public virtual decimal GetDecimal(int index) { return _reader.GetDecimal(index); }
        public virtual decimal? GetDecimalN(int index) { return GetAsNullable<decimal>(index, GetDecimal); }

        public virtual DateTime GetDateTime(int index) { return _reader.GetDateTime(index); }
        public virtual DateTime? GetDateTimeN(int index) { return GetAsNullable<DateTime>(index, GetDateTime); }

        public virtual string GetString(int index) { if(IsDBNull(index)) return null; return _reader.GetString(index); }

        public virtual byte[] GetBytes(int index)
        {
            object obj = _reader.GetValue(index);
            if (obj == null)
                return null; //nullable blob?
            byte[] bytes = obj as byte[];
            if (bytes != null)
                return bytes; //works for BLOB field
            Logger.Write(Level.Error,"GetBytes: received unexpected type:" + obj);
            //return _rdr.GetInt32(index);
            return new byte[0];
        }

        /// <summary>
        /// helper method for reading an integer, and casting it to enum
        /// </summary>
        public virtual T2 GetEnum<T2>(int index)
            where T2 : new()
        {
            int value = (_reader.IsDBNull(index))
                            ? 0
                            : _reader.GetInt32(index);
            return (T2) Enum.ToObject(typeof (T2), value);
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            return _reader.GetBytes(i, fieldOffset, buffer, bufferoffset, length);
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            return _reader.GetChars(i, fieldoffset, buffer, bufferoffset, length);
        }

        public IDataReader GetData(int i)
        {
            return _reader.GetData(i);
        }

        public Guid GetGuid(int i)
        {
            return _reader.GetGuid(i);
        }

        public int GetOrdinal(string name)
        {
            return _reader.GetOrdinal(name);
        }

        public object this[string name]
        {
            get
            {
                return _reader[name];
            }
        }

        public object this[int i]
        {
            get
            {
                return _reader[i];
            }
        }
    }
}

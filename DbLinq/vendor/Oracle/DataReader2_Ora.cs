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

using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Data.OracleClient;

namespace DBLinq.util
{
    /// <summary>
    /// This class wraps OracleDataReader.
    /// It logs exceptions and provides methods to retrieve nullable types.
    /// 
    /// When we have a workaround for FatalExecutionEngineError on nullables, 
    /// this can go away.
    /// </summary>
    public class DataReader2 : IDisposable, DBLinq.vendor.IDataReader2 //, IDataRecord
    {
        OracleDataReader _rdr;
        public DataReader2(OracleDataReader rdr)
        {
            _rdr = rdr;
        }
        
        /// <summary>
        /// Read added to support groupBy clauses, with more than one row returned at a time
        /// </summary>
        public bool Read(){ return _rdr.Read(); }

        public int FieldCount { get { return _rdr.FieldCount; } }
        public string GetName(int index){ return _rdr.GetName(index); }
        public string GetDataTypeName(int index){ return _rdr.GetDataTypeName(index); }
        public Type GetFieldType(int index){ return _rdr.GetFieldType(index); }
        public object GetValue(int index){ return _rdr.GetValue(index); }
        public int GetValues(object[] values){ return _rdr.GetValues(values); }
        
        public byte GetByte(int index){ return _rdr.GetByte(index); }
        public long GetInt64(int index){ return _rdr.GetInt64(index); }
        public long? GetInt64N(int index)
        {
            try
            {
                if (_rdr.IsDBNull(index))
                    return null;
                return _rdr.GetInt64(index);
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetInt64N failed: " + ex);
                return 0;
            }
        }


        public bool IsDBNull(int index){ return _rdr.IsDBNull(index); }

        public short GetInt16(int index){ return _rdr.GetInt16(index); }
        public short? GetInt16N(int index)
        {
            try
            {
                if(_rdr.IsDBNull(index))
                    return null;
                return _rdr.GetInt16(index);
            } 
            catch(Exception ex)
            {
                Console.WriteLine("GetInt16N failed: "+ex);
                return null;
            }
        }

        public char GetChar(int index){ return _rdr.GetChar(index); }
        public char? GetCharN(int index)
        {
            try
            {
                if(_rdr.IsDBNull(index))
                    return null;
                return _rdr.GetChar(index);
            } 
            catch(Exception ex)
            {
                Console.WriteLine("GetCharN failed: "+ex);
                return null;
            }
        }

        public bool GetBoolean(int index){ return _rdr.GetBoolean(index); }
        public bool? GetBooleanN(int index)
        {
            try
            {
                if(_rdr.IsDBNull(index))
                    return null;
                return _rdr.GetBoolean(index);
            } 
            catch(Exception ex)
            {
                Console.WriteLine("GetBooleanN failed: "+ex);
                return null;
            }
        }

        public int GetInt32(int index)
        {
            try
            {
                return _rdr.GetInt32(index);
            } 
            catch(Exception ex)
            {
                Console.WriteLine("GetInt32 failed: "+ex);
                return 0;
            }
        }
        public int? GetInt32N(int index)
        {
            try
            {
                if (_rdr.IsDBNull(index))
                    return null;
                return _rdr.GetInt32(index);
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetInt32N(" + index + ") failed: " + ex);
                return 0;
            }
        }


        public uint GetUInt32(int index)
        {
            try
            {
                //return _rdr.GetUInt32(index);
                int i32 = _rdr.GetInt32(index);
                return (uint)i32;
            } 
            catch(Exception ex)
            {
                Console.WriteLine("GetUInt32("+index+") failed: "+ex);
                try {
                        object obj = _rdr.GetValue(index);
                        Console.WriteLine("GetUInt32 failed, offending val: "+obj);
                } catch(Exception){}
                return 0;
            }
        }

        public uint? GetUInt32N(int index)
        {
            try
            {
                if(_rdr.IsDBNull(index))
                    return null;
                //return _rdr.GetUInt32(index);
                int i32 = _rdr.GetInt32(index);
                return (uint)i32;
            } 
            catch(Exception ex)
            {
                Console.WriteLine("GetUInt32 failed: "+ex);
                return null;
            }
        }

        public float GetFloat(int index)
        {
            try
            {
                return _rdr.GetFloat(index);
            } 
            catch(Exception ex)
            {
                Console.WriteLine("GetInt32 failed: "+ex);
                return 0;
            }
        }
        public double GetDouble(int index)
        {
            try
            {
                return _rdr.GetDouble(index);
            } 
            catch(Exception ex)
            {
                Console.WriteLine("GetInt32 failed: "+ex);
                return 0;
            }
        }
        public double? GetDoubleN(int index)
        {
            try
            {
                if (_rdr.IsDBNull(index))
                    return null;
                return _rdr.GetDouble(index);
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetInt32 failed: " + ex);
                return 0;
            }
        }

        public decimal GetDecimal(int index)
        {
            try
            {
                return _rdr.GetDecimal(index);
            } 
            catch(Exception ex)
            {
                Console.WriteLine("GetInt32 failed: "+ex);
                return 0;
            }
        }
        public decimal? GetDecimalN(int index)
        {
            try
            {
                if (_rdr.IsDBNull(index))
                    return null;
                return _rdr.GetDecimal(index);
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetDecimal(" + index + ") failed: " + ex);
                return 0;
            }
        }
        public DateTime GetDateTime(int index)
        {
            try
            {
                return _rdr.GetDateTime(index);
            } 
            catch(Exception ex)
            {
                Console.WriteLine("GetInt32 failed: "+ex);
                return new DateTime();
            }
        }
        public DateTime? GetDateTimeN(int index)
        {
            try
            {
                if(_rdr.IsDBNull(index))
                    return null;
                return _rdr.GetDateTime(index);
            } 
            catch(Exception ex)
            {
                Console.WriteLine("GetInt32 failed: "+ex);
                return new DateTime();
            }
        }
        public string GetString(int index)
        {
            try
            {
                return _rdr.GetString(index);
            } 
            catch(Exception ex)
            {
                Console.WriteLine("GetString("+index+") failed: "+ex);
                return null;
            }
        }

        public byte[] GetBytes(int index)
        {
            try
            {
                //System.Data.SqlClient.SqlDataReader rdr2;
                //rdr2.GetSqlBinary(); //SqlBinary does not seem to exist on MySql
                object obj = _rdr.GetValue(index);
                if(obj==null)
                    return null; //nullable blob?
                byte[] bytes = obj as byte[];
                if(bytes!=null)
                    return bytes; //works for BLOB field
                Console.WriteLine("GetBytes: received unexpected type:"+obj);
                //return _rdr.GetInt32(index);
                return new byte[0];
            } 
            catch(Exception ex)
            {
                Console.WriteLine("GetBytes failed: "+ex);
                return null;
            }
        }

        public void Dispose(){ _rdr.Dispose(); }

    }
}

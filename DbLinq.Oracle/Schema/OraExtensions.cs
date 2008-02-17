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
        if(rdr.IsDBNull(fieldID)) return null; 
        Type t = rdr.GetFieldType(fieldID);
        if(t!=typeof(string))
            throw new ApplicationException("Cannot call GetNString for field "+fieldID+" actual type:"+t);
        return rdr.GetString(fieldID); 
    }

        public static int? GetNInt(this IDataReader rdr, int fieldID)
    { 
        if(rdr.IsDBNull(fieldID)) 
            return null; 
        return rdr.GetInt32(fieldID); 
    }

        public static decimal? GetNDecimal(this IDataReader rdr, int fieldID)
    { 
        if(rdr.IsDBNull(fieldID)) 
            return null; 
        return rdr.GetDecimal(fieldID); 
    }

    }
}
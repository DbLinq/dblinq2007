using System;
using System.Collections.Generic;
using System.Text;

namespace DbLinq.Oracle.Schema
{
    public static class OraTypeMap
    {
        public static string mapSqlTypeToCsType(string dbType, decimal? precision)
        {
            switch(dbType)
            {
                case "NUMBER":
                    return "int";
                case "VARCHAR2":
                case "CHAR":      //pointed out by Farshid
                case "NVARCHAR2":
                    return "string";
                case "TIMESTAMP":
                case "DATE":
                    return "DateTime";
                case "BLOB":
                    return "byte[]";
                case "FLOAT":
                    return "double";
                case "LONG":
                    return "long";
                default:
                    return "UnknownOracleType_20 //(Unprepared for Oracle type "+dbType+") \n";
            }
        }
    }
}
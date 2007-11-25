using System;
using System.Collections.Generic;
using System.Text;

namespace SqlMetal.schema.oracle
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
                    return "string";
                case "TIMESTAMP":
                    return "DateTime";
                default:
                    return "UnknownOracleType_20 //(Unprepared for Oracle type "+dbType+") \n";
            }
        }
    }
}

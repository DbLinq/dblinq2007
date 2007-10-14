using System;
using System.Collections.Generic;
using System.Text;

namespace SqlMetal.schema.pgsql
{
    static class Mappings
    {
        /// <summary>
        /// map 'varchar' to string
        /// </summary>
        /// <param name="mysqlType"></param>
        /// <param name="column_type">e.g. 'unsigned'</param>
        public static string mapSqlTypeToCsType(string mysqlType, string column_type)
        {
            #region map "varchar" to "string"
            switch (mysqlType)
            {
                case "varchar": return "string";
                case "longtext": return "string";
                case "text": return "string";
                case "char": return "string";

                case "int": 
                    if(column_type.Contains("unsigned"))
                        return "uint";
                    return "int";
                case "tinyint": 
                    if(column_type=="tinyint(1)")
                        return "bool";
                    return "char";
                case "smallint": return "short";
                case "mediumint": return "short";
                case "bigint": return "long";

                case "datetime": return "DateTime";
                case "timestamp": return "DateTime";
                case "enum": return "Enum";
                case "float": return "float";
                case "double":
                case "double precision":
                    return "double";
                case "decimal":
                case "numeric":
                    return "decimal";
                case "blob": return "byte[]";
                    //TODO: blob,longblob,set, ...
                case "character varying": return "string";
                case "integer": return "int";
                case "timestamp without time zone": return "DateTime";
                default:
                    return "L52_mapCsType_unprepared_for_postgresqltype_"+mysqlType;
            }
            #endregion
        }

    }
}

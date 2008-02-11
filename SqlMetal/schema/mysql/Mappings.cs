using System;
using System.Collections.Generic;
using System.Text;

namespace SqlMetal.schema.mysql
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
                case "varchar":
                case "longtext":
                case "text":
                case "char":
                    return "string";

                case "int":
                    if (column_type.Contains("unsigned"))
                        return "uint";
                    return "int";
                case "bit":
                    return "bool";
                case "tinyint":
                    if (column_type == "tinyint(1)"
                        || column_type == "tinyint(1) unsigned")
                        return "bool";
                    return "char";
                case "smallint": return "short";
                case "mediumint": return "int";
                case "bigint": return "long";

                case "date":
                case "datetime":
                case "timestamp":
                    return "DateTime";

                case "enum": return "Enum";
                case "float": return "float";
                case "double": return "double";
                case "decimal": return "decimal";

                case "blob":
                case "longblob":
                    return "byte[]";
                //TODO: blob,longblob,set, ...
                default:
                    return "L80_mapCsType_unprepared_for_mysqltype_" + mysqlType;
            }
            #endregion
        }

    }
}

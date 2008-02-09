using System;
using System.Collections.Generic;
using System.Text;

namespace SqlMetal.schema.sqlite
{
    static class Mappings
    {
        /// <summary>
        /// map 'varchar' to string
        /// </summary>
        /// <param name="sqliteType"></param>
        /// <param name="column_type">e.g. 'unsigned'</param>
        public static string mapSqlTypeToCsType(string sqliteType, string column_type)
        {
            #region map "varchar" to "string"
            if (sqliteType.Contains("("))
            {
                sqliteType = sqliteType.Substring(0, sqliteType.IndexOf("("));
            }
            System.Console.WriteLine("sqliteType : {0}", sqliteType);
            switch (sqliteType.ToLower())
            {
                case "varchar":
                case "longtext":
                case "text":
                case "string":
                case "char": 
                    return "string";

                case "integer": 
                case "integer unsigned": 
                case "int": 
                    if(column_type.Contains("unsigned"))
                        return "uint";
                    return "int";

                case "bool": 
                case "boolean": 
                case "bit":
                    return "bool";
                case "byte":
                case "tinyint":
                case "tinyint signed":
                case "tinyint unsigned":
                    if (column_type == "tinyint(1)")
                        return "bool";
                    return "byte";

                case "short":
                case "smallint unsigned": 
                case "smallint":
                case "mediumint unsigned": 
                case "mediumint":
                    if (column_type.Contains("unsigned"))
                        return "ushort";
                    return "short";

                case "long": 
                case "bigint": 
                    return "long";

                case "datetime":
                case "timestamp":
                    return "DateTime";

                case "enum": 
                    return "Enum";

                case "float": 
                case "real": 
                    return "float";

                case "double": 
                    return "double";

                case "decimal": 
                    return "decimal";

                case "byte[]": 
                case "blob": 
                    return "byte[]";

                    //TODO: blob,longblob,set, ...
                case "numeric": 
                    return "decimal";

                default:
                    return "L80_mapCsType_unprepared_for_mysqltype_"+sqliteType;
            }

            #endregion
        }

    }
}

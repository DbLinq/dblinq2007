using System;
using System.Collections.Generic;
using System.Text;

namespace SqlMetal.schema
{
    class CSharp
    {
        static readonly string[] Keywords = 
        {
           "int", "uint", "byte", "short", "ushort", "char"
            ,"decimal", "float", "double"
            ,"string", "DateTime"
            , "void", "object"

            ,"private", "protected", "public", "internal"
            ,"override", "virtual", "abstract", "partial", "static", "sealed", "readonly"
            ,"class", "struct", "namespace", "enum", "interface", "using", "const", "enum"

            ,"return", "if", "while", "for", "foreach"
            ,"yield", "break", "goto", "switch", "case", "default"

            , "as", "catch", "continue", "default", "delegate", "do"
            , "else", "false", "true", "fixed", "finally", "in", "is", "lock"
            , "new", "null", "out", "ref", "sizeof", "stackalloc", "throw", "typeof"
        };

        public static bool IsCsharpKeyword(string name)
        {
            //return Keywords.Contains(name); error CS0117: 'System.Array' does not contain a definition for 'Contains'???
            //return Keywords.Contains<string>(name);
            int index = System.Array.IndexOf<string>(Keywords,name);
            return index>=0;
        }

        public static bool IsValueType(string csType)
        {
            switch(csType)
            {
                case "System.DateTime":
                case "System.Int64":
                case "System.Int16":
                case "System.Int32":
                case "System.Char":
                case "System.Byte":
                case "System.Double":
                case "System.Decimal":
                case "int": 
                case "uint": 
                case "char": 
                case "byte": 
                case "bool": 
                case "long": 
                case "short": 
                case "ulong":
                case "ushort":
                case "decimal":
                case "double":
                case "float":
                case "DateTime": 
                    return true;
                default: 
                    return false;
            }
        }

        /// <summary>
        /// convert ('System.Int32',true) into 'int?'
        /// </summary>
        public static string FormatType(string csType, bool isNullable)
        {
            string csType2 = TypeNickName(csType);
            if (isNullable && IsValueType(csType))
                return csType2 + "?";
            return csType2;
        }

        /// <summary>
        /// given 'dbo.Order Details', return 'Order_Details'
        /// </summary>
        public static string FormatTableClassName(string sqlTableName)
        {
            if (sqlTableName.Contains("."))
            {
                int indx = sqlTableName.IndexOf(".");
                sqlTableName = sqlTableName.Substring(indx+1);
            }
            sqlTableName = sqlTableName.Replace(" ", "_");
            return sqlTableName;
        }

        static string TypeNickName(string csType)
        {
            switch (csType)
            {
                case "System.String": return "string";
                case "System.Int64": return "long";
                case "System.Int16": return "short";
                case "System.Int32": return "int";
                case "System.Char": return "char";
                case "System.Byte": return "byte";
                case "System.Double": return "double";
                case "System.Decimal": return "decimal";
                default: return csType;
            }
        }


    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace SqlMetal.schema
{
    class CSharp
    {
        static readonly string[] Keywords = 
        {
            "int", "byte", "short", "uint", "char",
            "decimal", "float", "double"
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
            switch(csType){
                case "int": 
                case "uint": 
                case "char": 
                case "byte": 
                case "bool": 
                case "long": 
                case "short": 
                case "ulong": 
                case "ushort": 
                case "DateTime": 
                    return true;
                default: 
                    return false;
            }
        }

    }
}

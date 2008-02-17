using System;
using System.Linq;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Text;

namespace DbLinq.Util
{
    public static class Name
    {
        /// <summary>
        /// convert 'product' into 'Product'
        /// convert 'PRODUCT' into 'Product' (Oracle)
        /// convert 'dbo.table' into 'Dbo.Table'
        /// convert 'order details' into 'Order Details'
        /// </summary>
        public static string Capitalize(this string word)
        {
            if (word == null)
                throw new ArgumentNullException("word");
            if (word.Length < 2)
                return word;
            //string ret = Char.ToUpper(word[0])+word.Substring(1).ToLower();
            StringBuilder sb = new StringBuilder(word);
            char prev_ch = ' ';
            for (int i = 0; i < sb.Length; i++)
            {
                char ch = sb[i];
                bool prev_was_space = Char.IsWhiteSpace(prev_ch) || prev_ch == '_' || prev_ch == '.';
                bool mustConvertToLower = Char.IsUpper(ch) && !prev_was_space;
                bool mustConvertToUpper = prev_was_space;
                if (mustConvertToLower)
                {
                    sb[i] = Char.ToLower(ch);
                }
                else if (mustConvertToUpper)
                {
                    sb[i] = Char.ToUpper(ch); //ensure uppercase 'D' in 'Order Details'
                }
                prev_ch = ch;
            }

            string ret = sb.ToString();

            if (/*mmConfig.forceUcaseID &&*/ ret.EndsWith("id"))
            {
                //convert Oracle's 'Productid' to 'ProductID'
                ret = ret.Substring(0, ret.Length - 2) + "ID";
            }
            return ret;
        }

        /// <summary>
        /// using English heuristics, convert 'dogs' to 'dog',
        /// 'categories' to 'category',
        /// 'cat' remains unchanged.
        /// </summary>
        public static string Singularize(this string word)
        {
            if (word.Length < 2)
                return word;
            if (word.EndsWith("ies"))
                return word.Substring(0, word.Length - 3) + "y"; //Territories->Territory
            if (word.EndsWith("s"))
                return word.Substring(0, word.Length - 1);
            return word;
        }

        /// <summary>
        /// using English heuristics, convert 'dog' to 'dogs',
        /// 'bass' remains unchanged.
        /// </summary>
        public static string Pluralize(this string word)
        {
            if (word.Length < 2)
                return word;
            if (word.EndsWith("s"))
                return word;
            return word + "s";
        }

#if TODO // TODO

        /// <summary>
        /// given 'categories', return either singular 'Category' or unchanged 'Catagories'
        /// given 'order details', return 'OrderDetails'.
        /// given 'dbo.t1', return 'Dbo.T1'.
        /// </summary>
        public static string FormatTableName(string table_name, PluralEnum pluralEnum)
        {
            //TODO: allow custom renames via config file - 
            //- this could solve keyword conflict etc
            string name1 = table_name;
            string name2 = mmConfig.forceUcaseTableName
                ? name1.Capitalize() //Char.ToUpper(name1[0])+name1.Substring(1)
                : name1;

            //heuristic to convert 'Products' table to class 'Product'
            //TODO: allow customized tableName-className mappings from an XML file
            if (mmConfig.pluralize && pluralEnum == PluralEnum.Singularize)
            {
                // "-pluralize" flag: apply english-language rules for plural, singular
                name2 = name2.Singularize();
            }

            name2 = name2.Replace(" ", ""); // "Order Details" -> "OrderDetails"

            if (mmConfig.pluralize && pluralEnum == PluralEnum.Pluralize)
            {
                // "-pluralize" flag: apply english-language rules for plural, singular
                name2 = name2.Pluralize();
            }

            return name2;
        }

        /// <summary>
        /// given name 'EMPLOYEE', return 'Employees'
        /// </summary>
        public static string TableNamePlural(string name)
        {
            if (s_renamings != null)
            {
                //check if the XML file specifies a new name
                var q = from r in s_renamings.Arr where r.old == name select r.@new;
                foreach (var @new in q) { return @new; }
            }

            if (IsMixedCase(name))
                return name.Pluralize(); //on Microsoft, preserve case

            //if we get here, there was no renaming
            return name.Capitalize().Pluralize();
        }

        /// <summary>
        /// given name 'EMPLOYEE', return 'Employee'
        /// </summary>
        public static string TableNameSingular(string name)
        {
            if (s_renamings != null)
            {
                //check if the XML file specifies a new name
                var q = from r in s_renamings.Arr where r.old == name select r.@new;
                foreach (var @new in q) { return @new; }
            }
            //if we get here, there was no renaming
            return name.Capitalize().Singularize();
        }


        /// <summary>
        /// given 'getproductcount', return 'GetProductCount'
        /// </summary>
        public static string Rename(string name)
        {
            if (s_renamings != null)
            {
                //check if the XML file specifies a new name
                var q = from r in s_renamings.Arr where r.old == name select r.@new;
                string newName = q.FirstOrDefault();
                return newName ?? name;
            }
            return name;
        }

        /// <summary>
        /// given 'productid', return 'ProductID'
        /// </summary>
        public static string FieldName(string name)
        {
            if (s_renamings != null)
            {
                //check if the XML file specifies a new name
                var q = from r in s_renamings.Arr where r.old == name select r.@new;
                foreach (var @new in q) { return @new; }
            }

            //if name has a mixture of uppercase/lowercase, don't change it (don't capitalize)
            //(Microsfot SQL Server preserves case)
            if (IsMixedCase(name))
                return name;

            string name2 = mmConfig.forceUcaseTableName
                ? name.Capitalize() //Char.ToUpper(column.Name[0])+column.Name.Substring(1)
                : name;

            string name3 = CSharp.IsCsharpKeyword(name2)
                ? name2 + "_" //avoid keyword conflict - append underscore
                : name2;
            return name3;
        }

        public static bool IsMixedCase(string s)
        {
            bool foundL = false, foundU = false;
            foreach (char c in s)
            {
                if (Char.IsUpper(c))
                    foundU = true;
                if (Char.IsLower(c))
                    foundL = true;
                if (foundL && foundU)
                    return true;
            }
            return false;
        }

#endif
    }

    public enum PluralEnum
    {
        Pluralize,
        Unchanged,
        Singularize
    }
}

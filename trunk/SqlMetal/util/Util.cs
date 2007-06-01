using System;
using System.Query;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Text;

namespace SqlMetal.util
{
    public static class Util
    {
        static Renamings s_renamings = null;

        /// <summary>
        /// convert 'product' into 'Product'
        /// convert 'PRODUCT' into 'Product' (Oracle)
        /// </summary>
        public static string Capitalize(this string word)
        {
            if(word==null)
                throw new ArgumentNullException("word");
            if(word.Length<2)
                return word;
            string ret = Char.ToUpper(word[0])+word.Substring(1).ToLower();
            
            if(mmConfig.forceUcaseID && ret.EndsWith("id"))
            {
                //convert Oracle's 'Productid' to 'ProductID'
                ret = ret.Substring(0,ret.Length-2)+"ID";
            }
            return ret;
        }

        /// <summary>
        /// using English heuristics, convert 'dogs' to 'dog',
        /// 'cat' remains unchanged.
        /// </summary>
        public static string Singularize(this string word)
        {
            if(word.Length<2)
                return word;
            if(word.EndsWith("s"))
                return word.Substring(0,word.Length-1);
            return word;
        }

        /// <summary>
        /// using English heuristics, convert 'dog' to 'dogs',
        /// 'bass' remains unchanged.
        /// </summary>
        public static string Pluralize(this string word)
        {
            if(word.Length<2)
                return word;
            if(word.EndsWith("s"))
                return word;
            return word+"s";
        }

        /// <summary>
        /// given name 'EMPLOYEE', return 'Employees'
        /// </summary>
        public static string TableNamePlural(string name)
        {
            if(s_renamings!=null)
            {
                //check if the XML file specifies a new name
                var q = from r in s_renamings.Arr where r.old==name select r.@new;
                foreach(var @new in q){ return @new; }
            }
            
            if(IsMixedCase(name))
                return name.Pluralize(); //on Microsoft, preserve case

            //if we get here, there was no renaming
            return name.Capitalize().Pluralize();
        }

        /// <summary>
        /// given name 'EMPLOYEE', return 'Employee'
        /// </summary>
        public static string TableNameSingular(string name)
        {
            if(s_renamings!=null)
            {
                //check if the XML file specifies a new name
                var q = from r in s_renamings.Arr where r.old==name select r.@new;
                foreach(var @new in q){ return @new; }
            }
            //if we get here, there was no renaming
            return name.Capitalize().Singularize();
        }
        /// <summary>
        /// given 'productid', return 'ProductID'
        /// </summary>
        public static string FieldName(string name)
        {
            if(s_renamings!=null)
            {
                //check if the XML file specifies a new name
                var q = from r in s_renamings.Arr where r.old==name select r.@new;
                foreach(var @new in q){ return @new; }
            }

            //if name has a mixture of uppercase/lowercase, don't change it (don't capitalize)
            //(Microsfot SQL Server preserves case)
            if(IsMixedCase(name))
                return name;

            return mmConfig.forceUcaseTableName 
                ? name.Capitalize() //Char.ToUpper(column.Name[0])+column.Name.Substring(1)
                : name;
        }

        /// <summary>
        /// load renames xml file, if it exists.
        /// </summary>
        public static void InitRenames()
        {
            if(mmConfig.renamesFile==null)
                return;
            if(!System.IO.File.Exists(mmConfig.renamesFile))
                throw new ArgumentException("Renames file missing:"+mmConfig.renamesFile);

            Console.WriteLine("Loading renames file: "+mmConfig.renamesFile);
            XmlSerializer xser = new XmlSerializer(typeof(Renamings));
            object obj = xser.Deserialize(System.IO.File.OpenText(mmConfig.renamesFile));
            s_renamings = (Renamings)obj;
        }

        public static bool IsMixedCase(string s)
        {
            bool foundL = false, foundU = false;
            foreach(char c in s)
            {
                if(Char.IsUpper(c))
                    foundU = true;
                if(Char.IsLower(c))
                    foundL = true;
                if(foundL && foundU)
                    return true;
            }
            return false;
        }

    }

    /// <summary>
    /// specifies format of XML file for renaming
    /// </summary>
    public class Renamings
    {
        [XmlElement("Renaming")] 
        public readonly List<Renaming> Arr = new List<Renaming>();
    }

    public class Renaming
    {
        [XmlAttribute] public string old;
        [XmlAttribute] public string @new;
    }
}

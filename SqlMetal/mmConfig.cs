using System;
using System.Linq;
using System.Configuration;
using System.Reflection;
using System.Collections.Generic;
using System.Text;

namespace SqlMetal
{
    public class mmConfig
    {
        //set fields to 'null' to prevent compile warnings.
        public string user = null;
        public string password = null;
        public string server = null;
        public string database = null;

        /// <summary>
        /// the namespace to put our classes into
        /// </summary>
        public string @namespace = null;

        /// <summary>
        /// If present, write out C# code
        /// </summary>
        public string code = null;

        /// <summary>
        /// If present, write out DBML XML representing the DB
        /// </summary>
        public string dbml = null;

        /// <summary>
        /// convert table name 'products' to class 'Products'
        /// </summary>
        public bool forceUcaseTableName = true;

        /// <summary>
        /// for mysql, we want to keep case as specified in DB.
        /// </summary>
        public bool forceUcaseFieldName = false;

        /// <summary>
        /// rename object 'productid' to 'productID'?
        /// </summary>
        public bool forceUcaseID = true;

        /// <summary>
        /// load object renamings from an xml file?
        /// </summary>
        public string renamesFile = null;

        public string schemaXmlFile = null;

        public string baseClass = "IModified";

        public bool sprocs = false;

        public bool verboseForeignKeys = false;

        /// <summary>
        /// when true, we will call Singularize()/Pluralize() functions.
        /// </summary>
        public bool pluralize = true;

        /// <summary>
        /// if true, and PostgreSql database contains DOMAINS (typedefs), 
        /// we will generate code DbType='DerivedType'.
        /// if false, generate code DbType='BaseType'.
        /// </summary>
        public bool useDomainTypes = true;

        /// <summary>
        /// force a Console.ReadKey at end of program.
        /// Useful when running from Studio, so the output window does not disappear
        /// </summary>
        public bool readLineAtExit = false;

        public string connectionString = null;

        public string dbLinqSchemaLoaderType = null;

        public string databaseConnectionType = null;

        public string dbType = null;

        /// <summary>
        /// give preference to commandline options over app.config options
        /// </summary>
        public mmConfig(string [] args)
        {
            try
            {
                Type t = GetType();
                Dictionary<string,string> argMap = new Dictionary<string,string>();
                foreach(string arg in args)
                {
                    int colon = arg.IndexOf(":");
                    if (colon == -1)
                    {
                        string left = arg.Substring(1);
                        argMap[left] = ""; //allow boolean flag '-sprocs'
                        continue; 
                    }
                    if(arg.StartsWith("-") || arg.StartsWith("/"))
                    {
                        string left = arg.Substring(1,colon-1);
                        string right = arg.Substring(colon+1);
                        argMap[left] = right;
                    }
                }

                MemberInfo[] minfos = t.FindMembers(MemberTypes.Field,BindingFlags.Instance|BindingFlags.Public, null, null);
                foreach(MemberInfo minfo in minfos)
                {
                    FieldInfo finfo = minfo as FieldInfo;
                    if(finfo==null)
                        continue;

                    //string valueFromAppConfig = ConfigurationSettings.AppSettings[minfo.Name];
                    string valueFromAppConfig = ConfigurationManager.AppSettings[minfo.Name];
                    //var valuesFromCmdline   = from arg in args 
                    //               where arg.StartsWith("-"+minfo.Name) && arg.StartsWith("/"+minfo.Name)
                    //               select arg.Substring(minfo.Name.Length+1);
                    //string valueFromCmdline = valuesFromCmdline.First();
                    string valueFromCmdline = null; // = argMap[minfo.Name];

                    //if (minfo.Name == "pluralize")
                    //    Console.WriteLine("Xxxx");

                    if(valueFromAppConfig==null && (!argMap.TryGetValue(minfo.Name,out valueFromCmdline)) )
                        continue; //value not specified for this setting

                    //command line args override values from app.config
                    string sval = valueFromCmdline ?? valueFromAppConfig;

                    if(sval==null)
                        continue;

                    sval = sval.Trim('\"');

                    if(finfo.FieldType==typeof(string)){
                        finfo.SetValue(this,sval);
                    } else if(finfo.FieldType==typeof(bool)){
                        bool bval = sval == ""
                            ? true //eg. '-sprocs'
                            : bool.Parse(sval);
                        finfo.SetValue(this,bval);
                    } else {
                        Console.WriteLine("mmConfig.cctor L39 unprepared for type:"+minfo.ReflectedType);
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("mmConfig.cctor L37 failed:"+ex);
            }
        }
    }
}

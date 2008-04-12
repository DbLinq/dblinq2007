#region MIT license
////////////////////////////////////////////////////////////////////
// MIT license:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//
// Authors:
//        Jiri George Moudry
////////////////////////////////////////////////////////////////////
#endregion

using System.Configuration;

namespace DbMetal.Configuration
{
    /// <summary>
    /// Handles the providers section.
    /// Each provider is defined as follows:
    ///  &lt;provider name="MySQL"      dbLinqSchemaLoader="DbLinq.MySql.MySqlSchemaLoader, DbLinq.MySql"
    ///                             databaseConnection="MySql.Data.MySqlClient.MySqlConnection, MySql.Data" />
    /// </summary>
    public class ProvidersSection : ConfigurationSection
    {
        public class ProviderElement : ConfigurationElement
        {
            [ConfigurationProperty("name", IsRequired = true)]
            public string Name
            {
                get { return (string)this["name"]; }
            }

            [ConfigurationProperty("dbLinqSchemaLoader", IsRequired = true)]
            public string DbLinqSchemaLoader
            {
                get { return (string)this["dbLinqSchemaLoader"]; }
            }

            [ConfigurationProperty("databaseConnection", IsRequired = true)]
            public string DatabaseConnection
            {
                get { return (string)this["databaseConnection"]; }
            }
        }

        public class ProvidersCollection : ConfigurationElementCollection
        {
            protected override ConfigurationElement CreateNewElement()
            {
                return new ProviderElement();
            }

            protected override object GetElementKey(ConfigurationElement element)
            {
                var provider = (ProviderElement)element;
                return provider.Name.ToLower();
            }

            public ProviderElement GetProvider(string name)
            {
                return (ProviderElement)BaseGet(name.ToLower());
            }
        }

        [ConfigurationProperty("providers", IsDefaultCollection = true)]
        [ConfigurationCollection(typeof(ProviderElement), AddItemName = "provider")]
        public ProvidersCollection Providers
        {
            get { return (ProvidersCollection)this["providers"]; }
        }
    }
}
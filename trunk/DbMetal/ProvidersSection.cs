
using System.Configuration;

namespace SqlMetal
{
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
                ProviderElement provider = (ProviderElement)element;
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

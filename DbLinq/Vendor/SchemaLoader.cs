
using System;
using System.Collections.Generic;
using System.Data;
using DbLinq.Linq;
using DbLinq.Util;

namespace DbLinq.Vendor
{
    public abstract class SchemaLoader: ISchemaLoader
    {
        public abstract string VendorName { get; }
        public abstract Type DataContextType { get; }
        public IDbConnection Connection { get; set; }
        public INameFormatter NameFormatter { get; set; }
        public abstract DlinqSchema.Database Load(string databaseName, IDictionary<string, string> tableAliases, bool loadStoredProcedures);

        public Words Words { get; private set; }

        public SchemaLoader()
        {
            NameFormatter = new NameFormatter();
        }

        protected string GetTableName(string name, IDictionary<string,string> tableAliases)
        {
            if (tableAliases.ContainsKey(name))
                return tableAliases[name];
            return NameFormatter.AdjustTableName(name);
        }

        protected string GetColumnName(string name)
        {
            return NameFormatter.AdjustColumnName(name);
        }

        protected string GetColumnFieldName(string name)
        {
            return NameFormatter.AdjustColumnFieldName(name);
        }

        protected string GetMethodName(string name)
        {
            return NameFormatter.AdjustMethodName(name);
        }
    }
}

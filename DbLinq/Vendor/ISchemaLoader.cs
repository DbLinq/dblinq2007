
using System;
using System.Collections.Generic;
using System.Data;
using DbLinq.Linq;

namespace DbLinq.Vendor
{
    public interface ISchemaLoader
    {
        string VendorName { get; }
        Type DataContextType { get; }
        IDbConnection Connection { get; set; }
        DlinqSchema.Database Load(string databaseName, IDictionary<string, string> tableAliases, bool loadStoredProcedures);
    }
}

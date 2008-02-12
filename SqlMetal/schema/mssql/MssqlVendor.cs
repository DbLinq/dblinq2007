using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using SqlMetal.util;

namespace SqlMetal.schema.mysql { } //this namespace is used from other csproj
namespace SqlMetal.schema.pgsql { } //this namespace is used from other csproj
namespace SqlMetal.schema.oracle { } //this namespace is used from other csproj
namespace SqlMetal.schema.sqlite { } //this namespace is used from other csproj

namespace SqlMetal.schema.mssql
{
    /// <summary>
    /// this class contains MsSql-specific way of retrieving DB schema.
    /// </summary>
    class Vendor : IDBVendor
    {
        public string VendorName(){ return "Microsoft"; }
        public string DataContextName() { return "DbLinq.SqlServer.SqlServerDataContext"; }
        public string ProviderClassName() { return "SqlServerVendor"; }

        /// <summary>
        /// main entry point to load schema
        /// </summary>
        public DlinqSchema.Database LoadSchema()
        {
            return null; 
        }


    }
}

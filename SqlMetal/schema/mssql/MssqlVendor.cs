using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using SqlMetal.util;

namespace SqlMetal.schema.mysql { } //dummy namespace
namespace SqlMetal.schema.pgsql { } //dummy namespace

namespace SqlMetal.schema.mssql
{
    /// <summary>
    /// this class contains MsSql-specific way of retrieving DB schema.
    /// </summary>
    class Vendor : IDBVendor
    {
        public string VendorName(){ return "Microsoft"; }

        /// <summary>
        /// main entry point to load schema
        /// </summary>
        public DlinqSchema.Database LoadSchema()
        {
            return null; 
        }


    }
}

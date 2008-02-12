using System;
using System.Collections.Generic;
using System.Text;

namespace SqlMetal.schema
{
    /// <summary>
    /// both MySql and Oracle can query the database 
    /// and return a schema description.
    /// </summary>
    public interface IDBVendor
    {
        DlinqSchema.Database LoadSchema();
        
        /// <summary>
        /// eg. 'Oracle'
        /// </summary>
        string VendorName();

        /// <summary>
        /// name of class with one-parameter ctor, derived from DBLinq.Linq.DataContext.
        /// eg. 'DBLinq.vendor.mysql.MysqlDataContext'
        /// </summary>
        string DataContextName();

        /// <summary>
        /// eg. 'VendorMysql'
        /// </summary>
        string ProviderClassName();
    }
}

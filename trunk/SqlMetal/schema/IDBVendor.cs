using System;
using System.Collections.Generic;
using System.Text;

namespace MysqlMetal.schema
{
    /// <summary>
    /// both MySql and Oracle can query the database 
    /// and return a schema description.
    /// </summary>
    public interface IDBVendor
    {
        DlinqSchema.Database LoadSchema();
    }
}

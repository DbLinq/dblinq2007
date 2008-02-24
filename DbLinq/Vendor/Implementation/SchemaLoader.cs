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

using System;
using System.Collections.Generic;
using System.Data;
using DbLinq.Linq;
using DbLinq.Util;

namespace DbLinq.Vendor.Implementation
{
    public abstract class SchemaLoader: ISchemaLoader
    {
        public abstract string VendorName { get; }
        public abstract Type DataContextType { get; }
        public IDbConnection Connection { get; set; }
        public INameFormatter NameFormatter { get; set; }
        public abstract DlinqSchema.Database Load(string databaseName, IDictionary<string, string> tableAliases, bool loadStoredProcedures);

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

        public virtual string GetManyToOneColumnName(string referencedTableName, string thisTableName)
        {
            // TODO: handle aliases?
            return NameFormatter.AdjustManyToOneColumnName(referencedTableName, thisTableName);
        }

        public virtual string GetOneToManyColumnName(string referencedTableName)
        {
            // TODO: handle aliases?
            return NameFormatter.AdjustOneToManyColumnName(referencedTableName);
        }
    }
}
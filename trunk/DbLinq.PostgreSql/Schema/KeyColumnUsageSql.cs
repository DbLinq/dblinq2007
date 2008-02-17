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
using System.Text;
using DbLinq.Util;

namespace DbLinq.PostgreSql.Schema
{
    /// <summary>
    /// represents one row from Postgres' information_schema.`Key_Column_Usage` table
    /// </summary>
    public class KeyColumnUsage
    {
        public string constraint_schema;
        public string constraint_name;
        public string table_schema;
        public string table_name;
        public string column_name;

        /// <summary>
        /// return table name eg. 'public.customer'
        /// </summary>
        public string TableNameWithSchema
        {
            get { return table_schema + "." + table_name; }
        }

        public override string ToString()
        {
            return "KeyColUsage "+constraint_name+":  "+table_name+"."+column_name;
        }
    }

    /// <summary>
    /// class for reading from "information_schema.`Key_Column_Usage`"
    /// </summary>
    class KeyColumnUsageSql
    {
        KeyColumnUsage fromRow(IDataReader rdr)
        {
            KeyColumnUsage t = new KeyColumnUsage();
            int field = 0;
            t.constraint_schema = rdr.GetString(field++);
            t.constraint_name = rdr.GetString(field++);
            t.table_schema  = rdr.GetString(field++);
            t.table_name    = rdr.GetString(field++);
            t.column_name    = rdr.GetString(field++);
            //t.referenced_table_schema = rdr.GetString(field++);
            //t.referenced_table_name = rdr.GetString(field++);
            //t.referenced_column_name = rdr.GetString(field++);
            return t;
        }

        public List<KeyColumnUsage> getConstraints(IDbConnection conn, string db)
        {
            string sql = @"
SELECT constraint_schema,constraint_name,table_schema,table_name
    ,column_name
FROM information_schema.KEY_COLUMN_USAGE
WHERE constraint_catalog=:db";

            return DataCommand.Find<KeyColumnUsage>(conn, sql, ":db", db, fromRow);
        }
    }
}
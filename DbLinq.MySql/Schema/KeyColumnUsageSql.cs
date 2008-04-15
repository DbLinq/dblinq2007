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

using System.Collections.Generic;
using System.Data;
using DbLinq.Util;

namespace DbLinq.MySql.Schema
{
    /// <summary>
    /// represents one row from MySQL's information_schema.`Key_Column_Usage` table
    /// </summary>
    public class KeyColumnUsage
    {
        public string constraint_schema;
        public string constraint_name;
        public string table_schema;
        public string table_name;
        public string column_name;
        public string referenced_table_schema;
        public string referenced_table_name;
        public string referenced_column_name;

        public override string ToString()
        {
            string detail = constraint_name == "PRIMARY"
                                ? table_name + " PK"
                                : constraint_name;
            return "KeyColUsage " + detail;
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
            t.constraint_schema = rdr.GetAsString(field++);
            t.constraint_name = rdr.GetAsString(field++);
            t.table_schema  = rdr.GetAsString(field++);
            t.table_name    = rdr.GetAsString(field++);
            t.column_name    = rdr.GetAsString(field++);
            t.referenced_table_schema = rdr.GetAsString(field++);
            t.referenced_table_name = rdr.GetAsString(field++);
            t.referenced_column_name = rdr.GetAsString(field++);
            return t;
        }

        public List<KeyColumnUsage> getConstraints(IDbConnection conn, string db)
        {
            string sql = @"
SELECT constraint_schema,constraint_name,table_schema,table_name
    ,column_name,referenced_table_schema,referenced_table_name,referenced_column_name
FROM information_schema.`KEY_COLUMN_USAGE`
WHERE table_schema=?db";

            return DataCommand.Find<KeyColumnUsage>(conn, sql,"?db", db, fromRow);
        }
    }
}
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
//        Thomas Glaser
////////////////////////////////////////////////////////////////////
#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using DbLinq.Util;

namespace DbLinq.Ingres.Schema
{
    /// <summary>
    /// represents one row from POSTGRES's information_schema.`TABLES` table
    /// </summary>
    public class TableRow
    {
        public string table_owner;
        public string table_name;
    }

    /// <summary>
    /// class for reading from "information_schema.`TABLES`"
    /// </summary>
    class TableSql
    {
        TableRow fromRow(IDataReader rdr)
        {
            TableRow t = new TableRow();
            int field = 0;
            t.table_name  = rdr.GetString(field++).Trim();
            t.table_owner = rdr.GetString(field++).Trim();
            return t;
        }

        public List<TableRow> getTables(IDbConnection conn, string db)
        {
            string sql = @"
SELECT table_name,table_owner
FROM iitables WHERE table_owner <> '$ingres' AND table_type in ('T', 'V')";

            return DataCommand.Find<TableRow>(conn, sql, fromRow);
        }
    }
}
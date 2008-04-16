#region MIT license
// 
// Copyright (c) 2007-2008 Jiri Moudry
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using DbLinq.Util;

namespace DbLinq.PostgreSql.Schema
{
    /// <summary>
    /// row data from tables table_constraints, constraint_column_usage
    /// </summary>
    public class ForeignKeyCrossRef
    {
        public string constraint_name;
        public string table_name_Child;
        public string constraint_type;
        public string table_schema_Parent;
        public string table_name_Parent;
        public string column_name;

        public override string ToString()
        {
            return "ForKeyXR "+constraint_name+": "+constraint_type+"  "+table_name_Child+"->"+table_name_Parent;
        }
    }

    /// <summary>
    /// class for reading from information_schema -
    /// (from tables table_constraints, constraint_column_usage)
    /// </summary>
    class ForeignKeySql
    {
        ForeignKeyCrossRef fromRow(IDataReader rdr)
        {
            ForeignKeyCrossRef t = new ForeignKeyCrossRef();
            int field = 0;
            t.constraint_name = rdr.GetString(field++);
            t.table_name_Child    = rdr.GetString(field++);
            t.constraint_type = rdr.GetString(field++);
            t.table_schema_Parent = rdr.GetString(field++);
            t.table_name_Parent = rdr.GetString(field++);
            t.column_name = rdr.GetString(field++);
            return t;
        }

        public List<ForeignKeyCrossRef> getConstraints(IDbConnection conn, string db)
        {
            string sql = @"
SELECT t.constraint_name, t.table_name, t.constraint_type,
    c.table_schema, c.table_name, c.column_name
FROM information_schema.table_constraints t,
    information_schema.constraint_column_usage c
WHERE t.constraint_name = c.constraint_name
    and t.constraint_type IN  ('FOREIGN KEY','PRIMARY KEY')";

            return DataCommand.Find<ForeignKeyCrossRef>(conn, sql, ":db", db, fromRow);
        }
    }
}
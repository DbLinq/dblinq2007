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
        public string ConstraintName;
        public string TableSchema;
        public string TableName;
        public string ColumnName;

        public override string ToString()
        {
            return "KeyColUsage "+ConstraintName+":  "+TableName+"."+ColumnName;
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
            t.ConstraintName = rdr.GetAsString(field++);
            t.TableSchema  = rdr.GetAsString(field++);
            t.TableName    = rdr.GetAsString(field++);
            t.ColumnName    = rdr.GetAsString(field++);
            return t;
        }

        public List<KeyColumnUsage> getConstraints(IDbConnection conn, string db)
        {
            string sql = @"
SELECT constraint_name,table_schema,table_name
    ,column_name
FROM information_schema.KEY_COLUMN_USAGE
WHERE constraint_catalog=:db";

            return DataCommand.Find<KeyColumnUsage>(conn, sql, ":db", db, fromRow);
        }
    }
}
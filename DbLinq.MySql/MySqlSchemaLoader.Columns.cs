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
using System.Linq;
using System.Text;
using DbLinq.Util;
using DbLinq.Vendor;

namespace DbLinq.MySql
{
    partial class MySqlSchemaLoader
    {
        protected virtual IDataTableColumn fromRow(IDataReader rdr)
        {
            var t = new DataTableColumn();
            int field = 0;
            t.TableSchema = rdr.GetAsString(field++);
            t.TableName = rdr.GetAsString(field++);
            t.ColumnName = rdr.GetAsString(field++);
            string nullableStr = rdr.GetAsString(field++);
            t.Nullable = nullableStr == "YES";
            t.Type = rdr.GetAsString(field++);
            t.DefaultValue = rdr.GetAsString(field++); // picrap TODO: this is NOT the default value, so find a way to also collect the default value
            t.FullType = rdr.GetAsString(field++);
            t.Unsigned = t.FullType.Contains("unsigned");
            string columnKey = rdr.GetAsString(field++);
            t.PrimaryKey = columnKey == "PRI";
            t.Length = rdr.GetAsNullableNumeric<long>(field++);
            t.Precision = rdr.GetAsNullableNumeric<int>(field++);
            t.Scale = rdr.GetAsNullableNumeric<int>(field++);
            return t;
        }

        public override IList<IDataTableColumn> ReadColumns(IDbConnection connectionString, string databaseName)
        {
            const string sql = @"
SELECT table_schema,table_name,column_name
    ,is_nullable,data_type,extra,column_type
    ,column_key,character_maximum_length,numeric_precision,numeric_scale
FROM information_schema.`COLUMNS`
WHERE table_schema=?db";

            return DataCommand.Find<IDataTableColumn>(connectionString, sql, "?db", databaseName, fromRow);
        }
    }
}

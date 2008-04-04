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
using DbLinq.Vendor.Implementation;

namespace DbLinq.Ingres.Schema
{
    /// <summary>
    /// represents one row from information_schema.`COLUMNS`
    /// </summary>
    public class Column: SchemaLoader.DataType
    {
        public string table_owner;
        public string table_name;
        public string column_name;
        public bool isNullable;

        /// <summary>
        /// eg. next value for "linquser"."categories_seq"
        /// </summary>
        public string column_default;

        /// <summary>
        /// return 'varchar(50)' or 'decimal(30,2)'
        /// </summary>
        public string DataTypeWithWidth
        {
            get
            {
                switch (Type)
                {
                    case "C":
                    case "CHAR":
                    case "NCHAR":
                    case "VARCHAR":
                    case "NVARCHAR":
                    case "LONG VARCHAR":
                    case "TEXT":
                    case "INTEGER":
                        return Type + "(" + Length + ")";

                    case "DECIMAL":
                        return Type + "(" + Length + ", " + Scale + ")";

                }
                return Type;
            }
        }

        public override string ToString()
        {
            return "Column " + table_name + "." + column_name + "  " + Type.Substring(0, 4);
        }
    }

    /// <summary>
    /// class for sleecting from information_schema.`COLUMNS`
    /// </summary>
    class ColumnSql
    {
        Column fromRow(IDataReader rdr)
        {
            Column t = new Column();
            int field = 0;
            t.table_owner = rdr.GetString(field++).Trim();
            t.table_name = rdr.GetString(field++).Trim();
            t.column_name = rdr.GetString(field++).Trim();
            string nullableStr = rdr.GetString(field++);
            t.isNullable = nullableStr == "Y";
            t.Type = rdr.GetString(field++).Trim();
            t.column_default = GetStringN(rdr, field++);
            //t.extra         = null; //rdr.GetString(field++);
            //t.column_key    = null; //rdr.GetString(field++);

            t.Length = GetIntN(rdr, field++);
            t.Scale = GetIntN(rdr, field++);

            return t;
        }

        public int? GetIntN(IDataReader rdr, int field)
        {
            return rdr.IsDBNull(field) ? (int?)null : rdr.GetInt32(field);
        }
        public string GetStringN(IDataReader rdr, int field)
        {
            return rdr.IsDBNull(field) ? (string)null : rdr.GetString(field);
        }

        public List<Column> getColumns(IDbConnection conn, string db)
        {
            string sql = @"
SELECT t.table_owner, t.table_name, column_name
    ,column_nulls, column_datatype, column_default_val
    ,column_length, column_scale
FROM iicolumns c join iitables t on (c.table_name=t.table_name and c.table_owner=t.table_owner) 
            WHERE t.table_owner <> '$ingres' and t.table_type in ('T', 'V')
ORDER BY column_sequence
";

            return DataCommand.Find<Column>(conn, sql, fromRow);
        }

    }
}
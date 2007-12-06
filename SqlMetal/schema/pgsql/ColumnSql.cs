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
using System.Text;
using Npgsql;

namespace SqlMetal.schema.pgsql
{
    /// <summary>
    /// represents one row from information_schema.`COLUMNS`
    /// </summary>
    public class Column
    {
        public string table_catalog;
        public string table_schema;
        public string table_name;
        public string column_name;
        public bool isNullable;

        /// <summary>
        /// eg 'int' or 'datetime'
        /// </summary>
        public string datatype;
        //public string extra;

        /// <summary>
        /// eg. 'int(10) unsigned'
        /// </summary>
        public string column_type;

        /// <summary>
        /// if you use domains to typedef a new type, this will be non-null
        /// </summary>
        public string domain_schema;
        public string domain_name;

        /// <summary>
        /// eg. for column called 'int' we use csharpName='int_'
        /// </summary>
        public string csharpFieldName;

        /// <summary>
        /// eg. "nextval('products_productid_seq'::regclass)"
        /// </summary>
        public string column_default;

        public int? character_maximum_length;
        public int? numeric_precision;
        public int? numeric_scale;

        /// <summary>
        /// return table name eg. 'public.customer'
        /// </summary>
        public string TableNameWithSchema
        {
            get { return table_schema + "." + table_name; }
        }

        /// <summary>
        /// return 'varchar(50)' or 'decimal(30,2)'
        /// </summary>
        public string DataTypeWithWidth
        {
            get
            {
                if (mmConfig.useDomainTypes && domain_name != null)
                    return domain_schema +"." + domain_name; //without precision - precision is already defined in CREATE DOMAIN

                if (character_maximum_length != null)
                    return datatype + "(" + character_maximum_length + ")";
                if (numeric_precision != null && numeric_scale != null)
                    return datatype + "(" + numeric_precision + "," + numeric_scale + ")";
                return datatype;
            }
        }

        public override string ToString()
        {
            return "Column " + table_name + "." + column_name + "  " + datatype.Substring(0, 4);
        }
    }

    /// <summary>
    /// class for sleecting from information_schema.`COLUMNS`
    /// </summary>
    class ColumnSql
    {
        Column fromRow(NpgsqlDataReader rdr)
        {
            Column t = new Column();
            int field = 0;
            t.table_catalog = rdr.GetString(field++);
            t.table_schema = rdr.GetString(field++);
            t.table_name = rdr.GetString(field++);
            t.column_name = rdr.GetString(field++);
            string nullableStr = rdr.GetString(field++);
            t.isNullable = nullableStr == "YES";
            t.datatype = rdr.GetString(field++);
            t.domain_schema = GetStringN(rdr, field++);
            t.domain_name = GetStringN(rdr, field++);
            t.column_default = GetStringN(rdr, field++);
            //t.extra         = null; //rdr.GetString(field++);
            t.column_type = null; //rdr.GetString(field++);
            //t.column_key    = null; //rdr.GetString(field++);

            t.character_maximum_length = GetIntN(rdr, field++);
            t.numeric_precision = GetIntN(rdr, field++);
            t.numeric_scale = GetIntN(rdr, field++);

            return t;
        }

        public int? GetIntN(NpgsqlDataReader rdr, int field)
        {
            return rdr.IsDBNull(field) ? (int?)null : rdr.GetInt32(field);
        }
        public string GetStringN(NpgsqlDataReader rdr, int field)
        {
            return rdr.IsDBNull(field) ? (string)null : rdr.GetString(field);
        }

        public List<Column> getColumns(NpgsqlConnection conn, string db)
        {
            string sql = @"
SELECT table_catalog, table_schema, table_name, column_name
    ,is_nullable, data_type, domain_schema, domain_name, column_default
    ,character_maximum_length, numeric_precision, numeric_scale
FROM information_schema.COLUMNS
WHERE table_catalog=:db
AND table_schema NOT IN ('pg_catalog','information_schema')
ORDER BY ordinal_position
";

            using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
            {
                cmd.Parameters.Add(":db", db);
                using (NpgsqlDataReader rdr = cmd.ExecuteReader())
                {
                    List<Column> list = new List<Column>();
                    while (rdr.Read())
                    {
                        list.Add(fromRow(rdr));
                    }
                    return list;
                }
            }

        }

    }
}

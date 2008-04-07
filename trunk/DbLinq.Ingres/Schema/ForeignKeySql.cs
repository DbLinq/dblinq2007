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
using System.Text;
using System.Data;
using DbLinq.Util;

namespace DbLinq.Ingres.Schema
{
    /// <summary>
    /// row data from tables table_constraints, constraint_column_usage
    /// </summary>
    public class ForeignKeyCrossRef
    {
        public string schema_name_parent;
        public string table_name_parent;
        /// <summary>
        /// e.g.
        /// PRIMARY KEY (productid)
        /// FOREIGN KEY (productid) REFERENCES "linquser".products(productid)
        /// </summary>
        public string text_segment;

        /// <summary>
        /// P = PRIMARY KEY; R = FOREIGN KEY
        /// </summary>
        public string constraint_type;

        public override string ToString()
        {
            return text_segment;
        }

        public string constraint_name
        {
            get
            {
                return schema_name_parent + "_" + table_name_parent + "_" + column_name_parent + "_" +
                    schema_name_child + "_" + table_name_child + "_" + column_name_child;
            }
        }

        public string[] column_name_primaries
        {
            get
            {
                string[] tmp = text_segment
                    .Replace("PRIMARY KEY(", "")
                    .Replace(")", "")
                    .Split(',');
                for (int i = 0; i < tmp.Length; i++)
                {
                    tmp[i] = tmp[i].Trim();
                    tmp[i] = tmp[i].Replace("\"", "");
                }
                return tmp;
            }
        }

        public string column_name_parent
        {
            get
            {
                string[] tmp = text_segment.Split('(');
                return tmp[1].Substring(0, tmp[1].IndexOf(")"));
            }
        }

        public string column_name_child
        {
            get
            {
                string tmp = text_segment
                    .Substring(text_segment.LastIndexOf("(") + 1);
                return tmp
                    .Substring(0, tmp.IndexOf(")"));
            }
        }

        public string schema_name_child
        {
            get
            {
                string tmp = text_segment
                    .Substring(text_segment.IndexOf("\"") + 1);
                return tmp
                    .Substring(0, tmp.IndexOf("\""));
            }
        }

        public string table_name_child
        {
            get
            {
                string tmp = text_segment
                    .Substring(text_segment.IndexOf("\".") + 2);
                if (tmp.Contains("("))
                {
                    return tmp
                        .Substring(0, tmp.IndexOf("("));
                }
                return tmp;
            }
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
            t.schema_name_parent = rdr.GetString(field++).Trim();
            t.table_name_parent = rdr.GetString(field++).Trim();
            t.text_segment = rdr.GetString(field++).Trim();
            t.constraint_type = rdr.GetString(field++).Trim();
            return t;
        }

        public List<ForeignKeyCrossRef> getConstraints(IDbConnection conn, string db)
        {
            string sql = @"
SELECT
    schema_name,
    table_name,
    text_segment,
    constraint_type
FROM
    iiconstraints
WHERE
    system_use = 'U'";

            return DataCommand.Find<ForeignKeyCrossRef>(conn, sql, fromRow);
        }
    }
}
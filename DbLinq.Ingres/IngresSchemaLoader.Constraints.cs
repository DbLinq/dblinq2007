#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne
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

using System.Collections.Generic;
using System.Data;
using DbLinq.Util;

namespace DbLinq.Ingres
{
    partial class IngresSchemaLoader
    {
        /// <summary>
        /// row data from tables table_constraints, constraint_column_usage
        /// </summary>
        protected class DataConstraint
        {
            public string TableSchema;
            public string TableName;
            /// <summary>
            /// e.g.
            /// PRIMARY KEY (productid)
            /// FOREIGN KEY (productid) REFERENCES "linquser".products(productid)
            /// </summary>
            public string TextSegment;

            /// <summary>
            /// P = PRIMARY KEY; R = FOREIGN KEY
            /// </summary>
            public string ConstraintType;

            public override string ToString()
            {
                return TextSegment;
            }

            public string ConstraintName
            {
                get
                {
                    return TableSchema + "_" + TableName + "_" + ColumnName + "_" +
                        ReferencedTableSchema + "_" + ReferencedTableName + "_" + ReferencedColumnName;
                }
            }

            public string[] column_name_primaries
            {
                get
                {
                    string[] tmp = TextSegment
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

            public string ColumnName
            {
                get
                {
                    string[] tmp = TextSegment.Split('(');
                    return tmp[1].Substring(0, tmp[1].IndexOf(")"));
                }
            }

            public string ReferencedColumnName
            {
                get
                {
                    string tmp = TextSegment
                        .Substring(TextSegment.LastIndexOf("(") + 1);
                    return tmp
                        .Substring(0, tmp.IndexOf(")"));
                }
            }

            public string ReferencedTableSchema
            {
                get
                {
                    string tmp = TextSegment
                        .Substring(TextSegment.IndexOf("\"") + 1);
                    return tmp
                        .Substring(0, tmp.IndexOf("\""));
                }
            }

            public string ReferencedTableName
            {
                get
                {
                    string tmp = TextSegment
                        .Substring(TextSegment.IndexOf("\".") + 2);
                    if (tmp.Contains("("))
                    {
                        return tmp
                            .Substring(0, tmp.IndexOf("("));
                    }
                    return tmp;
                }
            }

        }

        protected virtual DataConstraint ReadContraint(IDataReader rdr)
        {
            var contraint = new DataConstraint();
            int field = 0;
            contraint.TableSchema = rdr.GetString(field++).Trim();
            contraint.TableName = rdr.GetString(field++).Trim();
            contraint.TextSegment = rdr.GetString(field++).Trim();
            contraint.ConstraintType = rdr.GetString(field++).Trim();
            return contraint;
        }

        protected virtual List<DataConstraint> ReadConstraints(IDbConnection conn, string db)
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

            return DataCommand.Find<DataConstraint>(conn, sql, ReadContraint);
        }
    }
}

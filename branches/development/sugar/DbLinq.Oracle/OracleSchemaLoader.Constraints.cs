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

namespace DbLinq.Oracle
{
    partial class OracleSchemaLoader
    {
        protected class DataConstraint
        {
            public string TableSchema;
            public string ConstraintName;
            public string TableName;
            public string ColumnName;
            public string ConstraintType;
            public string ReverseConstraintName;

            public override string ToString()
            {
                return "User_Constraint  " + TableName + "." + ColumnName;
            }
        }

        protected virtual DataConstraint ReadConstraint(IDataReader rdr)
        {
            var constraint = new DataConstraint();
            int field = 0;
            constraint.TableSchema = rdr.GetAsString(field++);
            constraint.ConstraintName = rdr.GetAsString(field++);
            constraint.TableName = rdr.GetAsString(field++);
            constraint.ColumnName = rdr.GetAsString(field++);
            constraint.ConstraintType = rdr.GetAsString(field++);
            constraint.ReverseConstraintName = rdr.GetAsString(field++);
            return constraint;
        }

        protected virtual List<DataConstraint> ReadConstraints(IDbConnection conn, string db)
        {
            string sql = @"
SELECT UCC.owner, UCC.constraint_name, UCC.table_name, UCC.column_name, UC.constraint_type, UC.R_constraint_name
FROM all_cons_columns UCC, all_constraints UC
WHERE UCC.constraint_name=UC.constraint_name
AND UCC.table_name=UC.table_name
AND UCC.TABLE_NAME NOT LIKE '%$%' AND UCC.TABLE_NAME NOT LIKE 'LOGMNR%' AND UCC.TABLE_NAME NOT IN ('HELP','SQLPLUS_PRODUCT_PROFILE')
AND UC.CONSTRAINT_TYPE!='C'
and lower(UCC.owner) = :owner";

            return DataCommand.Find<DataConstraint>(conn, sql, ":owner", db.ToLower(), ReadConstraint);
        }
    }
}

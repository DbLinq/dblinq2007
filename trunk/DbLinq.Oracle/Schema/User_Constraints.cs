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

namespace DbLinq.Oracle.Schema
{
    public class User_Constraints_Row
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

    public class User_Constraints_Sql
    {
        User_Constraints_Row fromRow(IDataReader rdr)
        {
            User_Constraints_Row t = new User_Constraints_Row();
            int field = 0;
            t.TableSchema = rdr.GetAsString(field++);
            t.ConstraintName  = rdr.GetAsString(field++);
            t.TableName    = rdr.GetAsString(field++);
            t.ColumnName   = rdr.GetAsString(field++);
            t.ConstraintType = rdr.GetAsString(field++);
            t.ReverseConstraintName = rdr.GetAsString(field++);
            return t;
        }

        public List<User_Constraints_Row> getConstraints1(IDbConnection conn, string db)
        {
            string sql = @"
SELECT UCC.owner, UCC.constraint_name, UCC.table_name, UCC.column_name, UC.constraint_type, UC.R_constraint_name
FROM all_cons_columns UCC, all_constraints UC
WHERE UCC.constraint_name=UC.constraint_name
AND UCC.table_name=UC.table_name
AND UCC.TABLE_NAME NOT LIKE '%$%' AND UCC.TABLE_NAME NOT LIKE 'LOGMNR%' AND UCC.TABLE_NAME NOT IN ('HELP','SQLPLUS_PRODUCT_PROFILE')
AND UC.CONSTRAINT_TYPE!='C'
and lower(UCC.owner) = :owner";

            return DataCommand.Find<User_Constraints_Row>(conn, sql, ":owner", db.ToLower(), fromRow);
        }

    }
}
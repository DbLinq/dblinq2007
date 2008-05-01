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
using System.Linq;
using System.Linq.Expressions;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using DbLinq.Logging;
using DbLinq.Util;
using DbLinq.Vendor;

namespace DbLinq.Linq.Clause
{
    class FromClauseBuilder
    {
        /// <summary>
        /// given type Employee, select all its fields: 'SELECT e.ID, e.Name,... FROM Employee'
        /// (by examining [Table] and [Column] attribs)
        /// </summary>
        /// <param name="selectParts">'output' - gets populated with query parts.</param>
        /// <param name="t">input type</param>
        /// <param name="nick">nickname such as $o for an Order</param>
        /// <returns></returns>
        public static void SelectAllFields(SessionVarsParsed vars, SqlExpressionParts selectParts, Type t1, string nick)
        {
            Type t = AttribHelper.ExtractTypeFromMSet(t1);
            TableAttribute tAttrib = AttribHelper.GetTableAttrib(t);
            string PROJECTED_CLASS_NAME = "<>f__AnonymousType";
            if (vars != null && tAttrib == null && t.Name.StartsWith(PROJECTED_CLASS_NAME))
            {
                //GroupBy: given t=Proj, find our table type
                //example: GroupBy-proj: {c => new {PostalCode = c.PostalCode, ContactName = c.ContactName}}
                if (t == vars.GroupByNewExpression.Body.Type)
                {
                    Type groupByParamT = vars.GroupByNewExpression.Parameters[0].Type; //this is 'c' in the example
                    tAttrib = AttribHelper.GetTableAttrib(groupByParamT);
                }
            }

            if (tAttrib == null)
                throw new ApplicationException("Class " + t + " is missing [Table] attribute");

            if (selectParts.DoneClauses.Contains(nick))
            {
                vars.Context.Logger.Write(Level.Warning, "Duplicate GetClause for {0}, skipping", nick);
                return;
            }
            selectParts.DoneClauses.Add(nick);

            if (vars.ProjectionData == null)
            {
                vars.ProjectionData = ProjectionData.FromDbType(t);
            }

            ColumnAttribute[] colAttribs2 = vars.ProjectionData.fields
                .Select(f => f.columnAttribute)
                .ToArray();

            foreach (ColumnAttribute colAtt in colAttribs2)
            {
                string safeColumnName = vars.Context.Vendor.GetSqlFieldSafeName(colAtt.Name);
                string part = nick + "." + safeColumnName; //eg. '$o.OrderID'
                selectParts.AddSelect(part);
            }

            //build string '[Order Details] o$'
            string tableName2 = vars.Context.Vendor.GetSqlFieldSafeName(tAttrib.Name) + " " + nick;
            selectParts.AddFrom(tableName2);
        }

    }
}

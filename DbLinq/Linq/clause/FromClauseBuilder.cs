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

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using DBLinq.util;

namespace DBLinq.Linq.clause
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
            if (vars!=null && tAttrib == null && t.Name.StartsWith(PROJECTED_CLASS_NAME))
            {
                //GroupBy: given t=Proj, find our table type
                //example: GroupBy-proj: {c => new {PostalCode = c.PostalCode, ContactName = c.ContactName}}
                if(t==vars.groupByNewExpr.Body.Type){
                    Type groupByParamT = vars.groupByNewExpr.Parameters[0].Type; //this is 'c' in the example
                    tAttrib = AttribHelper.GetTableAttrib(groupByParamT);
                }
            }

            if(tAttrib==null)
                throw new ApplicationException("Class "+t+" is missing [Table] attribute");

            if(selectParts.doneClauses.Contains(nick))
            {
                Console.WriteLine("Duplicate GetClause for "+nick+", skipping");
                return;
            }
            selectParts.doneClauses.Add(nick);

            ColumnAttribute[] colAttribs = AttribHelper.GetColumnAttribs(t);

            foreach(ColumnAttribute colAtt in colAttribs)
            {
                string safeColumnName = vendor.Vendor.FieldName_Safe(colAtt.Name);
                string part = nick + "." + safeColumnName; //eg. '$o.OrderID'
                selectParts.AddSelect( part );
            }

            string tableName2 = tAttrib.Name + " " + nick;
            selectParts.AddFrom( tableName2 );
        }

    }
}

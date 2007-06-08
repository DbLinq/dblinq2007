////////////////////////////////////////////////////////////////////
//Initial author: Jiri George Moudry, 2006.
//License: LGPL. (Visit http://www.gnu.org)
//Commercial code may call into this library, if it's in a different module (DLL)
////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

#if LINQ_PREVIEW_2006
//Visual Studio 2005 with Linq Preview May 2006 - can run on Win2000
using System.Expressions;
using System.Data.DLinq;
#else
//Visual Studio Orcas - requires WinXP
using System.Linq.Expressions;
using System.Data.Linq;
#endif

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
        public static void SelectAllFields(SessionVars vars, SqlExpressionParts selectParts, Type t1, string nick)
        {
            Type t = AttribHelper.ExtractTypeFromMSet(t1);
            TableAttribute tAttrib = AttribHelper.GetTableAttrib(t);
#if LINQ_PREVIEW_2006
            string PROJECTED_CLASS_NAME = "<Projection>";
#else
            string PROJECTED_CLASS_NAME = "<>f__AnonymousType";
#endif
            if (tAttrib == null && t.Name.StartsWith(PROJECTED_CLASS_NAME))
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

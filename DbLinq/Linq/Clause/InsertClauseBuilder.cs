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
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Data.Linq.Mapping;
using System.Data;

#if MONO_STRICT
using System.Data.Linq;
#else
using DbLinq.Data.Linq;
#endif

using DbLinq.Linq;
using DbLinq.Linq.Database;
using DbLinq.Vendor;
using DbLinq.Util;
using System.Diagnostics;

namespace DbLinq.Linq.Clause
{
#if MONO_STRICT
    internal
#else
    public
#endif
    class InsertClauseBuilder
    {
        /// <summary>
        /// given projData constructed from type Product, 
        /// return string '(ProductName, SupplierID, CategoryID, QuantityPerUnit)'
        /// (suitable for use in INSERT statement)
        /// </summary>
        public static string InsertRowHeader(ProjectionData projData)
        {
            StringBuilder sbNames = new StringBuilder("(");
            int numFieldsAdded = 0;
            foreach (ProjectionData.ProjectionField projFld in projData.fields)
            {
                ColumnAttribute colAtt = projFld.columnAttribute;

                if (colAtt.IsDbGenerated)
                    continue; //if field is auto-generated ID , don't send field

                //append string, eg. ",Name"
                if (numFieldsAdded++ > 0)
                {
                    sbNames.Append(", ");
                }
                sbNames.Append(colAtt.Name);
            }
            return sbNames.Append(")").ToString();
        }

        /// <summary>
        /// given an object we want to insert into a table, build the '(?p1,?p2,P3)' 
        /// and package up a list of SqlParameter objects.
        /// 
        /// In Mysql, called multiple times in a row to do a 'bulk insert'.
        /// </summary>
        public static string InsertRowFields(IVendor vendor, IDbCommand cmd, object objectToInsert, ProjectionData projData
            , List<IDbDataParameter> paramList, ref int numFieldsAdded)
        {
            StringBuilder sbVals = new StringBuilder("(");
            string separator = "";
            //int numFieldsAdded = 0;
            foreach (ProjectionData.ProjectionField projFld in projData.fields)
            {
                ColumnAttribute colAtt = projFld.columnAttribute;
                if (colAtt.IsDbGenerated)
                    continue; //if field is auto-generated ID , don't send field

                object paramValue = projFld.GetFieldValue(objectToInsert);

                //get either ":p0" or "?p0"
                string paramName = vendor.GetOrderableParameterName(numFieldsAdded++);
                sbVals.Append(separator).Append(paramName);
                separator = ", ";

                IDbDataParameter param = vendor.CreateDbDataParameter(cmd, colAtt.DbType, paramName);

                param.Value = paramValue;
                paramList.Add(param);
            }
            return sbVals.Append(")").ToString();
        }
    }
}

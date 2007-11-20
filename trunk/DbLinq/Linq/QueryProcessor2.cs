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
using System.Linq.Expressions;
using DBLinq.Linq.clause;
using DBLinq.util;

namespace DBLinq.Linq
{
    /// <summary>
    /// after all Lambdas are collected, and GetEnumerator() is called:
    /// QueryProcessor calls ExpressionTreeParser to build SQL expression from parts
    /// </summary>
    partial class QueryProcessor
    {
        void processWhereClause(LambdaExpression lambda)
        {
            ParseResult result = null;
            ParseInputs inputs = new ParseInputs(result);
            result = ExpressionTreeParser.Parse(this, lambda.Body, inputs);

            if (GroupHelper.IsGrouping(lambda.Parameters[0].Type))
            {
                _vars._sqlParts.AddHaving(result.columns);
            }
            else
            {
                _vars._sqlParts.AddWhere(result.columns);
            }

            result.CopyInto(_vars._sqlParts); //transfer params and tablesUsed
        }

        void processSelectClause(LambdaExpression selectExpr)
        {
            this.selectExpr = selectExpr; //store for GroupBy & friends

            //necesary for projections?
            if (_vars.groupByExpr == null)
            {
                _vars.projectionData = ProjectionData.FromSelectExpr(selectExpr);
            }
            else
            {
                _vars.projectionData = ProjectionData.FromSelectGroupByExpr(selectExpr, _vars.groupByExpr, _vars._sqlParts);
            }

            ParseResult result = null;
            ParseInputs inputs = new ParseInputs(result);
            inputs.groupByExpr = _vars.groupByExpr;
            if (selectExpr.Body.NodeType == ExpressionType.Parameter)
            {
                //'from p in Products select p' - do nothing, will result in SelectAllFields() later
            }
            else
            {
                result = ExpressionTreeParser.Parse(this, selectExpr.Body, inputs);
                _vars._sqlParts.AddSelect(result.columns);
                result.CopyInto(_vars._sqlParts); //transfer params and tablesUsed

                //support for subsequent Count() - see F2_ProductCount_Clause
                if (result.columns.Count > 0)
                {
                    // currentVarNames[int] = "p$.ProductID";
                    this.currentVarNames[selectExpr.Body.Type] = result.columns[0];
                }
            }
        }

        void processOrderByClause(LambdaExpression orderByExpr, string orderBy_desc)
        {
            ParseResult result = null;
            ParseInputs inputs = new ParseInputs(result);
            result = ExpressionTreeParser.Parse(this, orderByExpr.Body, inputs);
            string orderByFields = string.Join(",", result.columns.ToArray());
            _vars._sqlParts.orderByList.Add(orderByFields);
            _vars._sqlParts.orderBy_desc = orderBy_desc; //copy 'DESC' specifier
        }
    }
}

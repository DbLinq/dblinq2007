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
using System.Linq.Expressions;

#if MONO_STRICT
using System.Data.Linq.Sugar.Expressions;
#else
using DbLinq.Data.Linq.Sugar.Expressions;
#endif

namespace DbLinq.Vendor
{
#if MONO_STRICT
    internal
#else
    public
#endif
    interface ISqlProvider
    {
        string NewLine { get; }

        /// <summary>
        /// Converts a constant value to a literal representation
        /// </summary>
        /// <param name="literal"></param>
        /// <returns></returns>
        string GetLiteral(object literal);

        /// <summary>
        /// Converts a standard operator to an expression
        /// </summary>
        /// <param name="operationType"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        string GetLiteral(ExpressionType operationType, IList<string> p);

        /// <summary>
        /// Converts a special expression type to literal
        /// </summary>
        /// <param name="operationType"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        string GetLiteral(SpecialExpressionType operationType, IList<string> p);

        /// <summary>
        /// Places the expression into parenthesis
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        string GetParenthesis(string a);

        /// <summary>
        /// Returns a column related to a table.
        /// Ensures about the right case
        /// </summary>
        /// <param name="table"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        string GetColumn(string table, string column);

        /// <summary>
        /// Returns a column related to a table.
        /// Ensures about the right case
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        string GetColumn(string column);

        /// <summary>
        /// Returns a table alias
        /// Ensures about the right case
        /// </summary>
        /// <param name="table"></param>
        /// <param name="alias"></param>
        /// <returns></returns>
        string GetTableAsAlias(string table, string alias);

        /// <summary>
        /// Returns a table alias
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        string GetTable(string table);

        /// <summary>
        /// Returns a literal parameter name
        /// </summary>
        /// <returns></returns>
        string GetParameterName(string nameBase);

        /// <summary>
        /// Joins a list of table selection to make a FROM clause
        /// </summary>
        /// <param name="tables"></param>
        /// <returns></returns>
        string GetFromClause(string[] tables);

        /// <summary>
        /// Joins a list of conditions to make a WHERE clause
        /// </summary>
        /// <param name="wheres"></param>
        /// <returns></returns>
        string GetWhereClause(string[] wheres);

        /// <summary>
        /// Returns a valid alias syntax for the given table
        /// </summary>
        /// <param name="nameBase"></param>
        /// <returns></returns>
        string GetTableAlias(string nameBase);

        /// <summary>
        /// Joins a list of operands to make a SELECT clause
        /// </summary>
        /// <param name="selects"></param>
        /// <returns></returns>
        string GetSelectClause(string[] selects);

        /// <summary>
        /// Returns all table columns (*)
        /// </summary>
        /// <returns></returns>
        string GetColumns();

        /// <summary>
        /// Returns a LIMIT clause around a SELECT clause
        /// </summary>
        /// <param name="select">SELECT clause</param>
        /// <param name="limit">limit value (number of columns to be returned)</param>
        /// <returns></returns>
        string GetLiteralLimit(string select, string limit);

        /// <summary>
        /// Returns a LIMIT clause around a SELECT clause, with offset
        /// </summary>
        /// <param name="select">SELECT clause</param>
        /// <param name="limit">limit value (number of columns to be returned)</param>
        /// <param name="offset">first row to be returned (starting from 0)</param>
        /// <param name="offsetAndLimit">limit+offset</param>
        /// <returns></returns>
        string GetLiteralLimit(string select, string limit, string offset, string offsetAndLimit);

        /// <summary>
        /// Returns an ORDER criterium
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="descending"></param>
        /// <returns></returns>
        string GetOrderByColumn(string expression, bool descending);

        /// <summary>
        /// Joins a list of conditions to make a ORDER BY clause
        /// </summary>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        string GetOrderByClause(string[] orderBy);

        /// <summary>
        /// Joins a list of conditions to make a GROUP BY clause
        /// </summary>
        /// <param name="groupBy"></param>
        /// <returns></returns>
        string GetGroupByClause(string[] groupBy);

        /// <summary>
        /// Joins a list of conditions to make a HAVING clause
        /// </summary>
        /// <param name="havings"></param>
        /// <returns></returns>
        string GetHavingClause(string[] havings);

        /// <summary>
        /// Returns an operation between two SELECT clauses (UNION, UNION ALL, etc.)
        /// </summary>
        /// <param name="selectOperator"></param>
        /// <param name="selectA"></param>
        /// <param name="selectB"></param>
        /// <returns></returns>
        string GetLiteral(SelectOperatorType selectOperator, string selectA, string selectB);

        /// <summary>
        /// Builds an insert clause
        /// </summary>
        /// <param name="table">Table name</param>
        /// <param name="inputColumns">Columns to be inserted</param>
        /// <param name="inputValues">Values to be inserted into columns</param>
        /// <returns></returns>
        string GetInsert(string table, IList<string> inputColumns, IList<string> inputValues);

        /// <summary>
        /// Builds the statements that gets back the IDs for the inserted statement
        /// </summary>
        /// <param name="outputParameters">Expected output parameters</param>
        /// <param name="outputExpressions">Expressions (to help generate output parameters)</param>
        /// <returns></returns>
        string GetInsertIds(IList<string> outputParameters, IList<string> outputExpressions);

        /// <summary>
        /// Builds an update clause
        /// </summary>
        /// <param name="table"></param>
        /// <param name="inputColumns">Columns to be inserted</param>
        /// <param name="inputValues">Values to be inserted into columns</param>
        /// <param name="outputParameters">Expected output parameters</param>
        /// <param name="outputExpressions">Expressions (to help generate output parameters)</param>
        /// <param name="inputPKColumns">PK columns for reference</param>
        /// <param name="inputPKValues">PK values for reference</param>
        /// <returns></returns>
        string GetUpdate(string table, IList<string> inputColumns, IList<string> inputValues,
                                         IList<string> outputParameters, IList<string> outputExpressions,
                                         IList<string> inputPKColumns, IList<string> inputPKValues);

        /// <summary>
        /// Builds a delete clause
        /// </summary>
        /// <param name="table"></param>
        /// <param name="inputPKColumns">PK columns for reference</param>
        /// <param name="inputPKValues">PK values for reference</param>
        /// <returns></returns>
        string GetDelete(string table, IList<string> inputPKColumns, IList<string> inputPKValues);

        /// <summary>
        /// given 'User', return '[User]' to prevent a SQL keyword conflict
        /// </summary>
        /// <param name="name"></param>
        string GetSafeName(string name);

        /// <summary>
        /// Returns a case safe query, converting quoted names &lt;&ltMixedCaseName>> to "MixedCaseName"
        /// </summary>
        /// <param name="sqlString"></param>
        /// <returns></returns>
        string GetSafeQuery(string sqlString);

        ///<summary>
        ///Returns a string with a conversion of an expression(value) to a type(newType)
        ///</summary>
        /// <example>
        /// In sqlServer: 
        /// value= OrderDetail.Quantity
        /// newType= boolean
        /// 
        /// it should return CONVERT(bit,OrderDetail.Quantity)
        /// </example>
        /// <returns></returns>
        string GetLiteralConvert(string value, System.Type newType);
    }
}

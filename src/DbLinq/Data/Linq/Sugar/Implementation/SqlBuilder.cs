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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

#if MONO_STRICT
using System.Data.Linq.Sugar;
using System.Data.Linq.Sugar.ExpressionMutator;
using System.Data.Linq.Sugar.Expressions;
#else
using DbLinq.Data.Linq.Sugar.ExpressionMutator;
using DbLinq.Data.Linq.Sugar.Expressions;
using DbLinq.Data.Linq.Sugar;
#endif

using DbLinq.Factory;
using DbLinq.Util;

#if MONO_STRICT
namespace System.Data.Linq.Sugar.Implementation
#else
namespace DbLinq.Data.Linq.Sugar.Implementation
#endif
{
    internal class SqlBuilder : ISqlBuilder
    {
        public IExpressionQualifier ExpressionQualifier { get; set; }

        public SqlBuilder()
        {
            ExpressionQualifier = ObjectFactory.Get<IExpressionQualifier>();
        }

        /// <summary>
        /// Builds a SQL string, based on a QueryContext
        /// The build indirectly depends on ISqlProvider which provides all SQL parts.
        /// </summary>
        /// <param name="expressionQuery"></param>
        /// <param name="queryContext"></param>
        /// <returns></returns>
        public string BuildSelect(ExpressionQuery expressionQuery, QueryContext queryContext)
        {
            return Build(expressionQuery.Select, queryContext);
        }

        public string Build(SelectExpression selectExpression, QueryContext queryContext)
        {
            // A scope usually has:
            // - a SELECT: the operation creating a CLR object with data coming from SQL tier
            // - a FROM: list of tables
            // - a WHERE: list of conditions
            // - a GROUP BY: grouping by selected columns
            // - a ORDER BY: sort
            var from = BuildFrom(selectExpression.Tables, queryContext);
            var join = BuildJoin(selectExpression.Tables, queryContext);
            var where = BuildWhere(selectExpression.Tables, selectExpression.Where, queryContext);
            var select = BuildSelect(selectExpression, queryContext);
            var groupBy = BuildGroupBy(selectExpression.Group, queryContext);
            var having = BuildHaving(selectExpression.Where, queryContext);
            var orderBy = BuildOrderBy(selectExpression.OrderBy, queryContext);
            select = Join(queryContext, select, from, join, where, groupBy, having, orderBy);
            select = BuildLimit(selectExpression, select, queryContext);

            if (selectExpression.NextSelectExpression != null)
            {
                var nextLiteralSelect = Build(selectExpression.NextSelectExpression, queryContext);
                select = queryContext.DataContext.Vendor.SqlProvider.GetLiteral(
                    selectExpression.NextSelectExpressionOperator,
                    select, nextLiteralSelect);
            }

            return select;
        }

        public string Join(QueryContext queryContext, params string[] clauses)
        {
            return string.Join(queryContext.DataContext.Vendor.SqlProvider.NewLine,
                               (from clause in clauses where !string.IsNullOrEmpty(clause) select clause).ToArray());
        }

        /// <summary>
        /// The simple part: converts an expression to SQL
        /// This is not used for FROM clause
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="queryContext"></param>
        /// <returns></returns>
        protected virtual string BuildExpression(Expression expression, QueryContext queryContext)
        {
            var sqlProvider = queryContext.DataContext.Vendor.SqlProvider;
            var currentPrecedence = ExpressionQualifier.GetPrecedence(expression);
            // first convert operands
            var operands = expression.GetOperands();
            var literalOperands = new List<string>();
            foreach (var operand in operands)
            {
                var operandPrecedence = ExpressionQualifier.GetPrecedence(operand);
                string literalOperand = BuildExpression(operand, queryContext);
                if (operandPrecedence > currentPrecedence)
                    literalOperand = sqlProvider.GetParenthesis(literalOperand);
                literalOperands.Add(literalOperand);
            }

            // then converts expression
            if (expression is SpecialExpression)
                return sqlProvider.GetLiteral(((SpecialExpression)expression).SpecialNodeType, literalOperands);
            if (expression is TableExpression)
            {
                var tableExpression = (TableExpression)expression;
                if (tableExpression.Alias != null) // if we have an alias, use it
                {
                    return sqlProvider.GetColumn(sqlProvider.GetTableAlias(tableExpression.Alias),
                                                 sqlProvider.GetColumns());
                }
                return sqlProvider.GetColumns();
            }
            if (expression is ColumnExpression)
            {
                var columnExpression = (ColumnExpression)expression;
                if (columnExpression.Table.Alias != null)
                {
                    return sqlProvider.GetColumn(sqlProvider.GetTableAlias(columnExpression.Table.Alias),
                                                 columnExpression.Name);
                }
                return sqlProvider.GetColumn(columnExpression.Name);
            }
            if (expression is InputParameterExpression)
                return sqlProvider.GetParameterName(((InputParameterExpression)expression).Alias);
            if (expression is SelectExpression)
                return Build((SelectExpression)expression, queryContext);
            if (expression is ConstantExpression)
                return sqlProvider.GetLiteral(((ConstantExpression)expression).Value);
            if (expression is GroupExpression)
                return BuildExpression(((GroupExpression)expression).GroupedExpression, queryContext);
            if (expression.NodeType == ExpressionType.Convert || expression.NodeType == ExpressionType.ConvertChecked)
            {
                var unaryExpression = (UnaryExpression)expression;
                var firstOperand = literalOperands.First();
                if (IsConversionRequired(unaryExpression))
                    return sqlProvider.GetLiteralConvert(firstOperand, unaryExpression.Type);
                return firstOperand;
            }
            return sqlProvider.GetLiteral(expression.NodeType, literalOperands);
        }

        /// <summary>
        /// Determines if a SQL conversion is required
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        private bool IsConversionRequired(UnaryExpression expression)
        {
            // obvious (and probably never happens), conversion to the same type
            if (expression.Type == expression.Operand.Type)
                return false;
            // second, nullable to non-nullable for the same type
            if (expression.Type.IsNullable() && !expression.Operand.Type.IsNullable())
            {
                if (expression.Type.GetNullableType() == expression.Operand.Type)
                    return false;
            }
            // third, non-nullable to nullable
            if (!expression.Type.IsNullable() && expression.Operand.Type.IsNullable())
            {
                if (expression.Type == expression.Operand.Type.GetNullableType())
                    return false;
            }
            // found no excuse not to convert? then convert
            return true;
        }

        protected virtual bool MustDeclareAsJoin(IList<TableExpression> tables, TableExpression table)
        {
            return false; // for now, outer joins are disabled, until the work is done

            // the first table can not be declared as join
            if (table == tables[0])
                return false;
            // we must declare as join, whatever the join is,
            // if some of the registered tables are registered as complex join
            if (tables.Any(t => t.JoinType != TableJoinType.Inner))
                return table.JoinExpression != null;
            return false;
        }

        protected virtual string BuildFrom(IList<TableExpression> tables, QueryContext queryContext)
        {
            var sqlProvider = queryContext.DataContext.Vendor.SqlProvider;
            var fromClauses = new List<string>();
            foreach (var tableExpression in tables)
            {
                if (!MustDeclareAsJoin(tables, tableExpression))
                {
                    if (tableExpression.Alias != null)
                    {
                        var tableAlias = sqlProvider.GetTableAsAlias(tableExpression.Name, tableExpression.Alias);
                        if ((tableExpression.JoinType & TableJoinType.LeftOuter) != 0)
                            tableAlias = "/* LEFT OUTER */ " + tableAlias;
                        if ((tableExpression.JoinType & TableJoinType.RightOuter) != 0)
                            tableAlias = "/* RIGHT OUTER */ " + tableAlias;
                        fromClauses.Add(tableAlias);
                    }
                    else
                    {
                        fromClauses.Add(sqlProvider.GetTable(tableExpression.Name));
                    }
                }
            }
            return sqlProvider.GetFromClause(fromClauses.ToArray());
        }

        /// <summary>
        /// Builds join clauses
        /// </summary>
        /// <param name="tables"></param>
        /// <param name="queryContext"></param>
        /// <returns></returns>
        protected virtual string BuildJoin(IList<TableExpression> tables, QueryContext queryContext)
        {
            var sqlProvider = queryContext.DataContext.Vendor.SqlProvider;
            var joinClauses = new List<string>();
            foreach (var tableExpression in tables)
            {
                // this is the pending declaration of direct tables
                if (MustDeclareAsJoin(tables, tableExpression))
                {
                    // get constitutive parts
                    var joinExpression = BuildExpression(tableExpression.JoinExpression, queryContext);
                    var tableAlias = sqlProvider.GetTableAsAlias(tableExpression.Name, tableExpression.Alias);
                    string joinClause;
                    switch (tableExpression.JoinType)
                    {
                    case TableJoinType.Inner:
                        joinClause = sqlProvider.GetInnerJoinClause(tableAlias, joinExpression);
                        break;
                    case TableJoinType.LeftOuter:
                        joinClause = sqlProvider.GetLeftOuterJoinClause(tableAlias, joinExpression);
                        break;
                    case TableJoinType.RightOuter:
                        joinClause = sqlProvider.GetRightOuterJoinClause(tableAlias, joinExpression);
                        break;
                    case TableJoinType.FullOuter:
                        throw new NotImplementedException();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                    }
                    joinClauses.Add(joinClause);
                }
            }
            return sqlProvider.GetJoinClauses(joinClauses.ToArray());
        }

        protected virtual bool IsHavingClause(Expression expression)
        {
            bool isHaving = false;
            expression.Recurse(delegate(Expression e)
                                   {
                                       if (e is GroupExpression)
                                           isHaving = true;
                                       return e;
                                   });
            return isHaving;
        }

        protected virtual string BuildWhere(IList<TableExpression> tables, IList<Expression> wheres, QueryContext queryContext)
        {
            var sqlProvider = queryContext.DataContext.Vendor.SqlProvider;
            var whereClauses = new List<string>();
            foreach (var tableExpression in tables)
            {
                if (!MustDeclareAsJoin(tables, tableExpression) && tableExpression.JoinExpression != null)
                    whereClauses.Add(BuildExpression(tableExpression.JoinExpression, queryContext));
            }
            foreach (var whereExpression in wheres)
            {
                if (!IsHavingClause(whereExpression))
                    whereClauses.Add(BuildExpression(whereExpression, queryContext));
            }
            return sqlProvider.GetWhereClause(whereClauses.ToArray());
        }

        protected virtual string BuildHaving(IList<Expression> wheres, QueryContext queryContext)
        {
            var sqlProvider = queryContext.DataContext.Vendor.SqlProvider;
            var havingClauses = new List<string>();
            foreach (var whereExpression in wheres)
            {
                if (IsHavingClause(whereExpression))
                    havingClauses.Add(BuildExpression(whereExpression, queryContext));
            }
            return sqlProvider.GetHavingClause(havingClauses.ToArray());
        }

        protected virtual string GetGroupByClause(ColumnExpression columnExpression, QueryContext queryContext)
        {
            var sqlProvider = queryContext.DataContext.Vendor.SqlProvider;
            if (columnExpression.Table.Alias != null)
            {
                return sqlProvider.GetColumn(sqlProvider.GetTableAlias(columnExpression.Table.Alias),
                                             columnExpression.Name);
            }
            return sqlProvider.GetColumn(columnExpression.Name);
        }

        protected virtual string BuildGroupBy(IList<GroupExpression> groupByExpressions, QueryContext queryContext)
        {
            var sqlProvider = queryContext.DataContext.Vendor.SqlProvider;
            var groupByClauses = new List<string>();
            foreach (var groupByExpression in groupByExpressions)
            {
                foreach (var operand in groupByExpression.Clauses)
                {
                    var columnOperand = operand as ColumnExpression;
                    if (columnOperand == null)
                        throw Error.BadArgument("S0201: Groupby argument must be a ColumnExpression");
                    groupByClauses.Add(GetGroupByClause(columnOperand, queryContext));
                }
            }
            return sqlProvider.GetGroupByClause(groupByClauses.ToArray());
        }

        protected virtual string BuildOrderBy(IList<OrderByExpression> orderByExpressions, QueryContext queryContext)
        {
            var sqlProvider = queryContext.DataContext.Vendor.SqlProvider;
            var orderByClauses = new List<string>();
            foreach (var clause in orderByExpressions)
            {
                orderByClauses.Add(sqlProvider.GetOrderByColumn(BuildExpression(clause.ColumnExpression, queryContext),
                                                                clause.Descending));
            }
            return sqlProvider.GetOrderByClause(orderByClauses.ToArray());
        }

        protected virtual string BuildSelect(Expression select, QueryContext queryContext)
        {
            var sqlProvider = queryContext.DataContext.Vendor.SqlProvider;
            var selectClauses = new List<string>();
            foreach (var selectExpression in select.GetOperands())
            {
                string expressionString = BuildExpression(selectExpression, queryContext);
                if (selectExpression is SelectExpression)
                    selectClauses.Add(sqlProvider.GetParenthesis(expressionString));
                else
                    selectClauses.Add(expressionString);
            }
            return sqlProvider.GetSelectClause(selectClauses.ToArray());
        }

        protected virtual string BuildLimit(SelectExpression select, string literalSelect, QueryContext queryContext)
        {
            if (select.Limit != null)
            {
                var literalLimit = BuildExpression(select.Limit, queryContext);
                if (select.Offset != null)
                {
                    var literalOffset = BuildExpression(select.Offset, queryContext);
                    var literalOffsetAndLimit = BuildExpression(select.OffsetAndLimit, queryContext);
                    return queryContext.DataContext.Vendor.SqlProvider.GetLiteralLimit(literalSelect, literalLimit,
                                                                                       literalOffset,
                                                                                       literalOffsetAndLimit);
                }
                return queryContext.DataContext.Vendor.SqlProvider.GetLiteralLimit(literalSelect, literalLimit);
            }
            return literalSelect;
        }
    }
}
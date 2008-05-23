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

using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using DbLinq.Factory;
using DbLinq.Linq.Data.Sugar.ExpressionMutator;
using DbLinq.Linq.Data.Sugar.Expressions;

namespace DbLinq.Linq.Data.Sugar.Implementation
{
    public class SqlBuilder
    {
        public ExpressionPrecedence ExpressionPrecedence { get; set; }

        public SqlBuilder()
        {
            ExpressionPrecedence = ObjectFactory.Get<ExpressionPrecedence>();
        }

        public string Build(ExpressionQuery expressionQuery, QueryContext queryContext)
        {
            return Build(expressionQuery.Select, queryContext);
        }

        public string Build(ScopeExpression scopeExpression, QueryContext queryContext)
        {
            // A scope usually has:
            // - a SELECT: TODO
            // - a FROM: list of tables
            // - a WHERE: list of conditions
            // TODO: all other clauses
            string from = BuildFrom(scopeExpression.Tables, queryContext);
            string where = BuildWhere(scopeExpression.Tables, scopeExpression.Where, queryContext);
            string select = "";
            return string.Format("{0}{3}{1}{3}{2}", select, from, where,
                                queryContext.DataContext.Vendor.SqlProvider.NewLine);
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
            var currentPrecedence = ExpressionPrecedence.Get(expression);
            // first convert operands
            var operands = expression.GetOperands();
            var literalOperands = new List<string>();
            foreach (var operand in operands)
            {
                var operandPrecedence = ExpressionPrecedence.Get(operand);
                string literalOperand = BuildExpression(operand, queryContext);
                if (operandPrecedence > currentPrecedence)
                    literalOperand = sqlProvider.GetParenthesis(literalOperand);
                literalOperands.Add(literalOperand);
            }

            // then converts expression
            if (expression is SpecialExpression)
                return sqlProvider.GetLiteral(((SpecialExpression)expression).SpecialNodeType, literalOperands);
            if (expression is TableExpression)
                return sqlProvider.GetColumns(((TableExpression)expression).Alias);
            if (expression is ColumnExpression)
            {
                var columnExpression = (ColumnExpression)expression;
                return sqlProvider.GetColumn(sqlProvider.GetTableAlias(columnExpression.Table.Alias), columnExpression.Name);
            }
            if (expression is ExternalParameterExpression)
                return sqlProvider.GetParameterName(((ExternalParameterExpression)expression).Alias);
            if (expression is ScopeExpression)
                return Build((ScopeExpression)expression, queryContext);
            if (expression is ConstantExpression)
                return sqlProvider.GetLiteral(((ConstantExpression)expression).Value);
            return sqlProvider.GetLiteral(expression.NodeType, literalOperands);
        }

        protected virtual bool MustDeclareAsJoin(TableExpression table)
        {
            return false;
        }

        protected virtual string BuildFrom(IList<TableExpression> tables, QueryContext queryContext)
        {
            // TODO: as of today, all joins (since we generate only inner) are in the from and where clauses
            //       we may consider switching to explicit syntax, join <t> on <e>
            var sqlProvider = queryContext.DataContext.Vendor.SqlProvider;
            var fromClauses = new List<string>();
            foreach (var tableExpression in tables)
            {
                if (!MustDeclareAsJoin(tableExpression))
                {
                    fromClauses.Add(sqlProvider.GetTableAsAlias(sqlProvider.GetTable(tableExpression.Name),
                                                                tableExpression.Alias));
                }
            }
            return sqlProvider.GetFromClause(fromClauses.ToArray());
        }

        protected virtual string BuildWhere(IList<TableExpression> tables, IList<Expression> wheres, QueryContext queryContext)
        {
            var sqlProvider = queryContext.DataContext.Vendor.SqlProvider;
            var whereClauses = new List<string>();
            foreach (var tableExpression in tables)
            {
                if (!MustDeclareAsJoin(tableExpression) && tableExpression.JoinExpression != null)
                    whereClauses.Add(BuildExpression(tableExpression.JoinExpression, queryContext));
            }
            foreach (var whereExpression in wheres)
            {
                whereClauses.Add(BuildExpression(whereExpression, queryContext));
            }
            return sqlProvider.GetWhereClause(whereClauses.ToArray());
        }
    }
}

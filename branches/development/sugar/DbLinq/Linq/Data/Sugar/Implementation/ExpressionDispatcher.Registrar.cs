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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DbLinq.Linq.Data.Sugar.Expressions;
using DbLinq.Util;

namespace DbLinq.Linq.Data.Sugar.Implementation
{
    public partial class ExpressionDispatcher
    {

        /// <summary>
        /// Returns a registered column, or null if not found
        /// This method requires the table to be already registered
        /// </summary>
        /// <param name="table"></param>
        /// <param name="name"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual ColumnExpression GetRegisteredColumn(TableExpression table, string name,
                                               BuilderContext builderContext)
        {
            return
                (from queryColumn in builderContext.EnumerateScopeColumns()
                 where queryColumn.Table.IsEqualTo(table) && queryColumn.Name == name
                 select queryColumn).SingleOrDefault();
        }

        /// <summary>
        /// Returns an existing table or registers the current one
        /// </summary>
        /// <param name="tableExpression"></param>
        /// <param name="builderContext"></param>
        /// <returns>A registered table or the current newly registered one</returns>
        public virtual TableExpression RegisterTable(TableExpression tableExpression, BuilderContext builderContext)
        {
            // 1. Find the table in current scope
            var foundTableExpression = (from t in builderContext.EnumerateScopeTables()
                                        where t.IsEqualTo(tableExpression)
                                        select t).SingleOrDefault();
            if (foundTableExpression != null)
                return foundTableExpression;
            // 2. Find it in all scopes, and promote it to current scope.
            foundTableExpression = PromoteTable(tableExpression, builderContext);
            if (foundTableExpression != null)
                return foundTableExpression;
            // 3. Add it
            builderContext.CurrentScope.Tables.Add(tableExpression);
            return tableExpression;
        }

        /// <summary>
        /// Promotes a table to a common parent between its current scope and our current scope
        /// </summary>
        /// <param name="tableExpression"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual TableExpression PromoteTable(TableExpression tableExpression, BuilderContext builderContext)
        {
            // 1. Find the table ScopeExpression
            ScopeExpression oldScope = FindTableScope(ref tableExpression, builderContext);
            if (oldScope == null)
                return null;
            // 2. Find a common ScopeExpression
            var commonScope = FindCommonScope(oldScope, builderContext.CurrentScope);
            commonScope.Tables.Add(tableExpression);
            return tableExpression;
        }

        protected virtual ScopeExpression FindTableScope(ref TableExpression tableExpression, BuilderContext builderContext)
        {
            foreach (var scope in builderContext.ScopeExpressions)
            {
                for (int tableIndex = 0; tableIndex < scope.Tables.Count; tableIndex++)
                {
                    if (scope.Tables[tableIndex].IsEqualTo(tableExpression))
                    {
                        tableExpression = scope.Tables[tableIndex];
                        scope.Tables.RemoveAt(tableIndex);
                        return scope;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Find the common ancestor between two ScopeExpressions
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        protected virtual ScopeExpression FindCommonScope(ScopeExpression a, ScopeExpression b)
        {
            for (var aScope = a; aScope != null; aScope = aScope.Parent)
            {
                for (var bScope = b; bScope != null; bScope = bScope.Parent)
                {
                    if (aScope == bScope)
                        return aScope;
                }
            }
            throw Error.BadArgument("S0127: No common ScopeExpression found");
        }

        /// <summary>
        /// Registers a column
        /// This method requires the table to be already registered
        /// </summary>
        /// <param name="table"></param>
        /// <param name="memberInfo"></param>
        /// <param name="name"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        public ColumnExpression RegisterColumn(TableExpression table,
                                          MemberInfo memberInfo, string name,
                                          BuilderContext builderContext)
        {
            if (memberInfo == null)
                return null;
            var queryColumn = GetRegisteredColumn(table, name, builderContext);
            if (queryColumn == null)
            {
                table = RegisterTable(table, builderContext);
                queryColumn = new ColumnExpression(table, name, memberInfo);
                builderContext.CurrentScope.Columns.Add(queryColumn);
            }
            return queryColumn;
        }

        /// <summary>
        /// Registers a column with only a table and a MemberInfo (this is the preferred method overload)
        /// </summary>
        /// <param name="tableExpression"></param>
        /// <param name="memberInfo"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        public ColumnExpression RegisterColumn(TableExpression tableExpression, MemberInfo memberInfo,
                                          BuilderContext builderContext)
        {
            var dataMember = builderContext.QueryContext.DataContext.Mapping.GetTable(tableExpression.Type).RowType
                .GetDataMember(memberInfo);
            if (dataMember == null)
                return null;
            return RegisterColumn(tableExpression, memberInfo, dataMember.MappedName, builderContext);
        }

        public ColumnExpression CreateColumn(TableExpression table, MemberInfo memberInfo, BuilderContext builderContext)
        {
            var dataMember = builderContext.QueryContext.DataContext.Mapping.GetTable(table.Type).RowType
                .GetDataMember(memberInfo);
            if (dataMember == null)
                return null;
            return new ColumnExpression(table, dataMember.MappedName, memberInfo);
        }

        /// <summary>
        /// Creates a default TableExpression
        /// </summary>
        /// <param name="tableType"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        public virtual TableExpression CreateTable(Type tableType, BuilderContext builderContext)
        {
            return new TableExpression(tableType, DataMapper.GetTableName(tableType, builderContext.QueryContext.DataContext));
        }

        /// <summary>
        /// Registers an association
        /// </summary>
        /// <param name="joinedTableExpression">The table holding the member, to become the joinedTable</param>
        /// <param name="joinMemberInfo"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        public virtual TableExpression RegisterAssociation(TableExpression joinedTableExpression, MemberInfo joinMemberInfo,
                                                      BuilderContext builderContext)
        {
            IList<MemberInfo> foreignKeys, joinedKeys;
            TableJoinType joinType;
            string joinID;
            var tableType = DataMapper.GetAssociation(joinedTableExpression, joinMemberInfo, out foreignKeys, out joinedKeys, out joinType,
                                                      out joinID, builderContext.QueryContext.DataContext);
            // if the memberInfo has no corresponding association, we get a null, that we propagate
            if (tableType == null)
                return null;

            // the current table has the foreign key, the other table the referenced (usually primary) key
            if (foreignKeys.Count != joinedKeys.Count)
                throw Error.BadArgument("S0128: Association arguments (FK and ref'd PK) don't match");

            // we first create the table
            var tableExpression = new TableExpression(tableType, DataMapper.GetTableName(tableType, builderContext.QueryContext.DataContext));

            Expression joinExpression = null;
            var createdColumns = new List<ColumnExpression>();
            for (int keyIndex = 0; keyIndex < foreignKeys.Count; keyIndex++)
            {
                // joinedKey is registered, even if unused by final select (required columns will be filtered anyway)
                Expression joinedKey = RegisterColumn(joinedTableExpression, joinedKeys[keyIndex], builderContext);
                // foreign is created, we will store it later if this assocation is registered too
                var foreignKey = CreateColumn(tableExpression, foreignKeys[keyIndex], builderContext);
                createdColumns.Add(foreignKey);

                // if the key is nullable, then convert it
                // TODO: this will probably need to be changed
                if (joinedKey.Type.IsNullable())
                    joinedKey = Expression.Convert(joinedKey, joinedKey.Type.GetNullableType());
                var referenceExpression = Expression.Equal(foreignKey, joinedKey);
                // if we already have a join expression, then we have a double condition here, so "AND" it
                if (joinExpression != null)
                    joinExpression = Expression.AndAlso(joinExpression, referenceExpression);
                else
                    joinExpression = referenceExpression;
            }
            tableExpression.Join(joinType, joinedTableExpression, joinExpression, joinID);

            // our table is created, with the expressions
            // now check if we didn't register exactly the same
            if ((from t in builderContext.EnumerateScopeTables() where t.IsEqualTo(tableExpression) select t).SingleOrDefault() == null)
            {
                builderContext.CurrentScope.Tables.Add(tableExpression);
                foreach (var createdColumn in createdColumns)
                    builderContext.CurrentScope.Columns.Add(createdColumn);
            }

            return tableExpression;
        }

        /// <summary>
        /// Registers an external parameter
        /// Since these can be complex expressions, we don't try to identify them
        /// and push them every time
        /// The only loss may be a small memory loss (if anyone can prove me that the same Expression can be used twice)
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="alias"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        public virtual ExternalParameterExpression RegisterParameter(Expression expression, string alias, BuilderContext builderContext)
        {
            var queryParameterExpression = new ExternalParameterExpression(expression, alias);
            builderContext.ExpressionQuery.Parameters.Add(queryParameterExpression);
            return queryParameterExpression;
        }

        /// <summary>
        /// Registers a MetaTable
        /// </summary>
        /// <param name="metaTableType"></param>
        /// <param name="aliases"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        public virtual MetaTableExpression RegisterMetaTable(Type metaTableType, IDictionary<MemberInfo, TableExpression> aliases,
            BuilderContext builderContext)
        {
            MetaTableExpression metaTableExpression;
            if (!builderContext.MetaTables.TryGetValue(metaTableType, out metaTableExpression))
            {
                metaTableExpression = new MetaTableExpression(aliases);
                builderContext.MetaTables[metaTableType] = metaTableExpression;
            }
            return metaTableExpression;
        }

        /// <summary>
        /// Registers a where clause in the current context scope
        /// </summary>
        /// <param name="whereExpression"></param>
        /// <param name="builderContext"></param>
        public virtual void RegisterWhere(Expression whereExpression, BuilderContext builderContext)
        {
            if(whereExpression==null)
            {
                int e = 0;
            }
            builderContext.CurrentScope.Where.Add(whereExpression);
        }

        /// <summary>
        /// Registers all columns of a table.
        /// </summary>
        /// <param name="tableExpression"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        public virtual IEnumerable<ColumnExpression> RegisterAllColumns(TableExpression tableExpression, BuilderContext builderContext)
        {
            foreach (var metaMember in builderContext.QueryContext.DataContext.Mapping.GetTable(tableExpression.Type).RowType.PersistentDataMembers)
            {
                yield return RegisterColumn(tableExpression, metaMember.Member, builderContext);
            }
        }

        /// <summary>
        /// Registers an expression to be returned by main request.
        /// The strategy is to try to find it in the already registered parameters, and if not found, add it
        /// </summary>
        /// <param name="expression">The expression to be registered</param>
        /// <param name="builderContext"></param>
        /// <returns>Expression index</returns>
        public virtual int RegisterSelectOperand(Expression expression, BuilderContext builderContext)
        {
            var scope = builderContext.CurrentScope;
            var operands = scope.Operands.ToList();
            for (int index = 0; index < operands.Count; index++)
            {
                if (ExpressionEquals(operands[index], expression))
                    return index;
            }
            operands.Add(expression);
            builderContext.CurrentScope = (ScopeExpression)scope.Mutate(operands);
            return operands.Count - 1;
        }

        protected virtual bool ExpressionEquals(Expression a, Expression b)
        {
            // TODO: something smarter, to compare contents and not only references (works fine only for columns)
            return a == b;
        }

        protected virtual Expression RegisterParameterTable(TableExpression tableExpression, ParameterExpression dataRecordParameter, ParameterExpression mappingContextParameter, BuilderContext builderContext)
        {
            var bindings = new List<MemberBinding>();
            foreach (var columnExpression in RegisterAllColumns(tableExpression, builderContext))
            {
                var parameterColumn = RegisterParameterColumnInvoke(columnExpression,
                                                    dataRecordParameter, mappingContextParameter, builderContext);
                var binding = Expression.Bind(columnExpression.MemberInfo, parameterColumn);
                bindings.Add(binding);
            }
            var newExpression = Expression.New(tableExpression.Type);
            var initExpression = Expression.MemberInit(newExpression, bindings);
            return initExpression;
        }

        protected virtual Expression RegisterParameterColumnInvoke(Expression expression,
            ParameterExpression dataRecordParameter, ParameterExpression mappingContextParameter,
            BuilderContext builderContext)
        {
            int valueIndex = RegisterSelectOperand(expression, builderContext);
            var propertyReaderLambda = DataRecordReader.GetPropertyReader(expression.Type);
            Expression invoke = Expression.Invoke(propertyReaderLambda, dataRecordParameter,
                                                  mappingContextParameter, Expression.Constant(valueIndex));
            if (!expression.Type.IsNullable())
                invoke = Expression.Convert(invoke, expression.Type);
            return invoke;
        }
    }
}

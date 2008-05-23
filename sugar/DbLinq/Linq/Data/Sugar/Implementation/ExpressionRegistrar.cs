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
using DbLinq.Factory;
using DbLinq.Linq.Data.Sugar.Expressions;
using DbLinq.Util;

namespace DbLinq.Linq.Data.Sugar.Implementation
{
    public class ExpressionRegistrar : IExpressionRegistrar
    {
        public IDataMapper DataMapper { get; set; }

        public ExpressionRegistrar()
        {
            DataMapper = ObjectFactory.Get<IDataMapper>();
        }

        /// <summary>
        /// Returns a registered column, or null if not found
        /// This method requires the table to be already registered
        /// </summary>
        /// <param name="table"></param>
        /// <param name="name"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        public ColumnExpression GetRegisteredColumn(TableExpression table, string name,
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
        protected virtual TableExpression RegisterTable(TableExpression tableExpression, BuilderContext builderContext)
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

        protected virtual ScopeExpression FindCommonScope(ScopeExpression a, ScopeExpression b)
        {
            for (var aScope = a; aScope != null; aScope = aScope.ParentScopePiece)
            {
                for (var bScope = b; bScope != null; bScope = bScope.ParentScopePiece)
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
                queryColumn = new ColumnExpression(table, name, memberInfo.GetMemberType());
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
            return new ColumnExpression(table, dataMember.MappedName, memberInfo.GetMemberType());
        }

        // TODO: check and remove
        /// <summary>
        /// Find a registered table in the current query, or null if none
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        public virtual TableExpression GetRegisteredTable(string tableName, BuilderContext builderContext)
        {
            return
                (from queryTable in builderContext.EnumerateScopeTables()
                 where queryTable.Name == tableName
                 select queryTable).SingleOrDefault();
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
                var joinedKey = RegisterColumn(joinedTableExpression, joinedKeys[keyIndex], builderContext);
                // foreign is created, we will store it later if this assocation is registered too
                var foreignKey = CreateColumn(tableExpression, foreignKeys[keyIndex], builderContext);
                createdColumns.Add(foreignKey);

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
        /// Returns a registered MetaTable, by Type
        /// </summary>
        /// <param name="metaTableType"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        public virtual MetaTableExpression GetRegisteredMetaTable(Type metaTableType, BuilderContext builderContext)
        {
            MetaTableExpression metaTableExpression;
            builderContext.MetaTables.TryGetValue(metaTableType, out metaTableExpression);
            return metaTableExpression;
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
            builderContext.CurrentScope.Where.Add(whereExpression);
        }
    }
}

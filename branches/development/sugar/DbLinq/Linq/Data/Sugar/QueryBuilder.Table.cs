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

namespace DbLinq.Linq.Data.Sugar
{
    partial class QueryBuilder
    {
        /// <summary>
        /// Find a registered table in the current query, or null if none
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual QueryTableExpression GetRegisteredTable(string tableName, BuilderContext builderContext)
        {
            return
                (from queryTable in builderContext.ExpressionQuery.Tables
                 where queryTable.Name == tableName
                 select queryTable).SingleOrDefault();
        }

        /// <summary>
        /// Registers a table by its name, and returns the registered table
        /// If the tableName parameter is null, returns null
        /// </summary>
        /// <param name="tableType"></param>
        /// <param name="tableName"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual QueryTableExpression RegisterTable(Type tableType, string tableName, QueryTableExpression.JoinType joinType,
                                                            BuilderContext builderContext)
        {
            if (tableName == null)
                return null;
            // Is it possible to have a table twice in a request for different reasons?
            var queryTable = GetRegisteredTable(tableName, builderContext);
            if (queryTable == null)
            {
                queryTable = new QueryTableExpression(tableType, tableName, joinType);
                builderContext.ExpressionQuery.Tables.Add(queryTable);
            }
            return queryTable;
        }

        protected virtual QueryTableExpression RegisterTable(Type tableType, string tableName, BuilderContext builderContext)
        {
            return RegisterTable(tableType, tableName, QueryTableExpression.JoinType.Default, builderContext);
        }

        /// <summary>
        /// Registers a table by its type, or the current registered table.
        /// If the tableType is not a table type, then returns null
        /// </summary>
        /// <param name="tableType"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual QueryTableExpression RegisterTable(Type tableType, BuilderContext builderContext)
        {
            return RegisterTable(tableType, GetTableName(tableType, builderContext.QueryContext.DataContext), builderContext);
        }

        /// <summary>
        /// Registers an association
        /// </summary>
        /// <param name="tableExpression"></param>
        /// <param name="memberInfo"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual QueryTableExpression RegisterAssociation(QueryTableExpression tableExpression, MemberInfo memberInfo,
            BuilderContext builderContext)
        {
            IList<MemberInfo> foreignKeys, referencedKeys;
            QueryTableExpression.JoinType joinType;
            var referencedTableType = GetAssociation(tableExpression, memberInfo, out foreignKeys, out referencedKeys, out joinType,
                builderContext.QueryContext.DataContext);
            // if the memberInfo has no corresponding association, we get a null, that we propagate
            if (referencedTableType == null)
                return null;

            // the current table has the foreign key, the other table the referenced (usually primary) key
            if (foreignKeys.Count != referencedKeys.Count)
                throw BadArgument("S0128: Association arguments (FK and ref'd PK) don't match");

            var referencedTableExpression = RegisterTable(referencedTableType,
                GetTableName(referencedTableType, builderContext.QueryContext.DataContext),
                joinType, builderContext);

            QueryExpression joinExpression = null;
            for (int referenceIndex = 0; referenceIndex < foreignKeys.Count; referenceIndex++)
            {
                var foreignKey = RegisterColumn(tableExpression,
                                            foreignKeys[referenceIndex],
                                            builderContext);
                var referencedKey = RegisterColumn(referencedTableExpression,
                                            referencedKeys[referenceIndex],
                                            builderContext);
                var referenceExpression = new QueryOperationExpression(ExpressionType.Equal, foreignKey, referencedKey);
                // if we already have a join expression, then we have a double condition here, so "AND" it
                if (joinExpression != null)
                {
                    joinExpression = new QueryOperationExpression(ExpressionType.AndAlso,
                        joinExpression,
                        referenceExpression);
                }
                else
                    joinExpression = referenceExpression;
            }

            builderContext.ExpressionQuery.Associations[referencedTableExpression] = joinExpression;

            return referencedTableExpression;
        }
    }
}

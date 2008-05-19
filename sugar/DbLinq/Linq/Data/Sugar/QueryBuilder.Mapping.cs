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
using System.Data.Linq.Mapping;
using System.Linq;
using System.Reflection;
using DbLinq.Linq.Data.Sugar.Expressions;

namespace DbLinq.Linq.Data.Sugar
{
    partial class QueryBuilder
    {
        /// <summary>
        /// Returns a table given a type, or null if the type is not mapped
        /// </summary>
        /// <param name="tableType"></param>
        /// <param name="dataContext"></param>
        /// <returns></returns>
        protected virtual string GetTableName(Type tableType, DataContext dataContext)
        {
            var tableDescription = dataContext.Mapping.GetTable(tableType);
            if (tableDescription != null)
                return tableDescription.TableName;
            return null;
        }

        protected virtual string GetColumnName(QueryTableExpression tableExpression, MemberInfo memberInfo, DataContext dataContext)
        {
            var tableDescription = dataContext.Mapping.GetTable(tableExpression.Type);
            var columnDescription = tableDescription.RowType.GetDataMember(memberInfo);
            if (columnDescription != null)
                return columnDescription.MappedName;
            return null;
        }

        protected virtual IList<MemberInfo> GetPrimaryKeys(QueryTableExpression tableExpression, DataContext dataContext)
        {
            var tableDescription = dataContext.Mapping.GetTable(tableExpression.Type);
            if (tableDescription != null)
                return GetPrimaryKeys(tableDescription);
            return null;
        }

        protected virtual IList<MemberInfo> GetPrimaryKeys(MetaTable tableDescription)
        {
            return
                (from column in tableDescription.RowType.PersistentDataMembers
                 where column.IsPrimaryKey
                 select column.Member).ToList();
        }

        protected virtual Type GetAssociation(QueryTableExpression tableExpression, MemberInfo memberInfo,
            out IList<MemberInfo> foreignKey, out IList<MemberInfo> referencedKey, out QueryTableExpression.JoinType joinType,
            DataContext dataContext)
        {
            var tableDescription = dataContext.Mapping.GetTable(tableExpression.Type);
            var columnDescription = tableDescription.RowType.GetDataMember(memberInfo);
            var associationDescription =
                (from association in tableDescription.RowType.Associations
                 where association.ThisMember == columnDescription
                 select association).SingleOrDefault();
            if (associationDescription != null)
            {
                var referencedTableType = associationDescription.OtherType.Type;
                // TODO: something if ThisKey is null
                foreignKey = (from key in associationDescription.ThisKey select key.Member).ToList();
                if (associationDescription.OtherKey.Count == 0)
                    referencedKey = GetPrimaryKeys(associationDescription.OtherType.Table);
                else
                    referencedKey = (from key in associationDescription.OtherKey select key.Member).ToList();
                joinType = QueryTableExpression.JoinType.Inner;
                return referencedTableType;
            }
            foreignKey = null;
            referencedKey = null;
            joinType = QueryTableExpression.JoinType.Default;
            return null;
        }
    }
}

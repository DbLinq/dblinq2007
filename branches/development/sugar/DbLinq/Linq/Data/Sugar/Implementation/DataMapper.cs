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

namespace DbLinq.Linq.Data.Sugar.Implementation
{
    public class DataMapper : IDataMapper
    {
        /// <summary>
        /// Returns a table given a type, or null if the type is not mapped
        /// </summary>
        /// <param name="tableType"></param>
        /// <param name="dataContext"></param>
        /// <returns></returns>
        public virtual string GetTableName(Type tableType, DataContext dataContext)
        {
            var tableDescription = dataContext.Mapping.GetTable(tableType);
            if (tableDescription != null)
                return tableDescription.TableName;
            return null;
        }

        public virtual string GetColumnName(TableExpression tableExpression, MemberInfo memberInfo, DataContext dataContext)
        {
            var tableDescription = dataContext.Mapping.GetTable(tableExpression.Type);
            var columnDescription = tableDescription.RowType.GetDataMember(memberInfo);
            if (columnDescription != null)
                return columnDescription.MappedName;
            return null;
        }

        public virtual IList<MemberInfo> GetPrimaryKeys(TableExpression tableExpression, DataContext dataContext)
        {
            var tableDescription = dataContext.Mapping.GetTable(tableExpression.Type);
            if (tableDescription != null)
                return GetPrimaryKeys(tableDescription);
            return null;
        }

        public virtual IList<MemberInfo> GetPrimaryKeys(MetaTable tableDescription)
        {
            return
                (from column in tableDescription.RowType.PersistentDataMembers
                 where column.IsPrimaryKey
                 select column.Member).ToList();
        }

        /// <summary>
        /// Returns association definition, if any
        /// </summary>
        /// <param name="joinedTableExpression">The table referenced by the assocation (the type holding the member)</param>
        /// <param name="memberInfo">The memberInfo related to association</param>
        /// <param name="foreignKey">The keys in the joined table</param>
        /// <param name="joinedKey">The keys in the associated table</param>
        /// <param name="joinType"></param>
        /// <param name="dataContext"></param>
        /// <returns></returns>
        public virtual Type GetAssociation(TableExpression joinedTableExpression, MemberInfo memberInfo,
                                           out IList<MemberInfo> foreignKey, out IList<MemberInfo> joinedKey, out TableJoinType joinType,
                                           DataContext dataContext)
        {
            var joinedTableDescription = dataContext.Mapping.GetTable(joinedTableExpression.Type);
            var associationDescription =
                (from association in joinedTableDescription.RowType.Associations
                 where association.ThisMember.Member == memberInfo
                 select association).SingleOrDefault();
            if (associationDescription != null)
            {
                if (associationDescription.OtherKey.Count == 0)
                    foreignKey = GetPrimaryKeys(associationDescription.OtherType.Table);
                else
                    foreignKey = (from key in associationDescription.OtherKey select key.Member).ToList();
                if (associationDescription.ThisKey.Count == 0)
                    joinedKey = GetPrimaryKeys(joinedTableDescription);
                else
                    joinedKey = (from key in associationDescription.ThisKey select key.Member).ToList();
                var tableType = foreignKey[0].DeclaringType;
                joinType = TableJoinType.Inner;
                return tableType;
            }
            foreignKey = null;
            joinedKey = null;
            joinType = TableJoinType.Default;
            return null;
        }
    }
}

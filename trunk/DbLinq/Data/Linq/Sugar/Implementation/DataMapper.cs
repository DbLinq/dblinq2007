#region MIT license
// 
// MIT license
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
using DbLinq.Data.Linq.Sugar;
using DbLinq.Data.Linq.Sugar.Expressions;

namespace DbLinq.Data.Linq.Sugar.Implementation
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

        protected virtual void GetAssociation(MetaAssociation child, out IList<MemberInfo> foreignKey,
                                              out IList<MemberInfo> referencedKey, DataContext dataContext)
        {
            // the parent type is a Set<Table>, we keep the Table
            var parent = child.OtherMember.Association;

            if (child.ThisKey.Count == 0) // which sounds odd
                foreignKey = GetPrimaryKeys(dataContext.Mapping.GetTable(child.ThisMember.Type));
            else
                foreignKey = (from key in child.ThisKey select key.Member).ToList();

            if (parent.OtherKey.Count == 0)
                referencedKey = GetPrimaryKeys(dataContext.Mapping.GetTable(parent.OtherMember.Type));
            else
                referencedKey = (from key in parent.OtherKey select key.Member).ToList();
        }

        protected virtual bool IsChild(MetaAssociation association)
        {
            return !typeof(IQueryable).IsAssignableFrom(association.ThisMember.Type);
        }

        /// <summary>
        /// Returns association definition, if any
        /// </summary>
        /// <param name="joinedTableExpression">The table referenced by the assocation (the type holding the member)</param>
        /// <param name="memberInfo">The memberInfo related to association</param>
        /// <param name="foreignKey">The keys in the joined table</param>
        /// <param name="joinedKey">The keys in the associated table</param>
        /// <param name="joinType"></param>
        /// <param name="joinID"></param>
        /// <param name="dataContext"></param>
        /// <returns></returns>
        public virtual Type GetAssociation(TableExpression joinedTableExpression, MemberInfo memberInfo,
                                           out IList<MemberInfo> foreignKey, out IList<MemberInfo> joinedKey, out TableJoinType joinType,
                                           out string joinID, DataContext dataContext)
        {
            var joinedTableDescription = dataContext.Mapping.GetTable(joinedTableExpression.Type);
            var joinedAssociation =
                (from association in joinedTableDescription.RowType.Associations
                 where association.ThisMember.Member == memberInfo
                 select association).SingleOrDefault();
            if (joinedAssociation != null)
            {
                joinType = TableJoinType.Inner;
                joinID = joinedAssociation.ThisMember.MappedName;
                if (string.IsNullOrEmpty(joinID))
                    throw Error.BadArgument("S0108: Association name is required to ensure join uniqueness");

                Type referencedType;
                if (IsChild(joinedAssociation))
                {
                    GetAssociation(joinedAssociation, out foreignKey, out joinedKey, dataContext);
                    referencedType = joinedAssociation.ThisMember.Type; // the parent type is the type returned by the member
                }
                else
                {
                    GetAssociation(joinedAssociation.OtherMember.Association, out joinedKey, out foreignKey, dataContext);
                    referencedType = joinedAssociation.ThisMember.Type.GetGenericArguments()[0];
                }

                return referencedType;
            }
            foreignKey = null;
            joinedKey = null;
            joinType = TableJoinType.Default;
            joinID = null;
            return null;
        }
    }
}
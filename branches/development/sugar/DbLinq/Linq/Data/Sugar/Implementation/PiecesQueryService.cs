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
using DbLinq.Linq.Data.Sugar.Implementation;
using DbLinq.Linq.Data.Sugar.Pieces;
using DbLinq.Util;

namespace DbLinq.Linq.Data.Sugar.Implementation
{
    public class PiecesQueryService : IPiecesQueryService
    {
        public IMapper Mapper { get; set; }

        public PiecesQueryService()
        {
            Mapper = ObjectFactory.Get<IMapper>();
        }

        /// <summary>
        /// Returns a registered column, or null if not found
        /// This method requires the table to be already registered
        /// </summary>
        /// <param name="table"></param>
        /// <param name="name"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        public ColumnPiece GetRegisteredColumn(TablePiece table, string name,
                                               BuilderContext builderContext)
        {
            return
                (from queryColumn in builderContext.PiecesQuery.Columns
                 where queryColumn.Table == table && queryColumn.Name == name
                 select queryColumn).SingleOrDefault();
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
        public ColumnPiece RegisterColumn(TablePiece table,
                                          MemberInfo memberInfo, string name,
                                          BuilderContext builderContext)
        {
            if (memberInfo == null)
                return null;
            var queryColumn = GetRegisteredColumn(table, name, builderContext);
            if (queryColumn == null)
            {
                queryColumn = new ColumnPiece(table, name, memberInfo.GetMemberType());
                builderContext.PiecesQuery.Columns.Add(queryColumn);
            }
            return queryColumn;
        }

        /// <summary>
        /// Registers a column with only a table and a MemberInfo (this is the preferred method overload)
        /// </summary>
        /// <param name="table"></param>
        /// <param name="memberInfo"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        public ColumnPiece RegisterColumn(TablePiece table, MemberInfo memberInfo,
                                          BuilderContext builderContext)
        {
            var dataMember = builderContext.QueryContext.DataContext.Mapping.GetTable(table.Type).RowType
                .GetDataMember(memberInfo);
            if (dataMember == null)
                return null;
            return RegisterColumn(table, memberInfo, dataMember.MappedName, builderContext);
        }

        /// <summary>
        /// Find a registered table in the current query, or null if none
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        public virtual TablePiece GetRegisteredTable(string tableName, BuilderContext builderContext)
        {
            return
                (from queryTable in builderContext.PiecesQuery.Tables
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
        public virtual TablePiece RegisterTable(Type tableType, string tableName, TablePiece.JoinType joinType,
                                                BuilderContext builderContext)
        {
            if (tableName == null)
                return null;
            // Is it possible to have a table twice in a request for different reasons?
            var queryTable = GetRegisteredTable(tableName, builderContext);
            if (queryTable == null)
            {
                queryTable = new TablePiece(tableType, tableName, joinType);
                builderContext.PiecesQuery.Tables.Add(queryTable);
                if (builderContext.PiecesQuery.Select != null)
                    throw Error.BadArgument("S0140: builderContext.PiecesQuery.Select should be null here");
                builderContext.PiecesQuery.Select = queryTable;
            }
            return queryTable;
        }

        public virtual TablePiece RegisterTable(Type tableType, string tableName, BuilderContext builderContext)
        {
            return RegisterTable(tableType, tableName, TablePiece.JoinType.Default, builderContext);
        }

        /// <summary>
        /// Registers a table by its type, or the current registered table.
        /// If the tableType is not a table type, then returns null
        /// </summary>
        /// <param name="tableType"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        public virtual TablePiece RegisterTable(Type tableType, BuilderContext builderContext)
        {
            return RegisterTable(tableType, Mapper.GetTableName(tableType, builderContext.QueryContext.DataContext), builderContext);
        }

        /// <summary>
        /// Registers an association
        /// </summary>
        /// <param name="tablePiece"></param>
        /// <param name="memberInfo"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        public virtual TablePiece RegisterAssociation(TablePiece tablePiece, MemberInfo memberInfo,
                                                      BuilderContext builderContext)
        {
            IList<MemberInfo> foreignKeys, referencedKeys;
            TablePiece.JoinType joinType;
            var referencedTableType = Mapper.GetAssociation(tablePiece, memberInfo, out foreignKeys, out referencedKeys, out joinType,
                                                            builderContext.QueryContext.DataContext);
            // if the memberInfo has no corresponding association, we get a null, that we propagate
            if (referencedTableType == null)
                return null;

            // the current table has the foreign key, the other table the referenced (usually primary) key
            if (foreignKeys.Count != referencedKeys.Count)
                throw Error.BadArgument("S0128: Association arguments (FK and ref'd PK) don't match");

            var referencedTableExpression = RegisterTable(referencedTableType,
                                                          Mapper.GetTableName(referencedTableType, builderContext.QueryContext.DataContext),
                                                          joinType, builderContext);

            Piece joinPiece = null;
            for (int referenceIndex = 0; referenceIndex < foreignKeys.Count; referenceIndex++)
            {
                var foreignKey = RegisterColumn(tablePiece,
                                                foreignKeys[referenceIndex],
                                                builderContext);
                var referencedKey = RegisterColumn(referencedTableExpression,
                                                   referencedKeys[referenceIndex],
                                                   builderContext);
                var referenceExpression = new OperationPiece(ExpressionType.Equal, foreignKey, referencedKey);
                // if we already have a join expression, then we have a double condition here, so "AND" it
                if (joinPiece != null)
                {
                    joinPiece = new OperationPiece(ExpressionType.AndAlso,
                                                   joinPiece,
                                                   referenceExpression);
                }
                else
                    joinPiece = referenceExpression;
            }

            builderContext.PiecesQuery.Associations[referencedTableExpression] = joinPiece;

            return referencedTableExpression;
        }

        /// <summary>
        /// Registers an external parameter
        /// Since these can be complex expressions, we don't try to identify them
        /// and push them every time
        /// The only loss may be a small memory loss (if anyone can prove me that the same Expression can be used twice)
        /// </summary>
        /// <param name="piece"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        public virtual ParameterPiece RegisterParameter(Piece piece, BuilderContext builderContext)
        {
            if (!piece.Is<OperationPiece>())
                return null;
            var queryParameterExpression =
                new ParameterPiece(((OperationPiece)piece).OriginalExpression);
            builderContext.PiecesQuery.Parameters.Add(queryParameterExpression);
            return queryParameterExpression;
        }
    }
}
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
using System.Reflection;
using DbLinq.Linq.Data.Sugar.Pieces;

namespace DbLinq.Linq.Data.Sugar
{
    public interface IPiecesQueryService
    {
        /// <summary>
        /// Returns a registered column, or null if not found
        /// This method requires the table to be already registered
        /// </summary>
        /// <param name="table"></param>
        /// <param name="name"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        ColumnPiece GetRegisteredColumn(TablePiece table, string name,
                                                        BuilderContext builderContext);

        /// <summary>
        /// Registers a column
        /// This method requires the table to be already registered
        /// </summary>
        /// <param name="table"></param>
        /// <param name="memberInfo"></param>
        /// <param name="name"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        ColumnPiece RegisterColumn(TablePiece table,
                                                   MemberInfo memberInfo, string name,
                                                   BuilderContext builderContext);

        /// <summary>
        /// Registers a column with only a table and a MemberInfo (this is the preferred method overload)
        /// </summary>
        /// <param name="table"></param>
        /// <param name="memberInfo"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        ColumnPiece RegisterColumn(TablePiece table, MemberInfo memberInfo,
                                                   BuilderContext builderContext);

        /// <summary>
        /// Find a registered table in the current query, or null if none
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        TablePiece GetRegisteredTable(string tableName, BuilderContext builderContext);

        /// <summary>
        /// Registers a table by its name, and returns the registered table
        /// If the tableName parameter is null, returns null
        /// </summary>
        /// <param name="tableType"></param>
        /// <param name="tableName"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        TablePiece RegisterTable(Type tableType, string tableName, TablePiece.JoinType joinType,
                                                 BuilderContext builderContext);

        TablePiece RegisterTable(Type tableType, string tableName, BuilderContext builderContext);

        /// <summary>
        /// Registers a table by its type, or the current registered table.
        /// If the tableType is not a table type, then returns null
        /// </summary>
        /// <param name="tableType"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        TablePiece RegisterTable(Type tableType, BuilderContext builderContext);

        /// <summary>
        /// Registers an association
        /// </summary>
        /// <param name="tablePiece"></param>
        /// <param name="memberInfo"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        TablePiece RegisterAssociation(TablePiece tablePiece, MemberInfo memberInfo,
                                                       BuilderContext builderContext);

        /// <summary>
        /// Registers an external parameter
        /// Since these can be complex expressions, we don't try to identify them
        /// and push them every time
        /// The only loss may be a small memory loss (if anyone can prove me that the same Expression can be used twice)
        /// </summary>
        /// <param name="piece"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        ParameterPiece RegisterParameter(Piece piece, BuilderContext builderContext);
    }
}
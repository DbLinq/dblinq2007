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
using System.Linq.Expressions;
using System.Reflection;
using DbLinq.Linq.Data.Sugar.Expressions;

namespace DbLinq.Linq.Data.Sugar
{
    public interface IExpressionRegistrar
    {
        /// <summary>
        /// Creates a default TableExpression
        /// </summary>
        /// <param name="tableType"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        TableExpression CreateTable(Type tableType, BuilderContext builderContext);

        /// <summary>
        /// Registers an association
        /// </summary>
        /// <param name="joinedTableExpression">The table holding the member, to become the joinedTable</param>
        /// <param name="joinMemberInfo"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        TableExpression RegisterAssociation(TableExpression joinedTableExpression, MemberInfo joinMemberInfo,
                                                       BuilderContext builderContext);

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
        ExternalParameterExpression RegisterParameter(Expression expression, string alias, BuilderContext builderContext);

        /// <summary>
        /// Registers a column with only a table and a MemberInfo (this is the preferred method overload)
        /// </summary>
        /// <param name="tableExpression"></param>
        /// <param name="memberInfo"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        ColumnExpression RegisterColumn(TableExpression tableExpression, MemberInfo memberInfo,
                                                   BuilderContext builderContext);

        /// <summary>
        /// Registers a MetaTable
        /// </summary>
        /// <param name="metaTableType"></param>
        /// <param name="aliases"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        MetaTableExpression RegisterMetaTable(Type metaTableType, IDictionary<MemberInfo, TableExpression> aliases,
                                                         BuilderContext builderContext);

        /// <summary>
        /// Registers a where clause in the current context scope
        /// </summary>
        /// <param name="whereExpression"></param>
        /// <param name="builderContext"></param>
        void RegisterWhere(Expression whereExpression, BuilderContext builderContext);

        /// <summary>
        /// Registers all columns of a table.
        /// </summary>
        /// <param name="tableExpression"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        IEnumerable<ColumnExpression> RegisterAllColumns(TableExpression tableExpression, BuilderContext builderContext);

        /// <summary>
        /// Registers an expression to be returned by main request.
        /// </summary>
        /// <param name="expression">The expression to be registered</param>
        /// <param name="builderContext"></param>
        /// <returns>Expression index</returns>
        int RegisterSelectOperand(Expression expression, BuilderContext builderContext);

        /// <summary>
        /// Returns an existing table or registers the current one
        /// </summary>
        /// <param name="tableExpression"></param>
        /// <param name="builderContext"></param>
        /// <returns>A registered table or the current newly registered one</returns>
        TableExpression RegisterTable(TableExpression tableExpression, BuilderContext builderContext);
    }
}

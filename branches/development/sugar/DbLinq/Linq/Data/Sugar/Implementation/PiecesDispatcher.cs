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
using DbLinq.Linq.Data.Sugar.Pieces;

namespace DbLinq.Linq.Data.Sugar.Implementation
{
    public class PiecesDispatcher: PiecesParser, IPiecesDispatcher
    {
        public IPiecesQueryService PiecesQueryService { get; set; }

        public PiecesDispatcher()
        {
            PiecesQueryService = ObjectFactory.Get<IPiecesQueryService>();
        }

        /// <summary>
        /// Entry point to analyze query related patterns.
        /// They start by a method, like Where(), Select()
        /// </summary>
        /// <param name="piece"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        public virtual Piece AnalyzeQueryPatterns(Piece piece, BuilderContext builderContext)
        {
            if (piece.Is(ExpressionType.Call))
            {
                return AnalyzeQuery(GetMethodInfo(piece.Operands[0]).Name,
                                    GetQueriedType(piece.Operands[2]),
                                    new List<Piece>((from q in piece.Operands select q).Skip(3)),
                                    builderContext);
            }
            throw Error.BadArgument(string.Format("S0052: Don't know what to do with top-level expression {0}", piece));
        }

        /// <summary>
        /// Returns a MethodInfo from a given expression, or null if the types are not related
        /// </summary>
        /// <param name="piece"></param>
        /// <returns></returns>
        protected virtual MethodInfo GetMethodInfo(Piece piece)
        {
            return piece.GetConstantOrDefault<MethodInfo>();
        }

        /// <summary>
        /// Returns a MemberInfo from a given expression, or null on unrelated types
        /// </summary>
        /// <param name="piece"></param>
        /// <returns></returns>
        protected virtual MemberInfo GetMemberInfo(Piece piece)
        {
            return piece.GetConstantOrDefault<MemberInfo>();
        }

        /// <summary>
        /// Returns a member name, from a given expression, or null if it can not be extracted
        /// </summary>
        /// <param name="piece"></param>
        /// <returns></returns>
        protected virtual string GetMemberName(Piece piece)
        {
            var memberInfo = GetMemberInfo(piece);
            if (memberInfo != null)
                return memberInfo.Name;
            return piece.GetConstantOrDefault<string>();
        }

        /// <summary>
        /// Returns a queried type from a given expression, or null if no type can be found
        /// </summary>
        /// <param name="piece"></param>
        /// <returns></returns>
        protected virtual Type GetQueriedType(Piece piece)
        {
            var constantExpression = piece as ConstantPiece;
            if (constantExpression != null)
                return GetQueriedType(constantExpression.Value.GetType());
            return null;
        }

        /// <summary>
        /// Extracts the type from the potentially generic type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        protected virtual Type GetQueriedType(Type type)
        {
            if (typeof(IQueryable).IsAssignableFrom(type))
            {
                if (type.IsGenericType)
                    return type.GetGenericArguments()[0];
            }
            return null;
        }

        /// <summary>
        /// Once the method is extracted, we tread it separately
        /// </summary>
        /// <param name="name"></param>
        /// <param name="queriedType"></param>
        /// <param name="parameters"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual Piece AnalyzeQuery(string name, Type queriedType, IList<Piece> parameters, BuilderContext builderContext)
        {
            // all methods to handle are listed here:
            // ms-help://MS.VSCC.v90/MS.MSDNQTR.v90.en/fxref_system.core/html/2a54ce9d-76f2-81e2-95bb-59740c85386b.htm
            switch (name)
            {
                case "Select":
                    return AnalyzeSelectQuery(queriedType, parameters, builderContext);
                case "Where":
                    return AnalyzeWhereQuery(queriedType, parameters, builderContext);
                case "Average":
                case "Count":
                case "Max":
                case "Min":
                case "Sum":
                    return AnalyzeProjectionQuery(queriedType, name, builderContext);
                default:
                    throw Error.BadArgument("S0133: Implement QueryMethod '{0}'", name);
            }
        }

        /// <summary>
        /// Returns a projection method call
        /// </summary>
        /// <param name="queriedType"></param>
        /// <param name="name"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual Piece AnalyzeProjectionQuery(Type queriedType, string name, BuilderContext builderContext)
        {
            return new OperationPiece(ExpressionType.Call,
                                      new ConstantPiece(name), // method name
                                      new ConstantPiece(null), // method object (null for static/extension methods)
                                      builderContext.ExpressionQuery.Select); // we project on previous request (hope there is one)
        }

        /// <summary>
        /// Entry point for a Select()
        /// </summary>
        /// <param name="queriedType"></param>
        /// <param name="parameters"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual Piece AnalyzeSelectQuery(Type queriedType, IList<Piece> parameters, BuilderContext builderContext)
        {
            var queryExpression = AnalyzeTableQuery(queriedType, parameters, builderContext);
            // do something with the select
            return queryExpression;
        }

        /// <summary>
        /// Entry point for a Where()
        /// </summary>
        /// <param name="queriedType"></param>
        /// <param name="parameters"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual Piece AnalyzeWhereQuery(Type queriedType, IList<Piece> parameters, BuilderContext builderContext)
        {
            var queryExpression = AnalyzeTableQuery(queriedType, parameters, builderContext);
            builderContext.ExpressionQuery.Where.Add(queryExpression);
            return queryExpression;
        }

        /// <summary>
        /// Helper for most entry points methods, such as Where(), Select(), etc.
        /// Registers the requested table and pushes it as first parameter to next call
        /// Then analyzes the called method context, and returns the (mainly unused) resulting expression
        /// </summary>
        /// <param name="queriedType"></param>
        /// <param name="parameters"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual Piece AnalyzeTableQuery(Type queriedType, IList<Piece> parameters, BuilderContext builderContext)
        {
            // the input table is the parameter to the lambda following this,
            // so we register it, in case it wouldn't be already registered and push it as lambda parameter
            var queryTable = PiecesQueryService.RegisterTable(queriedType, builderContext);
            builderContext.CallStack.Push(queryTable);
            // we should have only one QueryExpression here, which is the query to parse
            if (parameters.Count != 1)
                throw Error.BadArgument("S0185: wrong number of arguments ({0})", parameters.Count);
            return AnalyzeQuerySubPatterns(parameters[0], builderContext);
        }

        protected virtual Piece AnalyzeQuerySubPatterns(Piece piece, BuilderContext builderContext)
        {
            return Recurse(piece, AnalyzeQuerySubPattern, Recursion.TopDown, builderContext);
        }

        protected virtual string GetParameterName(Piece piece)
        {
            string name = null;
            piece.Is(ExpressionType.Parameter).LoadOperand(0, m => m.GetConstant(out name));
            return name;
        }

        protected virtual Piece AnalyzeQuerySubPattern(Piece piece, BuilderContext builderContext)
        {
            if (piece is OperationPiece)
            {
                var queryOperationExpression = (OperationPiece)piece;
                switch (queryOperationExpression.Operation)
                {
                    case ExpressionType.Lambda:
                        return AnalyzeQueryLambda(piece, builderContext);
                    case ExpressionType.Parameter:
                        return AnalyzeQueryParameter(piece, builderContext);
                    case ExpressionType.Quote:
                        // TODO: save local variables and restore previous state at exit
                        return piece;
                    case ExpressionType.MemberAccess:
                        return AnalyzeQueryMember(piece, builderContext);
                }
            }
            return piece;
        }

        /// <summary>
        /// Handling a lambda consists in:
        /// - filling its input parameters with what's on the stack
        /// - using the body (parameters are registered in the context)
        /// </summary>
        /// <param name="piece"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual Piece AnalyzeQueryLambda(Piece piece, BuilderContext builderContext)
        {
            var lambdaExpression = (OperationPiece)piece;
            // for a lambda, first parameter is body, others are input parameters
            for (int parameterIndex = 1; parameterIndex < lambdaExpression.Operands.Count; parameterIndex++)
            {
                var parameter = GetParameterName(lambdaExpression.Operands[parameterIndex]);
                if (parameter == null)
                    throw Error.BadArgument("S0238: unknown argument type ({0})", lambdaExpression.Operands[parameterIndex]);
                builderContext.Parameters[parameter] = builderContext.CallStack.Pop();
            }
            // we keep only the body, the header is now useless
            return lambdaExpression.Operands[0];
        }

        /// <summary>
        /// When a parameter is used, we replace it with its original value
        /// </summary>
        /// <param name="piece"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual Piece AnalyzeQueryParameter(Piece piece, BuilderContext builderContext)
        {
            Piece unaliasedPiece;
            var parameterName = GetParameterName(piece);
            builderContext.Parameters.TryGetValue(parameterName, out unaliasedPiece);
            if (unaliasedPiece == null)
                throw Error.BadArgument("S0257: can not find parameter '{0}'", parameterName);
            return unaliasedPiece;
        }

        /// <summary>
        /// Analyzes a member access.
        /// This analyzis is down to top: the highest identifier is at bottom
        /// </summary>
        /// <param name="piece"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual Piece AnalyzeQueryMember(Piece piece, BuilderContext builderContext)
        {
            return Recurse(piece, AnalyzeQuerySubMember, Recursion.DownTop, builderContext);
        }

        protected virtual Piece AnalyzeQuerySubMember(Piece piece, BuilderContext builderContext)
        {
            // then, we treat member access and try to identify if the object is a
            // MetaTable, Table, Association
            if (piece.Is(ExpressionType.MemberAccess))
            {
                // first parameter is object, second is member
                var objectExpression = piece.Operands[0];
                var memberExpression = GetMemberInfo(piece.Operands[1]);
                // then see what we can do, depending on object type
                // - MetaTable --> then the result is a table
                // - Table --> the result may be a column or a join
                // - Object --> external parameter or table (can this happen here? probably not... to be checked)

                // if object is a table, then we need a column, or an association
                if (objectExpression.Is<TablePiece>())
                {
                    var queryTableExpression = (TablePiece)objectExpression;
                    // first of all, then, try to find the association
                    var queryAssociationExpression = PiecesQueryService.RegisterAssociation(queryTableExpression, memberExpression,
                                                                                            builderContext);
                    if (queryAssociationExpression != null)
                        return queryAssociationExpression;
                    // then, try the column
                    var queryColumnExpression = PiecesQueryService.RegisterColumn(queryTableExpression, memberExpression, builderContext);
                    if (queryColumnExpression != null)
                        return queryColumnExpression;
                    // then, cry
                    throw Error.BadArgument("S0293: Column must be mapped. Non-mapped columns are not handled by now.");
                }

                // if object is still an object (== a constant), then we have an external parameter
                if (objectExpression.Is(ExpressionType.Constant))
                {
                    var queryParameterExpression = PiecesQueryService.RegisterParameter(piece, builderContext);
                    if (queryParameterExpression != null)
                        return queryParameterExpression;
                    throw Error.BadArgument("S0302: Can not created parameter from expression '{0}'", piece);
                }
            }
            else
            {
                // here, we're at the bottom: we replace parameters with tables
                piece = AnalyzeQuerySubPatterns(piece, builderContext);
            }
            return piece;
        }
    }
}
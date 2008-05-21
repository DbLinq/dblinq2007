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
using DbLinq.Factory;
using DbLinq.Linq.Data.Sugar.Pieces;
using DbLinq.Util;

namespace DbLinq.Linq.Data.Sugar.Implementation
{
    public class PiecesDispatcher : IPiecesDispatcher
    {
        public IPiecesRegistrar PiecesRegistrar { get; set; }
        public PiecesService PiecesService { get; set; } // TODO: use interface when it's stable

        public PiecesDispatcher()
        {
            PiecesRegistrar = ObjectFactory.Get<IPiecesRegistrar>();
            PiecesService = ObjectFactory.Get<PiecesService>();
        }

        /// <summary>
        /// Registers the first table. Extracts the table type and registeres the piece
        /// </summary>
        /// <param name="requestingExpression"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        public virtual Piece RegisterTable(Expression requestingExpression, BuilderContext builderContext)
        {
            var callExpression = (MethodCallExpression)requestingExpression;
            var requestingType = callExpression.Arguments[0].Type;
            return PiecesRegistrar.RegisterTable(PiecesService.GetQueriedType(requestingType), builderContext);
        }

        /// <summary>
        /// Entry point for Analyzis
        /// </summary>
        /// <param name="piece"></param>
        /// <param name="parameter"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        public virtual Piece Analyze(Piece piece, Piece parameter, BuilderContext builderContext)
        {
            return Analyze(piece, new[] { parameter }, builderContext);
        }

        protected virtual Piece Analyze(Piece piece, BuilderContext builderContext)
        {
            return Analyze(piece, new Piece[0], builderContext);
        }

        protected virtual Piece Analyze(Piece piece, IList<Piece> parameters, BuilderContext builderContext)
        {
            // for constants, there's nothing we can do
            if (piece is ConstantPiece)
                return piece;

            var operationPiece = piece as OperationPiece;
            if (operationPiece != null)
            {
                switch (operationPiece.Operation)
                {
                case OperationType.Call:
                    return AnalyzeCall(PiecesService.GetMethodInfo(piece.Operands[0]).Name,
                                       PiecesService.MergeParameters(parameters,
                                       PiecesService.ExtractParameters(piece.Operands, 2 + parameters.Count)), // 0 is the method call, 1 is the "this" (null for static methods)
                        // if extra parameters are specified in "parameters", ignore them as Operands
                                       builderContext);
                case OperationType.Lambda:
                    return AnalyzeLambda(piece, parameters, builderContext);
                case OperationType.Parameter:
                    return AnalyzeParameter(piece, builderContext);
                case OperationType.Quote:
                    return AnalyzeQuote(piece, parameters, builderContext);
                case OperationType.MemberAccess:
                    return AnalyzeMember(piece, builderContext);
                #region case OperationType.<Common operators>:
                case OperationType.Add:
                case OperationType.AddChecked:
                case OperationType.Divide:
                case OperationType.Modulo:
                case OperationType.Multiply:
                case OperationType.MultiplyChecked:
                case OperationType.Power:
                case OperationType.Subtract:
                case OperationType.SubtractChecked:
                case OperationType.And:
                case OperationType.Or:
                case OperationType.ExclusiveOr:
                case OperationType.LeftShift:
                case OperationType.RightShift:
                case OperationType.AndAlso:
                case OperationType.OrElse:
                case OperationType.Equal:
                case OperationType.NotEqual:
                case OperationType.GreaterThanOrEqual:
                case OperationType.GreaterThan:
                case OperationType.LessThan:
                case OperationType.LessThanOrEqual:
                case OperationType.Coalesce:
                //case OperationType.ArrayIndex
                //case OperationType.ArrayLength
                //case OperationType.Convert
                //case OperationType.ConvertChecked
                case OperationType.Negate:
                //case OperationType.NegateChecked
                case OperationType.Not:
                //case OperationType.TypeAs
                case OperationType.UnaryPlus:
                #endregion
                    return AnalyzeOperator(piece, builderContext);
                case OperationType.New:
                    return AnalyzeNew(piece, parameters, builderContext);
                }
                throw Error.BadArgument(string.Format("S0052: Don't know what to do with expression {0}", piece));
            }
            if (parameters.Count != 0)
            {
                throw Error.BadArgument(
                    "S0088: There should be no parameter to a non-OperationPiece Piece (found {0} parameter(s))",
                    parameters.Count);
            }
            return piece;
        }

        protected virtual Piece AnalyzeCall(string methodName, IList<Piece> parameters, BuilderContext builderContext)
        {
            // all methods to handle are listed here:
            // ms-help://MS.VSCC.v90/MS.MSDNQTR.v90.en/fxref_system.core/html/2a54ce9d-76f2-81e2-95bb-59740c85386b.htm
            switch (methodName)
            {
            case "Select":
                return AnalyzeSelect(parameters, builderContext);
            case "Where":
                return AnalyzeWhere(parameters, builderContext);
            case "SelectMany":
                return AnalyzeSelectMany(parameters, builderContext);
            case "All":
                return AnalyzeAll(parameters, builderContext);
            case "Average":
            case "Count":
            case "Max":
            case "Min":
            case "Sum":
                return AnalyzeProjectionQuery(methodName, builderContext);
            default:
                throw Error.BadArgument("S0133: Implement QueryMethod '{0}'", methodName);
            }
        }

        /// <summary>
        /// Returns a projection method call
        /// </summary>
        /// <param name="name"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual Piece AnalyzeProjectionQuery(string name, BuilderContext builderContext)
        {
            // TODO: review this code
            return new OperationPiece(OperationType.Call,
                                      new ConstantPiece(name), // method name
                                      new ConstantPiece(null), // method object (null for static/extension methods)
                                      builderContext.PiecesQuery.Select); // we project on previous request (hope there is one)
        }

        /// <summary>
        /// Entry point for a Select()
        /// static Select(this Expression table, λ(table))
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual Piece AnalyzeSelect(IList<Piece> parameters, BuilderContext builderContext)
        {
            // just call back the underlying lambda (or quote, whatever)
            return Analyze(parameters[1], new[] { parameters[0] }, builderContext);
        }

        /// <summary>
        /// Entry point for a Where()
        /// static Where(this Expression table, λ(table))
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual Piece AnalyzeWhere(IList<Piece> parameters, BuilderContext builderContext)
        {
            var tablePiece = parameters[0];
            PiecesRegistrar.RegisterWhere(Analyze(parameters[1], new[] { tablePiece }, builderContext), builderContext);
            return tablePiece;
        }

        /// <summary>
        /// Handling a lambda consists in:
        /// - filling its input parameters with what's on the stack
        /// - using the body (parameters are registered in the context)
        /// </summary>
        /// <param name="piece"></param>
        /// <param name="parameters"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual Piece AnalyzeLambda(Piece piece, IList<Piece> parameters, BuilderContext builderContext)
        {
            var lambdaPiece = (OperationPiece)piece;
            // for a lambda, first parameter is body, others are input parameters
            // so we create a parameters stack
            for (int parameterIndex = 1; parameterIndex < lambdaPiece.Operands.Count; parameterIndex++)
            {
                var parameter = PiecesService.GetParameterName(lambdaPiece.Operands[parameterIndex]);
                if (parameter == null)
                    throw Error.BadArgument("S0238: unknown argument type ({0})", lambdaPiece.Operands[parameterIndex]);
                builderContext.Parameters[parameter] = Analyze(parameters[parameterIndex - 1], builderContext);
            }
            // we keep only the body, the header is now useless
            // and once the parameters have been substituted, we don't pass one anymore
            return Analyze(lambdaPiece.Operands[0], builderContext);
        }

        /// <summary>
        /// When a parameter is used, we replace it with its original value
        /// </summary>
        /// <param name="piece"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual Piece AnalyzeParameter(Piece piece, BuilderContext builderContext)
        {
            Piece unaliasedPiece;
            var parameterName = PiecesService.GetParameterName(piece);
            builderContext.Parameters.TryGetValue(parameterName, out unaliasedPiece);
            if (unaliasedPiece == null)
                throw Error.BadArgument("S0257: can not find parameter '{0}'", parameterName);

            #region set alias helper

            // for table...
            var unaliasedTablePiece = unaliasedPiece as TablePiece;
            if (unaliasedTablePiece != null && unaliasedTablePiece.Alias == null)
                unaliasedTablePiece.Alias = parameterName;
            // .. or column
            var unaliasedColumnPiece = unaliasedPiece as ColumnPiece;
            if (unaliasedColumnPiece != null && unaliasedColumnPiece.Alias == null)
                unaliasedColumnPiece.Alias = parameterName;

            #endregion

            return unaliasedPiece;
        }

        /// <summary>
        /// Analyzes a member access.
        /// This analyzis is down to top: the highest identifier is at bottom
        /// </summary>
        /// <param name="piece"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual Piece AnalyzeMember(Piece piece, BuilderContext builderContext)
        {
            // first parameter is object, second is member
            var objectPiece = Analyze(piece.Operands[0], builderContext);
            var memberPiece = PiecesService.GetMemberInfo(piece.Operands[1]);
            // then see what we can do, depending on object type
            // - MetaTable --> then the result is a table
            // - Table --> the result may be a column or a join
            // - Object --> external parameter or table (can this happen here? probably not... to be checked)

            if (objectPiece.Is<MetaTablePiece>())
            {
                var metaTablePiece = (MetaTablePiece)objectPiece;
                var tablePiece = metaTablePiece.GetTablePiece(memberPiece);
                if (tablePiece == null)
                    throw Error.BadArgument("S0270: MemberInfo '{0}' not found in MetaTable", memberPiece.Name);
                return tablePiece;
            }

            // if object is a table, then we need a column, or an association
            if (objectPiece.Is<TablePiece>())
            {
                var tablePiece = (TablePiece)objectPiece;
                // first of all, then, try to find the association
                var queryAssociationExpression = PiecesRegistrar.RegisterAssociation(tablePiece, memberPiece,
                                                                                        builderContext);
                if (queryAssociationExpression != null)
                    return queryAssociationExpression;
                // then, try the column
                var queryColumnExpression = PiecesRegistrar.RegisterColumn(tablePiece, memberPiece, builderContext);
                if (queryColumnExpression != null)
                    return queryColumnExpression;
                // then, cry
                throw Error.BadArgument("S0293: Column must be mapped. Non-mapped columns are not handled by now.");
            }

            // if object is still an object (== a constant), then we have an external parameter
            if (objectPiece.Is(OperationType.Constant))
            {
                var parameterPiece = PiecesRegistrar.RegisterParameter(piece, builderContext);
                if (parameterPiece != null)
                    return parameterPiece;
                throw Error.BadArgument("S0302: Can not created parameter from expression '{0}'", piece);
            }

            throw Error.BadArgument("S0238: Don't know how to handle Piece");
        }

        /// <summary>
        /// A Quote creates a new local context, outside which created parameters disappear
        /// This is why we clone the BuilderContext
        /// </summary>
        /// <param name="piece"></param>
        /// <param name="parameters"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual Piece AnalyzeQuote(Piece piece, IList<Piece> parameters, BuilderContext builderContext)
        {
            var builderContextClone = builderContext.NewQuote();
            return Analyze(piece.Operands[0], parameters, builderContextClone);
        }

        /// <summary>
        /// Operator analysis consists in anlyzing all operands
        /// </summary>
        /// <param name="piece"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual Piece AnalyzeOperator(Piece piece, BuilderContext builderContext)
        {
            for (int operandIndex = 0; operandIndex < piece.Operands.Count; operandIndex++)
            {
                var operand = piece.Operands[operandIndex];
                piece.Operands[operandIndex] = Analyze(operand, builderContext);
            }
            return piece;
        }

        /// <summary>
        /// SelectMany() joins tables
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual Piece AnalyzeSelectMany(IList<Piece> parameters, BuilderContext builderContext)
        {
            if (parameters.Count == 3)
            {
                // ms-help://MS.VSCC.v90/MS.MSDNQTR.v90.en/fxref_system.core/html/3371348f-7811-b0bc-8c0a-2a595e08e086.htm
                var tablePiece = parameters[0] as TablePiece;
                if (tablePiece == null)
                    throw Error.BadArgument("S0335: Expected a TablePiece for SelectMany()");
                var projectionPiece = Analyze(parameters[1], new[] { tablePiece }, builderContext);
                var manyPiece = Analyze(parameters[2], new[] { tablePiece, projectionPiece }, builderContext);
                // from here, our manyPiece is a MetaTable definition
                var associations = new Dictionary<MemberInfo, TablePiece>();
                int ctorParametersCount = (manyPiece.Operands.Count - 1) / 2;
                Type metaTableType = null;
                for (int ctorParameterIndex = 0; ctorParameterIndex < ctorParametersCount; ctorParameterIndex++)
                {
                    var aliasedTablePiece = manyPiece.Operands[1 + ctorParameterIndex] as TablePiece;
                    if (aliasedTablePiece == null)
                        throw Error.BadArgument("S0343: Expected a TablePiece for SelectMany()");
                    var memberInfo = manyPiece.Operands[ctorParametersCount + 1 + ctorParameterIndex].GetConstantOrDefault<MemberInfo>();
                    if (memberInfo == null)
                        throw Error.BadArgument("S0343: Expected a MemberInfo for SelectMany()");
                    metaTableType = memberInfo.ReflectedType;
                    // the property info is the reflecting property for the memberInfo, if memberInfo is a get_*
                    // otherwise we keep the memberInfo as is, since it is a field
                    var propertyInfo = memberInfo.GetExposingProperty() ?? memberInfo;
                    associations[propertyInfo] = aliasedTablePiece;
                }
                if (metaTableType == null)
                    throw Error.BadArgument("S0355: Empty MetaTable found"); // this should never happen, otherwise we may simply ignore it or take the type from elsewhere
                return PiecesRegistrar.RegisterMetaTable(metaTableType, associations, builderContext);
            }
            throw Error.BadArgument("S0358: Don't know how to handle this SelectMany() overload ({0} parameters)", parameters.Count);
        }

        /// <summary>
        /// New returns a new type with two possible uses:
        /// 1. A projection type for the select
        /// 2. A MetaTable creation
        /// There are n*2+1 operands:
        /// 0: ctor reflection info 
        /// [1..n]: arguments to ctor
        /// [n+1..2*n]: ordered memberInfo corresponding to ctor arguments
        /// </summary>
        /// <param name="piece"></param>
        /// <param name="parameters"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual Piece AnalyzeNew(Piece piece, IList<Piece> parameters, BuilderContext builderContext)
        {
            // parse all arguments, even input parameters
            for (int operandIndex = 1; operandIndex < piece.Operands.Count; operandIndex++)
            {
                var operand = piece.Operands[operandIndex];
                piece.Operands[operandIndex] = Analyze(operand, builderContext);
            }
            return piece;
        }

        /// <summary>
        /// All() returns true if the given condition satisfies all provided elements
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual Piece AnalyzeAll(IList<Piece> parameters, BuilderContext builderContext)
        {
            var allBuilderContext = builderContext.NewScope();
            var tablePiece = Analyze(parameters[0], allBuilderContext);
            var allClause = Analyze(parameters[1], tablePiece, allBuilderContext);
            // from here we build a custom clause:
            // <allClause> ==> "(select count(*) from <table> where not <allClause>)==0"
            // TODO (later...): see if some vendors support native All operator and avoid this substitution
            var wherePiece = new OperationPiece(OperationType.Not, allClause);
            PiecesRegistrar.RegisterWhere(wherePiece, allBuilderContext);
            var select = new OperationPiece(OperationType.Count, tablePiece);
            // TODO: see if we need to register the tablePiece here (we probably don't)

            // we now switch back to current context, and compare the result with 0
            var allPiece = new OperationPiece(OperationType.Equal, select, new ConstantPiece(0));
            return allPiece;
        }
    }
}

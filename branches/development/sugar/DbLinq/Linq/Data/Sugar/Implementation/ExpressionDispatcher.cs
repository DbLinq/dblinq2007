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
using DbLinq.Linq.Data.Sugar.ExpressionMutator;
using DbLinq.Linq.Data.Sugar.Expressions;
using DbLinq.Util;

namespace DbLinq.Linq.Data.Sugar.Implementation
{
    public class ExpressionDispatcher : IExpressionDispatcher
    {
        public IExpressionRegistrar ExpressionRegistrar { get; set; }
        public ExpressionService ExpressionService { get; set; } // TODO: use interface when it's stable

        public ExpressionDispatcher()
        {
            ExpressionRegistrar = ObjectFactory.Get<IExpressionRegistrar>();
            ExpressionService = ObjectFactory.Get<ExpressionService>();
        }

        /// <summary>
        /// Registers the first table. Extracts the table type and registeres the piece
        /// </summary>
        /// <param name="requestingExpression"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        public virtual Expression RegisterTable(Expression requestingExpression, BuilderContext builderContext)
        {
            var callExpression = (MethodCallExpression)requestingExpression;
            var requestingType = callExpression.Arguments[0].Type;
            return ExpressionRegistrar.RegisterTable(ExpressionService.GetQueriedType(requestingType), builderContext);
        }

        /// <summary>
        /// Entry point for Analyzis
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="parameter"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        public virtual Expression Analyze(Expression expression, Expression parameter, BuilderContext builderContext)
        {
            return Analyze(expression, new[] { parameter }, builderContext);
        }

        protected virtual Expression Analyze(Expression expression, BuilderContext builderContext)
        {
            return Analyze(expression, new Expression[0], builderContext);
        }

        protected virtual Expression Analyze(Expression expression, IList<Expression> parameters, BuilderContext builderContext)
        {
            // for constants, there's nothing we can do
            //if (piece is ConstantPiece)
            //    return piece;
            switch (expression.NodeType)
            {
            case ExpressionType.Call:
                return AnalyzeCall(((MethodCallExpression)expression).Method.Name,
                                   ExpressionService.MergeParameters(parameters,
                                   ExpressionService.ExtractParameters(expression.GetOperands(), 1 + parameters.Count)), // 0 is the "this" (null for static methods)
                    // if extra parameters are specified in "parameters", ignore them as Operands
                                   builderContext);
            case ExpressionType.Lambda:
                return AnalyzeLambda(expression, parameters, builderContext);
            case ExpressionType.Parameter:
                return AnalyzeParameter(expression, builderContext);
            case ExpressionType.Quote:
                return AnalyzeQuote(expression, parameters, builderContext);
            case ExpressionType.MemberAccess:
                return AnalyzeMember(expression, builderContext);
            #region case ExpressionType.<Common operators>:
            case ExpressionType.Add:
            case ExpressionType.AddChecked:
            case ExpressionType.Divide:
            case ExpressionType.Modulo:
            case ExpressionType.Multiply:
            case ExpressionType.MultiplyChecked:
            case ExpressionType.Power:
            case ExpressionType.Subtract:
            case ExpressionType.SubtractChecked:
            case ExpressionType.And:
            case ExpressionType.Or:
            case ExpressionType.ExclusiveOr:
            case ExpressionType.LeftShift:
            case ExpressionType.RightShift:
            case ExpressionType.AndAlso:
            case ExpressionType.OrElse:
            case ExpressionType.Equal:
            case ExpressionType.NotEqual:
            case ExpressionType.GreaterThanOrEqual:
            case ExpressionType.GreaterThan:
            case ExpressionType.LessThan:
            case ExpressionType.LessThanOrEqual:
            case ExpressionType.Coalesce:
            //case ExpressionType.ArrayIndex
            //case ExpressionType.ArrayLength
            //case ExpressionType.Convert
            //case ExpressionType.ConvertChecked
            case ExpressionType.Negate:
            //case ExpressionType.NegateChecked
            case ExpressionType.Not:
            //case ExpressionType.TypeAs
            case ExpressionType.UnaryPlus:
            case ExpressionType.New: // Yes dude, new is an operator
            #endregion
                return AnalyzeOperator(expression, builderContext);
            }
            //    throw Error.BadArgument(string.Format("S0052: Don't know what to do with expression {0}", piece));
            //if (parameters.Count != 0)
            //{
            //    throw Error.BadArgument(
            //        "S0088: There should be no parameter to a non-OperationPiece Piece (found {0} parameter(s))",
            //        parameters.Count);
            //}
            return expression;
        }

        protected virtual Expression AnalyzeCall(string methodName, IList<Expression> parameters, BuilderContext builderContext)
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
        protected virtual Expression AnalyzeProjectionQuery(string name, BuilderContext builderContext)
        {
            // TODO: review this code
            //return new OperationPiece(OperationType.Call,
            //                          new ConstantPiece(name), // method name
            //                          new ConstantPiece(null), // method object (null for static/extension methods)
            //                          builderContext.PiecesQuery.Select); // we project on previous request (hope there is one)
            return null;
        }

        /// <summary>
        /// Entry point for a Select()
        /// static Select(this Expression table, λ(table))
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual Expression AnalyzeSelect(IList<Expression> parameters, BuilderContext builderContext)
        {
            // just call back the underlying lambda (or quote, whatever)
            return Analyze(parameters[1], parameters[0], builderContext);
        }

        /// <summary>
        /// Entry point for a Where()
        /// static Where(this Expression table, λ(table))
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual Expression AnalyzeWhere(IList<Expression> parameters, BuilderContext builderContext)
        {
            var tablePiece = parameters[0];
            ExpressionRegistrar.RegisterWhere(Analyze(parameters[1], tablePiece, builderContext), builderContext);
            return tablePiece;
        }

        /// <summary>
        /// Handling a lambda consists in:
        /// - filling its input parameters with what's on the stack
        /// - using the body (parameters are registered in the context)
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="parameters"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual Expression AnalyzeLambda(Expression expression, IList<Expression> parameters, BuilderContext builderContext)
        {
            var lambdaExpression = expression as LambdaExpression;
            if (lambdaExpression == null)
                throw Error.BadArgument("S0227: Unknown type for AnalyzeLambda() ({0})", expression.GetType());
            // for a lambda, first parameter is body, others are input parameters
            // so we create a parameters stack
            for (int parameterIndex = 0; parameterIndex < lambdaExpression.Parameters.Count; parameterIndex++)
            {
                var parameterExpression = lambdaExpression.Parameters[parameterIndex];
                builderContext.Parameters[parameterExpression.Name] = Analyze(parameters[parameterIndex], builderContext);
            }
            // we keep only the body, the header is now useless
            // and once the parameters have been substituted, we don't pass one anymore
            return Analyze(lambdaExpression.Body, builderContext);
        }

        /// <summary>
        /// When a parameter is used, we replace it with its original value
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual Expression AnalyzeParameter(Expression expression, BuilderContext builderContext)
        {
            Expression unaliasedPiece;
            var parameterName = ExpressionService.GetParameterName(expression);
            builderContext.Parameters.TryGetValue(parameterName, out unaliasedPiece);
            if (unaliasedPiece == null)
                throw Error.BadArgument("S0257: can not find parameter '{0}'", parameterName);

            #region set alias helper

            // for table...
            var unaliasedTablePiece = unaliasedPiece as TableExpression;
            if (unaliasedTablePiece != null && unaliasedTablePiece.Alias == null)
                unaliasedTablePiece.Alias = parameterName;
            // .. or column
            var unaliasedColumnPiece = unaliasedPiece as ColumnExpression;
            if (unaliasedColumnPiece != null && unaliasedColumnPiece.Alias == null)
                unaliasedColumnPiece.Alias = parameterName;

            #endregion

            return unaliasedPiece;
        }

        /// <summary>
        /// Analyzes a member access.
        /// This analyzis is down to top: the highest identifier is at bottom
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual Expression AnalyzeMember(Expression expression, BuilderContext builderContext)
        {
            var memberExpression = (MemberExpression)expression;
            // first parameter is object, second is member
            var objectExpression = Analyze(memberExpression.Expression, builderContext);
            var memberInfo = memberExpression.Member;
            // then see what we can do, depending on object type
            // - MetaTable --> then the result is a table
            // - Table --> the result may be a column or a join
            // - Object --> external parameter or table (can this happen here? probably not... to be checked)

            if (objectExpression is MetaTableExpression)
            {
                var metaTableExpression = (MetaTableExpression)objectExpression;
                var tableExpression = metaTableExpression.GetTableExpression(memberInfo);
                if (tableExpression == null)
                    throw Error.BadArgument("S0270: MemberInfo '{0}' not found in MetaTable", memberInfo.Name);
                return tableExpression;
            }

            // if object is a table, then we need a column, or an association
            if (objectExpression is TableExpression)
            {
                var tableExpression = (TableExpression)objectExpression;
                // first of all, then, try to find the association
                var queryAssociationExpression = ExpressionRegistrar.RegisterAssociation(tableExpression, memberInfo,
                                                                                        builderContext);
                if (queryAssociationExpression != null)
                    return queryAssociationExpression;
                // then, try the column
                var queryColumnExpression = ExpressionRegistrar.RegisterColumn(tableExpression, memberInfo, builderContext);
                if (queryColumnExpression != null)
                    return queryColumnExpression;
                // then, cry
                throw Error.BadArgument("S0293: Column must be mapped. Non-mapped columns are not handled by now.");
            }

            // if object is still an object (== a constant), then we have an external parameter
            if (objectExpression is ConstantExpression)
            {
                var parameterExpression = ExpressionRegistrar.RegisterParameter(expression, builderContext);
                if (parameterExpression != null)
                    return parameterExpression;
                throw Error.BadArgument("S0302: Can not created parameter from expression '{0}'", expression);
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
        protected virtual Expression AnalyzeQuote(Expression piece, IList<Expression> parameters, BuilderContext builderContext)
        {
            var builderContextClone = builderContext.NewQuote();
            return Analyze(piece.GetOperands().First(), parameters, builderContextClone);
        }

        /// <summary>
        /// Operator analysis consists in anlyzing all operands
        /// </summary>
        /// <param name="piece"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual Expression AnalyzeOperator(Expression piece, BuilderContext builderContext)
        {
            var operands = piece.GetOperands().ToList();
            for (int operandIndex = 0; operandIndex < operands.Count; operandIndex++)
            {
                var operand = operands[operandIndex];
                operands[operandIndex] = Analyze(operand, builderContext);
            }
            return piece.ChangeOperands(operands);
        }

        /// <summary>
        /// SelectMany() joins tables
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual Expression AnalyzeSelectMany(IList<Expression> parameters, BuilderContext builderContext)
        {
            if (parameters.Count == 3)
            {
                // ms-help://MS.VSCC.v90/MS.MSDNQTR.v90.en/fxref_system.core/html/3371348f-7811-b0bc-8c0a-2a595e08e086.htm
                var tableExpression = parameters[0] as TableExpression;
                if (tableExpression == null)
                    throw Error.BadArgument("S0335: Expected a TablePiece for SelectMany()");
                var projectionExpression = Analyze(parameters[1], new[] { tableExpression }, builderContext);
                var manyPiece = Analyze(parameters[2], new[] { tableExpression, projectionExpression }, builderContext);
                // from here, our manyPiece is a MetaTable definition
                var associations = new Dictionary<MemberInfo, TableExpression>();
                var newExpression = manyPiece as NewExpression;
                if (newExpression == null)
                    throw Error.BadArgument("S0377: Expected a NewExpression as SelectMany() return value");
                Type metaTableType = null;
                for (int ctorParameterIndex = 0; ctorParameterIndex < newExpression.Arguments.Count; ctorParameterIndex++)
                {
                    var aliasedTableExpression = newExpression.Arguments[ctorParameterIndex] as TableExpression;
                    if (aliasedTableExpression == null)
                        throw Error.BadArgument("S0343: Expected a TablePiece for SelectMany()");
                    var memberInfo = newExpression.Members[ctorParameterIndex];
                    metaTableType = memberInfo.ReflectedType;
                    // the property info is the reflecting property for the memberInfo, if memberInfo is a get_*
                    // otherwise we keep the memberInfo as is, since it is a field
                    var propertyInfo = memberInfo.GetExposingProperty() ?? memberInfo;
                    associations[propertyInfo] = aliasedTableExpression;
                }
                if (metaTableType == null)
                    throw Error.BadArgument("S0355: Empty MetaTable found"); // this should never happen, otherwise we may simply ignore it or take the type from elsewhere
                return ExpressionRegistrar.RegisterMetaTable(metaTableType, associations, builderContext);
            }
            throw Error.BadArgument("S0358: Don't know how to handle this SelectMany() overload ({0} parameters)", parameters.Count);
        }

        /// <summary>
        /// All() returns true if the given condition satisfies all provided elements
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual Expression AnalyzeAll(IList<Expression> parameters, BuilderContext builderContext)
        {
            var allBuilderContext = builderContext.NewScope();
            var tableExpression = Analyze(parameters[0], allBuilderContext);
            var allClause = Analyze(parameters[1], tableExpression, allBuilderContext);
            // from here we build a custom clause:
            // <allClause> ==> "(select count(*) from <table> where not <allClause>)==0"
            // TODO (later...): see if some vendors support native All operator and avoid this substitution
            var whereExpression = Expression.Not(allClause);
            ExpressionRegistrar.RegisterWhere(whereExpression, allBuilderContext);
            allBuilderContext.CurrentScope = allBuilderContext.CurrentScope.Select(new SpecialExpression(SpecialExpressionType.Count, tableExpression));
            // TODO: see if we need to register the tablePiece here (we probably don't)

            // we now switch back to current context, and compare the result with 0
            var allPiece = Expression.Equal(allBuilderContext.CurrentScope, Expression.Constant(0));
            return allPiece;
        }
    }
}

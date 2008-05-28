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
using DbLinq.Linq.Data.Sugar.ExpressionMutator;
using DbLinq.Linq.Data.Sugar.Expressions;
using DbLinq.Util;

namespace DbLinq.Linq.Data.Sugar.Implementation
{
    partial class ExpressionDispatcher
    {
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
            switch (expression.NodeType)
            {
            case ExpressionType.Call:
                return AnalyzeCall((MethodCallExpression)expression, parameters, builderContext);
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
            case ExpressionType.Convert:
            case ExpressionType.ConvertChecked:
            case ExpressionType.Negate:
            case ExpressionType.NegateChecked:
            case ExpressionType.Not:
            //case ExpressionType.TypeAs
            case ExpressionType.UnaryPlus:
            case ExpressionType.New: // Yes dude, new is an operator
            #endregion
                return AnalyzeOperator(expression, builderContext);
            }
            return expression;
        }

        protected virtual Expression AnalyzeCall(MethodCallExpression expression, IList<Expression> parameters, BuilderContext builderContext)
        {
            var operands = expression.GetOperands().ToList();
            var operarandsToSkip = expression.Method.IsStatic ? 1 : 0;
            var originalParameters = ExtractParameters(operands, parameters.Count + operarandsToSkip);
            var newParameters = MergeParameters(parameters, originalParameters);
            return AnalyzeCall(expression.Method.Name, newParameters, builderContext);
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
                return AnalyzeProjectionQuery(SpecialExpressionType.Average, parameters, builderContext);
            case "Count":
                return AnalyzeProjectionQuery(SpecialExpressionType.Count, parameters, builderContext);
            case "Max":
                return AnalyzeProjectionQuery(SpecialExpressionType.Max, parameters, builderContext);
            case "Min":
                return AnalyzeProjectionQuery(SpecialExpressionType.Min, parameters, builderContext);
            case "Sum":
                return AnalyzeProjectionQuery(SpecialExpressionType.Sum, parameters, builderContext);
            case "StartsWith":
                return AnalyzeLikeStart(parameters, builderContext);
            case "EndsWith":
                return AnalyzeLikeEnd(parameters, builderContext);
            case "Contains":
                if (parameters[0].Type is string)
                    return AnalyzeLike(parameters, builderContext);
                return AnalyzeContains(parameters, builderContext);
            case "First":
            case "FirstOrDefault":
                return AnalyzeScalar(methodName, 1, parameters, builderContext);
            case "Single":
            case "SingleOrDefault":
                return AnalyzeScalar(methodName, 2, parameters, builderContext);
            case "Last":
                return AnalyzeScalar(methodName, null, parameters, builderContext);
            case "Take":
                return AnalyzeTake(parameters, builderContext);
            case "Skip":
                return AnalyzeSkip(parameters, builderContext);
            case "ToUpper":
                return AnalyzeToUpper(parameters, builderContext);
            case "ToLower":
                return AnalyzeToLower(parameters, builderContext);
            default:
                throw Error.BadArgument("S0133: Implement QueryMethod '{0}'", methodName);
            }
        }

        /// <summary>
        /// Limits selection count
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual Expression AnalyzeTake(IList<Expression> parameters, BuilderContext builderContext)
        {
            AddLimit(Analyze(parameters[1], builderContext), builderContext);
            return Analyze(parameters[0], builderContext);
        }

        protected virtual void AddLimit(Expression limit, BuilderContext builderContext)
        {
            var previousLimit = builderContext.CurrentScope.Limit;
            if (previousLimit != null)
                builderContext.CurrentScope.Limit = Expression.Condition(Expression.LessThan(previousLimit, limit),
                                                                         previousLimit, limit);
            else
                builderContext.CurrentScope.Limit = limit;
        }

        /// <summary>
        /// Skip selection items
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual Expression AnalyzeSkip(IList<Expression> parameters, BuilderContext builderContext)
        {
            AddOffset(Analyze(parameters[1], builderContext), builderContext);
            return Analyze(parameters[0], builderContext);
        }

        protected virtual void AddOffset(Expression offset, BuilderContext builderContext)
        {
            var previousOffset = builderContext.CurrentScope.Offset;
            if (previousOffset != null)
                builderContext.CurrentScope.Offset = Expression.Add(offset, previousOffset);
            else
                builderContext.CurrentScope.Offset = offset;
        }

        /// <summary>
        /// Registers a scalar method call for result
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="limit"></param>
        /// <param name="parameters"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual Expression AnalyzeScalar(string methodName, int? limit, IList<Expression> parameters, BuilderContext builderContext)
        {
            builderContext.CurrentScope.ExecuteMethodName = methodName;
            if (limit.HasValue)
                AddLimit(Expression.Constant(limit.Value), builderContext);
            return Analyze(parameters[0], builderContext);
        }

        /// <summary>
        /// Returns a projection method call
        /// </summary>
        /// <param name="specialExpressionType"></param>
        /// <param name="parameters"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual Expression AnalyzeProjectionQuery(SpecialExpressionType specialExpressionType, IList<Expression> parameters,
            BuilderContext builderContext)
        {
            var projectionOperand = Analyze(parameters[0], builderContext);
            if (projectionOperand is TableExpression)
                projectionOperand = RegisterTable((TableExpression)projectionOperand, builderContext);
            return new SpecialExpression(specialExpressionType, projectionOperand);
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
            RegisterWhere(Analyze(parameters[1], tablePiece, builderContext), builderContext);
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
            var parameterName = GetParameterName(expression);
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
                var queryAssociationExpression = RegisterAssociation(tableExpression, memberInfo,
                                                                                        builderContext);
                if (queryAssociationExpression != null)
                    return queryAssociationExpression;
                // then, try the column
                var queryColumnExpression = RegisterColumn(tableExpression, memberInfo, builderContext);
                if (queryColumnExpression != null)
                    return queryColumnExpression;
                // then, cry
                throw Error.BadArgument("S0293: Column must be mapped. Non-mapped columns are not handled by now.");
            }

            // if object is still an object (== a constant), then we have an external parameter
            if (objectExpression is ConstantExpression)
            {
                // the memberInfo.Name is provided here only to ease the SQL reading
                var parameterExpression = RegisterParameter(expression, memberInfo.Name, builderContext);
                if (parameterExpression != null)
                    return parameterExpression;
                throw Error.BadArgument("S0302: Can not created parameter from expression '{0}'", expression);
            }

            // we have here a special cases for nullables
            if (objectExpression.Type.IsNullable())
            {
                // Value means we convert the nullable to a value --> use Convert instead (works both on CLR and SQL, too)
                if (memberInfo.Name == "Value")
                    return Expression.Convert(objectExpression, memberInfo.GetMemberType());
                // HasValue means not null (works both on CLR and SQL, too)
                if (memberInfo.Name == "HasValue")
                    return new SpecialExpression(SpecialExpressionType.IsNotNull, objectExpression);
            }

            return AnalyzeCommonMember(objectExpression, memberInfo, builderContext);
        }

        private Expression AnalyzeCommonMember(Expression objectExpression, MemberInfo memberInfo, BuilderContext builderContext)
        {
            if (typeof(string).IsAssignableFrom(objectExpression.Type))
            {
                switch (memberInfo.Name)
                {
                case "Length":
                    return new SpecialExpression(SpecialExpressionType.StringLength, objectExpression);
                }
            }
            throw Error.BadArgument("S0324: Don't know how to handle Piece");
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
                return RegisterMetaTable(metaTableType, associations, builderContext);
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
            RegisterWhere(whereExpression, allBuilderContext);
            allBuilderContext.CurrentScope = allBuilderContext.CurrentScope.ChangeOperands(new SpecialExpression(SpecialExpressionType.Count, tableExpression));
            // TODO: see if we need to register the tablePiece here (we probably don't)

            // we now switch back to current context, and compare the result with 0
            var allPiece = Expression.Equal(allBuilderContext.CurrentScope, Expression.Constant(0));
            return allPiece;
        }

        protected virtual Expression AnalyzeLikeStart(IList<Expression> parameters, BuilderContext builderContext)
        {
            return AnalyzeLike(parameters[0], null, parameters[1], "%", builderContext);
        }

        protected virtual Expression AnalyzeLikeEnd(IList<Expression> parameters, BuilderContext builderContext)
        {
            return AnalyzeLike(parameters[0], "%", parameters[1], null, builderContext);
        }

        protected virtual Expression AnalyzeLike(IList<Expression> parameters, BuilderContext builderContext)
        {
            return AnalyzeLike(parameters[0], "%", parameters[1], "%", builderContext);
        }

        protected virtual Expression AnalyzeLike(Expression value, string before, Expression operand, string after, BuilderContext builderContext)
        {
            operand = Analyze(operand, builderContext);
            if (before != null)
                operand = new SpecialExpression(SpecialExpressionType.Concat, Expression.Constant(before), operand);
            if (after != null)
                operand = new SpecialExpression(SpecialExpressionType.Concat, operand, Expression.Constant(after));
            return new SpecialExpression(SpecialExpressionType.Like, Analyze(value, builderContext), operand);
        }

        protected virtual Expression AnalyzeContains(IList<Expression> parameters, BuilderContext builderContext)
        {
            return AnalyzeLike(parameters, builderContext);
        }

        protected virtual Expression AnalyzeToUpper(IList<Expression> parameters, BuilderContext builderContext)
        {
            return new SpecialExpression(SpecialExpressionType.ToUpper, Analyze(parameters[0], builderContext));
        }

        protected virtual Expression AnalyzeToLower(IList<Expression> parameters, BuilderContext builderContext)
        {
            return new SpecialExpression(SpecialExpressionType.ToLower, Analyze(parameters[0], builderContext));
        }
    }
}

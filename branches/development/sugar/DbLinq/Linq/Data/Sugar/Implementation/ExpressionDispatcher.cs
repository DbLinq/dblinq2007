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
using System.Data;
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
        public ExpressionQualifier ExpressionQualifier { get; set; } // TODO: the same
        public IDataRecordReader DataRecordReader { get; set; }

        public ExpressionDispatcher()
        {
            ExpressionRegistrar = ObjectFactory.Get<IExpressionRegistrar>();
            ExpressionService = ObjectFactory.Get<ExpressionService>();
            ExpressionQualifier = ObjectFactory.Get<ExpressionQualifier>();
            DataRecordReader = ObjectFactory.Get<IDataRecordReader>();
        }

        /// <summary>
        /// Registers the first table. Extracts the table type and registeres the piece
        /// </summary>
        /// <param name="requestingExpression"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        public virtual Expression CreateTableExpression(Expression requestingExpression, BuilderContext builderContext)
        {
            var callExpression = (MethodCallExpression)requestingExpression;
            var requestingType = callExpression.Arguments[0].Type;
            return ExpressionRegistrar.CreateTable(ExpressionService.GetQueriedType(requestingType), builderContext);
        }

        /// <summary>
        /// Registers the first table. Extracts the table type and registeres the piece
        /// </summary>
        /// <param name="requestingExpression"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        public virtual Expression GetTable(Expression requestingExpression, BuilderContext builderContext)
        {
            var callExpression = (MethodCallExpression)requestingExpression;
            var requestingType = callExpression.Arguments[0].Type;
            return ExpressionRegistrar.CreateTable(ExpressionService.GetQueriedType(requestingType), builderContext);
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
            return expression;
        }

        protected virtual Expression AnalyzeCall(MethodCallExpression expression, IList<Expression> parameters, BuilderContext builderContext)
        {
            var operands = expression.GetOperands().ToList();
            var operarandsToSkip = expression.Method.IsStatic ? 1 : 0;
            return AnalyzeCall(expression.Method.Name, ExpressionService.MergeParameters(parameters,
                                                                           ExpressionService.ExtractParameters(
                                                                               operands, parameters.Count + operarandsToSkip)),
                                                                                   builderContext);
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
            case "StartsWith":
                return AnalyzeLikeStart(parameters, builderContext);
            case "EndsWith":
                return AnalyzeLikeEnd(parameters, builderContext);
            case "Contains":
                if (parameters[0].Type is string)
                    return AnalyzeLike(parameters, builderContext);
                else
                    return AnalyzeContains(parameters, builderContext);
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
                // the memberInfo.Name is provided here only to ease the SQL reading
                var parameterExpression = ExpressionRegistrar.RegisterParameter(expression, memberInfo.Name, builderContext);
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
            // TODO
            return null;
        }

        /// <summary>
        /// Builds the upper select clause
        /// </summary>
        /// <param name="selectExpression"></param>
        /// <param name="builderContext"></param>
        public virtual void BuildSelect(Expression selectExpression, BuilderContext builderContext)
        {
            // first thing, look for tables and use columns instead
            selectExpression = selectExpression.Recurse(e => CheckTableExpression(e, builderContext));
            // then collect columns, split Expression in
            // - things we will do in CLR
            // - things we will do in SQL
            selectExpression = CutOutOperands(selectExpression, builderContext);
            // the last return value becomes the select, with CurrentScope
            builderContext.CurrentScope.Select = selectExpression;
            builderContext.ExpressionQuery.Select = builderContext.CurrentScope;
        }

        /// <summary>
        /// Cuts Expressions between CLR and SQL:
        /// - Replaces Expressions moved to SQL by calls to DataRecord values reader
        /// - SQL expressions are placed into Operands
        /// - Return value creator is the returned Expression
        /// </summary>
        /// <param name="selectExpression"></param>
        /// <param name="builderContext"></param>
        protected virtual Expression CutOutOperands(Expression selectExpression, BuilderContext builderContext)
        {
            var dataRecordParameter = Expression.Parameter(typeof(IDataRecord), "rdr");
            var mappingContextParameter = Expression.Parameter(typeof(MappingContext), "mapping");
            return selectExpression.Recurse(e => CutOutOperand(e, dataRecordParameter, mappingContextParameter,
                                                                builderContext));
        }

        /// <summary>
        /// If we operand is an SQL operand, then cut it out and return a DataRecord value reader instead
        /// </summary>
        /// <param name="operand"></param>
        /// <param name="mappingContextParameter"></param>
        /// <param name="builderContext"></param>
        /// <param name="dataRecordParameter"></param>
        /// <returns></returns>
        protected virtual Expression CutOutOperand(Expression operand,
            ParameterExpression dataRecordParameter, ParameterExpression mappingContextParameter,
            BuilderContext builderContext)
        {
            if (GetCutOutOperand(operand, builderContext))
            {
                int valueIndex = ExpressionRegistrar.RegisterSelectOperand(operand, builderContext);
                var propertyReader = DataRecordReader.GetPropertyReader(dataRecordParameter, mappingContextParameter, operand.Type,
                                                   valueIndex);
                return propertyReader;
            }
            return operand;
        }

        /// <summary>
        /// Returns true if we must cut out the given Expression
        /// </summary>
        /// <param name="operand"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        private bool GetCutOutOperand(Expression operand, BuilderContext builderContext)
        {
            bool cutOut = false;
            var tier = ExpressionQualifier.GetTier(operand);
            if ((tier & ExpressionTier.Sql) != 0) // we can cut out only if the following expressiong can go to SQL
            {
                // then we have two possible strategies, load the DB at max, then it's always true from here
                if (builderContext.QueryContext.MaximumDatabaseLoad)
                    cutOut = true;
                else // if no max database load then it's min: we switch to SQL only when CLR doesn't support the Expression
                    cutOut = (tier & ExpressionTier.Clr) == 0;
            }
            return cutOut;
        }

        /// <summary>
        /// Checks any expression for a TableExpression, and eventually replaces it with the convenient columns selection
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual Expression CheckTableExpression(Expression expression, BuilderContext builderContext)
        {
            if (expression is TableExpression)
                return GetSelectTableExpression((TableExpression)expression, builderContext);
            return expression;
        }

        /// <summary>
        /// Replaces a table selection by a selection of all mapped columns (ColumnExpressions).
        /// ColumnExpressions will be replaced at a later time by the tier splitter
        /// </summary>
        /// <param name="tableExpression"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual Expression GetSelectTableExpression(TableExpression tableExpression, BuilderContext builderContext)
        {
            var bindings = new List<MemberBinding>();
            foreach (var columnExpression in ExpressionRegistrar.RegisterAllColumns(tableExpression, builderContext))
            {
                var binding = Expression.Bind(columnExpression.MemberInfo, columnExpression);
                bindings.Add(binding);
            }
            var newExpression = Expression.New(tableExpression.Type);
            return Expression.MemberInit(newExpression, bindings);
        }
    }
}

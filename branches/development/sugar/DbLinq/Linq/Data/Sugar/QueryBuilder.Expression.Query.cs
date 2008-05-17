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
using System.Linq;
using DbLinq.Linq.Data.Sugar.Expressions;

namespace DbLinq.Linq.Data.Sugar
{
    partial class QueryBuilder
    {
        /// <summary>
        /// Entry point to analyze query related patterns.
        /// They start by a method, like Where(), Select()
        /// </summary>
        /// <param name="queryExpression"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual QueryExpression AnalyzeQueryPatterns(QueryExpression queryExpression, BuilderContext builderContext)
        {
            if (queryExpression.Parse().Is(ExpressionType.Call))
            {
                return AnalyzeQuery(GetMethodInfo(queryExpression.Operands[0]).Name,
                             GetQueriedType(queryExpression.Operands[2]),
                             new List<QueryExpression>((from q in queryExpression.Operands select q).Skip(3)),
                             builderContext);
            }
            return queryExpression;
        }

        /// <summary>
        /// Returns a MethodInfo from a given expression, or null if the types are not related
        /// </summary>
        /// <param name="queryExpression"></param>
        /// <returns></returns>
        protected virtual MethodInfo GetMethodInfo(QueryExpression queryExpression)
        {
            var constantExpression = queryExpression as QueryConstantExpression;
            if (constantExpression != null)
                return constantExpression.Value as MethodInfo;
            return null;
        }

        /// <summary>
        /// Returns a queried type from a given expression, or null if no type can be found
        /// </summary>
        /// <param name="queryExpression"></param>
        /// <returns></returns>
        protected virtual Type GetQueriedType(QueryExpression queryExpression)
        {
            var constantExpression = queryExpression as QueryConstantExpression;
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
        protected virtual QueryExpression AnalyzeQuery(string name, Type queriedType, IList<QueryExpression> parameters, BuilderContext builderContext)
        {
            switch (name)
            {
            case "Select":
                return AnalyzeSelectQuery(queriedType, parameters, builderContext);
            case "Where":
                return AnalyzeWhereQuery(queriedType, parameters, builderContext);
            default:
                throw new NotImplementedException(string.Format("S1: Implement QueryMethod '{0}'", name));
            }
        }

        /// <summary>
        /// Entry point for a Select()
        /// </summary>
        /// <param name="queriedType"></param>
        /// <param name="parameters"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual QueryExpression AnalyzeSelectQuery(Type queriedType, IList<QueryExpression> parameters, BuilderContext builderContext)
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
        protected virtual QueryExpression AnalyzeWhereQuery(Type queriedType, IList<QueryExpression> parameters, BuilderContext builderContext)
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
        protected virtual QueryExpression AnalyzeTableQuery(Type queriedType, IList<QueryExpression> parameters, BuilderContext builderContext)
        {
            // the input table is the parameter to the lambda following this,
            // so we register it, in case it wouldn't be already registered and push it as lambda parameter
            var queryTable = RegisterTable(queriedType, builderContext);
            builderContext.CallStack.Push(queryTable);
            // we should have only one QueryExpression here, which is the query to parse
            if (parameters.Count != 1)
                throw new ArgumentException(string.Format("S2: wrong number of arguments ({0})", parameters.Count));
            return AnalyzeQuerySubPatterns(parameters[0], builderContext);
        }

        protected virtual QueryExpression AnalyzeQuerySubPatterns(QueryExpression queryExpression, BuilderContext builderContext)
        {
            return AnalyzePatterns(queryExpression, AnalyzeQuerySubPattern, builderContext);
        }

        protected virtual string GetParameterName(QueryExpression queryExpression)
        {
            string name = null;
            queryExpression.Parse().Is(ExpressionType.Parameter).LoadOperand(0, m => m.GetConstant(out name));
            return name;
        }

        protected virtual QueryExpression AnalyzeQuerySubPattern(QueryExpression queryExpression, BuilderContext builderContext)
        {
            if (queryExpression is QueryOperationExpression)
            {
                var queryOperationExpression = (QueryOperationExpression)queryExpression;
                switch (queryOperationExpression.Operation)
                {
                case ExpressionType.Lambda:
                    return AnalyzeQueryLambda(queryExpression, builderContext);
                case ExpressionType.Parameter:
                    return AnalyzeQueryParameter(queryExpression, builderContext);
                case ExpressionType.Quote:
                    // TODO: save local variables and restore previous state at exit
                    return queryExpression;
                case ExpressionType.MemberAccess:
                    return AnalyzeQueryMember(queryExpression, builderContext);
                }
            }
            return queryExpression;
        }

        /// <summary>
        /// Handling a lambda consists in:
        /// - filling its input parameters with what's on the stack
        /// - using the body (parameters are registered in the context)
        /// </summary>
        /// <param name="queryExpression"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual QueryExpression AnalyzeQueryLambda(QueryExpression queryExpression, BuilderContext builderContext)
        {
            var lambdaExpression = (QueryOperationExpression)queryExpression;
            // for a lambda, first parameter is body, others are input parameters
            for (int parameterIndex = 1; parameterIndex < lambdaExpression.Operands.Count; parameterIndex++)
            {
                var parameter = GetParameterName(lambdaExpression.Operands[parameterIndex]);
                if (parameter == null)
                    throw new ArgumentException(string.Format("S3: unknown argument type ({0})", lambdaExpression.Operands[parameterIndex]));
                builderContext.Parameters[parameter] = builderContext.CallStack.Pop();
            }
            // we keep only the body, the header is now useless
            return lambdaExpression.Operands[0];
        }

        /// <summary>
        /// When a parameter is used, we replace it with its original value
        /// </summary>
        /// <param name="queryExpression"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual QueryExpression AnalyzeQueryParameter(QueryExpression queryExpression, BuilderContext builderContext)
        {
            QueryExpression unaliasedExpression;
            var parameterName = GetParameterName(queryExpression);
            builderContext.Parameters.TryGetValue(parameterName, out unaliasedExpression);
            if (unaliasedExpression == null)
                throw new ArgumentException(string.Format("S4: can not find parameter '{0}'", parameterName));
            return unaliasedExpression;
        }

        protected virtual QueryExpression AnalyzeQueryMember(QueryExpression queryExpression, BuilderContext builderContext)
        {
            return queryExpression;
        }
    }
}

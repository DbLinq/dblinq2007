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
using DbLinq.Linq.Data.Sugar.Pieces;

namespace DbLinq.Linq.Data.Sugar.Implementation
{
    public class PiecesBuilder : IPiecesBuilder
    {
        public virtual Piece CreateQueryExpression(Expression expression, BuilderContext builderContext)
        {
            var queryExpression = CreateQueryExpressionDispatch(expression, builderContext);
            // also keep track of original expression, it will be directly used when possible
            queryExpression.OriginalExpression = expression;
            return queryExpression;
        }

        protected virtual OperationPiece CreateQueryExpressionDispatch(Expression expression, BuilderContext builderContext)
        {
            if (expression == null)
                return new ConstantPiece(null);
            if (expression is BinaryExpression)
                return CreateQueryExpressionSpecific((BinaryExpression)expression, builderContext);
            if (expression is ConditionalExpression)
                return CreateQueryExpressionSpecific((ConditionalExpression)expression, builderContext);
            if (expression is ConstantExpression)
                return CreateQueryExpressionSpecific((ConstantExpression)expression, builderContext);
            if (expression is InvocationExpression)
                return CreateQueryExpressionSpecific((InvocationExpression)expression, builderContext);
            if (expression is LambdaExpression)
                return CreateQueryExpressionSpecific((LambdaExpression)expression, builderContext);
            if (expression is MemberExpression)
                return CreateQueryExpressionSpecific((MemberExpression)expression, builderContext);
            if (expression is MethodCallExpression)
                return CreateQueryExpressionSpecific((MethodCallExpression)expression, builderContext);
            if (expression is NewExpression)
                return CreateQueryExpressionSpecific((NewExpression)expression, builderContext);
            if (expression is NewArrayExpression)
                return CreateQueryExpressionSpecific((NewArrayExpression)expression, builderContext);
            if (expression is MemberInitExpression)
                return CreateQueryExpressionSpecific((MemberInitExpression)expression, builderContext);
            if (expression is ListInitExpression)
                return CreateQueryExpressionSpecific((ListInitExpression)expression, builderContext);
            if (expression is ParameterExpression)
                return CreateQueryExpressionSpecific((ParameterExpression)expression, builderContext);
            if (expression is TypeBinaryExpression)
                return CreateQueryExpressionSpecific((TypeBinaryExpression)expression, builderContext);
            if (expression is UnaryExpression)
                return CreateQueryExpressionSpecific((UnaryExpression)expression, builderContext);
            throw Error.BadArgument("S0074: Unknown Expression type");
        }

        protected virtual OperationPiece CreateQueryExpressionSpecific(BinaryExpression expression, BuilderContext builderContext)
        {
            #region // Possible NodeType
            //Add
            //AddChecked
            //Divide
            //Modulo
            //Multiply
            //MultiplyChecked
            //Power
            //Subtract
            //SubtractChecked

            //And
            //Or
            //ExclusiveOr

            //LeftShift
            //RightShift

            //AndAlso
            //OrElse

            //Equal
            //NotEqual
            //GreaterThanOrEqual
            //GreaterThan
            //LessThan
            //LessThanOrEqual

            //Coalesce

            //ArrayIndex
            #endregion
            return new OperationPiece(expression.NodeType,
                                      CreateQueryExpression(expression.Left, builderContext),
                                      CreateQueryExpression(expression.Right, builderContext));
        }

        protected virtual OperationPiece CreateQueryExpressionSpecific(ConditionalExpression expression, BuilderContext builderContext)
        {
            //  Possible NodeType "Conditional"
            return new OperationPiece(expression.NodeType,
                                      CreateQueryExpression(expression.Test, builderContext),
                                      CreateQueryExpression(expression.IfTrue, builderContext),
                                      CreateQueryExpression(expression.IfFalse, builderContext));
        }

        protected virtual OperationPiece CreateQueryExpressionSpecific(ConstantExpression expression, BuilderContext builderContext)
        {
            //  Possible NodeType "Constant"
            return new ConstantPiece(expression.Value);
        }

        protected virtual OperationPiece CreateQueryExpressionSpecific(InvocationExpression expression, BuilderContext builderContext)
        {
            throw new NotImplementedException();
        }

        protected virtual OperationPiece CreateQueryExpressionSpecific(LambdaExpression expression, BuilderContext builderContext)
        {
            //  Possible NodeType "Lambda"
            var parameters = new List<Piece>();
            parameters.Add(CreateQueryExpression(expression.Body, builderContext));
            foreach (var parameter in expression.Parameters)
                parameters.Add(CreateQueryExpression(parameter, builderContext));
            return new OperationPiece(expression.NodeType, parameters);
        }

        protected virtual OperationPiece CreateQueryExpressionSpecific(MemberExpression expression, BuilderContext builderContext)
        {
            // Possible NodeType "MemberAccess"
            return new OperationPiece(expression.NodeType,
                                      CreateQueryExpression(expression.Expression, builderContext),
                                      new ConstantPiece(expression.Member));
        }

        protected virtual OperationPiece CreateQueryExpressionSpecific(MethodCallExpression expression, BuilderContext builderContext)
        {
            //  Possible NodeType "Call"
            var parameters = new List<Piece>();
            parameters.Add(new ConstantPiece(expression.Method));
            parameters.Add(CreateQueryExpression(expression.Object, builderContext));
            foreach (var argument in expression.Arguments)
                parameters.Add(CreateQueryExpression(argument, builderContext));
            return new OperationPiece(expression.NodeType, parameters);
        }

        protected virtual OperationPiece CreateQueryExpressionSpecific(NewExpression expression, BuilderContext builderContext)
        {
            throw new NotImplementedException();
        }

        protected virtual OperationPiece CreateQueryExpressionSpecific(NewArrayExpression expression, BuilderContext builderContext)
        {
            throw new NotImplementedException();
        }

        protected virtual OperationPiece CreateQueryExpressionSpecific(MemberInitExpression expression, BuilderContext builderContext)
        {
            throw new NotImplementedException();
        }

        protected virtual OperationPiece CreateQueryExpressionSpecific(ListInitExpression expression, BuilderContext builderContext)
        {
            throw new NotImplementedException();
        }

        protected virtual OperationPiece CreateQueryExpressionSpecific(ParameterExpression expression, BuilderContext builderContext)
        {
            // Possible NodeType "Parameter"
            return new OperationPiece(expression.NodeType,
                                      new ConstantPiece(expression.Name));
        }

        protected virtual OperationPiece CreateQueryExpressionSpecific(TypeBinaryExpression expression, BuilderContext builderContext)
        {
            throw new NotImplementedException();
        }

        protected virtual OperationPiece CreateQueryExpressionSpecific(UnaryExpression expression, BuilderContext builderContext)
        {
            #region // Possible NodeType
            //ArrayLength
            //Convert
            //ConvertChecked
            //Negate
            //NegateChecked
            //Not
            //Quote
            //TypeAs
            //UnaryPlus
            #endregion
            return new OperationPiece(expression.NodeType,
                                      CreateQueryExpression(expression.Operand, builderContext));
        }
    }
}
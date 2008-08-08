#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne
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

#if MONO_STRICT
using System.Data.Linq.Sugar;
using System.Data.Linq.Sugar.ExpressionMutator;
using System.Data.Linq.Sugar.Expressions;
#else
using DbLinq.Data.Linq.Sugar;
using DbLinq.Data.Linq.Sugar.ExpressionMutator;
using DbLinq.Data.Linq.Sugar.Expressions;
#endif

#if MONO_STRICT
namespace System.Data.Linq.Sugar.Implementation
#else
namespace DbLinq.Data.Linq.Sugar.Implementation
#endif
{
    internal class SpecialExpressionTranslator : ISpecialExpressionTranslator
    {
        /// <summary>
        /// Translate a hierarchy's SpecialExpressions to Expressions
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public Expression Translate(Expression expression)
        {
            return expression.Recurse(Analyzer);
        }

        protected virtual Expression Analyzer(Expression expression)
        {
            if (expression is SpecialExpression)
                return Translate((SpecialExpression)expression);
            return expression;
        }

        /// <summary>
        /// Translates a SpecialExpression to standard Expression equivalent
        /// </summary>
        /// <param name="specialExpression"></param>
        /// <returns></returns>
        protected virtual Expression Translate(SpecialExpression specialExpression)
        {
            var operands = specialExpression.Operands.ToList();
            switch (specialExpression.SpecialNodeType)  // SETuse
            {
                case SpecialExpressionType.IsNull:
                    return TranslateIsNull(operands);
                case SpecialExpressionType.IsNotNull:
                    return TranslateIsNotNull(operands);
                case SpecialExpressionType.Concat:
                    return TranslateConcat(operands);
                //case SpecialExpressionType.Count:
                //    break;
                //case SpecialExpressionType.Like:
                //    break;
                //case SpecialExpressionType.Min:
                //    break;
                //case SpecialExpressionType.Max:
                //    break;
                //case SpecialExpressionType.Sum:
                //    break;
                //case SpecialExpressionType.Average:
                //    break;
                //case SpecialExpressionType.StringLength:
                //    break;
                case SpecialExpressionType.ToUpper:
                    return TranslateToUpper(operands);
                case SpecialExpressionType.ToLower:
                    return TranslateToLower(operands);
                //case SpecialExpressionType.In:
                //    break;
                case SpecialExpressionType.SubString:
                    return TranslateSubString(operands);
                case SpecialExpressionType.Trim:
                    return TranslateTrim(operands);
                case SpecialExpressionType.StringInsert:
                    return TranslateInsertString(operands);
                case SpecialExpressionType.Replace:
                    return TranslateReplace(operands);

                default:
                    throw Error.BadArgument("S0078: Implement translator for {0}", specialExpression.SpecialNodeType);
            }
        }

        protected virtual Expression TranslateReplace(List<Expression> operands)
        {
            return Expression.Call(operands[0],
                               typeof(string).GetMethod("Replace", new[] { typeof(string), typeof(string) }),
                               operands[1], operands[2]);
        }
        protected virtual Expression TranslateInsertString(List<Expression> operands)
        {
            return Expression.Call(operands.First(), typeof(string).GetMethod("Insert"), operands[1], operands[2]);
        }

        protected virtual Expression TranslateTrim(List<Expression> operands)
        {
            return Expression.Call(operands.First(), typeof(string).GetMethod("Trim", new Type[] { }));
        }
        protected virtual Expression TranslateSubString(List<Expression> operands)
        {
            if (operands.Count > 2)
            {
                return Expression.Call(operands[0],
                                       typeof(string).GetMethod("Substring", new[] { operands[1].Type, operands[2].Type }),
                                       operands[1], operands[2]);
            }

            return Expression.Call(operands[0],
                                   typeof(string).GetMethod("Substring", new[] { operands[1].Type }),
                                   operands[1]);
        }

        protected virtual Expression TranslateToLower(List<Expression> operands)
        {
            return Expression.Call(operands[0], typeof(string).GetMethod("ToLower", new Type[0]));
        }

        protected virtual Expression TranslateToUpper(List<Expression> operands)
        {
            return Expression.Call(operands[0], typeof(string).GetMethod("ToUpper", new Type[0]));
        }

        protected virtual Expression TranslateConcat(List<Expression> operands)
        {
            return Expression.Add(operands[0], operands[1]);
        }

        protected virtual Expression TranslateIsNotNull(List<Expression> operands)
        {
            return Expression.NotEqual(operands[0], Expression.Constant(null));
        }

        protected virtual Expression TranslateIsNull(List<Expression> operands)
        {
            return Expression.Equal(operands[0], Expression.Constant(null));
        }
    }
}
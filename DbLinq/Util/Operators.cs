#region MIT license
////////////////////////////////////////////////////////////////////
// MIT license:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//
// Authors:
//        Jiri George Moudry
////////////////////////////////////////////////////////////////////
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;

namespace DBLinq.Util
{
    public static class Operators
    {
        /// <summary>
        /// return MySql precedence of an operator.
        /// See document:
        /// http://www.mysql.org/doc/refman/5.1/en/operator-precedence.html
        /// <returns></returns>
        public static int GetPrecedence(ExpressionType nodeType)
        {
            switch(nodeType)
            {
                case ExpressionType.OrElse:
                case ExpressionType.ExclusiveOr:
                    return 11;
                case ExpressionType.AndAlso:
                    return 12;
                case ExpressionType.Not:
                    return 13;
                case ExpressionType.Equal:
                    //Console.WriteLine("TODO: verify Mysql precedence of operator '='");
                    return 14;
                //between, case, when, then, else: 
                //  14
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.GreaterThan:
                //is, like, regexp, in
                    return 15;
                case ExpressionType.Or:
                    return 16;
                case ExpressionType.And:
                    return 17;
                case ExpressionType.RightShift:
                    return 18;
                case ExpressionType.Add:
                case ExpressionType.Subtract:
                    return 19;
                case ExpressionType.Multiply:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                    return 20;
                //case ExpressionType.Not: //BitwiseNot - "^"?
                //    return 21; //already above as logical not?

                //case ExpressionType.Not:
                //    return 22; //what is the difference between "!" and "NOT"?
                //binary, collate:
                //  return 23
                default:
                    Console.WriteLine("OperatorPrecedence L23 TODO:"+nodeType);
                    return 0;
            }
        }

        public static string FormatBinaryOperator(ExpressionType nodeType)
        {
            switch(nodeType)
            {
                case ExpressionType.GreaterThan: return ">";
                case ExpressionType.GreaterThanOrEqual: return ">=";
                case ExpressionType.LessThan: return "<";
                case ExpressionType.LessThanOrEqual: return "<=";
                case ExpressionType.Equal: return "=";

                case ExpressionType.NotEqual: 
                    return "<>";  //Thanks to Laurent Morisseau for spotting the omission

                case ExpressionType.AndAlso: return "AND";
                case ExpressionType.OrElse: return "OR";

                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                    return "*";

                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                    return "+";
                default: return "L36_TODO_Format_"+nodeType;
            }
        }

    }
}

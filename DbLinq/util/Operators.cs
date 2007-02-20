////////////////////////////////////////////////////////////////////
//Initial author: Jiri George Moudry, 2006.
//License: LGPL. (Visit http://www.gnu.org)
//Commercial code may call into this library, if it's in a different module (DLL)
////////////////////////////////////////////////////////////////////

using System;
using System.Expressions;
using System.Collections.Generic;
using System.Text;

namespace DBLinq.util
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
                case ExpressionType.BitwiseXor:
                    return 11;
                case ExpressionType.AndAlso:
                    return 12;
                case ExpressionType.Not:
                    return 13;
                case ExpressionType.EQ:
                    Console.WriteLine("TODO: verify Mysql precedence of operator '='");
                    return 14;
                //between, case, when, then, else: 
                //  14
                case ExpressionType.LT:
                case ExpressionType.GE:
                case ExpressionType.GT:
                //is, like, regexp, in
                    return 15;
                case ExpressionType.BitwiseOr:
                    return 16;
                case ExpressionType.BitwiseAnd:
                    return 17;
                case ExpressionType.RShift:
                    return 18;
                case ExpressionType.Add:
                case ExpressionType.Subtract:
                    return 19;
                case ExpressionType.Multiply:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                    return 20;
                case ExpressionType.BitwiseNot: //"^"?
                    return 21;
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
                case ExpressionType.GT: return ">";
                case ExpressionType.LT: return "<";
                case ExpressionType.EQ: return "=";
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

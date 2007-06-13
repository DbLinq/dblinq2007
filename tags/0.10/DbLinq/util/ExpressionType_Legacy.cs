using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace DBLinq.util
{
#if LINQ_PREVIEW_2006
    static class ExpressionType_Legacy
    {
        static readonly ExpressionType Call = ExpressionType.MethodCall;
        static readonly ExpressionType CallVirtual = ExpressionType.MethodCallVirtual;
        static readonly ExpressionType Equal = ExpressionType.EQ;
        static readonly ExpressionType LessThanOrEqual = ExpressionType.LE;
        static readonly ExpressionType LessThan = ExpressionType.LT;
        static readonly ExpressionType Convert = ExpressionType.Cast;
        static readonly ExpressionType GreaterThanOrEqual = ExpressionType.GE;
        static readonly ExpressionType GreaterThan = ExpressionType.GT;
        static readonly ExpressionType Not = ExpressionType.BitwiseNot;
        static readonly ExpressionType RightShift = ExpressionType.RShift;
        static readonly ExpressionType ExclusiveOr = ExpressionType.BitwiseXor;
        static readonly ExpressionType Or = ExpressionType.BitwiseOr;
        static readonly ExpressionType And = ExpressionType.BitwiseAnd;
    
    
    }
#endif
}

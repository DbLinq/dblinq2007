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

using System.Linq.Expressions;

namespace DbLinq.Linq.Data.Sugar.Pieces
{
    public enum OperationType
    {
        #region ExpressionType synonyms
        Add = ExpressionType.Add,
        AddChecked = ExpressionType.AddChecked,
        And = ExpressionType.And,
        AndAlso = ExpressionType.AndAlso,
        ArrayLength = ExpressionType.ArrayLength,
        ArrayIndex = ExpressionType.ArrayIndex,
        Call = ExpressionType.Call,
        Coalesce = ExpressionType.Coalesce,
        Conditional = ExpressionType.Conditional,
        Constant = ExpressionType.Constant,
        Convert = ExpressionType.Convert,
        ConvertChecked = ExpressionType.ConvertChecked,
        Divide = ExpressionType.Divide,
        Equal = ExpressionType.Equal,
        ExclusiveOr = ExpressionType.ExclusiveOr,
        GreaterThan = ExpressionType.GreaterThan,
        GreaterThanOrEqual = ExpressionType.GreaterThanOrEqual,
        Invoke = ExpressionType.Invoke,
        Lambda = ExpressionType.Lambda,
        LeftShift = ExpressionType.LeftShift,
        LessThan = ExpressionType.LessThan,
        LessThanOrEqual = ExpressionType.LessThanOrEqual,
        ListInit = ExpressionType.ListInit,
        MemberAccess = ExpressionType.MemberAccess,
        MemberInit = ExpressionType.MemberInit,
        Modulo = ExpressionType.Modulo,
        Multiply = ExpressionType.Multiply,
        MultiplyChecked = ExpressionType.MultiplyChecked,
        Negate = ExpressionType.Negate,
        UnaryPlus = ExpressionType.UnaryPlus,
        NegateChecked = ExpressionType.NegateChecked,
        New = ExpressionType.New,
        NewArrayInit = ExpressionType.NewArrayInit,
        NewArrayBounds = ExpressionType.NewArrayBounds,
        Not = ExpressionType.Not,
        NotEqual = ExpressionType.NotEqual,
        Or = ExpressionType.Or,
        OrElse = ExpressionType.OrElse,
        Parameter = ExpressionType.Parameter,
        Power = ExpressionType.Power,
        Quote = ExpressionType.Quote,
        RightShift = ExpressionType.RightShift,
        Subtract = ExpressionType.Subtract,
        SubtractChecked = ExpressionType.SubtractChecked,
        TypeAs = ExpressionType.TypeAs,
        TypeIs = ExpressionType.TypeIs,
        #endregion

        IsNull,
        IsNotNull,
        Concat,
        Count,
    }
}

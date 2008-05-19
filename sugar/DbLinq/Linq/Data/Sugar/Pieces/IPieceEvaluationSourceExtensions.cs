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

namespace DbLinq.Linq.Data.Sugar.Pieces
{
    public static class IPieceEvaluationSourceExtensions
    {
        // Match public methods
        public static PieceEvaluationSource Is(this IPieceEvaluationSource sourceEvaluation, ExpressionType expressionType)
        {
            var source = sourceEvaluation.GetEvaluationSource();
            Is<OperationPiece>(source);
            source.IsEvaluationValid = source.IsEvaluationValid && ((OperationPiece)source.EvaluatedPiece).Operation == expressionType;
            return source;
        }

        public static PieceEvaluationSource Is<T>(this IPieceEvaluationSource sourceEvaluation)
            where T : Piece
        {
            var source = sourceEvaluation.GetEvaluationSource();
            source.IsEvaluationValid = source.IsEvaluationValid && source.EvaluatedPiece is T;
            return source;
        }

        public static PieceEvaluationSource IsConstant(this IPieceEvaluationSource sourceEvaluation, object value)
        {
            var source = sourceEvaluation.GetEvaluationSource();
            Is<ConstantPiece>(source);
            source.IsEvaluationValid = source.IsEvaluationValid && ((ConstantPiece)source.EvaluatedPiece).Value == value;
            return source;
        }

        public static PieceEvaluationSource GetConstant<T>(this IPieceEvaluationSource sourceEvaluation, out T value)
        {
            var source = sourceEvaluation.GetEvaluationSource();
            value = default(T);
            Is<ConstantPiece>(source);
            if (source.IsEvaluationValid)
            {
                if (((ConstantPiece)source.EvaluatedPiece).Value is T)
                    value = (T)((ConstantPiece)source.EvaluatedPiece).Value;
                else
                    source.IsEvaluationValid = false;
            }
            return source;
        }

        public static T GetConstantOrDefault<T>(this IPieceEvaluationSource sourceEvaluation)
        {
            T value;
            GetConstant(sourceEvaluation, out value);
            return value;
        }

        public static PieceEvaluationSource IsFunction(this IPieceEvaluationSource sourceEvaluation, string functionName)
        {
            return Is<ConstantPiece>(sourceEvaluation).LoadOperand(0, match => match.IsConstant(functionName));
        }

        public static PieceEvaluationSource Or(this IPieceEvaluationSource sourceEvaluation,
                                                  IEnumerable<Action<IPieceEvaluationSource>> evaluations)
        {
            var source = sourceEvaluation.GetEvaluationSource();
            if (source.IsEvaluationValid)
            {
                bool stop = true;
                foreach (var evaluation in evaluations)
                {
                    var newMatch = source.CloneEvaluationSource();
                    evaluation(newMatch);
                    if (newMatch.IsEvaluationValid)
                    {
                        stop = false;
                        break;
                    }
                }
                if (stop)
                    source.IsEvaluationValid = false;
            }
            return source;
        }

        public static PieceEvaluationSource LoadOperand(this IPieceEvaluationSource sourceEvaluation, int index,
                                                           Action<IPieceEvaluationSource> evaluation)
        {
            var source = sourceEvaluation.GetEvaluationSource();
            if (source.IsEvaluationValid)
            {
                if (index < 0)
                    index = source.EvaluatedPiece.Operands.Count - index;
                var newMatch = source.CloneEvaluationSource();
                newMatch.EvaluatedPiece = source.EvaluatedPiece.Operands[index];
                evaluation(newMatch);
                source.IsEvaluationValid = newMatch.IsEvaluationValid;
            }
            return source;
        }

        public static PieceEvaluationSource Process(this IPieceEvaluationSource sourceEvaluation, Func<Piece, bool> evaluationPart)
        {
            var source = sourceEvaluation.GetEvaluationSource();
            source.IsEvaluationValid = source.IsEvaluationValid && evaluationPart(source.EvaluatedPiece);
            return source;
        }
    }
}
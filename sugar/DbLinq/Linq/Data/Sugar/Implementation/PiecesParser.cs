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
using DbLinq.Linq.Data.Sugar.Pieces;

namespace DbLinq.Linq.Data.Sugar.Implementation
{
    public class PiecesParser
    {
        protected enum Recursion
        {
            TopDown,
            DownTop,
        }

        /// <summary>
        /// Top-down pattern analysis.
        /// From here, we convert common QueryExpressions to tables, columns, parameters.
        /// </summary>
        /// <param name="piece">The original expression</param>
        /// <param name="recursion"></param>
        /// <param name="analyzer"></param>
        /// <param name="builderContext">The operation specific context</param>
        /// <returns>A new QueryExpression or the original one</returns>
        protected virtual Piece Recurse(Piece piece,
                                        Func<Piece, BuilderContext, Piece> analyzer,
                                        Recursion recursion,
                                        BuilderContext builderContext)
        {
            // we first may replace the current expression
            Piece previousPiece;
            do
            {
                previousPiece = piece;

                if (recursion == Recursion.TopDown)
                    piece = analyzer(previousPiece, builderContext);

                // and then, eventually replace its children
                // important: evaluations are right to left, since parameters are pushed left to right
                // and lambda bodies are at first position
                for (int operandIndex = piece.Operands.Count - 1; operandIndex >= 0; operandIndex--)
                {
                    // the new child takes the original place
                    piece.Operands[operandIndex] = Recurse(
                        piece.Operands[operandIndex], analyzer, recursion, builderContext);
                }

                if (recursion == Recursion.DownTop)
                    piece = analyzer(previousPiece, builderContext);

                // the loop is repeated until there's nothing new
            } while (piece != previousPiece);
            return piece;
        }
    }
}
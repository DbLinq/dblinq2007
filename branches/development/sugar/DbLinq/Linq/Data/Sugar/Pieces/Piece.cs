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
using DbLinq.Linq.Data.Sugar.Pieces;

namespace DbLinq.Linq.Data.Sugar.Pieces
{
    public class Piece : IPieceEvaluationSource, IEquatable<Piece>
    {
        public IList<Piece> Operands { get; private set; }

        protected Piece()
        {
            Operands = new List<Piece>();
        }

        protected Piece(IList<Piece> operands)
        {
            Operands = operands;
        }

        #region IExpressionEvaluationSource Members

        PieceEvaluationSource IPieceEvaluationSource.GetEvaluationSource()
        {
            return new PieceEvaluationSource { EvaluatedPiece = this, IsEvaluationValid = true };
        }

        PieceEvaluationSource IPieceEvaluationSource.CloneEvaluationSource()
        {
            return new PieceEvaluationSource { EvaluatedPiece = this, IsEvaluationValid = true };
        }

        Piece IPieceEvaluationSource.EvaluatedPiece
        {
            get { return this; }
            set { throw new Exception("No dude. Not here."); }
        }

        bool IPieceEvaluationSource.IsEvaluationValid
        {
            get { return true; }
            set { throw new Exception("No dude. Not here."); }
        }

        #endregion

        #region IEquatable<Piece>

        public bool Equals(Piece other)
        {
            if (ReferenceEquals(this, other))
                return true;

            if (other == null)
                return false;

            if (GetType() != other.GetType())
                return false;

            if (!InnerEquals(other))
                return false;

            if (Operands.Count != other.Operands.Count)
                return false;

            for (int operandIndex = 0; operandIndex < Operands.Count; operandIndex++)
            {
                if (!Operands[operandIndex].Equals(other.Operands[operandIndex]))
                    return false;
            }

            return true;
        }

        protected virtual bool InnerEquals(Piece other)
        {
            return true;
        }

        #endregion
    }
}

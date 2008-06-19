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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;

#if MONO_STRICT
namespace System.Data.Linq
#else
namespace DbLinq.Data.Linq
#endif
{
    public class EntityMSet<T> : IQueryable<T>
                                 , IQueryProvider //new as of Beta2
    {

        /// <summary>
        /// 'S' is the projected type. If you say 'from e in Employees select e.ID', then type S will be int.
        /// If you say 'select new {e.ID}', then type S will be something like Projection.f__1
        /// </summary>
        public IQueryable<S> CreateQuery<S>(Expression expr)
        {
            return null;
        }

        public S Execute<S>(Expression expression)
        {
            throw new ApplicationException("L51 Not implemented");
        }

        /// <summary>
        /// entry point for 'foreach' statement.
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            throw new ApplicationException("Not impl");
            //SessionVars vars = GetVars();
            //IEnumerator<T> enumerator = new RowEnumerator<T>(vars);
            //return (IEnumerator<T>)enumerator;
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new ApplicationException("L68 Not implemented");
        }

        public Type ElementType
        {
            get
            {
                throw new ApplicationException("L75 Not implemented");
            }
        }

        public Expression Expression
        {
            //copied from RdfProvider
            get { return Expression.Constant(this); }
        }


        public IQueryable CreateQuery(Expression expression)
        {
            throw new ApplicationException("L88 Not implemented");
        }

        public object Execute(Expression expression)
        {
            throw new ApplicationException("L93 Not implemented");
        }

        /// <summary>
        /// TODO: Add(row)
        /// </summary>
        [Obsolete("NOT IMPLEMENTED YET")]
        public void Add(T t)
        {
            throw new NotImplementedException();
        }

        //New as of Orcas Beta2 - what does it do?
        public IQueryProvider Provider
        {
            get { return this; }
        }

    }

    public struct EntityMRef
    {
    }
}

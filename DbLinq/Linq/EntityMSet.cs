////////////////////////////////////////////////////////////////////
//Initial author: Jiri George Moudry, 2006.
//License: LGPL. (Visit http://www.gnu.org)
//Commercial code may call into this library, if it's in a different module (DLL)
////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;

namespace DBLinq.Linq
{
    public class EntityMSet<T> : IQueryable<T>
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
            throw new ApplicationException("Not implemented");
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
            throw new ApplicationException("Not implemented");
        }

        public Type ElementType { 
            get {
                throw new ApplicationException("Not implemented");
            }
        }
        public Expression Expression { 
            //copied from RdfProvider
            get { return Expression.Constant(this); }
        }
 

        public IQueryable CreateQuery(Expression expression)
        {
            throw new ApplicationException("Not implemented");
        }

        public object Execute(Expression expression)
        {
            throw new ApplicationException("Not implemented");
        }


    }

    public struct EntityMRef
    {
    }

}

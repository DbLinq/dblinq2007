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
using System.ComponentModel;
using DbLinq;

#if MONO_STRICT
namespace System.Data.Linq
#else
namespace DbLinq.Data.Linq
#endif
{
    public sealed class EntitySet<TEntity> : ICollection, ICollection<TEntity>, IEnumerable, IEnumerable<TEntity>, IList, IList<TEntity>, IListSource
        where TEntity : class
    {
        private Action<TEntity> onAdd;
        private Action<TEntity> onRemove;

        private IEnumerable<TEntity> Source;


        [DbLinqToDo]
        public EntitySet(Action<TEntity> onAdd, Action<TEntity> onRemove)
            : this()
        {
            this.onAdd = onAdd;
            this.onRemove = onRemove;
        }

        public EntitySet()
        {
            Source = System.Linq.Enumerable.Empty<TEntity>();
        }

        /// <summary>
        /// entry point for 'foreach' statement.
        /// </summary>
        public IEnumerator<TEntity> GetEnumerator()
        {
            return Source.GetEnumerator();
            //vars = GetVars();
            //IEnumerator<T> enumerator = new RowEnumerator<T>(vars);
            //return (IEnumerator<T>)enumerator;
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return Source.GetEnumerator();
        }



        internal Expression Expression
        {
            get
            {
                if (this.Source is IQueryable<TEntity>)
                    return (Source as IQueryable<TEntity>).Expression;
                else
                    return Expression.Constant(this);
            }
        }




        /// <summary>
        /// TODO: Add(row)
        /// </summary>
        [DbLinqToDo]
        public void Add(TEntity entity)
        {
            throw new NotImplementedException();
        }


        [DbLinqToDo]
        bool IListSource.ContainsListCollection
        {
            get { throw new NotImplementedException(); }
        }

        [DbLinqToDo]
        IList IListSource.GetList()
        {
            throw new NotImplementedException();
        }


        #region IList<TEntity> Members

        [DbLinqToDo]
        public int IndexOf(TEntity entity)
        {
            throw new NotImplementedException();
        }

        [DbLinqToDo]
        public void Insert(int index, TEntity entity)
        {
            throw new NotImplementedException();
        }

        [DbLinqToDo]
        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        [DbLinqToDo]
        public TEntity this[int index]
        {
            get
            {
                return Source.ElementAt(index);
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region ICollection<TEntity> Members


        [DbLinqToDo]
        public void Clear()
        {
            Source = Enumerable.Empty<TEntity>();
        }

        [DbLinqToDo]
        public bool Contains(TEntity entity)
        {
            return Source.Contains(entity);
        }

        [DbLinqToDo]
        public void CopyTo(TEntity[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        [DbLinqToDo]
        public int Count
        {
            get
            {
                return Source.Count();
            }
        }

        [DbLinqToDo]
        bool ICollection<TEntity>.IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        [DbLinqToDo]
        public bool Remove(TEntity entity)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IList Members

        [DbLinqToDo]
        int IList.Add(object value)
        {
            throw new NotImplementedException();
        }

        [DbLinqToDo]
        void IList.Clear()
        {
            this.Clear();
        }

        [DbLinqToDo]
        bool IList.Contains(object value)
        {
            if (value is TEntity)
                return this.Contains(value as TEntity);
            else
                return false;
        }

        [DbLinqToDo]
        int IList.IndexOf(object value)
        {
            throw new NotImplementedException();
        }

        [DbLinqToDo]
        void IList.Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

        [DbLinqToDo]
        bool IList.IsFixedSize
        {
            get { throw new NotImplementedException(); }
        }

        [DbLinqToDo]
        bool IList.IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        [DbLinqToDo]
        void IList.Remove(object value)
        {
            throw new NotImplementedException();
        }

        [DbLinqToDo]
        void IList.RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        [DbLinqToDo]
        object IList.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region ICollection Members

        [DbLinqToDo]
        void ICollection.CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        [DbLinqToDo]
        int ICollection.Count
        {
            get { throw new NotImplementedException(); }
        }

        [DbLinqToDo]
        bool ICollection.IsSynchronized
        {
            get { throw new NotImplementedException(); }
        }

        [DbLinqToDo]
        object ICollection.SyncRoot
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        [DbLinqToDo]
        public bool IsDeferred
        {
            get { throw new NotImplementedException(); }
        }

        [DbLinqToDo]
        public void AddRange(IEnumerable<TEntity> collection)
        {
            throw new NotImplementedException();
        }

        [DbLinqToDo]
        public void Assign(IEnumerable<TEntity> entitySource)
        {
            throw new NotImplementedException();
        }

        [DbLinqToDo]
        public void Load()
        {
            throw new NotImplementedException();
        }

        [DbLinqToDo]
        public void SetSource(IEnumerable<TEntity> entitySource)
        {
            this.Source = entitySource;
        }

        [DbLinqToDo]
        public bool HasLoadedOrAssignedValues
        {
            get { throw new NotImplementedException(); }
        }

        [DbLinqToDo]
        public IBindingList GetNewBindingList()
        {
            throw new NotImplementedException();
        }

        public event ListChangedEventHandler ListChanged;
    }
}

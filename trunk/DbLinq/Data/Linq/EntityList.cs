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

#if MONO_STRICT
namespace System.Data.Linq
#else
namespace DbLinq.Data.Linq
#endif
{
    /// <summary>
    /// The class helps keeping a list of entities, sorted by type, then by list (of same type)
    /// </summary>
    internal class EntityList
    {
        private readonly Hashtable _entitiesByType = new Hashtable();
        private readonly object _lock = new object();

        private IList GetList(Type t)
        {
            if (t == typeof(object))
                throw new ArgumentException("L00048: Type must not be object - You omitted the parameter");
            lock (_lock)
            {
                if (!_entitiesByType.Contains(t))
                    _entitiesByType[t] = new ArrayList();
                return (IList)_entitiesByType[t];
            }
        }

        public IEnumerable<object> EnumerateAll()
        {
            lock (_lock)
            {
                foreach (Type type in _entitiesByType)
                {
                    foreach (var o in (IEnumerable)_entitiesByType[type])
                    {
                        yield return o;
                    }
                }
            }
        }

        /// <summary>
        /// Enumerates all items with the same type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IEnumerable<T> Enumerate<T>()
        {
            lock (_lock)
            {
                foreach (T t in GetList(typeof(T)))
                    yield return t;
            }
        }

        /// <summary>
        /// Adds an item
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        public void Add<T>(T t)
        {
            Add(t, typeof(T));
        }

        public void Add(object e, Type asType)
        {
            lock (_lock)
            {
                var list = GetList(asType);
                if (!list.Contains(e))
                    list.Add(e);
            }
        }

        /// <summary>
        /// Removes an item (and don't care if missing)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        public void Remove<T>(T t)
        {
            Remove(t, typeof(T));
        }

        public void Remove(object e, Type asType)
        {
            lock (_lock)
            {
                GetList(asType).Remove(e);
            }
        }

        /// <summary>
        /// Removes items (and don't care if missing)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ts"></param>
        public void RemoveRange<T>(IEnumerable<T> ts)
        {
            lock (_lock)
            {
                var list = GetList(typeof(T));
                foreach (var t in ts)
                    list.Remove(t);
            }
        }

        /// <summary>
        /// Returns the number of items given a type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public int Count<T>()
        {
            lock (_lock)
            {
                return GetList(typeof(T)).Count;
            }
        }

        /// <summary>
        /// Determines if the given entity is registered
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        public bool Contains<T>(T t)
        {
            return Contains(t, typeof(T));
        }

        public bool Contains(object e, Type asType)
        {
            lock (_lock)
            {
                return GetList(asType).Contains(e);
            }
        }
    }
}

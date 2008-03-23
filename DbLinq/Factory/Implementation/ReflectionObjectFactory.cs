#region MIT license
////////////////////////////////////////////////////////////////////
// MIT license:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//
// Authors:
//        Jiri George Moudry
////////////////////////////////////////////////////////////////////
#endregion

using System;
using System.Collections.Generic;
using System.Reflection;

namespace DbLinq.Factory.Implementation
{
    /// <summary>
    /// Object factory. Main objects (most of them are stateless) are created with this class
    /// This may allow later to inject dependencies with a third party injector (I'm a Spring.NET big fan)
    /// </summary>
    public class ReflectionObjectFactory : IObjectFactory
    {
        private IDictionary<Type, IList<Type>> implementations = new Dictionary<Type, IList<Type>>();
        private IDictionary<Type, object> singletons = new Dictionary<Type, object>();

        public ReflectionObjectFactory()
        {
            Parse(Assembly.GetExecutingAssembly());
        }

        protected void Parse(Assembly assembly)
        {
            foreach (Type type in assembly.GetTypes())
            {
                if (type.IsAbstract)
                    continue;
                foreach (Type i in type.GetInterfaces())
                {
                    if (i.Assembly == assembly)
                    {
                        IList<Type> types;
                        if (!implementations.TryGetValue(i, out types))
                            implementations[i] = types = new List<Type>();
                        types.Add(type);
                    }
                }
            }
        }

        public T Get<T>()
        {
            object r;
            if (!singletons.TryGetValue(typeof(T), out r))
                singletons[typeof(T)] = r = Create<T>();
            return (T)r;
        }

        public T Create<T>()
        {
            IList<Type> types;
            if (!implementations.TryGetValue(typeof(T), out types))
                throw new ArgumentException(string.Format("Type '{0}' has no implementation", typeof(T)));
            if (types.Count > 1)
                throw new ArgumentException(string.Format("Type '{0}' has too many implementations", typeof(T)));
            return (T)Activator.CreateInstance(types[0]);
        }
    }
}

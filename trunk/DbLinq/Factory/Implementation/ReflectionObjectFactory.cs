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
using System.Reflection;
using DbLinq.Util;

namespace DbLinq.Factory.Implementation
{
    /// <summary>
    /// Object factory. Main objects (most of them are stateless) are created with this class
    /// This may allow later to inject dependencies with a third party injector (I'm a Spring.NET big fan)
    /// </summary>
    public class ReflectionObjectFactory : AbstractObjectFactory
    {
        private IDictionary<Type, IList<Type>> implementations = new Dictionary<Type, IList<Type>>();
        private IDictionary<Type, object> singletons = new Dictionary<Type, object>();

        public ReflectionObjectFactory()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
                Parse(assembly);
        }

        protected void Parse(Assembly assembly)
        {
            try
            {
                var assemblyTypes = assembly.GetTypes();
                foreach (Type type in assemblyTypes)
                {
                    if (type.IsAbstract)
                        continue;
                    foreach (Type i in type.GetInterfaces())
                    {
                        if (i.Assembly.GetCustomAttributes(typeof(DbLinqAttribute), false).Length > 0)
                        {
                            IList<Type> types;
                            if (!implementations.TryGetValue(i, out types))
                                implementations[i] = types = new List<Type>();
                            types.Add(type);
                        }
                    }
                }
            }
            catch (ReflectionTypeLoadException)
            {
            }
        }

        private object GetSingleton(Type t)
        {
            object r;
            if (!singletons.TryGetValue(t, out r))
                singletons[t] = r = GetNewInstance(t);
            return r;
        }

        private object GetNewInstance(Type t)
        {
            if (t.IsInterface)
            {
                IList<Type> types;
                if (!implementations.TryGetValue(t, out types))
                    throw new ArgumentException(string.Format("Type '{0}' has no implementation", t));
                if (types.Count > 1)
                    throw new ArgumentException(string.Format("Type '{0}' has too many implementations", t));
                return Activator.CreateInstance(types[0]);
            }
            else
            {
                return Activator.CreateInstance(t);
            }
        }

        public override object GetInstance(Type t, bool newInstanceRequired)
        {
            if (newInstanceRequired)
                return GetNewInstance(t);
            return GetSingleton(t);
        }

        public override IEnumerable<Type> GetImplementations(Type interfaceType)
        {
            return implementations[interfaceType];
        }
    }
}

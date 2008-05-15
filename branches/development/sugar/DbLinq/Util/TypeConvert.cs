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
using System.Reflection;

namespace DbLinq.Util
{
    /// <summary>
    /// Types conversion.
    /// A "smart" extension to System.Convert (at least that's what we hope)
    /// </summary>
    public static class TypeConvert
    {
        public static object ToNumber(object o, Type numberType)
        {
            if (o.GetType() == numberType)
                return o;
            string methodName = string.Format("To{0}", numberType.Name);
            MethodInfo convertMethod = typeof(Convert).GetMethod(methodName, new[] { o.GetType() });
            if (convertMethod != null)
                return convertMethod.Invoke(null, new[] { o });
            throw new InvalidCastException(string.Format("Can't convert type {0} in Convert.{1}()", o.GetType().Name, methodName));
        }

        public static U ToNumber<U>(object o)
        {
            return (U)ToNumber(o, typeof(U));
        }

        /// <summary>
        /// Returns the default value for a specified type.
        /// Reflection equivalent of default(T)
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static object GetDefault(Type t)
        {
            if (!t.IsValueType)
                return null;
            return Activator.CreateInstance(t);
        }

        /// <summary>
        /// Converts a value to an enum
        /// (work with literals string values and numbers)
        /// </summary>
        /// <param name="o">The literal to convert</param>
        /// <param name="enumType">The target enum type</param>
        /// <returns></returns>
        public static int ToEnum(object o, Type enumType)
        {
            var e = (int)Enum.Parse(enumType, o.ToString());
            return e;
        }

        public static E ToEnum<E>(object o)
        {
            return (E)(object)ToEnum(o, typeof(E));
        }
    }
}

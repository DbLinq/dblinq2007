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
using System.Collections.Generic;
using System.Text;

namespace DbLinq.Util
{
    /// <summary>
    /// TypeEnum: is a type a primitive type, a DB column, or a projection?
    /// Call CSharp.GetCategory(T) to examine a type.
    /// </summary>
    public enum TypeCategory
    {
        /// <summary>
        /// specifies builtin type, eg. a string or uint.
        /// </summary>
        Primitive,

        /// <summary>
        /// specifies DB Columns (entities) marked up with [Table] attribute.
        /// This type, when retrieved, must go into liveObjectCache
        /// </summary>
        Column,

        /// <summary>
        /// anything else, eg. projection types
        /// </summary>
        Other
    }
}

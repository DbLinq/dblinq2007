////////////////////////////////////////////////////////////////////
//Initial author: Jiri George Moudry, 2006.
//License: LGPL. (Visit http://www.gnu.org)
//Commercial code may call into this library, if it's in a different module (DLL)
////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

namespace DBLinq.util
{
    /// <summary>
    /// TypeEnum: is a type a primitive type, a DB column, or a projection?
    /// Call CSharp.CategorizeType(T) to examine a type.
    /// </summary>
    public enum TypeEnum 
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

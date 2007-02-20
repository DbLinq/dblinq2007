using System;
using System.Collections.Generic;
using System.Text;

namespace DBLinq.util
{
    /// <summary>
    /// TypeEnum: is a type a primitive type, a DB column, or a projection?
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

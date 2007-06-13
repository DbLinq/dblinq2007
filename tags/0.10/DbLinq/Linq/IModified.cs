////////////////////////////////////////////////////////////////////
//Initial author: Jiri George Moudry, 2006.
//License: LGPL. (Visit http://www.gnu.org)
//Commercial code may call into this library, if it's in a different module (DLL)
////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

namespace DBLinq.Linq
{
    /// <summary>
    /// Simple interface, which allows to query which business objects have been modified.
    /// Only objects with primary keys implement this interface.
    /// </summary>
    public interface IModified
    {
        /// <summary>
        /// after object is saved to database, 
        /// Linq sets it's IsModified status to false.
        /// </summary>
        bool IsModified { get; set; }

        /// <summary>
        /// if an object was modifed, Linq updates the row specified by this Primary Key.
        /// </summary>
        //object PrimaryKeyValue { get; }
    }
}

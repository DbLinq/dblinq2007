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
    /// MySqlMetal will mark a field such as 'private int productID'
    /// with this attribute.
    /// Linq then knows to populate this field after a sql insert.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class AutoGenIdAttribute : Attribute
    {
    }
}

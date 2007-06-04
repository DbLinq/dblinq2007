////////////////////////////////////////////////////////////////////
//Initial author: Jiri George Moudry, 2006.
//License: LGPL. (Visit http://www.gnu.org)
//Commercial code may call into this library, if it's in a different module (DLL)
////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
#if LINQ_PREVIEW_2006
//Visual Studio 2005 with Linq Preview May 2006 - can run on Win2000
using System.Data.DLinq;
#else
//Visual Studio Orcas - requires WinXP
using System.Data.Linq;
#endif

namespace DBLinq.util
{
    /// <summary>
    /// data needed to find reverse association attribute 
    /// and build a 'join' SQL statement.
    /// </summary>
    public class ReverseAssociation
    {
        /// <summary>
        /// sample parent-child link between Customers and Orders: 
        /// 'c=o.Customer'
        /// </summary>
        public class Part
        {
            /// <summary>
            /// 'c' for the parent table
            /// </summary>
            public string varName;

            /// <summary>
            /// our retrieved attribute [Table(Name='customers')]
            /// </summary>
            public TableAttribute tableAttrib;

            public AssociationAttribute assocAttrib;

            public System.Reflection.PropertyInfo propInfo;
        }

        public readonly Part child = new Part();

        public readonly Part parent = new Part();
    }
}

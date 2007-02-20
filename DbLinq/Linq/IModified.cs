using System;
using System.Collections.Generic;
using System.Text;

namespace DBLinq.Linq
{
    public interface IModified
    {
        /// <summary>
        /// after object is saved to database, 
        /// Linq sets it's IsModified status to false.
        /// </summary>
        bool IsModified { get; set; }
    }
}

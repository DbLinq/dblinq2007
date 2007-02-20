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

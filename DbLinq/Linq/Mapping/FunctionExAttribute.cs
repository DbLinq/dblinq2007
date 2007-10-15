using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DBLinq.Linq.Mapping
{
    /// <summary>
    /// This is an 'extension' of Microsoft's [Linq.Mapping.Function] attribute.
    /// We need one extra field to indicate type of code - ProcOrFunction.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class FunctionExAttribute : Attribute
    {
        public string Name { get; set; }

        /// <summary>
        /// needed because MySql semantics of invoking a func are different from a proc.
        /// </summary>
        public string ProcedureOrFunction { get; set; }
    }

}

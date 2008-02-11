using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DBLinq.util
{
    /// <summary>
    /// used in Concat function - we are concatenating a list of expressions,
    /// and we need to know their types
    /// </summary>
    public class ExpressionAndType
    {
        public string expression;

        public Type type;
    }
}

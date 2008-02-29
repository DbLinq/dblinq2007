using System;

namespace DbLinq.Linq
{
    public class LinqEventArgs: EventArgs
    {
        public SessionVarsParsed SessionVarsParsed { get; set; }
    }
}

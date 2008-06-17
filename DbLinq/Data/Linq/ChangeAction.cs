using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if MONO_STRICT
namespace System.Data.Linq
{
    public enum ChangeAction
    {
        Delete, Insert, None, Update
    }
}
#endif

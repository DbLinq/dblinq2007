using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if MONO_STRICT
namespace System.Data.Linq
{
    public interface IExecuteResult
    {
        object GetParameterValue(int parameterIndex);
        object ReturnValue { get; }
    }
}
#endif

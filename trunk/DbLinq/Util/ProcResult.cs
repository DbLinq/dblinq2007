using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DbLinq.Util
{
    /// <summary>
    /// holds result of a stored proc call.
    /// </summary>
    public class ProcResult : System.Data.Linq.IExecuteResult
    {
        object[] outParamValues;

        public object GetParameterValue(int parameterIndex)
        {
            object value = outParamValues[parameterIndex];
            return value;
        }

        public object ReturnValue { get; set; }

        public void Dispose() { }

        public ProcResult(object retVal, object[] outParamValues_)
        {
            ReturnValue = retVal;
            outParamValues = outParamValues_;
        }
    }
}

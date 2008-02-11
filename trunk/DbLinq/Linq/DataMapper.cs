using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DBLinq.util;
using DBLinq.vendor;

namespace DBLinq.Linq
{
    public class DataMapper: IDataMapper
    {
        public Func<IDataReader2, T> GetMapper<T>(SessionVarsParsed vars)
        {
            int fieldID = 0;
            return RowEnumeratorCompiler<T>.CompileRowDelegate(vars, ref fieldID);
        }
    }
}

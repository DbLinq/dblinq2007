using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbLinq.Vendor;

namespace DbLinq.Linq
{
    public interface IDataMapper
    {
        Func<IDataReader2, T> GetMapper<T>(SessionVarsParsed vars);
    }
}

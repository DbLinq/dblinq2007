using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DbLinq.Vendor;

namespace DbLinq.Linq
{
    public interface IDataMapper
    {
        Func<IDataRecord, T> GetMapper<T>(SessionVarsParsed vars);
    }
}

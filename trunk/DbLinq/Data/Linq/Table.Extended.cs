using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if MONO_STRICT
namespace System.Data.Linq
#else
namespace DbLinq.Data.Linq
#endif
{
    /// <summary>
    /// T may be eg. class Employee or string - the output
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public partial class Table<T>
    {
        public void CancelDeleteOnSubmit(T entity)
        {
            
        }

        void ITable.CancelDeleteOnSubmit(object entity)
        {

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbLinq.Logging;
using DbLinq.Data.Linq.Sugar;
using System.Data.Linq;

namespace DbLinq.Data.Linq
{
    /// <summary>
    /// T may be eg. class Employee or string - the output
    /// </summary>
    /// <typeparam name="T"></typeparam>
    partial class Table<T>
    {
        public ILogger Logger { get { return _Logger; } set { _Logger = value; } }
        public void CancelDeleteOnSubmit(T entity)
        {
            
        }

        void ITable.CancelDeleteOnSubmit(object entity)
        {

        }

        protected virtual void Process(IEnumerable<T> ts, Action<T, QueryContext> process, ConflictMode failureMode,
            IList<Exception> exceptions)
        {
            this._Process(ts, process, failureMode, exceptions);
        }
    }
}

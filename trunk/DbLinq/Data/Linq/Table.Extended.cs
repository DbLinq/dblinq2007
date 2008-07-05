﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbLinq.Logging;
using DbLinq.Data.Linq.Sugar;
using System.Data.Linq;
using System.Linq.Expressions;

namespace DbLinq.Data.Linq
{
    /// <summary>
    /// T may be eg. class Employee or string - the output
    /// </summary>
    /// <typeparam name="T"></typeparam>
    partial class Table<TEntity>
    {
        public ILogger Logger { get { return _Logger; } set { _Logger = value; } }
        public void CancelDeleteOnSubmit(TEntity entity)
        {

        }

        void ITable.CancelDeleteOnSubmit(object entity)
        {

        }

        protected void Process(IEnumerable<TEntity> ts, Action<TEntity, QueryContext> process, ConflictMode failureMode,
            IList<Exception> exceptions)
        {
            this._Process(ts, process, failureMode, exceptions);
        }

        protected void ProcessInsert(ConflictMode failureMode, IList<Exception> exceptions)
        {
            this._ProcessInsert(failureMode, exceptions);
        }


        protected void ProcessUpdate(ConflictMode failureMode, List<Exception> exceptions)
        {
            this._ProcessUpdate(failureMode, exceptions);
        }

        protected void ProcessDelete(ConflictMode failureMode, List<Exception> exceptions)
        {
            this._ProcessDelete(failureMode, exceptions);
        }
    }
}

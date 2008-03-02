#region MIT license
////////////////////////////////////////////////////////////////////
// MIT license:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//
// Authors:
//        Jiri George Moudry
////////////////////////////////////////////////////////////////////
#endregion

#define BEGIN_TRANSACTION

using System;
using System.Data;
using System.Diagnostics;

namespace DbLinq.Util
{
    /// <summary>
    /// helps us to automatically commit/reject transaction at end of SubmitChanges().
    /// </summary>
    public class TransactionManager : IDisposable
    {
        private IDbConnection _conn;
        private System.Data.Linq.ConflictMode _failureMode;
#if BEGIN_TRANSACTION
        private readonly bool _top; // set to true if this is the first TransactionManager (we support nesting)
        private bool _didCommit;
        private System.Data.Common.DbTransaction _transaction;

        [ThreadStatic]
        private static TransactionManager _current;

        public IDbTransaction Transaction { get { return _transaction; } }

        public static void CheckCommandTransaction(IDbCommand command)
        {
            if (_current != null && command.Transaction == null)
                command.Transaction = _current.Transaction;
        }
#endif

        public TransactionManager(IDbConnection conn, System.Data.Linq.ConflictMode failureMode)
        {
            if (_current != null)
            {
                _top = false;
                return;
            }
            _top = true;

            _conn = conn;
            _failureMode = failureMode;

            //the transactions, as they are written below, currently fail on PostgreSql and Oracle.
#if BEGIN_TRANSACTION
            Trace.WriteLine("TranMgr.BeginTransation");
            IDbTransaction transaction1 = _conn.BeginTransaction();
            _transaction = transaction1 as System.Data.Common.DbTransaction;

            _current = this;
#endif
        }

        public void Commit()
        {
#if BEGIN_TRANSACTION
            if (!_top)
                return;

            _didCommit = true;
            Trace.WriteLine("TranMgr.Commit");
            _transaction.Commit(); //on PostgreSql, gives "No transaction in progress"
#endif
        }

        public void Dispose()
        {
#if BEGIN_TRANSACTION
            if (!_top)
                return;

            _current = null;

            if (!_didCommit)
            {
                Trace.WriteLine("TranMgr.Rollback");
                _transaction.Rollback();
            }
            Trace.WriteLine("TranMgr.Dispose");
            _transaction.Dispose();
#endif
        }
    }
}

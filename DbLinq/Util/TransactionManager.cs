using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DBLinq.Util
{
    /// <summary>
    /// helps us to automatically commit/reject transaction at end of SubmitChanges().
    /// </summary>
    public class TransactionManager : IDisposable
    {
        IDbConnection _conn;
        System.Data.Linq.ConflictMode _failureMode;
#if BEGIN_TRANSACTION
        bool _didCommit;
        System.Data.Common.DbTransaction _transaction;
#endif

        public TransactionManager(IDbConnection conn, System.Data.Linq.ConflictMode failureMode)
        {
            _conn = conn;
            _failureMode = failureMode;

            //the transactions, as they are written below, currently fail on PostgreSql and Oracle.
#if BEGIN_TRANSACTION
            IDbTransaction transaction1 = _conn.BeginTransaction();
            _transaction = transaction1 as System.Data.Common.DbTransaction;
#endif
        }

        public void Commit()
        {
#if BEGIN_TRANSACTION
            _didCommit = true;
            _transaction.Commit(); //on PostgreSql, gives "No transaction in progress"
#endif
        }

        public void Dispose()
        {
#if BEGIN_TRANSACTION
            if (!_didCommit)
            {
                _transaction.Rollback();
            }
            _transaction.Dispose();
#endif
        }
    }
}

using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DBLinq.Util
{
    /// <summary>
    /// if a connection is initially closed, ConnectionManager closes it in Dispose().
    /// if a connection is initially open, ConnectionManager does nothing.
    /// </summary>
    public class ConnectionManager : IDisposable
    {
        readonly IDbConnection _conn;
        readonly bool _mustCloseConnection;

        public ConnectionManager(IDbConnection conn)
        {
            _conn = conn;

            switch (conn.State)
            {
                case System.Data.ConnectionState.Open:
                    _mustCloseConnection = false;
                    break;
                case System.Data.ConnectionState.Closed:
                    _mustCloseConnection = true;
                    conn.Open();
                    break;
                default:
                    throw new ApplicationException("L33: Can only handle Open or Closed connection states, not " + conn.State);
            }
        }

        public void Dispose()
        {
            if (_mustCloseConnection)
            {
                try { _conn.Close(); }
                catch { }
            }
        }
    }
}

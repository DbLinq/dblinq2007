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

using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DbLinq.Util
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

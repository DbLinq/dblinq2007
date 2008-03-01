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

using System.Collections.Generic;
using System.Data;
using DbLinq.Util;

namespace DbLinq.MySql.Schema
{
    /// <summary>
    /// represents one row from MySQL's MYSQL.PROC table
    /// </summary>
    public class ProcRow
    {
        public string db;
        public string name;
        public string type;
        public string specific_name;
        public string param_list;
        public string returns;
        public string body;

        public override string ToString()
        {
            return "ProcRow " + name;
        }
    }

    /// <summary>
    /// class for reading from 'MYSQL.PROC'.
    /// We use mysql.PROC instead of information_schema.ROUTINES, because it saves us parsing of parameters.
    /// Note: higher permissions are required to access mysql.PROC.
    /// </summary>
    class ProcSql
    {
        ProcRow fromRow(IDataReader rdr)
        {
            ProcRow p = new ProcRow();
            int field = 0;
            p.db = rdr.GetString(field++);
            p.name = rdr.GetString(field++);
            p.type = rdr.GetString(field++);
            p.specific_name = rdr.GetString(field++);
            
            object oo = rdr.GetFieldType(field);
            p.param_list = rdr.GetString(field++);
            p.returns = rdr.GetString(field++);
            p.body = rdr.GetString(field++);
            return p;
        }

        public List<ProcRow> getProcs(IDbConnection conn, string db)
        {
            string sql = @"
SELECT db, name, type, specific_name, param_list, returns, body
FROM mysql.proc
WHERE db=?db AND type IN ('FUNCTION','PROCEDURE')";

            return DataCommand.Find<ProcRow>(conn, sql, "?db", db.ToLower(), fromRow);
        }
    }
}

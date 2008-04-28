#region MIT license
// 
// Copyright (c) 2007-2008 Jiri Moudry
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using DbLinq.Util;

namespace DbLinq.Sqlite.Schema
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
            p.db = rdr.GetAsString(field++);
            p.name = rdr.GetAsString(field++);
            p.type = rdr.GetAsString(field++);
            p.specific_name = rdr.GetAsString(field++);
            
            p.param_list = rdr.GetAsString(field++);
            p.returns = rdr.GetAsString(field++);
            p.body = rdr.GetAsString(field++);
            return p;
        }

        public List<ProcRow> getProcs(IDbConnection conn, string db)
        {
            // No function or stored procedure in SQLite
//            string sql = @"
//SELECT db, name, type, specific_name, param_list, returns, body
//FROM mysql.proc
//WHERE db=?db AND type IN ('FUNCTION','PROCEDURE')";

//            using (SQLiteCommand cmd = new SQLiteCommand(sql, conn))
//            {
//                cmd.Parameters.Add("?db", db.ToLower());
//                using (SQLiteDataReader rdr = cmd.ExecuteReader())
//                {
//                    List<ProcRow> list = new List<ProcRow>();
//                    while (rdr.Read())
//                    {
//                        list.Add(fromRow(rdr));
//                    }
//                    return list;
//                }
//            }
            //return null;
            return new List<ProcRow>();
        }
    }
}
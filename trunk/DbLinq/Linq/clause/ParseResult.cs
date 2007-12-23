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
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using DBLinq.vendor;
using DBLinq.util;

namespace DBLinq.Linq.clause
{
    /// <summary>
    /// holds strings parsed out of an expression tree.
    /// The outside user may afterward costruct a SELECT, WHERE, or any other clause with them.
    /// </summary>
    public class ParseResult
    {
        public List<string> joins = new List<string>();
        readonly StringBuilder sb = new StringBuilder(200);
        public List<string> columns = new List<string>();

        public ParseResult()
        {
        }

        public void addJoin(string joinStr)
        {
            if( ! joins.Contains(joinStr) )
                joins.Add(joinStr);
        }
        public readonly Dictionary<Type,string> tablesUsed = new Dictionary<Type,string>();
        public int MarkSbPosition(){ return sb.Length; }
        public string Substring(int markedPos)
        { 
            string full=sb.ToString(); return full.Substring(markedPos,full.Length-markedPos); 
        }
        public void Revert(int markedPos){ sb.Length=markedPos; }

        /// <summary>
        /// during building, append to our internal StringBuilder
        /// </summary>
        /// <param name="expr"></param>
        public void AppendString(string columnString)
        {
            if(columnString==",")
            {
                //end of prev columnString - user should really call EndField()
                columns.Add(sb.ToString());
                sb.Length=0;
                return;
            }
            sb.Append(columnString);
        }

        /// <summary>
        /// transfer params and tablesUsed, but not StringBuilder
        /// </summary>
        /// <param name="sqlParts"></param>
        public void CopyInto(QueryProcessor qp, SqlExpressionParts sqlParts)
        {
            //sqlParts.whereList.Add( this.sb.ToString());
            foreach(string key in qp.paramMap.Keys)
            {
                //sqlParts.paramMap.Add(key, this.paramMap[key]);
                sqlParts.paramMap[key] = qp.paramMap[key];
            }

            foreach (var t1 in tablesUsed)
            {
                TableAttribute tAttrib = AttribHelper.GetTableAttrib(t1.Key);
                if(tAttrib!=null)
                {
                    string fromClause = tAttrib.Name + " " + RemoveTransparentId(t1.Value);

                    sqlParts.AddFrom(fromClause);
                }
            }
            foreach(string joinStr in joins)
            {
                sqlParts.joinList.Add(joinStr);
            }
        }

        static readonly System.Text.RegularExpressions.Regex s_regexTransparentID = new System.Text.RegularExpressions.Regex(
            @"<>h__TransparentIdentifier\d+\$\.(.*)\.(.*)");

        /// <summary>
        /// shorten '<>h__TransparentIdentifier10$.c.City' into 'c$.City'
        /// </summary>
        string RemoveTransparentId(string name)
        {
            System.Text.RegularExpressions.Match m = s_regexTransparentID.Match(name);
            if (m.Success)
            {
                return m.Groups[1].Value + "$." + m.Groups[2].Value;
            }
            return name;
        }

        public void EndField()
        {
            if(this.sb.Length>0)
            {
                columns.Add(sb.ToString());
                sb.Length = 0;
            }
        }

    }
}

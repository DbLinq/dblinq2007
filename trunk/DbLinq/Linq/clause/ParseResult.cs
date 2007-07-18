////////////////////////////////////////////////////////////////////
//Initial author: Jiri George Moudry, 2006.
//License: LGPL. (Visit http://www.gnu.org)
//Commercial code may call into this library, if it's in a different module (DLL)
////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Data.Linq;
using DBLinq.vendor;
using DBLinq.util;

namespace DBLinq.Linq.clause
{
    public class ParseResult
    {
        public readonly Dictionary<string,object> paramMap; // = new Dictionary<string,object>();
        public string lastParamName;
        public List<string> joins = new List<string>();
        readonly StringBuilder sb = new StringBuilder(200);
        public List<string> columns = new List<string>();
        public readonly Dictionary<MemberExpression,string> memberExprNickames = new Dictionary<MemberExpression,string>();

        public ParseResult(ParseInputs input)
        {
            paramMap = input!=null
                ? input.paramMap
                : new Dictionary<string,object>();
        }

        public string storeParam(string value)
        { 
            int count = paramMap.Count;
            string paramName = Vendor.ParamName(count);
            paramMap[paramName] = value;
            lastParamName = paramName;
            return paramName;
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
            //if(columnString.Contains(",")) //OK - e.g. "CONCAT(p$.ProductName,p$.ProductID)"
            //    throw new Exception("L62 OOOPSW");
            sb.Append(columnString);
        }

        /// <summary>
        /// transfer params and tablesUsed, but not StringBuilder
        /// </summary>
        /// <param name="sqlParts"></param>
        public void CopyInto(SqlExpressionParts sqlParts)
        {
            //sqlParts.whereList.Add( this.sb.ToString());
            foreach(string key in this.paramMap.Keys)
            {
                //sqlParts.paramMap.Add(key, this.paramMap[key]);
                sqlParts.paramMap[key] = paramMap[key];
            }
            foreach(var t1 in tablesUsed)
            {
                TableAttribute tAttrib = AttribHelper.GetTableAttrib(t1.Key);
                if(tAttrib!=null)
                {
                    string fromClause = tAttrib.Name+" "+t1.Value;
                    sqlParts.AddFrom(fromClause);
                }
            }
            foreach(string joinStr in joins)
            {
                sqlParts.joinList.Add(joinStr);
            }
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

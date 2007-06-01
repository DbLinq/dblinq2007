////////////////////////////////////////////////////////////////////
//Initial author: Jiri George Moudry, 2006.
//License: LGPL. (Visit http://www.gnu.org)
//Commercial code may call into this library, if it's in a different module (DLL)
////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

namespace DBLinq.Linq.clause
{
    /// <summary>
    /// Object which holds the pieces of a SELECT as it's being put together.
    /// </summary>
    public class SqlExpressionParts //: ICloneable
    {
        static int s_serial = 0;
        int _serial;

        /// <summary>
        /// eg. {'Customers $c','Orders $o'}
        /// </summary>
        private readonly List<string> fromTableList = new List<string>();

        /// <summary>
        /// eg. {'$c.CustomerID','$o.Quantity'}
        /// </summary>
        readonly List<string> selectFieldList = new List<string>();

        /// <summary>
        /// eg. {'$c.CustomerID==$o.CustomerID', ...}
        /// </summary>
        public readonly List<string> joinList = new List<string>();

        /// <summary>
        /// keep track of which nickNames were already added via GetClause(), eg. {'$c','$o'}.
        /// Currently, author is not sure what's the best way to prevent duplicate addition.
        /// </summary>
        public readonly List<string> doneClauses = new List<string>();
        
        /// <summary>
        /// add 'Employee $e' FROM clause
        /// </summary>
        /// <param name="fromTable"></param>
        public void AddFrom(string fromTable)
        {
            string fromLower = fromTable.ToLower();
            if(fromTableList.Contains(fromLower))
                return; //prevent dupes
            fromTableList.Add(fromLower);
        }

        public void AddSelect(string column)
        {
            selectFieldList.Add(column);
        }
        public void AddSelect(List<string> columns)
        {
            selectFieldList.AddRange(columns);
        }
        public string GetSelect()
        {
            string joined = string.Join(",", selectFieldList.ToArray());
            return joined;
        }

        /// <summary>
        /// eg. {'$c.City==?P0'}
        /// </summary>
        private readonly List<string> whereList = new List<string>();

        public readonly List<string> groupByList = new List<string>();

        public string           countClause;
        public string           distinctClause;

        /// <summary>
        /// parameters, eg {'?P0'=>'London'}
        /// </summary>
        public readonly Dictionary<string,object> paramMap = new Dictionary<string,object>();

        public SqlExpressionParts()
        {
            _serial = s_serial++;
        }
        public SqlExpressionParts(SqlExpressionParts orig)
        {
            _serial = s_serial++;
            fromTableList = new List<string>(orig.fromTableList);
            selectFieldList = new List<string>(orig.selectFieldList);
            joinList = new List<string>(orig.joinList);
            doneClauses = new List<string>(orig.doneClauses);
            whereList = new List<string>(orig.whereList);
            groupByList = new List<string>(orig.groupByList);
            countClause = orig.countClause;
            distinctClause = orig.distinctClause;
            paramMap = new Dictionary<string,object>(orig.paramMap);
        }

        public void AddWhere(string sqlExpr)
        {
            this.whereList.Add(sqlExpr);
        }
        public void AddWhere(List<string> sqlExprs)
        {
            this.whereList.AddRange(sqlExprs);
        }

        #region ToString() produces a full SQL statement from parts
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(500);
            sb.Append("SELECT ");
            if(this.distinctClause!=null)
            {
                //SELECT COUNT(ProductID) FROM ...
                sb.Append(distinctClause).Append(" ");
            }

            if(this.countClause!=null)
            {
                //SELECT COUNT(ProductID) FROM ...
                sb.Append(countClause).Append("(")
                    .Append(selectFieldList[0]).Append(")");
            }
            else
            {
                //normal (non-count) select
                string opt_comma = "";
                foreach(string s in selectFieldList)
                {
                    sb.Append(opt_comma).Append(s);
                    opt_comma = ", ";
                }
            }
            //if(sb.Length>80){ sb.Append("\n"); } //for legibility, append a newline for long expressions
            sb.Append("\n FROM ");
            {
                string opt_comma = "";
                foreach(string s in fromTableList)
                {
                    sb.Append(opt_comma).Append(s);
                    opt_comma = ", ";
                }
            }

            //MySql docs for JOIN:
            //http://dev.mysql.com/doc/refman/4.1/en/join.html
            //for now, we will not be using the JOIN keyword
            List<string> whereAndjoins = new List<string>(whereList);
            whereAndjoins.AddRange(joinList);

            if(whereAndjoins.Count>0)
            {
                sb.Append("\n WHERE ");
                string separator = "";
                foreach(string s in whereAndjoins)
                {
                    sb.Append(separator).Append(s);
                    separator = " AND ";
                }
            }

            if(groupByList.Count>0)
            {
                sb.Append("\n GROUP BY ");
                string separator = "";
                foreach(string grp in groupByList)
                {
                    sb.Append(separator).Append(grp);
                    separator = ", ";
                }
            }

            return sb.ToString();
        }
        #endregion
    
        public bool IsEmpty()
        {
            return selectFieldList.Count==0;
        }

        public SqlExpressionParts Clone()
        {
            //clone._serial = s_serial++;
            return new SqlExpressionParts(this);
        }
    }
}

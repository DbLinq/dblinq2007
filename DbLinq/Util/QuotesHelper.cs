using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DbLinq.Util
{
    /// <summary>
    /// maintainer: Anton Andreev
    /// </summary>
    public static class QuotesHelper
    {
        public static void AddQuotesToQuery(IDbCommand cmd)
        {
            if (cmd.ToString().StartsWith("Npgsql"))
            {
                cmd.CommandText = Regex.Replace(cmd.CommandText, "\\.[^\\s,\\n]*", AddQuotes1); //select columns
                cmd.CommandText = Regex.Replace(cmd.CommandText, "\\([^=]*=[^(]+\\)", AddQuotes2); //in the where clauses
            }
        }

        public static string AddQuotesToSequence(string idColName, string sequenceName)
        {
            if (idColName != idColName.ToLower() && !sequenceName.StartsWith("\""))//toncho11: quotes are added due to issue http://code.google.com/p/dblinq2007/issues/detail?id=27}
            {
                sequenceName = "\"" + sequenceName.Replace(".", "\".\"") + "\"";
            }
            return sequenceName;
        }

        public static string AddQuotes(string name)
        {
            return "\""+name+"\"";
        }

        private static string AddQuotes1(Match m)
        {
            // Get the matched string.
            string x = m.ToString();

            // If it is NOT lower case and quotes are no already there
            if (x != x.ToLower() && !x.StartsWith("\""))
                x = ".\"" + x.Substring(1) + "\"";

            return x;
        }

        private static string AddQuotes2(Match m)
        {
            // Get the matched string.
            string x = m.ToString();

            // If it is NOT lower case and quotes are no already there
            if (x != x.ToLower() && !x.StartsWith("\""))
            {
                x = "(\"" + x.Substring(1);
                x = x.Replace("=", "\"=");
            }

            return x;
        }
    }
}

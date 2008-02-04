using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DBLinq.vendor
{
    public class Settings
    {
        /// <summary>
        /// allows you to pre-pend custom SQL string before any SELECT or INSERT statement.
        /// e.g. in PostgreSQL, you can use it to choose a schema within database:
        /// 'SET SEARCH_PATH TO public,Company1;'
        /// </summary>
        public static string sqlStatementProlog = "";

        /// <summary>
        /// True if trailing spaces from retrieved string columns
        /// are removed.
        /// </summary>
        public static bool TrimEnd = false;
    }
}

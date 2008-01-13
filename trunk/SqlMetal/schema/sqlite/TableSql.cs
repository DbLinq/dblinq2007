using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SQLite;

namespace SqlMetal.schema.sqlite
{
        /// <summary>
    /// represents one row from SQLite's information_schema.`TABLES` table
    /// </summary>
    public class TableRow
    {
        public string table_catalog;
        public string table_schema;
        public string table_name;

        /// <summary>
        /// dependencies are determined by analyzing foreign keys.
        /// </summary>
        public readonly List<TableRow> childTables = new List<TableRow>();

        public IEnumerable<TableRow> EnumChildTables(Dictionary<TableRow,bool> visitedMap)
        {
            //if (depth > 99)
            //{
            //    //prevent infinite recursion, in case of circular dependency
            //    throw new ApplicationException("L26 Circular dependency suspected");
            //}

            foreach (TableRow t in childTables)
            {
                if (t == this)
                    continue; //in Northwind database: Employee.ReportsTo points back to Employee - skip that child relationship

                if (visitedMap.ContainsKey(t))
                    continue; //prevent infinite recursion - don't yield twice 

                visitedMap[t] = true;
                yield return t;

                foreach (TableRow t2 in t.EnumChildTables(visitedMap))
                {
                    if (visitedMap.ContainsKey(t2))
                        continue; //prevent infinite recursion - don't yield twice 

                    visitedMap[t2] = true;
                    yield return t2;
                }
            }
        }

        public override string ToString()
        {
            return "TableRow " + table_schema + "." + table_name + "  child:" + childTables.Count;
        }
    }

    /// <summary>
    /// class for reading from "information_schema.`TABLES`"
    /// </summary>
    class TableSql
    {
        TableRow fromRow(SQLiteDataReader rdr)
        {
            TableRow t = new TableRow();
            int field = 0;
            t.table_catalog = "SQLite";
            t.table_schema  = "main";
            t.table_name    = rdr.GetString(field++);
            return t;
        }

        public List<TableRow> getTables(SQLiteConnection conn, string db)
        {
        	// As there is no foreign key, we are sorting table by name
            string sql = @" SELECT tbl_name FROM sqlite_master WHERE type='table' order by tbl_name";

            using (SQLiteCommand cmd = new SQLiteCommand(sql, conn))
            {
                //cmd.Parameters.Add("?db", db);
                using (SQLiteDataReader rdr = cmd.ExecuteReader())
                {
                    List<TableRow> list = new List<TableRow>();
                    while(rdr.Read())
                    {
                        list.Add( fromRow(rdr) );
                    }
                    return list;
                }
            }

        }
    }
}

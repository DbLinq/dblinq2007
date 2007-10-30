using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;

namespace SqlMetal.schema.mysql
{
        /// <summary>
    /// represents one row from MySQL's information_schema.`TABLES` table
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
        TableRow fromRow(MySqlDataReader rdr)
        {
            TableRow t = new TableRow();
            int field = 0;
            t.table_catalog = rdr.GetString(field++);
            t.table_schema  = rdr.GetString(field++);
            t.table_name    = rdr.GetString(field++);
            return t;
        }

        public List<TableRow> getTables(MySqlConnection conn, string db)
        {
            string sql = @"
SELECT table_catalog,table_schema,table_name
FROM information_schema.`TABLES`
WHERE table_schema=?db";

            using(MySqlCommand cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.Add("?db", db);
                using(MySqlDataReader rdr = cmd.ExecuteReader())
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

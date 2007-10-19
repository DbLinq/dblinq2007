using System;
using System.Collections.Generic;
using System.Text;
using SqlMetal.schema;

namespace SqlMetal.schema.mysql
{
    /// <summary>
    /// sort tables - parent tables first, child tables next.
    /// </summary>
    class TableSorter : IComparer<TableRow>
    {
        public static void Sort(List<TableRow> tables, List<KeyColumnUsage> keys)
        {
            //TODO: walk all
            foreach(KeyColumnUsage key in keys)
            {
                // ashmind: second condition may be redundant, but I'm leaving
                // it here in fear of breaking something (no tests for now)
                bool isForeignKey = key.referenced_table_name != null
                                 && !string.Equals(key.constraint_name, "PRIMARY", StringComparison.InvariantCultureIgnoreCase);

                if (!isForeignKey)
                    continue;

                string parentTableName = key.table_name;
                string childTableName = key.referenced_table_name;
                //Table parentTable = tables.Find( t=>t.table_name==parentTableName );
                //Table childTable = tables.Find( t=>t.table_name==childTableName );
                TableRow parentTable = tables.Find( delegate(TableRow t){ return t.table_name==parentTableName; } );
                TableRow childTable = tables.Find( delegate(TableRow t){ return t.table_name==childTableName; } );
                if(parentTable==null || childTable==null)
                {
                    Console.WriteLine("ERROR L26 parent/child table missing?");
                    continue;
                }
                parentTable.childTables.Add(childTable);
            }

            TableSorter sorter = new TableSorter();
            tables.Sort(sorter);
        }

        public int Compare(TableRow a,TableRow b)
        {
            foreach(TableRow aChild in a.EnumChildTables(0))
            {
                if(aChild==b)
                    return +1;
            }
            foreach(TableRow bChild in b.EnumChildTables(0))
            {
                if(bChild==a)
                    return -1;
            }
            return 0;
        }
    }
}

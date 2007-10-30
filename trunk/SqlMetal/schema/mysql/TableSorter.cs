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
//        Andrey Shchekin
////////////////////////////////////////////////////////////////////

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
            Dictionary<TableRow, bool> visitedMapA = new Dictionary<TableRow, bool>();
            foreach (TableRow aChild in a.EnumChildTables(visitedMapA))
            {
                if(aChild==b)
                    return +1;
            }
            Dictionary<TableRow, bool> visitedMapB = new Dictionary<TableRow, bool>();
            foreach (TableRow bChild in b.EnumChildTables(visitedMapB))
            {
                if(bChild==a)
                    return -1;
            }
            return 0;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlMetal.schema
{
    /// <summary>
    /// this class contains functionality common to all vendors -
    /// a) rename field Alltypes.Alltypes to Alltypes.Contents
    /// b) rename field Employees.Employees to Employees.RefersToEmployees
    /// c) rename field Alltypes.int to Alltypes.int_
    /// </summary>
    public class SchemaPostprocess
    {
        public static void PostProcess_DB(DlinqSchema.Database schema)
        {
            foreach (var tbl in schema.Tables)
            {
                PostProcess_Table(tbl);
            }
        }

        public static void PostProcess_Table(DlinqSchema.Table table)
        {
            foreach (DlinqSchema.Column col in table.Type.Columns)
            {
                if (CSharp.IsCsharpKeyword(col.Member))
                    col.Member += "_"; //rename column 'int' -> 'int_'

                if (col.Member == table.Type.Name)
                    col.Name = "Contents"; //rename field Alltypes.Alltypes to Alltypes.Contents
            }

            foreach (DlinqSchema.Association assoc in table.Type.Associations)
            {
                if (assoc.Member == table.Type.Name)
                {
                    string thisKey = assoc.ThisKey ?? "_TODO_L35";
                    assoc.Member = thisKey + assoc.Member; //rename field Employees.Employees to Employees.RefersToEmployees
                }
            }

        }
    }
}

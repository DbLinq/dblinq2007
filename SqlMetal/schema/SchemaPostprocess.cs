using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbLinq.Linq;
using DbLinq.Util;
using SqlMetal.Util;

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
            if (schema == null)
                return;

            //sort tables, parent tables first
            TableSorter sorter = new TableSorter(schema.Tables);
            schema.Tables.Sort(sorter);

            foreach (var tbl in schema.Tables)
            {
                PostProcess_Table(tbl);
            }
        }

        public static void PostProcess_Table(DlinqSchema.Table table)
        {
            // picrap: this is processed earlier
            //table.Member = Util.FormatTableName(table.Type.Name, util.PluralEnum.Pluralize);
            //table.Type.Name = Util.FormatTableName(table.Type.Name, util.PluralEnum.Singularize);

            //if (mmConfig.renamesFile != null)
            //{
            //    table.Member = Util.Rename(table.Member);
            //}

            foreach (DlinqSchema.Column col in table.Type.Columns)
            {
                if (col.Member == table.Type.Name)
                    col.Member = "Contents"; //rename field Alltypes.Alltypes to Alltypes.Contents

                // picrap processed earlier
                //col.Storage = "_" + col.Name;

                if (CSharp.IsCsharpKeyword(col.Storage))
                    col.Storage += "_"; //rename column 'int' -> 'int_'

                if (CSharp.IsCsharpKeyword(col.Member))
                    col.Member += "_"; //rename column 'int' -> 'int_'
            }

            Dictionary<string, bool> knownAssocs = new Dictionary<string, bool>();
            foreach (DlinqSchema.Association assoc in table.Type.Associations)
            {
                // picrap: processed earlier
                //assoc.Type = Util.FormatTableName(assoc.Type, util.PluralEnum.Singularize);

                //util.PluralEnum pluralEnum = assoc.IsForeignKey
                //    ? util.PluralEnum.Singularize
                //    : util.PluralEnum.Pluralize;

                //referring to parent: "public Employee Employee" 
                //referring to child:  "public EntityMSet<Product> Products"
                //assoc.Member = Util.FormatTableName(assoc.Member, pluralEnum);

                if (assoc.Member == table.Type.Name)
                {
                    string thisKey = assoc.ThisKey ?? "_TODO_L35";
                    //self-join: rename field Employees.Employees to Employees.RefersToEmployees
                    assoc.Member = thisKey + assoc.Member;
                }

                if (knownAssocs.ContainsKey(assoc.Member))
                {
                    //this is the Andrus test case in Pgsql:
                    //  create table t1 ( private int primary key);
                    //  create table t2 ( f1 int references t1, f2 int references t1 );

                    assoc.Member += "_" + assoc.Name;

                }

                // picrap: handled previously
                //if (mmConfig.renamesFile != null)
                //{
                //    assoc.Member = Util.Rename(assoc.Member);
                //}

                //if (assoc.Member == "employeeterritories" || assoc.Member == "Employeeterritories")
                //    assoc.Member = "EmployeeTerritories"; //hack to help with Northwind

                knownAssocs[assoc.Member] = true;
            }

        }
    }
}

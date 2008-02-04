using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SqlMetal.util;

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
            SqlMetal.util.TableSorter sorter = new SqlMetal.util.TableSorter(schema.Tables);
            schema.Tables.Sort(sorter);

            foreach (var tbl in schema.Tables)
            {
                PostProcess_Table(tbl);
            }
        }

        public static void PostProcess_Table(DlinqSchema.Table table)
        {
            table.Member = Util.FormatTableName(table.Type.Name, PluralEnum.Pluralize);
            table.Type.Name = Util.FormatTableName(table.Type.Name, PluralEnum.Singularize);

            if (mmConfig.renamesFile != null)
            {
                table.Member = Util.Rename(table.Member);
            }

            foreach (DlinqSchema.Column col in table.Type.Columns)
            {
                if (col.Member == table.Type.Name)
                    col.Member = "Contents"; //rename field Alltypes.Alltypes to Alltypes.Contents

                col.Storage = "_" + col.Name;

                if (CSharp.IsCsharpKeyword(col.Member))
                    col.Member += "_"; //rename column 'int' -> 'int_'
            }

            Dictionary<string, bool> knownAssocs = new Dictionary<string, bool>();
            foreach (DlinqSchema.Association assoc in table.Type.Associations)
            {
                assoc.Type = Util.FormatTableName(assoc.Type, PluralEnum.Singularize);

                PluralEnum pluralEnum = assoc.IsForeignKey
                    ? PluralEnum.Singularize
                    : PluralEnum.Pluralize;

                //referring to parent: "public Employee Employee" 
                //referring to child:  "public EntityMSet<Product> Products"
                assoc.Member = Util.FormatTableName(assoc.Member, pluralEnum);

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

                if (mmConfig.renamesFile != null)
                {
                    assoc.Member = Util.Rename(assoc.Member);
                }

                if (assoc.Member == "employeeterritories" || assoc.Member == "Employeeterritories")
                    assoc.Member = "EmployeeTerritories"; //hack to help with Northwind

                knownAssocs[assoc.Member] = true;
            }

        }
    }
}

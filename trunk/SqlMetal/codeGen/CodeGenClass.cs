#region MIT license
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
////////////////////////////////////////////////////////////////////
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using SqlMetal.schema;

namespace SqlMetal.codeGen
{
    /// <summary>
    /// Generates a c# class representing table.
    /// Calls into CodeGenField.
    /// </summary>
    public class CodeGenClass
    {
        const string NL = "\r\n";
        const string NLNL = "\r\n\r\n";
        const string NLT = "\r\n\t";

        public string generateClass(DlinqSchema.Database schema, DlinqSchema.Table table)
        {
            string template = @"
[Table(Name = ""$tableName"")]
public partial class $name $baseClass
{
    $fields

    $ctors

    $properies
    $equals_GetHashCode
    $linksToChildTables
    $linksToParentTables

    public bool IsModified { get; set; }
}
";
            List<string> fieldBodies = new List<string>();
            List<string> properties = new List<string>();
            properties.Add("#region properties - accessors");
            string className = table.Type.Name; //CSharp.FormatTableClassName(table.Class ?? table.Name);

            //foreach(DlinqSchema.Column col in table.Types[0].Columns)
            foreach (DlinqSchema.Column col in table.Type.Columns)
            {
                List<DlinqSchema.Association> constraintsOnField
                    = table.Type.Associations.FindAll(a => a.Name == col.Name);

                CodeGenField codeGenField = new CodeGenField(className, col, constraintsOnField);
                string fld = codeGenField.generateField();
                string prop = codeGenField.generateProperty();
                fld = fld.Replace(NL, NL + "\t");
                prop = prop.Replace(NL, NL + "\t");
                fieldBodies.Add(fld);
                properties.Add(prop);
            }
            properties.Add("#endregion");

            string ctor = genCtors(table);

            string fieldsConcat = string.Join("", fieldBodies.ToArray());
            string propsConcat = string.Join(NL, properties.ToArray());
            string equals = GenerateEqualsAndHash(table);
            string baseClass = (mmConfig.baseClass == null || mmConfig.baseClass == "")
                ? ""
                : ": " + mmConfig.baseClass;
            string childTables = GetLinksToChildTables(schema, table);
            string parentTables = GetLinksToParentTables(schema, table);


            template = template.Replace("    ", "\t");
            template = template.Replace("$name", className);
            template = template.Replace("$tableName", table.Name);
            template = template.Replace("$ctors", ctor);
            template = template.Replace("$fields", fieldsConcat);
            template = template.Replace("$properies", propsConcat);
            template = template.Replace("$baseClass", baseClass);
            template = template.Replace("$linksToChildTables", childTables);
            template = template.Replace("$linksToParentTables", parentTables);
            template = template.Replace("$equals_GetHashCode", equals);

            return template;
        }

        public string genCtors(DlinqSchema.Table table)
        {
            #region getCtors
            string template = @"
#region constructors
public $name()
{
}
public $name($argList)
{
    $statements
}
#endregion
";
            List<string> ctorArgs = new List<string>();
            List<string> ctorStatements = new List<string>();

            foreach (DlinqSchema.Column col in table.Type.Columns)
            {
                string property = col.Member ?? col.Name;
                string colType2 = CSharp.FormatType(col.Type, col.CanBeNull);
                string arg = colType2 + " " + property;
                ctorArgs.Add(arg);
                string assign = "this." + col.Storage + " = " + property + ";";
                ctorStatements.Add(assign);
            }

            string argsCsv = string.Join(",", ctorArgs.ToArray());
            string statements = string.Join(NL, ctorStatements.ToArray());
            string className = table.Type.Name; 

            template = template.Replace("$name", className);
            template = template.Replace("$argList", argsCsv);
            template = template.Replace("$statements", statements);
            template = template.Replace(NL, NLT);
            return template;
            #endregion
        }

        string GetLinksToChildTables(DlinqSchema.Database schema, DlinqSchema.Table table)
        {
            string childLinkTemplate = @"
[Association(Storage = ""null"", OtherKey = ""$childColName"", Name = ""$fkName"")]
public EntityMSet<$childClassName> $fieldName
{
    get { return null; } //L212 - child data available only when part of query
}";
            //child table contains a ManyToOneParent Association, pointing to parent
            //parent table contains a ManyToOneChild.
            var ourChildren = table.Type.Associations.Where( a=> a.OtherKey!=null );

            List<string> linksToChildTables = new List<string>();
            linksToChildTables.Add("#region childtables");
            foreach(DlinqSchema.Association assoc in ourChildren)
            {
                DlinqSchema.Table targetTable = schema.Tables.FirstOrDefault(t => t.Type.Name == assoc.Type);
                if(targetTable==null)
                {
                    Console.WriteLine("ERROR L143 target table class not found:" + assoc.Type);
                    continue;
                }


                string str = childLinkTemplate;

                string childTableName = assoc.Type;
                string childColName     = assoc.OtherKey; //.Columns[0].Name; //eg. 'CustomerID'
                string fkName           = assoc.Name; //eg. 'FK_Orders_Customers'
                string childClassName = assoc.Type;
                string fieldName = assoc.Member;
                str = str.Replace("$childTableName",    childTableName);
                str = str.Replace("$childColName",      childColName);
                str = str.Replace("$fkName",            fkName);
                str = str.Replace("$childClassName",    childClassName);
                str = str.Replace("$fieldName",         fieldName);
                linksToChildTables.Add(str);
            }
            linksToChildTables.Add("#endregion");
            string ret = string.Join(NL, linksToChildTables.ToArray());
            ret = ret.Replace(NL, NLT);
            return ret;
        }

        /// <summary>
        /// this template is compatible with Microsoft standard.
        /// You need this one to generate Northwind.cs correctly.
        /// </summary>
        const string childLinkTemplate_simple = @"
private System.Data.Linq.EntityRef<$parentClassTyp> $fieldName2;    

[Association(Storage=""$fieldName2"", ThisKey=""$thisKey"", Name=""$fkName"")]
[DebuggerNonUserCode]
public $parentClassTyp $member {
	get { return this.$fieldName2.Entity; }
	set { this.$fieldName2.Entity = value; }
}";

        /// <summary>
        /// if you specify Metal.exe -verboseForeignKeys,
        /// you will use this template, for slightly different code, 
        /// where 2 fields are guaranteed not to collide (not to have same name):
        /// (Note: collision avoidance code belongs in SchemaPostprocess.cs)
        /// </summary>
        string childLinkTemplate_verbose = @"
private System.Data.Linq.EntityRef<$parentClassTyp> _$fkName_$thisKey;
[Association(Storage=""_$fkName_$thisKey"", ThisKey=""$thisKey"",Name=""$fkName"")]
[DebuggerNonUserCode]
public $parentClassTyp $fkName_$thisKey {
    get { return this._$fkName_$thisKey.Entity; }
    set { this._$fkName_$thisKey.Entity = value; }
}";

        string GetLinksToParentTables(DlinqSchema.Database schema, DlinqSchema.Table table)
        {
            #region GetLinksToParentTables()
            string childLinkTemplate = mmConfig.verboseForeignKeys
                ? childLinkTemplate_verbose
                : childLinkTemplate_simple;

            var ourParents = table.Type.Associations.Where(a => a.ThisKey != null);

            Dictionary<string, bool> usedAssocNames = new Dictionary<string, bool>();
            List<string> linksToChildTables = new List<string>();
            linksToChildTables.Add("#region parenttables");
            foreach (DlinqSchema.Association assoc in ourParents)
            {
                DlinqSchema.Table targetTable = schema.Tables.FirstOrDefault(t => t.Type.Name == assoc.Type);
                if (targetTable == null)
                {
                    Console.WriteLine("ERROR L191 target table type not found: " + assoc.Type + "  (processing " + assoc.Name + ")");
                    continue;
                }

                string str = childLinkTemplate;

                //string childTableName   = assoc.Target;
                string thisKey = assoc.ThisKey; //.Columns[0].Name; //eg. 'CustomerID'
                string fkName = assoc.Name; //eg. 'FK_Orders_Customers'
                string parentClassName = targetTable.Type.Name;
                string fieldName2 = "_" + assoc.Member;
                if (assoc.Member == thisKey)
                {
                    assoc.Member = thisKey + parentClassName; //repeat name to prevent collision (same as Linq)
                    fieldName2 = "_x_" + assoc.Member;
                }
                usedAssocNames[fieldName2] = true;

                //str = str.Replace("$childTableName",    childTableName);
                str = str.Replace("$thisKey", thisKey);
                str = str.Replace("$fkName", fkName);
                str = str.Replace("$parentClassTyp", parentClassName);
                str = str.Replace("$member", assoc.Member);
                str = str.Replace("$fieldName2", fieldName2);
                linksToChildTables.Add(str);
            }
            linksToChildTables.Add("#endregion");
            string ret = string.Join(NL, linksToChildTables.ToArray());
            ret = ret.Replace(NL, NLT);
            return ret;
            #endregion
        }

        /// <summary>
        /// associations are created in pairs (one in parent, one in child table)
        /// This method find the other one in the pair
        /// </summary>
        DlinqSchema.Association findReverseAssoc(DlinqSchema.Database schema, DlinqSchema.Association assoc)
        {
            //first, find target table
            DlinqSchema.Table targetTable 
                = schema.Tables.FirstOrDefault( t => t.Name==assoc.Type );
            if(targetTable==null)
            {
                Console.WriteLine("findReverseAssoc: ERROR L158 target table not found: " + assoc.Type);
                return null;
            }

            //next, find reverse association (has the same name)
            DlinqSchema.Association reverseAssoc
                = targetTable.Type.Associations.FirstOrDefault( a2 => a2.Name==assoc.Name );
            if(reverseAssoc==null)
            {
                Console.WriteLine("findReverseAssoc: ERROR L167 reverse assoc not found: "+assoc.Name);
                return null;
            }
            return reverseAssoc;
        }


        public string GenerateEqualsAndHash(DlinqSchema.Table table)
        {
            string template = @"
    #region GetHashCode(),Equals() - uses column $fieldID to look up objects in liveObjectMap
    //TODO: move this logic our of user code, into a generated class
    public override int GetHashCode()
    {
        return $GetHashCode_list;
    }
    public override bool Equals(object obj)
    {
        $className o2 = obj as $className;
        if(o2==null)
            return false;
        return $Equals_list;
    }
    #endregion
";

            string tableName = table.Name;
            List<DlinqSchema.Column> primaryKeys = table.Type.Columns.Where(c => c.IsPrimaryKey).ToList();
            if(primaryKeys.Count==0)
            {
                return "#warning L189 table "+tableName+" has no primary key. Multiple c# objects will refer to the same row.\n";
            }

            List<string> getHash_list = new List<string>();
            List<string> equals_list = new List<string>();

            foreach (DlinqSchema.Column primaryKey in primaryKeys)
            {
                string fieldName = "_" + primaryKey.Name;
                string getHash = CSharp.IsValueType(primaryKey.Type)
                    ? fieldName+".GetHashCode()"
                    : "("+fieldName+" == null ? 0 : "+fieldName+".GetHashCode())";
                getHash_list.Add(getHash);

                // "return $fieldID.Equals(o2.$fieldID);"
                string equals = CSharp.IsValueType(primaryKey.Type)
                    ? fieldName + " == o2." + fieldName
                    : "object.Equals(" + fieldName + ", o2." + fieldName + ")";
                equals_list.Add(equals);
            }

            string getHash_str = string.Join(" ^ ", getHash_list.ToArray());
            string equals_str = string.Join(" && ", equals_list.ToArray());

            template = template.Replace("    ", "\t"); //four spaces mean a tab
            string result = template;
            result = result.Replace("$GetHashCode_list", getHash_str);
            result = result.Replace("$Equals_list", equals_str);
            result = result.Replace("$className", table.Type.Name);
            return result;
        }
    }
}

////////////////////////////////////////////////////////////////////
//Initial author: Jiri George Moudry, 2006.
//License: LGPL. (Visit http://www.gnu.org)
////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Text;
using System.Query;
using SqlMetal.schema;

namespace SqlMetal.codeGen
{
    /// <summary>
    /// Generates a c# class representing table.
    /// Calls into CodeGenField.
    /// </summary>
    public class CodeGenClass
    {
        public string generateClass(DlinqSchema.Database schema, DlinqSchema.Table table)
        {
            string template = @"
[Table(Name=""$tableName"")]
public partial class $name $baseClass
{
    bool _isModified_;
    public bool IsModified 
    { 
        get { return _isModified_; } 
        set { _isModified_ = value; } 
    }

    $fields

    $ctors

    $properies
    $equals_GetHashCode
    $linksToChildTables
    $linksToParentTables
}
";
            List<string> fieldBodies = new List<string>();
            List<string> properties = new List<string>();
            properties.Add("#region properties - accessors");

            foreach(DlinqSchema.Column col in table.Types[0].Columns)
            {
                List<DlinqSchema.Association> constraintsOnField 
                    = table.Types[0].Associations.FindAll( 
                    delegate(DlinqSchema.Association a){ return a.Name==col.Name; });

                CodeGenField codeGenField = new CodeGenField(col, constraintsOnField);
                string fld = codeGenField.generateField();
                string prop = codeGenField.generateProperty();
                fld = fld.Replace("\n", "\n\t");
                prop = prop.Replace("\n", "\n\t");
                fieldBodies.Add(fld);
                properties.Add(prop);
            }
            properties.Add("#endregion");

            string ctor = genCtors(table);

            string fieldsConcat = string.Join("\n", fieldBodies.ToArray());
            string propsConcat = string.Join("\n", properties.ToArray());
            string equals = GenerateEqualsAndHash(table);
            string name2 = table.Class;
            string baseClass = (mmConfig.baseClass==null || mmConfig.baseClass=="")
                ? ""
                : ": "+mmConfig.baseClass;
            string childTables = GetLinksToChildTables(schema, table);
            string parentTables = GetLinksToParentTables(schema, table);


            template = template.Replace("$name", name2);
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
#region costructors
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

            foreach(DlinqSchema.Column col in table.Types[0].Columns)
            {
                string arg = col.Type+" "+col.Property;
                ctorArgs.Add(arg);
                string assign = "this._"+col.Name+" = "+col.Property+";";
                ctorStatements.Add(assign);
            }

            string argsCsv = string.Join(",", ctorArgs.ToArray());
            string statements = string.Join("\n", ctorStatements.ToArray());
            template = template.Replace("$name", table.Class);
            template = template.Replace("$argList", argsCsv);
            template = template.Replace("$statements", statements);
            template = template.Replace("\n","\n\t");
            return template;
            #endregion
        }

        string GetLinksToChildTables(DlinqSchema.Database schema, DlinqSchema.Table table)
        {
            string childLinkTemplate = @"
[Association(Storage=""null"", OtherKey=""$childColName"", Name=""$fkName"")]
public EntityMSet<$childClassName> $fieldName
{
    get { return null; } //TODO L212
}";
            var ourChildren = table.Types[0].Associations 
                          .Where( a=> a.Kind==DlinqSchema.RelationshipKind.ManyToOneParent );

            List<string> linksToChildTables = new List<string>();
            foreach(DlinqSchema.Association assoc in ourChildren)
            {
                //DlinqSchema.Association assoc2 = findReverseAssoc(schema, assoc);
                //if(assoc2==null)
                //    continue; //error already printed
                DlinqSchema.Table targetTable = schema.Schemas[0].Tables.FirstOrDefault( t => t.Name==assoc.Target);
                if(targetTable==null)
                {
                    Console.WriteLine("ERROR L143 target table not found:"+assoc.Target);
                    continue;
                }


                string str = childLinkTemplate;

                string childTableName   = assoc.Target;
                string childColName     = assoc.Columns[0].Name; //eg. 'CustomerID'
                string fkName           = assoc.Name; //eg. 'FK_Orders_Customers'
                string childClassName   = targetTable.Class;
                string fieldName        = childClassName+"s";
                str = str.Replace("$childTableName",    childTableName);
                str = str.Replace("$childColName",      childColName);
                str = str.Replace("$fkName",            fkName);
                str = str.Replace("$childClassName",    childClassName);
                str = str.Replace("$fieldName",         fieldName);
                linksToChildTables.Add(str);
            }
            string ret = string.Join("\n", linksToChildTables.ToArray());
            ret = ret.Replace("\n","\n\t");
            return ret;
        }

        string GetLinksToParentTables(DlinqSchema.Database schema, DlinqSchema.Table table)
        {
            #region GetLinksToParentTables()
            string childLinkTemplate = @"
private EntityRef<$parentClass> $fieldName;    

[Association(Storage=""$fieldName"", ThisKey=""$thisKey"", Name=""$fkName"")]
[DebuggerNonUserCode]
public $parentClass $parentClass {
	get { return this.$fieldName.Entity; }
	set { this.$fieldName.Entity = value; }
}
";
            //List<DlinqSchema.Association> ourParents = table.Types[0].Associations.FindAll(
            //    delegate(DlinqSchema.Association a){ return true; }); //TODO - filter for child tables?
            var ourParents = table.Types[0].Associations 
                          .Where( a=> a.Kind==DlinqSchema.RelationshipKind.ManyToOneChild );

            List<string> linksToChildTables = new List<string>();
            foreach(DlinqSchema.Association assoc in ourParents)
            {
                DlinqSchema.Table targetTable = schema.Schemas[0].Tables.FirstOrDefault( t => t.Name==assoc.Target);
                if(targetTable==null)
                {
                    Console.WriteLine("ERROR L191 target table not found:"+assoc.Target);
                    continue;
                }

                string str = childLinkTemplate;

                string childTableName   = assoc.Target;
                string thisKey          = assoc.Columns[0].Name; //eg. 'CustomerID'
                string fkName           = assoc.Name; //eg. 'FK_Orders_Customers'
                string parentClassName  = targetTable.Class;
                string fieldName        = "_"+parentClassName;
                str = str.Replace("$childTableName",    childTableName);
                str = str.Replace("$thisKey",           thisKey);
                str = str.Replace("$fkName",            fkName);
                str = str.Replace("$parentClass",       parentClassName);
                str = str.Replace("$fieldName",         fieldName);
                linksToChildTables.Add(str);
            }
            string ret = string.Join("\n", linksToChildTables.ToArray());
            ret = ret.Replace("\n","\n\t");
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
                = schema.Schemas[0].Tables.FirstOrDefault( t => t.Name==assoc.Target);
            if(targetTable==null)
            {
                Console.WriteLine("findReverseAssoc: ERROR L158 target table not found: "+assoc.Target);
                return null;
            }

            //next, find reverse association (has the same name)
            DlinqSchema.Association reverseAssoc
                = targetTable.Types[0].Associations.FirstOrDefault( a2 => a2.Name==assoc.Name );
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
        return $fieldID.GetHashCode();
    }
    public override bool Equals(object obj)
    {
        $className o2 = obj as $className;
        if(o2==null)
            return false;
        return $fieldID.Equals(o2.$fieldID);
    }
    #endregion
";

            string tableName = table.Name;
            List<DlinqSchema.ColumnSpecifier> primaryKeys = table.PrimaryKey;
            if(primaryKeys.Count==0)
            {
                return "#warning L189 table "+tableName+" has no primary key. Multiple c# objects will refer to the same row.\n";
            }
            //TODO - handle composite keys
            //TODO - ensure primary key column is non-null, even for composite keys
            string fieldName = "_"+primaryKeys[0].Columns[0].Name;

            string result = template.Replace("$fieldID",fieldName);
            result = result.Replace("$className", table.Class);
            return result;
        }
    }
}

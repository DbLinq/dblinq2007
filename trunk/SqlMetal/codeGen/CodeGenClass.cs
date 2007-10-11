////////////////////////////////////////////////////////////////////
//Initial author: Jiri George Moudry, 2006.
//License: LGPL. (Visit http://www.gnu.org)
////////////////////////////////////////////////////////////////////
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
[Table(Name=""$tableName"")]
public partial class $name $baseClass
{
    public bool IsModified { get; set; }

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

            string fieldsConcat = string.Join(NL, fieldBodies.ToArray());
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

            foreach (DlinqSchema.Column col in table.Type.Columns)
            {
                string property = col.Member ?? col.Name;
                string colType2 = CSharp.FormatType(col.Type, col.CanBeNull);
                string arg = colType2 + " " + property;
                ctorArgs.Add(arg);
                string assign = "this._" + col.Name + " = " + property + ";";
                ctorStatements.Add(assign);
            }

            string argsCsv = string.Join(",", ctorArgs.ToArray());
            string statements = string.Join("\n", ctorStatements.ToArray());
            string className = table.Type.Name; 

            template = template.Replace("$name", className);
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
            //child table contains a ManyToOneParent Association, pointing to parent
            //parent table contains a ManyToOneChild.
            var ourChildren = table.Type.Associations.Where( a=> a.OtherKey!=null );

            List<string> linksToChildTables = new List<string>();
            foreach(DlinqSchema.Association assoc in ourChildren)
            {
                //DlinqSchema.Association assoc2 = findReverseAssoc(schema, assoc);
                //if(assoc2==null)
                //    continue; //error already printed
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
                string childClassName = assoc.Type; // targetTable.Class ?? targetTable.Name;
                string fieldName = assoc.Member; //childClassName+"s";
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
private EntityRef<$parentClassTyp> $fieldName2;    

[Association(Storage=""$fieldName1"", ThisKey=""$thisKey"", Name=""$fkName"")]
[DebuggerNonUserCode]
public $parentClassTyp $member {
	get { return this.$fieldName2.Entity; }
	set { this.$fieldName2.Entity = value; }
}
";
            var ourParents = table.Type.Associations.Where( a=>a.ThisKey!=null );

            List<string> linksToChildTables = new List<string>();
            foreach(DlinqSchema.Association assoc in ourParents)
            {
                DlinqSchema.Table targetTable = schema.Tables.FirstOrDefault( t => t.Type.Name==assoc.Type );
                if(targetTable==null)
                {
                    Console.WriteLine("ERROR L191 target table type not found: " + assoc.Type + "  (processing " + assoc.Name + ")");
                    continue;
                }

                string str = childLinkTemplate;

                //string childTableName   = assoc.Target;
                string thisKey          = assoc.ThisKey; //.Columns[0].Name; //eg. 'CustomerID'
                string fkName           = assoc.Name; //eg. 'FK_Orders_Customers'
                string parentClassName  = targetTable.Type.Name;
                string fieldName        = "_"+parentClassName;
                string fieldName2       = fieldName;
                if (assoc.Member == thisKey)
                {
                    assoc.Member = thisKey + parentClassName; //repeat name to prevent collision (same as Linq)
                    fieldName2 = "_" + assoc.Member;
                }

                //str = str.Replace("$childTableName",    childTableName);
                str = str.Replace("$thisKey",           thisKey);
                str = str.Replace("$fkName",            fkName);
                str = str.Replace("$parentClassTyp",    parentClassName);
                str = str.Replace("$member",            assoc.Member);
                str = str.Replace("$fieldName1",         fieldName);
                str = str.Replace("$fieldName2",         fieldName2);
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
            //List<DlinqSchema.ColumnSpecifier> primaryKeys = table.PrimaryKey;
            List<DlinqSchema.Column> primaryKeys = table.Type.Columns.Where(c => c.IsPrimaryKey).ToList();
            if(primaryKeys.Count==0)
            {
                return "#warning L189 table "+tableName+" has no primary key. Multiple c# objects will refer to the same row.\n";
            }
            //TODO - handle composite keys
            //TODO - ensure primary key column is non-null, even for composite keys
            if (primaryKeys == null || primaryKeys.Count == 0 //&& primaryKeys[0].Columns == null || primaryKeys[0].Columns.Count == 0
                )
            {
                Console.WriteLine("ERROR L269 - bad primary key data");
                return "_L269_BAD_PRIMARY_KEY_";
            }

            string fieldName = "_"+primaryKeys[0].Name;

            template = template.Replace("    ", "\t"); //four spaces mean a tab
            string result = template.Replace("$fieldID", fieldName);
            result = result.Replace("$className", table.Type.Name);
            return result;
        }
    }
}

//#define TEST_SERIALIZATION

using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Text;

namespace SqlMetal.schema
{
    /// <summary>
    /// This class represents DB schema, extracted from MySql or Oracle,
    /// and it's XML representation should correspond to Microsoft's 'DLinq Mapping Schema.xsd'
    /// We try to mimic Microsoft's schema syntax -
    /// - see 'DLinq Overview for CSharp Developers.doc', paragraph 'Here is a prototypical example of the XML syntax:'
    /// 
    /// 2007-Nov update: please see Microsoft documentation 'External Mapping Reference (LINQ to SQL)'
    /// http://msdn2.microsoft.com/en-us/library/bb386907(VS.90).aspx
    /// 
    /// Paul Welter says in http://community.codesmithtools.com/blogs/pwelter/atom.aspx: 
    ///   Safe Attributes to change in the Dbml file ...
    /// - Database/@Class - The name of the DataContext class that will be generated.
    /// - Database/@EntityNamespace - The namespace for the entity classes.
    /// - Database/@ContextNamespace - The namespace for the DataContext class.
    /// - Table/@Member - The property name for the table in the DataContext class.
    /// - Type/@Name - The name of the entity class.
    /// - Column/@Member - The property name for the column in the entity class.
    /// - Column/@Storage - The private field LINQ to SQL will us to assign values to.
    /// - Association/@Member - The property name for this association.
    /// - Association/@Storage - The private field LINQ to SQL will us to assign values the association to.
    /// - Function/@Method  - The name of the method for the database procedure.
    /// - Parameter/@Parameter - The method argument name that maps to the database procedure parameter.
    /// </summary>
    public class DlinqSchema
    {
        [XmlType(AnonymousType = true, Namespace = "http://schemas.microsoft.com/linqtosql/dbml/2007")]
        [XmlRoot(Namespace = "http://schemas.microsoft.com/linqtosql/dbml/2007", IsNullable = false)]
        public class Database
        {
            [XmlAttribute] public string Name;
            [XmlAttribute] public AccessEnum1 Access;            
            [XmlAttribute] public string Class;

            [XmlElement("Table")]
            public readonly List<Table> Tables = new List<Table>();

            /// <summary>
            /// load a DBML file
            /// </summary>
            public static Database LoadFromFile(string fname)
            {
                XmlSerializer xser = new XmlSerializer(typeof(Database));
                using(System.IO.TextReader rdr = System.IO.File.OpenText(fname))
                {
                    object obj = xser.Deserialize(rdr);
                    Database obj2 = (Database)obj;
                    return obj2;
                }
            }

            /// <summary>
            /// save a DBML file
            /// </summary>
            public static void SaveDbmlFile(string fname, Database db)
            {
                XmlSerializer xser = new XmlSerializer(typeof(Database));
                System.IO.StringWriter writer = new System.IO.StringWriter();
                //using (System.IO.StreamWriter writer = System.IO.File.CreateText(fname))
                //{
                //    xser.Serialize(writer, db);
                //}
                xser.Serialize(writer, db);
                string xml = writer.ToString();

                //remove default values which would produce a huge xml file
                xml = xml.Replace(" IsPrimaryKey=\"false\"", "");
                xml = xml.Replace(" IsDbGenerated=\"false\"", "");
                xml = xml.Replace(" IsDiscriminator=\"false\"", "");
                xml = xml.Replace(" IsVersion=\"false\"", "");
                xml = xml.Replace(" IsInheritanceDefault=\"false\"", "");
                xml = xml.Replace(" IsForeignKey=\"false\"", "");
                xml = xml.Replace(" IsUnique=\"false\"", "");
                xml = xml.Replace(" DeleteOnNull=\"false\"", "");
                xml = xml.Replace(" UpdateCheck=\"Always\"", "");
                xml = xml.Replace(" AutoSync=\"Never\"", "");
                xml = xml.Replace(" Access=\"public\"", "");

                System.IO.File.WriteAllText(fname, xml);
                //IsPrimaryKey="false" IsDbGenerated="false"
            }

        }

        public enum AccessEnum1 { @public, @internal };
        public enum AccessEnum2 { @public, @private, @internal, @protected };
        public enum UpdateCheck{ @public, @private, @internal, @protected };

        /* Index etc - unused in Orcas Beta 2?
        /// <summary>
        /// simple name of column, as used within PrimaryKey and Unique elements
        /// </summary>
        //[XmlElement("Column")]
        public class ColumnName
        {
            [XmlAttribute]
            public string Name;

            public ColumnName(){}
            public ColumnName(string name){ Name=name; }
        }
        public class ColumnSpecifier
        {
            /// <summary>
            /// name of this PrimaryKey or Unique
            /// </summary>
            [XmlAttribute]
            public string Name;

            [XmlElement("Column")]
            public readonly List<ColumnName> Columns = new List<ColumnName>();
        }

        public class Index : ColumnSpecifier
        {
            /// <summary>
            /// eg. 'clustered'
            /// </summary>
            [XmlAttribute] public string Style;
            [XmlAttribute] public bool IsUnique;
        }
         * */

        /// <summary>
        /// represents a dbml table - contains a type with columns.
        /// </summary>
        public class Table
        {
            [XmlAttribute]
            public string Name;

            [XmlAttribute]
            public AccessEnum1 Access;

            [XmlAttribute]
            public string Member;

            //[XmlElement]
            //public readonly List<ColumnSpecifier> Unique = new List<ColumnSpecifier>();

            //[XmlElement]
            //public readonly List<Index> Index = new List<Index>();

            public override string ToString()
            {
                string cols = Type==null 
                    ? "null Type "
                    : (Type.Columns.Count > 0 ? " columns=" + Type.Columns.Count : "");
                return "Dblinq.Table nom=" + Name + cols; // +prim;
            }

            public Type Type = new Type();
        }

        public class Type
        {
            [XmlAttribute]
            public string Name;

            [XmlAttribute]
            public AccessEnum2 Access;

            [XmlAttribute]
            public string InheritanceCode;

            [XmlAttribute]
            public bool IsInheritanceDefault;

            [XmlElement("Column")]
            public readonly List<Column> Columns = new List<Column>();

            [XmlElement("Association")]
            public readonly List<Association> Associations = new List<Association>();

            public override string ToString()
            {
                return "Type " + Name + " cols=" + Columns.Count + " assoc="+Associations.Count;
            }
        }

        public enum UpdateCheckEnum { Always, Never, WhenChanged }
        public enum AutoSyncEnum { Never, OnInsert, OnUpdate, Always, Default }

        /// <summary>
        /// represents a dbml column (a DB table contains a type with these columns).
        /// </summary>
        public class Column
        {
            [XmlAttribute]
            public string Name;

            [XmlAttribute]
            public string Member;

            [XmlAttribute]
            public string Storage;

            /// <summary>
            /// CLR type, eg. 'System.Int32'
            /// </summary>
            [XmlAttribute]
            public string Type;

            [XmlAttribute]
            public string DbType;

            [XmlAttribute]
            public bool IsPrimaryKey;

            [XmlAttribute]
            public bool IsDbGenerated;

            [XmlAttribute]
            public bool CanBeNull;

            [XmlAttribute]
            public UpdateCheckEnum UpdateCheck;

            [XmlAttribute]
            public bool IsDiscriminator;

            /// <summary>
            /// during CreateDatabase, describes expresion for a calculated column
            /// </summary>
            [XmlAttribute]
            public string Expression;

            [XmlAttribute]
            public bool IsVersion;

            [XmlAttribute]
            public AutoSyncEnum AutoSync;


            public override string ToString()
            {
                string pk = IsPrimaryKey ? " (PK)" : "";
                return "Column " + Name + "  " + DbType + pk;
            }
        }

        /// <summary>
        /// represents a DBML association. Lives in Table.Type .
        /// Associations always come in pairs - both parent and child table contain an Association.
        /// </summary>
        public class Association //: ColumnSpecifier
        {
            [XmlAttribute]
            public string Name;

            [XmlAttribute]
            public string Member;

            [XmlAttribute]
            public string Storage;

            /// <summary>
            /// eg. for Table 'Category', we have Assoc.Type='Products'
            /// </summary>
            [XmlAttribute]
            public string Type;


            [XmlAttribute]
            public string ThisKey;

            [XmlAttribute]
            public string OtherKey;

            [XmlAttribute]
            public bool IsForeignKey;

            [XmlAttribute]
            public bool IsUnique;

            //[XmlAttribute] public string UpdateRule;
            [XmlAttribute]
            public string DeleteRule;

            [XmlAttribute]
            public bool DeleteOnNull;

            public override string ToString()
            {
                string keys = ThisKey != null ? " ThisKey=" + Type + "." + ThisKey : "";
                keys += OtherKey != null ? " OtherKey=" + Type + "." + OtherKey : "";
                return "Assoc " + Name + keys;
            }
        }

        public class StoredProcedure
        {
        }

        public class Function
        {
        }

        public class Parameter
        {
        }

        public class ResultShape
        {
        }

#if TEST_SERIALIZATION
        static void Main()
        {
            Table tbl = new Table();
            tbl.Name = "Products"; 
            
            Type type = new Type();
            type.Name = "Categories";
            tbl.Type = type;

            Column col = new Column();
            col.Name = "col1";
            tbl.Type.Columns.Add(col);

            Database db = new Database();
            db.Name = "LinqTestDB";
            db.Tables.Add(tbl);


            XmlSerializer xser = new XmlSerializer(typeof(Database));
            //object obj = xser.Deserialize(System.IO.File.OpenText("northwind.dbml"));
            System.IO.StringWriter sw = new System.IO.StringWriter();
            xser.Serialize(sw,db);
            System.IO.File.WriteAllText("c:/temp/db_test.xml", sw.ToString());
        }

#endif
        //public enum RelationshipKind 
        //{
        //    OneToOneChild,
        //    OneToOneParent,
        //    ManyToOneChild,
        //    ManyToOneParent
        //}

    }
}

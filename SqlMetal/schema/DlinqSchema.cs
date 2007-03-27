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
    /// </summary>
    public class DlinqSchema
    {
        public class Database
        {
            [XmlAttribute] public string Name;
            [XmlAttribute] public AccessEnum1 Access;            
            [XmlAttribute] public string Class;

            [XmlElement] public readonly List<Schema> Schemas = new List<Schema>();
        }

        public enum AccessEnum1 { @public, @internal };
        public enum AccessEnum2 { @public, @private, @internal, @protected };

        public class Schema
        {
            [XmlAttribute] public string Name;
            [XmlAttribute] public bool Hidden;
            [XmlAttribute] public AccessEnum1 Access;            
            [XmlAttribute] public string Class;

            [XmlElement] public readonly List<Table> Tables = new List<Table>();
        }

        /// <summary>
        /// simple name of column, as used within PrimaryKey and Unique elements
        /// </summary>
        //[XmlElement("Column")]
        public class ColumnName
        {
            [XmlAttribute] public string Name;
            public ColumnName(){}
            public ColumnName(string name){ Name=name; }
        }
        public class ColumnSpecifier
        {
            /// <summary>
            /// name of this PrimaryKey or Unique
            /// </summary>
            [XmlAttribute] public string Name;
            [XmlAttribute] public bool Hidden;

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

        public class Table
        {
            [XmlAttribute] public string Name;
            [XmlAttribute] public bool Hidden;
            [XmlAttribute] public AccessEnum1 Access;            
            [XmlAttribute] public string Class;

            [XmlElement] public readonly List<ColumnSpecifier> PrimaryKey = new List<ColumnSpecifier>();
            [XmlElement] public readonly List<ColumnSpecifier> Unique = new List<ColumnSpecifier>();
            [XmlElement] public readonly List<Index> Index = new List<Index>();
            [XmlElement] public readonly List<Type> Types = new List<Type>();
        }

        public class Type
        {
            [XmlAttribute] public string Name;
            [XmlAttribute] public bool Hidden;
            [XmlAttribute] public AccessEnum2 Access;            
            [XmlAttribute] public string InheritanceCode;
            [XmlAttribute] public bool IsInheritanceDefault;

            [XmlElement] public readonly List<Column> Columns = new List<Column>();
            [XmlElement] public readonly List<Association> Associations = new List<Association>();
        }

        public enum UpdateCheckEnum { Always, Never, WhenChanged }

        public class Column
        {
            [XmlAttribute] public string Name;
            [XmlAttribute] public bool Hidden;
            [XmlAttribute] public AccessEnum2 Access;            
            [XmlAttribute] public string Property;
            [XmlAttribute] public string DBType;

            /// <summary>
            /// CLR-type
            /// </summary>
            [XmlAttribute] public string Type;
            [XmlAttribute] public bool Nullable;
            [XmlAttribute] public bool IsIdentity;
            [XmlAttribute] public bool IsAutogen;

            /// <summary>
            /// specifies whether the data value constitutes a version stamp maintained by the database.
            /// </summary>
            [XmlAttribute] public bool IsVersion;
            [XmlAttribute] public bool IsReadOnly;
            [XmlAttribute] public UpdateCheckEnum UpdateCheck;
        }

#if WRONG
        public class Association
        {
            [XmlAttribute] public string Name;

            /// <summary>
            /// The name of the underlying storage member. If specified it tells DLinq how to bypass the public property accessor for the data member and interact with the raw value itself. If not specified DLinq gets and sets the value using the public accessor. It is recommended that all association members be properties with separate storage members identified
            /// </summary>
            [XmlAttribute] public string Storage;

            /// <summary>
            /// A comma separated list of names of one or more members of this entity class that represent the key values on this side of the association.  If not specified, the members are assumed to be the members that make up the primary key
            /// </summary>
            [XmlAttribute] public string ThisKey;

            /// <summary>
            /// A comma separated list of names of one or more members of the target entity class that represent the key values on the other side of the association.  If not specified, the members are assumed to be the members that make up the other entity class’s primary key
            /// </summary>
            [XmlAttribute] public string OtherKey;
            
            /// <summary>
            /// True if there a uniqueness constraint on the foreign key, indicating a true 1:1 relationship. This property is seldom used as 1:1 relationships are near impossible to manage within the database. Mostly entity models are defined using 1:n relationships even when they are treated as 1:1 by application developers.
            /// </summary>
            [XmlAttribute] public bool Unique;

            /// <summary>
            /// True if the target ‘other’ type of the association is the parent of the source type. With foreign-key to primary-key relationships, the side holding the foreign-key is the child and the side holding the primary key is the parent.
            /// </summary>
            [XmlAttribute] public bool IsParent;

        }
#endif
        public class Association : ColumnSpecifier
        {
            [XmlAttribute] public RelationshipKind Kind;

            /// <summary>
            /// target-table name
            /// </summary>
            [XmlAttribute] public string Target;

            [XmlAttribute] public string UpdateRule;
            [XmlAttribute] public string DeleteRule;
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

            Schema sch = new Schema();
            sch.Name = "Schema1";
            sch.Tables.Add( tbl );
            Database db = new Database();
            db.Name = "LinqTestDB";
            db.Schemas.Add(sch);


            XmlSerializer xser = new XmlSerializer(typeof(Database));
            object obj = xser.Deserialize(System.IO.File.OpenText("c:/temp/db_test.xml"));
            System.IO.StringWriter sw = new System.IO.StringWriter();
            xser.Serialize(sw,db);
            System.IO.File.WriteAllText("c:/temp/db_test.xml", sw.ToString());
        }

#endif
        public enum RelationshipKind 
        {
		    OneToOneChild,
		    OneToOneParent,
		    ManyToOneChild,
		    ManyToOneParent
    	}

    }
}

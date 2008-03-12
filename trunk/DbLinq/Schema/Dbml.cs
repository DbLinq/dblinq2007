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
//        Andrey Shchekin
////////////////////////////////////////////////////////////////////
#endregion

//#define TEST_SERIALIZATION

using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace DbLinq.Schema.Dbml
{
    /// <summary>
    /// This class represents DB schema, extracted from MySql or Oracle,
    /// and it's XML representation should correspond to Microsoft's 'DLinq Mapping Schema.xsd'
    /// We try to mimic Microsoft's schema syntax -
    /// - see 'DLinq Overview for CSharp Developers.doc', paragraph 'Here is a prototypical example of the XML syntax:'
    /// 
    /// 2007-Nov update: the microsoft schema is now called 'DBML'.
    /// Please see also Microsoft documentation 'External Mapping Reference (LINQ to SQL)'
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
    [XmlType(AnonymousType = true, Namespace = "http://schemas.microsoft.com/linqtosql/dbml/2007")]
    [XmlRoot(Namespace = "http://schemas.microsoft.com/linqtosql/dbml/2007", IsNullable = false)]
    public class Database
    {
        [XmlAttribute]
        public string Name;

        [XmlAttribute]
        public string Provider { get; set; } // we will find our provider from here (DbLinq.MySQL for example)

        [XmlAttribute]
        [DefaultValue(typeof(AccessEnum1), "public")]
        public AccessEnum1 AccessModifier;

        [XmlAttribute]
        public string Class; // NONSTANDARD

        [XmlElement("Table")]
        public readonly List<Table> Tables = new List<Table>();

        [XmlElement("Function")]
        public readonly List<Function> Functions = new List<Function>();
    }

    public enum AccessEnum1 { @public, @internal };
    public enum AccessEnum2 { @public, @private, @internal, @protected };
    public enum UpdateCheck { @public, @private, @internal, @protected };


    /// <summary>
    /// represents a dbml table - contains a type with columns.
    /// </summary>
    public class Table
    {
        [XmlAttribute]
        public string Name;

        [XmlAttribute]
        public string Member;

        // TODO: see if we can get this field internal again, or remove it...
        [XmlIgnore]
        public bool _isChild; //used in TableSorter
        // NONSTANDARD (of course it is not)

        public override string ToString()
        {
            string cols = Type == null
                              ? "null Type "
                              : (Type.Columns.Count > 0 ? " columns=" + Type.Columns.Count : "");
            return "Dblinq.Table nom=" + Name + cols; // +prim;
        }

        public Type Type = new Type();
    }

    public class Type
    {
        /// <summary>
        /// This is the class name, representing the type mapped on the table
        /// </summary>
        [XmlAttribute]
        public string Name;

        [XmlAttribute]
        [DefaultValue(typeof(AccessEnum2), "public")]
        public AccessEnum2 Access; // NONSTANDARD

        /// <summary>
        /// if this code is matched in Discriminator column, we make this derived type instead of parent type.
        /// (eg. select returns HourlyEmployee instead of Employee, because EmployeeType column had our InheritanceCode)
        /// </summary>
        [XmlAttribute]
        public string InheritanceCode;

        [XmlAttribute]
        [DefaultValue(false)]
        public bool IsInheritanceDefault;

        [XmlElement("Column")]
        public readonly List<Column> Columns = new List<Column>();

        [XmlElement("Association")]
        public readonly List<Association> Associations = new List<Association>();

        /// <summary>
        /// eg. Employee base class contains HourlyEmployee and SalariedEmployee.
        /// </summary>
        [XmlElement("Type")]
        public readonly List<Type> DerivedTypes = new List<Type>();

        public override string ToString()
        {
            return "Type " + Name + " cols=" + Columns.Count + " assoc=" + Associations.Count;
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
        public string Type; // NONSTANDARD

        [XmlAttribute]
        public string DbType;

        [XmlAttribute]
        [DefaultValue(false)]
        public bool IsPrimaryKey;

        [XmlAttribute]
        [DefaultValue(false)]
        public bool IsDbGenerated;

        [XmlAttribute]
        [DefaultValue(true)]
        public bool CanBeNull;

        [XmlAttribute]
        [DefaultValue(typeof(UpdateCheckEnum), "Always")]
        public UpdateCheckEnum UpdateCheck;

        [XmlAttribute]
        [DefaultValue(false)]
        public bool IsDiscriminator;

        /// <summary>
        /// during CreateDatabase, describes expresion for a calculated column
        /// </summary>
        [XmlAttribute]
        public string Expression;

        [XmlAttribute]
        [DefaultValue(false)]
        public bool IsVersion;

        [XmlAttribute]
        [DefaultValue(typeof(AutoSyncEnum), "Never")]
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
    public class Association
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
        public string Type; // NONSTANDARD


        [XmlAttribute]
        public string ThisKey;

        [XmlAttribute]
        public string OtherKey;

        [XmlAttribute]
        [DefaultValue(false)]
        public bool IsForeignKey;

        [XmlAttribute]
        [DefaultValue(false)]
        public bool IsUnique;

        //[XmlAttribute] public string UpdateRule;

        [XmlAttribute]
        public string DeleteRule;

        [XmlAttribute]
        [DefaultValue(false)]
        public bool DeleteOnNull;

        public override string ToString()
        {
            string keys = ThisKey != null ? " ThisKey=" + Type + "." + ThisKey : "";
            keys += OtherKey != null ? " OtherKey=" + Type + "." + OtherKey : "";
            return "Assoc " + Name + keys;
        }
    }

    /// <summary>
    /// represents a Stored Proc or function.
    /// </summary>
    public class Function
    {
        [XmlAttribute]
        public string Name;

        [XmlAttribute]
        public string Method;

        /// <summary>
        /// True is the Function is a function, False if it is a procedure
        /// </summary>
        [XmlAttribute]
        public bool IsComposable;

        /// <summary>
        /// if a stored proc contains a select statement, we need to check if it returns a resultset.
        /// </summary>
        //[XmlAttribute]
        [XmlIgnore]
        [DefaultValue(false)]
        public bool BodyContainsSelectStatement;

        [XmlElement("Parameter")]
        public readonly List<Parameter> Parameters = new List<Parameter>();

        public Return Return;

        /// <summary>
        /// describes columns of resultset returned from proc.
        /// </summary>
        public ElementType ElementType;

        public override string ToString()
        {
            return "dbml.Function " + Name;
        }
    }

    /// <summary>
    /// Stored procedure return value
    /// </summary>
    public class Return
    {
        [XmlAttribute]
        public string Type; // NONSTANDARD

        [XmlAttribute]
        public string DbType;
    }

    /// <summary>
    /// represents a stored proc parameter
    /// </summary>
    public class Parameter
    {
        [XmlAttribute]
        public string Name;

        [XmlAttribute("Parameter")]
        public string parameter; // WTF? why lowercase?

        [XmlAttribute]
        public string Type; // NONSTANDARD

        [XmlAttribute]
        public string DbType;

        [XmlAttribute]
        public ParameterDirection Direction = ParameterDirection.In;

        [XmlIgnore]
        public bool DirectionIn { get { return Direction == ParameterDirection.In || Direction == ParameterDirection.InOut; } }
        [XmlIgnore]
        public bool DirectionOut { get { return Direction == ParameterDirection.Out || Direction == ParameterDirection.InOut; } }
    }

    /// <summary>
    /// represents a stored procedure parameter direction
    /// </summary>
    public enum ParameterDirection
    {
        In,
        Out,
        InOut,
    }

    /// <summary>
    /// if a stored proc returns a resultset, we represent it as ElementType having columns.
    /// </summary>
    public class ElementType
    {
        [XmlElement("Column")]
        public readonly List<Column> Columns = new List<Column>();
    }


}
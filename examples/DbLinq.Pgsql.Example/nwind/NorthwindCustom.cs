using System;
using System.Data;
using System.Data.Linq.Mapping;
using System.Diagnostics;
using System.Reflection;
using DbLinq.Data.Linq;
using DbLinq.Vendor;

namespace nwind
{
    public partial class NorthwindCustom : DataContext
    {
        public NorthwindCustom(IDbConnection connection)
        : base(connection, new DbLinq.PostgreSql.PgsqlVendor())
        {
        }

        public NorthwindCustom(IDbConnection connection, IVendor vendor)
        : base(connection, vendor)
        {
        }

        public Table<CategoryCustom> Categories { get { return GetTable<CategoryCustom>(); } }
    }

    [Table(Name = "public.\"Categories\"")]
    public partial class CategoryCustom
    {
        public bool propertyInvoked_CategoryName = false;
        public bool propertyInvoked_Description = false;
        
        // Tests the Storage without a setter for the property.
        private int _categoryID;
        [Column(Storage = "_categoryID", Name = "\"CategoryID\"", DbType = "integer(32,0)", IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false, Expression = "nextval('\"Categories_CategoryID_seq\"')")]
        public int CategoryID {
            get { return _categoryID; }
        }

        // No "Storage" attribute, this should go through the property.
        private string _categoryName;
        [Column(Name = "\"CategoryName\"", DbType = "character varying(15)", CanBeNull = false)]
        public string CategoryName {
            get { return _categoryName; }
            set {
                if (value != _categoryName)
                {
                    _categoryName = value;
                }
                propertyInvoked_CategoryName = true;
            }
        }

        // "Storage" and property, should set the field directly.
        private string _description;
        [DebuggerNonUserCode]
        [Column(Storage = "_description", Name = "\"Description\"", DbType = "text")]
        public string Description {
            get { return _description; }
            set
            {
                if (value != _description)
                {
                    _description = value;
                }
                propertyInvoked_Description = true;
            }
        }
    }
}
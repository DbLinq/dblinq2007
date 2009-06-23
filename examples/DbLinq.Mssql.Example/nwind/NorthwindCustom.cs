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
            : base(connection, new DbLinq.SqlServer.SqlServerVendor())
        {
        }

        public NorthwindCustom(IDbConnection connection, IVendor vendor)
            : base(connection, vendor)
        {
        }

        public Table<CategoryCustom> Categories { get { return GetTable<CategoryCustom>(); } }
    }

    partial class Employee
    {
        [Column(Storage = "_EmployeeID", Name = "EmployeeID", DbType = "Int NOT NULL IDENTITY", IsDbGenerated = true)]
        public string Identifier
        {
            get { return this._EmployeeID.ToString(); }
        }
    }

    [Table(Name = "dbo.Categories")]
    public partial class CategoryCustom
    {
        public bool propertyInvoked_CategoryName = false;
        public bool propertyInvoked_Description = false;

        // Tests the Storage without a setter for the property.
        private int _categoryID;
        [Column(Storage = "_CategoryID", AutoSync = AutoSync.OnInsert, DbType = "Int NOT NULL IDENTITY", IsPrimaryKey = true, IsDbGenerated = true)]
        public int CategoryID
        {
            get { return _categoryID; }
        }

        // No "Storage" attribute, this should go through the property.
        private string _categoryName;
        [Column(Storage = "_CategoryName", DbType = "NVarChar(15) NOT NULL", CanBeNull = false)]
        public string CategoryName
        {
            get { return _categoryName; }
            set
            {
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
        [Column(Storage = "_Description", DbType = "NText", UpdateCheck = UpdateCheck.Never)]
        public string Description
        {
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

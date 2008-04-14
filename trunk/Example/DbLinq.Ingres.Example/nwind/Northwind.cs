#region Auto-generated classes for Northwind database on 2008-04-06 12:50:44Z

//
//  ____  _     __  __      _        _
// |  _ \| |__ |  \/  | ___| |_ __ _| |
// | | | | '_ \| |\/| |/ _ \ __/ _` | |
// | |_| | |_) | |  | |  __/ || (_| | |
// |____/|_.__/|_|  |_|\___|\__\__,_|_|
//
// Auto-generated from Northwind on 2008-04-06 12:50:44Z
// Please visit http://linq.to/db for more information

#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Linq.Mapping;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using DbLinq.Linq;
using DbLinq.Linq.Mapping;

namespace nwind
{
	public partial class Northwind : DbLinq.Ingres.IngresDataContext
	{
		//public Northwind(string connectionString)
		//    : base(connectionString)
		//{
		//}

		public Northwind(IDbConnection connection)
		    : base(connection)
		{
		}

		public Table<Category> Categories { get { return GetTable<Category>(); } }
		public Table<Customer> Customers { get { return GetTable<Customer>(); } }
		public Table<Employee> Employees { get { return GetTable<Employee>(); } }
		public Table<EmployeeTerritory> EmployeeTerritories { get { return GetTable<EmployeeTerritory>(); } }
		public Table<IIeTab107108> IIeTab107108 { get { return GetTable<IIeTab107108>(); } }
		public Table<IIeTab12B12C> IIeTab12B12C { get { return GetTable<IIeTab12B12C>(); } }
		public Table<Order> Orders { get { return GetTable<Order>(); } }
		public Table<OrderDetail> OrderDetails { get { return GetTable<OrderDetail>(); } }
		public Table<Product> Products { get { return GetTable<Product>(); } }
		public Table<Region> Regions { get { return GetTable<Region>(); } }
		public Table<Supplier> Suppliers { get { return GetTable<Supplier>(); } }
		public Table<Territory> Territories { get { return GetTable<Territory>(); } }

	}

	[Table(Name = "linquser.categories")]
	public partial class Category : IModified
	{
		// IModified backing field
		public bool IsModified{ get; set; }

		#region int CategoryID

		[AutoGenId]
		private int categoryID;
		[Column(Storage = "categoryID", Name = "categoryid", DbType = "INTEGER(4)", IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false, Expression = "next value for \"linquser\".\"categories_seq\"")]
		[DebuggerNonUserCode]
		public int CategoryID
		{
			get
			{
				return categoryID;
			}
			set
			{
				if (value != categoryID)
				{
					categoryID = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region string CategoryName

		private string categoryName;
		[Column(Storage = "categoryName", Name = "categoryname", DbType = "VARCHAR(15)", CanBeNull = false)]
		[DebuggerNonUserCode]
		public string CategoryName
		{
			get
			{
				return categoryName;
			}
			set
			{
				if (value != categoryName)
				{
					categoryName = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region string Description

		private string description;
		[Column(Storage = "description", Name = "description", DbType = "VARCHAR(500)")]
		[DebuggerNonUserCode]
		public string Description
		{
			get
			{
				return description;
			}
			set
			{
				if (value != description)
				{
					description = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region System.Byte[] Picture

		private System.Byte[] picture;
		[Column(Storage = "picture", Name = "picture", DbType = "LONG BYTE")]
		[DebuggerNonUserCode]
		public System.Byte[] Picture
		{
			get
			{
				return picture;
			}
			set
			{
				if (value != picture)
				{
					picture = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region GetHashCode(), Equals() - uses column CategoryID to look up objects in liveObjectMap

		public override int GetHashCode()
		{
			return CategoryID.GetHashCode();
		}

		public override bool Equals(object o)
		{
			Category other = o as Category;
			if (other == null)
			{
				return false;
			}
			return CategoryID.Equals(other.CategoryID);
		}

		#endregion

		#region Children

		[Association(Storage = "null", OtherKey = "CategoryID", Name = "linquser_products_categoryid_linquser_categories_categoryid")]
		[DebuggerNonUserCode]
		public EntityMSet<Product> Products
		{
			get
			{
				// L212 - child data available only when part of query
				return null;
			}
		}


		#endregion

	}

    // This edit solves Issue 56, as picrap did it in rev316
    // But I don't know why...
	[Table(Name = "linquser.customers")]
	public partial class Customer //: IModified
	{
		// IModified backing field
		public bool IsModified{ get; set; }

		#region string Address

		private string address;
		[Column(Storage = "address", Name = "address", DbType = "VARCHAR(60)")]
		[DebuggerNonUserCode]
		public string Address
		{
			get
			{
				return address;
			}
			set
			{
				if (value != address)
				{
					address = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region string City

		private string city;
		[Column(Storage = "city", Name = "city", DbType = "VARCHAR(15)")]
		[DebuggerNonUserCode]
		public string City
		{
			get
			{
				return city;
			}
			set
			{
				if (value != city)
				{
					city = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region string CompanyName

		private string companyName;
		[Column(Storage = "companyName", Name = "companyname", DbType = "VARCHAR(40)", CanBeNull = false)]
		[DebuggerNonUserCode]
		public string CompanyName
		{
			get
			{
				return companyName;
			}
			set
			{
				if (value != companyName)
				{
					companyName = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region string ContactName

		private string contactName;
		[Column(Storage = "contactName", Name = "contactname", DbType = "VARCHAR(30)")]
		[DebuggerNonUserCode]
		public string ContactName
		{
			get
			{
				return contactName;
			}
			set
			{
				if (value != contactName)
				{
					contactName = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region string ContactTitle

		private string contactTitle;
		[Column(Storage = "contactTitle", Name = "contacttitle", DbType = "VARCHAR(30)")]
		[DebuggerNonUserCode]
		public string ContactTitle
		{
			get
			{
				return contactTitle;
			}
			set
			{
				if (value != contactTitle)
				{
					contactTitle = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region string Country

		private string country;
		[Column(Storage = "country", Name = "country", DbType = "VARCHAR(15)")]
		[DebuggerNonUserCode]
		public string Country
		{
			get
			{
				return country;
			}
			set
			{
				if (value != country)
				{
					country = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region string CustomerID

		private string customerID;
		[Column(Storage = "customerID", Name = "customerid", DbType = "VARCHAR(5)", IsPrimaryKey = true, CanBeNull = false)]
		[DebuggerNonUserCode]
		public string CustomerID
		{
			get
			{
				return customerID;
			}
			set
			{
				if (value != customerID)
				{
					customerID = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region string Fax

		private string fax;
		[Column(Storage = "fax", Name = "fax", DbType = "VARCHAR(24)")]
		[DebuggerNonUserCode]
		public string Fax
		{
			get
			{
				return fax;
			}
			set
			{
				if (value != fax)
				{
					fax = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region string Phone

		private string phone;
		[Column(Storage = "phone", Name = "phone", DbType = "VARCHAR(24)")]
		[DebuggerNonUserCode]
		public string Phone
		{
			get
			{
				return phone;
			}
			set
			{
				if (value != phone)
				{
					phone = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region string PostalCode

		private string postalCode;
		[Column(Storage = "postalCode", Name = "postalcode", DbType = "VARCHAR(10)")]
		[DebuggerNonUserCode]
		public string PostalCode
		{
			get
			{
				return postalCode;
			}
			set
			{
				if (value != postalCode)
				{
					postalCode = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region string Region

		private string region;
		[Column(Storage = "region", Name = "region", DbType = "VARCHAR(15)")]
		[DebuggerNonUserCode]
		public string Region
		{
			get
			{
				return region;
			}
			set
			{
				if (value != region)
				{
					region = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region GetHashCode(), Equals() - uses column CustomerID to look up objects in liveObjectMap

		public override int GetHashCode()
		{
			return CustomerID.GetHashCode();
		}

		public override bool Equals(object o)
		{
			Customer other = o as Customer;
			if (other == null)
			{
				return false;
			}
			return CustomerID.Equals(other.CustomerID);
		}

		#endregion

		#region Children

		[Association(Storage = "null", OtherKey = "CustomerID", Name = "linquser_orders_customerid_linquser_customers_customerid")]
		[DebuggerNonUserCode]
		public EntityMSet<Order> Orders
		{
			get
			{
				// L212 - child data available only when part of query
				return null;
			}
		}


		#endregion

	}

	[Table(Name = "linquser.employees")]
	public partial class Employee : IModified
	{
		// IModified backing field
		public bool IsModified{ get; set; }

		#region string Address

		private string address;
		[Column(Storage = "address", Name = "address", DbType = "VARCHAR(60)")]
		[DebuggerNonUserCode]
		public string Address
		{
			get
			{
				return address;
			}
			set
			{
				if (value != address)
				{
					address = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region System.DateTime? BirthDate

		private System.DateTime? birthDate;
		[Column(Storage = "birthDate", Name = "birthdate", DbType = "INGRESDATE")]
		[DebuggerNonUserCode]
		public System.DateTime? BirthDate
		{
			get
			{
				return birthDate;
			}
			set
			{
				if (value != birthDate)
				{
					birthDate = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region string City

		private string city;
		[Column(Storage = "city", Name = "city", DbType = "VARCHAR(15)")]
		[DebuggerNonUserCode]
		public string City
		{
			get
			{
				return city;
			}
			set
			{
				if (value != city)
				{
					city = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region string Country

		private string country;
		[Column(Storage = "country", Name = "country", DbType = "VARCHAR(15)")]
		[DebuggerNonUserCode]
		public string Country
		{
			get
			{
				return country;
			}
			set
			{
				if (value != country)
				{
					country = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region int EmployeeID

		[AutoGenId]
		private int employeeID;
		[Column(Storage = "employeeID", Name = "employeeid", DbType = "INTEGER(4)", IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false, Expression = "next value for \"linquser\".\"employees_seq\"")]
		[DebuggerNonUserCode]
		public int EmployeeID
		{
			get
			{
				return employeeID;
			}
			set
			{
				if (value != employeeID)
				{
					employeeID = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region string FirstName

		private string firstName;
		[Column(Storage = "firstName", Name = "firstname", DbType = "VARCHAR(10)", CanBeNull = false)]
		[DebuggerNonUserCode]
		public string FirstName
		{
			get
			{
				return firstName;
			}
			set
			{
				if (value != firstName)
				{
					firstName = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region System.DateTime? HireDate

		private System.DateTime? hireDate;
		[Column(Storage = "hireDate", Name = "hiredate", DbType = "INGRESDATE")]
		[DebuggerNonUserCode]
		public System.DateTime? HireDate
		{
			get
			{
				return hireDate;
			}
			set
			{
				if (value != hireDate)
				{
					hireDate = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region string HomePhone

		private string homePhone;
		[Column(Storage = "homePhone", Name = "homephone", DbType = "VARCHAR(24)")]
		[DebuggerNonUserCode]
		public string HomePhone
		{
			get
			{
				return homePhone;
			}
			set
			{
				if (value != homePhone)
				{
					homePhone = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region string LastName

		private string lastName;
		[Column(Storage = "lastName", Name = "lastname", DbType = "VARCHAR(20)", CanBeNull = false)]
		[DebuggerNonUserCode]
		public string LastName
		{
			get
			{
				return lastName;
			}
			set
			{
				if (value != lastName)
				{
					lastName = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region string Notes

		private string notes;
		[Column(Storage = "notes", Name = "notes", DbType = "VARCHAR(100)")]
		[DebuggerNonUserCode]
		public string Notes
		{
			get
			{
				return notes;
			}
			set
			{
				if (value != notes)
				{
					notes = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region System.Byte[] Photo

		private System.Byte[] photo;
		[Column(Storage = "photo", Name = "photo", DbType = "LONG BYTE")]
		[DebuggerNonUserCode]
		public System.Byte[] Photo
		{
			get
			{
				return photo;
			}
			set
			{
				if (value != photo)
				{
					photo = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region string PostalCode

		private string postalCode;
		[Column(Storage = "postalCode", Name = "postalcode", DbType = "VARCHAR(10)")]
		[DebuggerNonUserCode]
		public string PostalCode
		{
			get
			{
				return postalCode;
			}
			set
			{
				if (value != postalCode)
				{
					postalCode = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region string Region

		private string region;
		[Column(Storage = "region", Name = "region", DbType = "VARCHAR(15)")]
		[DebuggerNonUserCode]
		public string Region
		{
			get
			{
				return region;
			}
			set
			{
				if (value != region)
				{
					region = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region int? ReportsTo

		private int? reportsTo;
		[Column(Storage = "reportsTo", Name = "reportsto", DbType = "INTEGER(4)")]
		[DebuggerNonUserCode]
		public int? ReportsTo
		{
			get
			{
				return reportsTo;
			}
			set
			{
				if (value != reportsTo)
				{
					reportsTo = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region string Title

		private string title;
		[Column(Storage = "title", Name = "title", DbType = "VARCHAR(30)")]
		[DebuggerNonUserCode]
		public string Title
		{
			get
			{
				return title;
			}
			set
			{
				if (value != title)
				{
					title = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region GetHashCode(), Equals() - uses column EmployeeID to look up objects in liveObjectMap

		public override int GetHashCode()
		{
			return EmployeeID.GetHashCode();
		}

		public override bool Equals(object o)
		{
			Employee other = o as Employee;
			if (other == null)
			{
				return false;
			}
			return EmployeeID.Equals(other.EmployeeID);
		}

		#endregion

		#region Children

		[Association(Storage = "null", OtherKey = "EmployeeID", Name = "linquser_employees_reportsto_linquser_employees_employeeid")]
		[DebuggerNonUserCode]
		public EntityMSet<Employee> Employees
		{
			get
			{
				// L212 - child data available only when part of query
				return null;
			}
		}

		[Association(Storage = "null", OtherKey = "EmployeeID", Name = "linquser_employeeterritories_employeeid_linquser_employees_employeeid")]
		[DebuggerNonUserCode]
		public EntityMSet<EmployeeTerritory> EmployeeTerritories
		{
			get
			{
				// L212 - child data available only when part of query
				return null;
			}
		}

		[Association(Storage = "null", OtherKey = "EmployeeID", Name = "linquser_orders_employeeid_linquser_employees_employeeid")]
		[DebuggerNonUserCode]
		public EntityMSet<Order> Orders
		{
			get
			{
				// L212 - child data available only when part of query
				return null;
			}
		}


		#endregion

		#region Parents

		private System.Data.Linq.EntityRef<Employee> linqUserEmployeesReportsToLinqUserEmployeesEmployeeID;
		[Association(Storage = "linqUserEmployeesReportsToLinqUserEmployeesEmployeeID", ThisKey = "ReportsTo", Name = "linquser_employees_reportsto_linquser_employees_employeeid")]
		[DebuggerNonUserCode]
		public Employee ParentEmployee
		{
			get
			{
				return linqUserEmployeesReportsToLinqUserEmployeesEmployeeID.Entity;
			}
			set
			{
				linqUserEmployeesReportsToLinqUserEmployeesEmployeeID.Entity = value;
			}
		}


		#endregion

	}

	[Table(Name = "linquser.employeeterritories")]
	public partial class EmployeeTerritory : IModified
	{
		// IModified backing field
		public bool IsModified{ get; set; }

		#region int EmployeeID

		private int employeeID;
		[Column(Storage = "employeeID", Name = "employeeid", DbType = "INTEGER(4)", IsPrimaryKey = true, CanBeNull = false)]
		[DebuggerNonUserCode]
		public int EmployeeID
		{
			get
			{
				return employeeID;
			}
			set
			{
				if (value != employeeID)
				{
					employeeID = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region string TerritoryID

		private string territoryID;
		[Column(Storage = "territoryID", Name = "territoryid", DbType = "VARCHAR(20)", IsPrimaryKey = true, CanBeNull = false)]
		[DebuggerNonUserCode]
		public string TerritoryID
		{
			get
			{
				return territoryID;
			}
			set
			{
				if (value != territoryID)
				{
					territoryID = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region GetHashCode(), Equals() - uses column EmployeeID, TerritoryID to look up objects in liveObjectMap

		public override int GetHashCode()
		{
			return EmployeeID.GetHashCode() ^ TerritoryID.GetHashCode();
		}

		public override bool Equals(object o)
		{
			EmployeeTerritory other = o as EmployeeTerritory;
			if (other == null)
			{
				return false;
			}
			return EmployeeID.Equals(other.EmployeeID) && TerritoryID.Equals(other.TerritoryID);
		}

		#endregion

		#region Parents

		private System.Data.Linq.EntityRef<Territory> linqUserEmployeeTerritoriesTerritoryIDLinqUserTerritoriesTerritoryID;
		[Association(Storage = "linqUserEmployeeTerritoriesTerritoryIDLinqUserTerritoriesTerritoryID", ThisKey = "TerritoryID", Name = "linquser_employeeterritories_territoryid_linquser_territories_territoryid")]
		[DebuggerNonUserCode]
		public Territory Territory
		{
			get
			{
				return linqUserEmployeeTerritoriesTerritoryIDLinqUserTerritoriesTerritoryID.Entity;
			}
			set
			{
				linqUserEmployeeTerritoriesTerritoryIDLinqUserTerritoriesTerritoryID.Entity = value;
			}
		}

		private System.Data.Linq.EntityRef<Employee> linqUserEmployeeTerritoriesEmployeeIDLinqUserEmployeesEmployeeID;
		[Association(Storage = "linqUserEmployeeTerritoriesEmployeeIDLinqUserEmployeesEmployeeID", ThisKey = "EmployeeID", Name = "linquser_employeeterritories_employeeid_linquser_employees_employeeid")]
		[DebuggerNonUserCode]
		public Employee Employee
		{
			get
			{
				return linqUserEmployeeTerritoriesEmployeeIDLinqUserEmployeesEmployeeID.Entity;
			}
			set
			{
				linqUserEmployeeTerritoriesEmployeeIDLinqUserEmployeesEmployeeID.Entity = value;
			}
		}


		#endregion

	}

	[Table(Name = "linquser.iietab_107_108")]
	public partial class IIeTab107108 : IModified
	{
		// IModified backing field
		public bool IsModified{ get; set; }

		#region string PerKey

		private string perKey;
		[Column(Storage = "perKey", Name = "per_key", DbType = "CHAR(8)", CanBeNull = false)]
		[DebuggerNonUserCode]
		public string PerKey
		{
			get
			{
				return perKey;
			}
			set
			{
				if (value != perKey)
				{
					perKey = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region int PerNext

		private int perNext;
		[Column(Storage = "perNext", Name = "per_next", DbType = "INTEGER(4)", CanBeNull = false)]
		[DebuggerNonUserCode]
		public int PerNext
		{
			get
			{
				return perNext;
			}
			set
			{
				if (value != perNext)
				{
					perNext = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region int PerSegment0

		private int perSegment0;
		[Column(Storage = "perSegment0", Name = "per_segment0", DbType = "INTEGER(4)", CanBeNull = false)]
		[DebuggerNonUserCode]
		public int PerSegment0
		{
			get
			{
				return perSegment0;
			}
			set
			{
				if (value != perSegment0)
				{
					perSegment0 = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region int PerSegment1

		private int perSegment1;
		[Column(Storage = "perSegment1", Name = "per_segment1", DbType = "INTEGER(4)", CanBeNull = false)]
		[DebuggerNonUserCode]
		public int PerSegment1
		{
			get
			{
				return perSegment1;
			}
			set
			{
				if (value != perSegment1)
				{
					perSegment1 = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region System.Byte[] PerValue

		private System.Byte[] perValue;
		[Column(Storage = "perValue", Name = "per_value", DbType = "BYTE VARYING", CanBeNull = false)]
		[DebuggerNonUserCode]
		public System.Byte[] PerValue
		{
			get
			{
				return perValue;
			}
			set
			{
				if (value != perValue)
				{
					perValue = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#warning L189 table linquser.iietab_107_108 has no primary key. Multiple C# objects will refer to the same row.
	}

	[Table(Name = "linquser.iietab_12b_12c")]
	public partial class IIeTab12B12C : IModified
	{
		// IModified backing field
		public bool IsModified{ get; set; }

		#region string PerKey

		private string perKey;
		[Column(Storage = "perKey", Name = "per_key", DbType = "CHAR(8)", CanBeNull = false)]
		[DebuggerNonUserCode]
		public string PerKey
		{
			get
			{
				return perKey;
			}
			set
			{
				if (value != perKey)
				{
					perKey = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region int PerNext

		private int perNext;
		[Column(Storage = "perNext", Name = "per_next", DbType = "INTEGER(4)", CanBeNull = false)]
		[DebuggerNonUserCode]
		public int PerNext
		{
			get
			{
				return perNext;
			}
			set
			{
				if (value != perNext)
				{
					perNext = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region int PerSegment0

		private int perSegment0;
		[Column(Storage = "perSegment0", Name = "per_segment0", DbType = "INTEGER(4)", CanBeNull = false)]
		[DebuggerNonUserCode]
		public int PerSegment0
		{
			get
			{
				return perSegment0;
			}
			set
			{
				if (value != perSegment0)
				{
					perSegment0 = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region int PerSegment1

		private int perSegment1;
		[Column(Storage = "perSegment1", Name = "per_segment1", DbType = "INTEGER(4)", CanBeNull = false)]
		[DebuggerNonUserCode]
		public int PerSegment1
		{
			get
			{
				return perSegment1;
			}
			set
			{
				if (value != perSegment1)
				{
					perSegment1 = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region System.Byte[] PerValue

		private System.Byte[] perValue;
		[Column(Storage = "perValue", Name = "per_value", DbType = "BYTE VARYING", CanBeNull = false)]
		[DebuggerNonUserCode]
		public System.Byte[] PerValue
		{
			get
			{
				return perValue;
			}
			set
			{
				if (value != perValue)
				{
					perValue = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#warning L189 table linquser.iietab_12b_12c has no primary key. Multiple C# objects will refer to the same row.
	}

	[Table(Name = "linquser.orders")]
	public partial class Order : IModified
	{
		// IModified backing field
		public bool IsModified{ get; set; }

		#region string CustomerID

		private string customerID;
		[Column(Storage = "customerID", Name = "customerid", DbType = "VARCHAR(5)")]
		[DebuggerNonUserCode]
		public string CustomerID
		{
			get
			{
				return customerID;
			}
			set
			{
				if (value != customerID)
				{
					customerID = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region int? EmployeeID

		private int? employeeID;
		[Column(Storage = "employeeID", Name = "employeeid", DbType = "INTEGER(4)")]
		[DebuggerNonUserCode]
		public int? EmployeeID
		{
			get
			{
				return employeeID;
			}
			set
			{
				if (value != employeeID)
				{
					employeeID = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region decimal? Freight

		private decimal? freight;
		[Column(Storage = "freight", Name = "freight", DbType = "DECIMAL(5, 0)")]
		[DebuggerNonUserCode]
		public decimal? Freight
		{
			get
			{
				return freight;
			}
			set
			{
				if (value != freight)
				{
					freight = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region System.DateTime? OrderDate

		private System.DateTime? orderDate;
		[Column(Storage = "orderDate", Name = "orderdate", DbType = "INGRESDATE")]
		[DebuggerNonUserCode]
		public System.DateTime? OrderDate
		{
			get
			{
				return orderDate;
			}
			set
			{
				if (value != orderDate)
				{
					orderDate = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region int OrderID

		[AutoGenId]
		private int orderID;
		[Column(Storage = "orderID", Name = "orderid", DbType = "INTEGER(4)", IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false, Expression = "next value for \"linquser\".\"categories_seq\"")]
		[DebuggerNonUserCode]
		public int OrderID
		{
			get
			{
				return orderID;
			}
			set
			{
				if (value != orderID)
				{
					orderID = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region System.DateTime? RequiredDate

		private System.DateTime? requiredDate;
		[Column(Storage = "requiredDate", Name = "requireddate", DbType = "INGRESDATE")]
		[DebuggerNonUserCode]
		public System.DateTime? RequiredDate
		{
			get
			{
				return requiredDate;
			}
			set
			{
				if (value != requiredDate)
				{
					requiredDate = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region string ShipAddress

		private string shipAddress;
		[Column(Storage = "shipAddress", Name = "shipaddress", DbType = "VARCHAR(60)")]
		[DebuggerNonUserCode]
		public string ShipAddress
		{
			get
			{
				return shipAddress;
			}
			set
			{
				if (value != shipAddress)
				{
					shipAddress = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region string ShipCity

		private string shipCity;
		[Column(Storage = "shipCity", Name = "shipcity", DbType = "VARCHAR(15)")]
		[DebuggerNonUserCode]
		public string ShipCity
		{
			get
			{
				return shipCity;
			}
			set
			{
				if (value != shipCity)
				{
					shipCity = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region string ShipCountry

		private string shipCountry;
		[Column(Storage = "shipCountry", Name = "shipcountry", DbType = "VARCHAR(15)")]
		[DebuggerNonUserCode]
		public string ShipCountry
		{
			get
			{
				return shipCountry;
			}
			set
			{
				if (value != shipCountry)
				{
					shipCountry = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region string ShipName

		private string shipName;
		[Column(Storage = "shipName", Name = "shipname", DbType = "VARCHAR(40)")]
		[DebuggerNonUserCode]
		public string ShipName
		{
			get
			{
				return shipName;
			}
			set
			{
				if (value != shipName)
				{
					shipName = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region System.DateTime? ShippedDate

		private System.DateTime? shippedDate;
		[Column(Storage = "shippedDate", Name = "shippeddate", DbType = "INGRESDATE")]
		[DebuggerNonUserCode]
		public System.DateTime? ShippedDate
		{
			get
			{
				return shippedDate;
			}
			set
			{
				if (value != shippedDate)
				{
					shippedDate = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region string ShipPostalCode

		private string shipPostalCode;
		[Column(Storage = "shipPostalCode", Name = "shippostalcode", DbType = "VARCHAR(10)")]
		[DebuggerNonUserCode]
		public string ShipPostalCode
		{
			get
			{
				return shipPostalCode;
			}
			set
			{
				if (value != shipPostalCode)
				{
					shipPostalCode = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region string ShipRegion

		private string shipRegion;
		[Column(Storage = "shipRegion", Name = "shipregion", DbType = "VARCHAR(15)")]
		[DebuggerNonUserCode]
		public string ShipRegion
		{
			get
			{
				return shipRegion;
			}
			set
			{
				if (value != shipRegion)
				{
					shipRegion = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region int? ShipVia

		private int? shipVia;
		[Column(Storage = "shipVia", Name = "shipvia", DbType = "INTEGER(4)")]
		[DebuggerNonUserCode]
		public int? ShipVia
		{
			get
			{
				return shipVia;
			}
			set
			{
				if (value != shipVia)
				{
					shipVia = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region GetHashCode(), Equals() - uses column OrderID to look up objects in liveObjectMap

		public override int GetHashCode()
		{
			return OrderID.GetHashCode();
		}

		public override bool Equals(object o)
		{
			Order other = o as Order;
			if (other == null)
			{
				return false;
			}
			return OrderID.Equals(other.OrderID);
		}

		#endregion

		#region Children

		[Association(Storage = "null", OtherKey = "OrderID", Name = "linquser_orderdetails_orderid_linquser_orders_orderid")]
		[DebuggerNonUserCode]
		public EntityMSet<OrderDetail> OrderDetails
		{
			get
			{
				// L212 - child data available only when part of query
				return null;
			}
		}


		#endregion

		#region Parents

		private System.Data.Linq.EntityRef<Employee> linqUserOrdersEmployeeIDLinqUserEmployeesEmployeeID;
		[Association(Storage = "linqUserOrdersEmployeeIDLinqUserEmployeesEmployeeID", ThisKey = "EmployeeID", Name = "linquser_orders_employeeid_linquser_employees_employeeid")]
		[DebuggerNonUserCode]
		public Employee Employee
		{
			get
			{
				return linqUserOrdersEmployeeIDLinqUserEmployeesEmployeeID.Entity;
			}
			set
			{
				linqUserOrdersEmployeeIDLinqUserEmployeesEmployeeID.Entity = value;
			}
		}

		private System.Data.Linq.EntityRef<Customer> linqUserOrdersCustomerIDLinqUserCustomersCustomerID;
		[Association(Storage = "linqUserOrdersCustomerIDLinqUserCustomersCustomerID", ThisKey = "CustomerID", Name = "linquser_orders_customerid_linquser_customers_customerid")]
		[DebuggerNonUserCode]
		public Customer Customer
		{
			get
			{
				return linqUserOrdersCustomerIDLinqUserCustomersCustomerID.Entity;
			}
			set
			{
				linqUserOrdersCustomerIDLinqUserCustomersCustomerID.Entity = value;
			}
		}


		#endregion

	}

	[Table(Name = "linquser.orderdetails")]
	public partial class OrderDetail : IModified
	{
		// IModified backing field
		public bool IsModified{ get; set; }

		#region float Discount

		private float discount;
		[Column(Storage = "discount", Name = "discount", DbType = "FLOAT", CanBeNull = false)]
		[DebuggerNonUserCode]
		public float Discount
		{
			get
			{
				return discount;
			}
			set
			{
				if (value != discount)
				{
					discount = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region int OrderID

		private int orderID;
		[Column(Storage = "orderID", Name = "orderid", DbType = "INTEGER(4)", IsPrimaryKey = true, CanBeNull = false)]
		[DebuggerNonUserCode]
		public int OrderID
		{
			get
			{
				return orderID;
			}
			set
			{
				if (value != orderID)
				{
					orderID = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region int ProductID

		private int productID;
		[Column(Storage = "productID", Name = "productid", DbType = "INTEGER(4)", IsPrimaryKey = true, CanBeNull = false)]
		[DebuggerNonUserCode]
		public int ProductID
		{
			get
			{
				return productID;
			}
			set
			{
				if (value != productID)
				{
					productID = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region short Quantity

		private short quantity;
		[Column(Storage = "quantity", Name = "quantity", DbType = "INTEGER(2)", CanBeNull = false)]
		[DebuggerNonUserCode]
		public short Quantity
		{
			get
			{
				return quantity;
			}
			set
			{
				if (value != quantity)
				{
					quantity = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region decimal UnitPrice

		private decimal unitPrice;
		[Column(Storage = "unitPrice", Name = "unitprice", DbType = "DECIMAL(5, 0)", CanBeNull = false)]
		[DebuggerNonUserCode]
		public decimal UnitPrice
		{
			get
			{
				return unitPrice;
			}
			set
			{
				if (value != unitPrice)
				{
					unitPrice = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region GetHashCode(), Equals() - uses column OrderID, ProductID to look up objects in liveObjectMap

		public override int GetHashCode()
		{
			return OrderID.GetHashCode() ^ ProductID.GetHashCode();
		}

		public override bool Equals(object o)
		{
			OrderDetail other = o as OrderDetail;
			if (other == null)
			{
				return false;
			}
			return OrderID.Equals(other.OrderID) && ProductID.Equals(other.ProductID);
		}

		#endregion

		#region Parents

		private System.Data.Linq.EntityRef<Product> linqUserOrderDetailsProductIDLinqUserProductsProductID;
		[Association(Storage = "linqUserOrderDetailsProductIDLinqUserProductsProductID", ThisKey = "ProductID", Name = "linquser_orderdetails_productid_linquser_products_productid")]
		[DebuggerNonUserCode]
		public Product Product
		{
			get
			{
				return linqUserOrderDetailsProductIDLinqUserProductsProductID.Entity;
			}
			set
			{
				linqUserOrderDetailsProductIDLinqUserProductsProductID.Entity = value;
			}
		}

		private System.Data.Linq.EntityRef<Order> linqUserOrderDetailsOrderIDLinqUserOrdersOrderID;
		[Association(Storage = "linqUserOrderDetailsOrderIDLinqUserOrdersOrderID", ThisKey = "OrderID", Name = "linquser_orderdetails_orderid_linquser_orders_orderid")]
		[DebuggerNonUserCode]
		public Order Order
		{
			get
			{
				return linqUserOrderDetailsOrderIDLinqUserOrdersOrderID.Entity;
			}
			set
			{
				linqUserOrderDetailsOrderIDLinqUserOrdersOrderID.Entity = value;
			}
		}


		#endregion

	}

	[Table(Name = "linquser.products")]
	public partial class Product : IModified
	{
		// IModified backing field
		public bool IsModified{ get; set; }

		#region int? CategoryID

		private int? categoryID;
		[Column(Storage = "categoryID", Name = "categoryid", DbType = "INTEGER(4)")]
		[DebuggerNonUserCode]
		public int? CategoryID
		{
			get
			{
				return categoryID;
			}
			set
			{
				if (value != categoryID)
				{
					categoryID = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region bool Discontinued

        private bool discontinued;
		[Column(Storage = "discontinued", Name = "discontinued", DbType = "INTEGER(2)", CanBeNull = false)]
		[DebuggerNonUserCode]
        public bool Discontinued
		{
			get
			{
				return discontinued;
			}
			set
			{
				if (value != discontinued)
				{
					discontinued = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region int ProductID

		[AutoGenId]
		private int productID;
		[Column(Storage = "productID", Name = "productid", DbType = "INTEGER(4)", IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false, Expression = "next value for \"linquser\".\"products_seq\"")]
		[DebuggerNonUserCode]
		public int ProductID
		{
			get
			{
				return productID;
			}
			set
			{
				if (value != productID)
				{
					productID = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region string ProductName

		private string productName;
		[Column(Storage = "productName", Name = "productname", DbType = "VARCHAR(40)", CanBeNull = false)]
		[DebuggerNonUserCode]
		public string ProductName
		{
			get
			{
				return productName;
			}
			set
			{
				if (value != productName)
				{
					productName = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region string QuantityPerUnit

		private string quantityPerUnit;
		[Column(Storage = "quantityPerUnit", Name = "quantityperunit", DbType = "VARCHAR(20)")]
		[DebuggerNonUserCode]
		public string QuantityPerUnit
		{
			get
			{
				return quantityPerUnit;
			}
			set
			{
				if (value != quantityPerUnit)
				{
					quantityPerUnit = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region short? ReorderLevel

		private short? reorderLevel;
		[Column(Storage = "reorderLevel", Name = "reorderlevel", DbType = "INTEGER(2)")]
		[DebuggerNonUserCode]
		public short? ReorderLevel
		{
			get
			{
				return reorderLevel;
			}
			set
			{
				if (value != reorderLevel)
				{
					reorderLevel = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region int? SupplierID

		private int? supplierID;
		[Column(Storage = "supplierID", Name = "supplierid", DbType = "INTEGER(4)")]
		[DebuggerNonUserCode]
		public int? SupplierID
		{
			get
			{
				return supplierID;
			}
			set
			{
				if (value != supplierID)
				{
					supplierID = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region decimal? UnitPrice

		private decimal? unitPrice;
		[Column(Storage = "unitPrice", Name = "unitprice", DbType = "DECIMAL(5, 0)")]
		[DebuggerNonUserCode]
		public decimal? UnitPrice
		{
			get
			{
				return unitPrice;
			}
			set
			{
				if (value != unitPrice)
				{
					unitPrice = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region short? UnitsInStock

		private short? unitsInStock;
		[Column(Storage = "unitsInStock", Name = "unitsinstock", DbType = "INTEGER(2)")]
		[DebuggerNonUserCode]
		public short? UnitsInStock
		{
			get
			{
				return unitsInStock;
			}
			set
			{
				if (value != unitsInStock)
				{
					unitsInStock = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region short? UnitsOnOrder

		private short? unitsOnOrder;
		[Column(Storage = "unitsOnOrder", Name = "unitsonorder", DbType = "INTEGER(2)")]
		[DebuggerNonUserCode]
		public short? UnitsOnOrder
		{
			get
			{
				return unitsOnOrder;
			}
			set
			{
				if (value != unitsOnOrder)
				{
					unitsOnOrder = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region GetHashCode(), Equals() - uses column ProductID to look up objects in liveObjectMap

		public override int GetHashCode()
		{
			return ProductID.GetHashCode();
		}

		public override bool Equals(object o)
		{
			Product other = o as Product;
			if (other == null)
			{
				return false;
			}
			return ProductID.Equals(other.ProductID);
		}

		#endregion

		#region Children

		[Association(Storage = "null", OtherKey = "ProductID", Name = "linquser_orderdetails_productid_linquser_products_productid")]
		[DebuggerNonUserCode]
		public EntityMSet<OrderDetail> OrderDetails
		{
			get
			{
				// L212 - child data available only when part of query
				return null;
			}
		}


		#endregion

		#region Parents

		private System.Data.Linq.EntityRef<Supplier> linqUserProductsSupplierIDLinqUserSuppliersSupplierID;
		[Association(Storage = "linqUserProductsSupplierIDLinqUserSuppliersSupplierID", ThisKey = "SupplierID", Name = "linquser_products_supplierid_linquser_suppliers_supplierid")]
		[DebuggerNonUserCode]
		public Supplier Supplier
		{
			get
			{
				return linqUserProductsSupplierIDLinqUserSuppliersSupplierID.Entity;
			}
			set
			{
				linqUserProductsSupplierIDLinqUserSuppliersSupplierID.Entity = value;
			}
		}

		private System.Data.Linq.EntityRef<Category> linqUserProductsCategoryIDLinqUserCategoriesCategoryID;
		[Association(Storage = "linqUserProductsCategoryIDLinqUserCategoriesCategoryID", ThisKey = "CategoryID", Name = "linquser_products_categoryid_linquser_categories_categoryid")]
		[DebuggerNonUserCode]
		public Category Category
		{
			get
			{
				return linqUserProductsCategoryIDLinqUserCategoriesCategoryID.Entity;
			}
			set
			{
				linqUserProductsCategoryIDLinqUserCategoriesCategoryID.Entity = value;
			}
		}


		#endregion

	}

	[Table(Name = "linquser.region")]
	public partial class Region : IModified
	{
		// IModified backing field
		public bool IsModified{ get; set; }

		#region string RegionDescription

		private string regionDescription;
		[Column(Storage = "regionDescription", Name = "regiondescription", DbType = "VARCHAR(50)", CanBeNull = false)]
		[DebuggerNonUserCode]
		public string RegionDescription
		{
			get
			{
				return regionDescription;
			}
			set
			{
				if (value != regionDescription)
				{
					regionDescription = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region int RegionID

		[AutoGenId]
		private int regionID;
		[Column(Storage = "regionID", Name = "regionid", DbType = "INTEGER(4)", IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false, Expression = "next value for \"linquser\".\"region_seq\"")]
		[DebuggerNonUserCode]
		public int RegionID
		{
			get
			{
				return regionID;
			}
			set
			{
				if (value != regionID)
				{
					regionID = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region GetHashCode(), Equals() - uses column RegionID to look up objects in liveObjectMap

		public override int GetHashCode()
		{
			return RegionID.GetHashCode();
		}

		public override bool Equals(object o)
		{
			Region other = o as Region;
			if (other == null)
			{
				return false;
			}
			return RegionID.Equals(other.RegionID);
		}

		#endregion

		#region Children

		[Association(Storage = "null", OtherKey = "RegionID", Name = "linquser_territories_regionid_linquser_region_regionid")]
		[DebuggerNonUserCode]
		public EntityMSet<Territory> Territories
		{
			get
			{
				// L212 - child data available only when part of query
				return null;
			}
		}


		#endregion

	}

	[Table(Name = "linquser.suppliers")]
	public partial class Supplier : IModified
	{
		// IModified backing field
		public bool IsModified{ get; set; }

		#region string Address

		private string address;
		[Column(Storage = "address", Name = "address", DbType = "VARCHAR(60)")]
		[DebuggerNonUserCode]
		public string Address
		{
			get
			{
				return address;
			}
			set
			{
				if (value != address)
				{
					address = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region string City

		private string city;
		[Column(Storage = "city", Name = "city", DbType = "VARCHAR(15)")]
		[DebuggerNonUserCode]
		public string City
		{
			get
			{
				return city;
			}
			set
			{
				if (value != city)
				{
					city = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region string CompanyName

		private string companyName;
		[Column(Storage = "companyName", Name = "companyname", DbType = "VARCHAR(40)", CanBeNull = false)]
		[DebuggerNonUserCode]
		public string CompanyName
		{
			get
			{
				return companyName;
			}
			set
			{
				if (value != companyName)
				{
					companyName = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region string ContactName

		private string contactName;
		[Column(Storage = "contactName", Name = "contactname", DbType = "VARCHAR(30)")]
		[DebuggerNonUserCode]
		public string ContactName
		{
			get
			{
				return contactName;
			}
			set
			{
				if (value != contactName)
				{
					contactName = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region string ContactTitle

		private string contactTitle;
		[Column(Storage = "contactTitle", Name = "contacttitle", DbType = "VARCHAR(30)")]
		[DebuggerNonUserCode]
		public string ContactTitle
		{
			get
			{
				return contactTitle;
			}
			set
			{
				if (value != contactTitle)
				{
					contactTitle = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region string Country

		private string country;
		[Column(Storage = "country", Name = "country", DbType = "VARCHAR(15)")]
		[DebuggerNonUserCode]
		public string Country
		{
			get
			{
				return country;
			}
			set
			{
				if (value != country)
				{
					country = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region string Fax

		private string fax;
		[Column(Storage = "fax", Name = "fax", DbType = "VARCHAR(24)")]
		[DebuggerNonUserCode]
		public string Fax
		{
			get
			{
				return fax;
			}
			set
			{
				if (value != fax)
				{
					fax = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region string Phone

		private string phone;
		[Column(Storage = "phone", Name = "phone", DbType = "VARCHAR(24)")]
		[DebuggerNonUserCode]
		public string Phone
		{
			get
			{
				return phone;
			}
			set
			{
				if (value != phone)
				{
					phone = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region string PostalCode

		private string postalCode;
		[Column(Storage = "postalCode", Name = "postalcode", DbType = "VARCHAR(10)")]
		[DebuggerNonUserCode]
		public string PostalCode
		{
			get
			{
				return postalCode;
			}
			set
			{
				if (value != postalCode)
				{
					postalCode = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region string Region

		private string region;
		[Column(Storage = "region", Name = "region", DbType = "VARCHAR(15)")]
		[DebuggerNonUserCode]
		public string Region
		{
			get
			{
				return region;
			}
			set
			{
				if (value != region)
				{
					region = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region int SupplierID

		[AutoGenId]
		private int supplierID;
		[Column(Storage = "supplierID", Name = "supplierid", DbType = "INTEGER(4)", IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false, Expression = "next value for \"linquser\".\"suppliers_seq\"")]
		[DebuggerNonUserCode]
		public int SupplierID
		{
			get
			{
				return supplierID;
			}
			set
			{
				if (value != supplierID)
				{
					supplierID = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region GetHashCode(), Equals() - uses column SupplierID to look up objects in liveObjectMap

		public override int GetHashCode()
		{
			return SupplierID.GetHashCode();
		}

		public override bool Equals(object o)
		{
			Supplier other = o as Supplier;
			if (other == null)
			{
				return false;
			}
			return SupplierID.Equals(other.SupplierID);
		}

		#endregion

		#region Children

		[Association(Storage = "null", OtherKey = "SupplierID", Name = "linquser_products_supplierid_linquser_suppliers_supplierid")]
		[DebuggerNonUserCode]
		public EntityMSet<Product> Products
		{
			get
			{
				// L212 - child data available only when part of query
				return null;
			}
		}


		#endregion

	}

	[Table(Name = "linquser.territories")]
	public partial class Territory : IModified
	{
		// IModified backing field
		public bool IsModified{ get; set; }

		#region int RegionID

		private int regionID;
		[Column(Storage = "regionID", Name = "regionid", DbType = "INTEGER(4)", CanBeNull = false)]
		[DebuggerNonUserCode]
		public int RegionID
		{
			get
			{
				return regionID;
			}
			set
			{
				if (value != regionID)
				{
					regionID = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region string TerritoryDescription

		private string territoryDescription;
		[Column(Storage = "territoryDescription", Name = "territorydescription", DbType = "VARCHAR(50)", CanBeNull = false)]
		[DebuggerNonUserCode]
		public string TerritoryDescription
		{
			get
			{
				return territoryDescription;
			}
			set
			{
				if (value != territoryDescription)
				{
					territoryDescription = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region string TerritoryID

		private string territoryID;
		[Column(Storage = "territoryID", Name = "territoryid", DbType = "VARCHAR(20)", IsPrimaryKey = true, CanBeNull = false)]
		[DebuggerNonUserCode]
		public string TerritoryID
		{
			get
			{
				return territoryID;
			}
			set
			{
				if (value != territoryID)
				{
					territoryID = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region GetHashCode(), Equals() - uses column TerritoryID to look up objects in liveObjectMap

		public override int GetHashCode()
		{
			return TerritoryID.GetHashCode();
		}

		public override bool Equals(object o)
		{
			Territory other = o as Territory;
			if (other == null)
			{
				return false;
			}
			return TerritoryID.Equals(other.TerritoryID);
		}

		#endregion

		#region Children

		[Association(Storage = "null", OtherKey = "TerritoryID", Name = "linquser_employeeterritories_territoryid_linquser_territories_territoryid")]
		[DebuggerNonUserCode]
		public EntityMSet<EmployeeTerritory> EmployeeTerritories
		{
			get
			{
				// L212 - child data available only when part of query
				return null;
			}
		}


		#endregion

		#region Parents

		private System.Data.Linq.EntityRef<Region> linqUserTerritoriesRegionIDLinqUserRegionRegionID;
		[Association(Storage = "linqUserTerritoriesRegionIDLinqUserRegionRegionID", ThisKey = "RegionID", Name = "linquser_territories_regionid_linquser_region_regionid")]
		[DebuggerNonUserCode]
		public Region Region
		{
			get
			{
				return linqUserTerritoriesRegionIDLinqUserRegionRegionID.Entity;
			}
			set
			{
				linqUserTerritoriesRegionIDLinqUserRegionRegionID.Entity = value;
			}
		}


		#endregion

	}
}

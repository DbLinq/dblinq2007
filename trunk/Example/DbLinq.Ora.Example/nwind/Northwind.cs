#region Auto-generated classes for "Northwind" database on 2008-07-06 01:10:58Z

//
//  ____  _     __  __      _        _
// |  _ \| |__ |  \/  | ___| |_ __ _| |
// | | | | '_ \| |\/| |/ _ \ __/ _` | |
// | |_| | |_) | |  | |  __/ || (_| | |
// |____/|_.__/|_|  |_|\___|\__\__,_|_|
//
// Auto-generated from "Northwind" on 2008-07-06 01:10:58Z
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
using DbLinq.Data.Linq;
using DbLinq.Linq;
using DbLinq.Linq.Mapping;

namespace nwind
{
	public partial class Northwind : DbLinq.Data.Linq.DataContext
	{
		public Northwind(System.Data.IDbConnection connection)
		: base(connection, new DbLinq.Oracle.OracleVendor())
		{
		}

		public Northwind(System.Data.IDbConnection connection, DbLinq.Vendor.IVendor vendor)
		: base(connection, vendor)
		{
		}

		public Table<Category> Categories { get { return GetTable<Category>(); } }
		public Table<Customer> Customers { get { return GetTable<Customer>(); } }
		public Table<Employee> Employees { get { return GetTable<Employee>(); } }
		public Table<EmployeeTerritory> EmployeeTerritories { get { return GetTable<EmployeeTerritory>(); } }
		public Table<Order> Orders { get { return GetTable<Order>(); } }
		public Table<OrderDetail> OrderDetails { get { return GetTable<OrderDetail>(); } }
		public Table<Product> Products { get { return GetTable<Product>(); } }
		public Table<Region> Regions { get { return GetTable<Region>(); } }
		public Table<Supplier> Suppliers { get { return GetTable<Supplier>(); } }
		public Table<Territory> Territories { get { return GetTable<Territory>(); } }

		[Function(Name = "NORTHWIND.GETORDERCOUNT", IsComposable = true)]
		public decimal GetOrderCount([Parameter(Name = "cuSTID", DbType = "VARCHAR2")] string cuSTID)
		{
			var result = ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod(), cuSTID);
			return (decimal)result.ReturnValue;
		}

		[Function(Name = "NORTHWIND.HELLO0", IsComposable = true)]
		public string Hello0()
		{
			var result = ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod());
			return (string)result.ReturnValue;
		}

		[Function(Name = "NORTHWIND.HELLO1", IsComposable = true)]
		public string Hello1([Parameter(Name = "s", DbType = "VARCHAR2")] string s)
		{
			var result = ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod(), s);
			return (string)result.ReturnValue;
		}

		[Function(Name = "NORTHWIND.HELLO2", IsComposable = true)]
		public string Hello2([Parameter(Name = "s", DbType = "VARCHAR2")] string s, [Parameter(Name = "s2", DbType = "NUMBER")] decimal s2)
		{
			var result = ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod(), s, s2);
			return (string)result.ReturnValue;
		}

		[Function(Name = "NORTHWIND.SP_SELORDERS", IsComposable = false)]
		public void SpSelOrders([Parameter(Name = "s", DbType = "VARCHAR2")] string s, [Parameter(Name = "s2", DbType = "NUMBER")] out decimal s2)
		{
			var result = ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod(), s);
			s2 = (System.Decimal)result.GetParameterValue(1);
		}

	}

	[Table(Name = "NORTHWIND.\"Categories\"")]
	public partial class Category
	{
		#region decimal CategoryID

		private decimal _categoryID;
		[DebuggerNonUserCode]
		[Column(Storage = "_categoryID", Name = "\"CategoryID\"", DbType = "NUMBER", IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false, Expression = "Categories_seq.NEXTVAL")]
		public decimal CategoryID
		{
			get
			{
				return _categoryID;
			}
			set
			{
				if (value != _categoryID)
				{
					_categoryID = value;
				}
			}
		}

		#endregion

		#region string CategoryName

		private string _categoryName;
		[DebuggerNonUserCode]
		[Column(Storage = "_categoryName", Name = "\"CategoryName\"", DbType = "VARCHAR2", CanBeNull = false)]
		public string CategoryName
		{
			get
			{
				return _categoryName;
			}
			set
			{
				if (value != _categoryName)
				{
					_categoryName = value;
				}
			}
		}

		#endregion

		#region string Description

		private string _description;
		[DebuggerNonUserCode]
		[Column(Storage = "_description", Name = "\"Description\"", DbType = "VARCHAR2")]
		public string Description
		{
			get
			{
				return _description;
			}
			set
			{
				if (value != _description)
				{
					_description = value;
				}
			}
		}

		#endregion

		#region System.Byte[] Picture

		private System.Byte[] _picture;
		[DebuggerNonUserCode]
		[Column(Storage = "_picture", Name = "\"Picture\"", DbType = "BLOB")]
		public System.Byte[] Picture
		{
			get
			{
				return _picture;
			}
			set
			{
				if (value != _picture)
				{
					_picture = value;
				}
			}
		}

		#endregion

		#region Children

		[Association(Storage = null, OtherKey = "CategoryID", Name = "SYS_C005572")]
		[DebuggerNonUserCode]
		public EntitySet<Product> Products
		{
			get
			{
				// L212 - child data available only when part of query
				return null;
			}
		}


		#endregion

	}

	[Table(Name = "NORTHWIND.\"Customers\"")]
	public partial class Customer
	{
		#region string Address

		private string _address;
		[DebuggerNonUserCode]
		[Column(Storage = "_address", Name = "\"Address\"", DbType = "VARCHAR2")]
		public string Address
		{
			get
			{
				return _address;
			}
			set
			{
				if (value != _address)
				{
					_address = value;
				}
			}
		}

		#endregion

		#region string City

		private string _city;
		[DebuggerNonUserCode]
		[Column(Storage = "_city", Name = "\"City\"", DbType = "VARCHAR2")]
		public string City
		{
			get
			{
				return _city;
			}
			set
			{
				if (value != _city)
				{
					_city = value;
				}
			}
		}

		#endregion

		#region string CompanyName

		private string _companyName;
		[DebuggerNonUserCode]
		[Column(Storage = "_companyName", Name = "\"CompanyName\"", DbType = "VARCHAR2", CanBeNull = false)]
		public string CompanyName
		{
			get
			{
				return _companyName;
			}
			set
			{
				if (value != _companyName)
				{
					_companyName = value;
				}
			}
		}

		#endregion

		#region string ContactName

		private string _contactName;
		[DebuggerNonUserCode]
		[Column(Storage = "_contactName", Name = "\"ContactName\"", DbType = "VARCHAR2")]
		public string ContactName
		{
			get
			{
				return _contactName;
			}
			set
			{
				if (value != _contactName)
				{
					_contactName = value;
				}
			}
		}

		#endregion

		#region string ContactTitle

		private string _contactTitle;
		[DebuggerNonUserCode]
		[Column(Storage = "_contactTitle", Name = "\"ContactTitle\"", DbType = "VARCHAR2")]
		public string ContactTitle
		{
			get
			{
				return _contactTitle;
			}
			set
			{
				if (value != _contactTitle)
				{
					_contactTitle = value;
				}
			}
		}

		#endregion

		#region string Country

		private string _country;
		[DebuggerNonUserCode]
		[Column(Storage = "_country", Name = "\"Country\"", DbType = "VARCHAR2")]
		public string Country
		{
			get
			{
				return _country;
			}
			set
			{
				if (value != _country)
				{
					_country = value;
				}
			}
		}

		#endregion

		#region string CustomerID

		private string _customerID;
		[DebuggerNonUserCode]
		[Column(Storage = "_customerID", Name = "\"CustomerID\"", DbType = "VARCHAR2", IsPrimaryKey = true, CanBeNull = false)]
		public string CustomerID
		{
			get
			{
				return _customerID;
			}
			set
			{
				if (value != _customerID)
				{
					_customerID = value;
				}
			}
		}

		#endregion

		#region string Fax

		private string _fax;
		[DebuggerNonUserCode]
		[Column(Storage = "_fax", Name = "\"Fax\"", DbType = "VARCHAR2")]
		public string Fax
		{
			get
			{
				return _fax;
			}
			set
			{
				if (value != _fax)
				{
					_fax = value;
				}
			}
		}

		#endregion

		#region string Phone

		private string _phone;
		[DebuggerNonUserCode]
		[Column(Storage = "_phone", Name = "\"Phone\"", DbType = "VARCHAR2")]
		public string Phone
		{
			get
			{
				return _phone;
			}
			set
			{
				if (value != _phone)
				{
					_phone = value;
				}
			}
		}

		#endregion

		#region string PostalCode

		private string _postalCode;
		[DebuggerNonUserCode]
		[Column(Storage = "_postalCode", Name = "\"PostalCode\"", DbType = "VARCHAR2")]
		public string PostalCode
		{
			get
			{
				return _postalCode;
			}
			set
			{
				if (value != _postalCode)
				{
					_postalCode = value;
				}
			}
		}

		#endregion

		#region string Region

		private string _region;
		[DebuggerNonUserCode]
		[Column(Storage = "_region", Name = "\"Region\"", DbType = "VARCHAR2")]
		public string Region
		{
			get
			{
				return _region;
			}
			set
			{
				if (value != _region)
				{
					_region = value;
				}
			}
		}

		#endregion

		#region Children

		[Association(Storage = null, OtherKey = "CustomerID", Name = "SYS_C005589")]
		[DebuggerNonUserCode]
		public EntitySet<Order> Orders
		{
			get
			{
				// L212 - child data available only when part of query
				return null;
			}
		}


		#endregion

	}

	[Table(Name = "NORTHWIND.\"Employees\"")]
	public partial class Employee
	{
		#region string Address

		private string _address;
		[DebuggerNonUserCode]
		[Column(Storage = "_address", Name = "\"Address\"", DbType = "VARCHAR2")]
		public string Address
		{
			get
			{
				return _address;
			}
			set
			{
				if (value != _address)
				{
					_address = value;
				}
			}
		}

		#endregion

		#region System.DateTime? BirthDate

		private System.DateTime? _birthDate;
		[DebuggerNonUserCode]
		[Column(Storage = "_birthDate", Name = "\"BirthDate\"", DbType = "DATE")]
		public System.DateTime? BirthDate
		{
			get
			{
				return _birthDate;
			}
			set
			{
				if (value != _birthDate)
				{
					_birthDate = value;
				}
			}
		}

		#endregion

		#region string City

		private string _city;
		[DebuggerNonUserCode]
		[Column(Storage = "_city", Name = "\"City\"", DbType = "VARCHAR2")]
		public string City
		{
			get
			{
				return _city;
			}
			set
			{
				if (value != _city)
				{
					_city = value;
				}
			}
		}

		#endregion

		#region string Country

		private string _country;
		[DebuggerNonUserCode]
		[Column(Storage = "_country", Name = "\"Country\"", DbType = "VARCHAR2")]
		public string Country
		{
			get
			{
				return _country;
			}
			set
			{
				if (value != _country)
				{
					_country = value;
				}
			}
		}

		#endregion

		#region decimal EmployeeID

		private decimal _employeeID;
		[DebuggerNonUserCode]
		[Column(Storage = "_employeeID", Name = "\"EmployeeID\"", DbType = "NUMBER", IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false, Expression = "Employees_seq.NEXTVAL")]
		public decimal EmployeeID
		{
			get
			{
				return _employeeID;
			}
			set
			{
				if (value != _employeeID)
				{
					_employeeID = value;
				}
			}
		}

		#endregion

		#region string FirstName

		private string _firstName;
		[DebuggerNonUserCode]
		[Column(Storage = "_firstName", Name = "\"FirstName\"", DbType = "VARCHAR2", CanBeNull = false)]
		public string FirstName
		{
			get
			{
				return _firstName;
			}
			set
			{
				if (value != _firstName)
				{
					_firstName = value;
				}
			}
		}

		#endregion

		#region System.DateTime? HireDate

		private System.DateTime? _hireDate;
		[DebuggerNonUserCode]
		[Column(Storage = "_hireDate", Name = "\"HireDate\"", DbType = "DATE")]
		public System.DateTime? HireDate
		{
			get
			{
				return _hireDate;
			}
			set
			{
				if (value != _hireDate)
				{
					_hireDate = value;
				}
			}
		}

		#endregion

		#region string HomePhone

		private string _homePhone;
		[DebuggerNonUserCode]
		[Column(Storage = "_homePhone", Name = "\"HomePhone\"", DbType = "VARCHAR2")]
		public string HomePhone
		{
			get
			{
				return _homePhone;
			}
			set
			{
				if (value != _homePhone)
				{
					_homePhone = value;
				}
			}
		}

		#endregion

		#region string LastName

		private string _lastName;
		[DebuggerNonUserCode]
		[Column(Storage = "_lastName", Name = "\"LastName\"", DbType = "VARCHAR2", CanBeNull = false)]
		public string LastName
		{
			get
			{
				return _lastName;
			}
			set
			{
				if (value != _lastName)
				{
					_lastName = value;
				}
			}
		}

		#endregion

		#region string Notes

		private string _notes;
		[DebuggerNonUserCode]
		[Column(Storage = "_notes", Name = "\"Notes\"", DbType = "VARCHAR2")]
		public string Notes
		{
			get
			{
				return _notes;
			}
			set
			{
				if (value != _notes)
				{
					_notes = value;
				}
			}
		}

		#endregion

		#region System.Byte[] Photo

		private System.Byte[] _photo;
		[DebuggerNonUserCode]
		[Column(Storage = "_photo", Name = "\"Photo\"", DbType = "BLOB")]
		public System.Byte[] Photo
		{
			get
			{
				return _photo;
			}
			set
			{
				if (value != _photo)
				{
					_photo = value;
				}
			}
		}

		#endregion

		#region string PostalCode

		private string _postalCode;
		[DebuggerNonUserCode]
		[Column(Storage = "_postalCode", Name = "\"PostalCode\"", DbType = "VARCHAR2")]
		public string PostalCode
		{
			get
			{
				return _postalCode;
			}
			set
			{
				if (value != _postalCode)
				{
					_postalCode = value;
				}
			}
		}

		#endregion

		#region string Region

		private string _region;
		[DebuggerNonUserCode]
		[Column(Storage = "_region", Name = "\"Region\"", DbType = "VARCHAR2")]
		public string Region
		{
			get
			{
				return _region;
			}
			set
			{
				if (value != _region)
				{
					_region = value;
				}
			}
		}

		#endregion

		#region decimal? ReportsTo

		private decimal? _reportsTo;
		[DebuggerNonUserCode]
		[Column(Storage = "_reportsTo", Name = "\"ReportsTo\"", DbType = "NUMBER")]
		public decimal? ReportsTo
		{
			get
			{
				return _reportsTo;
			}
			set
			{
				if (value != _reportsTo)
				{
					_reportsTo = value;
				}
			}
		}

		#endregion

		#region string Title

		private string _title;
		[DebuggerNonUserCode]
		[Column(Storage = "_title", Name = "\"Title\"", DbType = "VARCHAR2")]
		public string Title
		{
			get
			{
				return _title;
			}
			set
			{
				if (value != _title)
				{
					_title = value;
				}
			}
		}

		#endregion

		#region Children

		[Association(Storage = null, OtherKey = "ReportsTo", Name = "SYS_C005581")]
		[DebuggerNonUserCode]
		public EntitySet<Employee> Employees
		{
			get
			{
				// L212 - child data available only when part of query
				return null;
			}
		}

		[Association(Storage = null, OtherKey = "EmployeeID", Name = "SYS_C005585")]
		[DebuggerNonUserCode]
		public EntitySet<EmployeeTerritory> EmployeeTerritories
		{
			get
			{
				// L212 - child data available only when part of query
				return null;
			}
		}

		[Association(Storage = null, OtherKey = "EmployeeID", Name = "SYS_C005590")]
		[DebuggerNonUserCode]
		public EntitySet<Order> Orders
		{
			get
			{
				// L212 - child data available only when part of query
				return null;
			}
		}


		#endregion

		#region Parents

		private DbLinq.Data.Linq.EntityRef<Employee> _reportsToEmployee;
		[Association(Storage = "_reportsToEmployee", ThisKey = "ReportsTo", Name = "SYS_C005581", IsForeignKey = true)]
		[DebuggerNonUserCode]
		public Employee ReportsToEmployee
		{
			get
			{
				return _reportsToEmployee.Entity;
			}
			set
			{
				_reportsToEmployee.Entity = value;
			}
		}


		#endregion

	}

	[Table(Name = "NORTHWIND.\"EmployeeTerritories\"")]
	public partial class EmployeeTerritory
	{
		#region decimal EmployeeID

		private decimal _employeeID;
		[DebuggerNonUserCode]
		[Column(Storage = "_employeeID", Name = "\"EmployeeID\"", DbType = "NUMBER", IsPrimaryKey = true, CanBeNull = false)]
		public decimal EmployeeID
		{
			get
			{
				return _employeeID;
			}
			set
			{
				if (value != _employeeID)
				{
					_employeeID = value;
				}
			}
		}

		#endregion

		#region string TerritoryID

		private string _territoryID;
		[DebuggerNonUserCode]
		[Column(Storage = "_territoryID", Name = "\"TerritoryID\"", DbType = "VARCHAR2", IsPrimaryKey = true, CanBeNull = false)]
		public string TerritoryID
		{
			get
			{
				return _territoryID;
			}
			set
			{
				if (value != _territoryID)
				{
					_territoryID = value;
				}
			}
		}

		#endregion

		#region Parents

		private DbLinq.Data.Linq.EntityRef<Territory> _territory;
		[Association(Storage = "_territory", ThisKey = "TerritoryID", Name = "SYS_C005586", IsForeignKey = true)]
		[DebuggerNonUserCode]
		public Territory Territory
		{
			get
			{
				return _territory.Entity;
			}
			set
			{
				_territory.Entity = value;
			}
		}

		private DbLinq.Data.Linq.EntityRef<Employee> _employee;
		[Association(Storage = "_employee", ThisKey = "EmployeeID", Name = "SYS_C005585", IsForeignKey = true)]
		[DebuggerNonUserCode]
		public Employee Employee
		{
			get
			{
				return _employee.Entity;
			}
			set
			{
				_employee.Entity = value;
			}
		}


		#endregion

	}

	[Table(Name = "NORTHWIND.\"Orders\"")]
	public partial class Order
	{
		#region string CustomerID

		private string _customerID;
		[DebuggerNonUserCode]
		[Column(Storage = "_customerID", Name = "\"CustomerID\"", DbType = "VARCHAR2")]
		public string CustomerID
		{
			get
			{
				return _customerID;
			}
			set
			{
				if (value != _customerID)
				{
					_customerID = value;
				}
			}
		}

		#endregion

		#region decimal? EmployeeID

		private decimal? _employeeID;
		[DebuggerNonUserCode]
		[Column(Storage = "_employeeID", Name = "\"EmployeeID\"", DbType = "NUMBER")]
		public decimal? EmployeeID
		{
			get
			{
				return _employeeID;
			}
			set
			{
				if (value != _employeeID)
				{
					_employeeID = value;
				}
			}
		}

		#endregion

		#region decimal? Freight

		private decimal? _freight;
		[DebuggerNonUserCode]
		[Column(Storage = "_freight", Name = "\"Freight\"", DbType = "NUMBER")]
		public decimal? Freight
		{
			get
			{
				return _freight;
			}
			set
			{
				if (value != _freight)
				{
					_freight = value;
				}
			}
		}

		#endregion

		#region System.DateTime? OrderDate

		private System.DateTime? _orderDate;
		[DebuggerNonUserCode]
		[Column(Storage = "_orderDate", Name = "\"OrderDate\"", DbType = "DATE")]
		public System.DateTime? OrderDate
		{
			get
			{
				return _orderDate;
			}
			set
			{
				if (value != _orderDate)
				{
					_orderDate = value;
				}
			}
		}

		#endregion

		#region decimal OrderID

		private decimal _orderID;
		[DebuggerNonUserCode]
		[Column(Storage = "_orderID", Name = "\"OrderID\"", DbType = "NUMBER", IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false, Expression = "Orders_seq.NEXTVAL")]
		public decimal OrderID
		{
			get
			{
				return _orderID;
			}
			set
			{
				if (value != _orderID)
				{
					_orderID = value;
				}
			}
		}

		#endregion

		#region System.DateTime? RequiredDate

		private System.DateTime? _requiredDate;
		[DebuggerNonUserCode]
		[Column(Storage = "_requiredDate", Name = "\"RequiredDate\"", DbType = "DATE")]
		public System.DateTime? RequiredDate
		{
			get
			{
				return _requiredDate;
			}
			set
			{
				if (value != _requiredDate)
				{
					_requiredDate = value;
				}
			}
		}

		#endregion

		#region string ShipAddress

		private string _shipAddress;
		[DebuggerNonUserCode]
		[Column(Storage = "_shipAddress", Name = "\"ShipAddress\"", DbType = "VARCHAR2")]
		public string ShipAddress
		{
			get
			{
				return _shipAddress;
			}
			set
			{
				if (value != _shipAddress)
				{
					_shipAddress = value;
				}
			}
		}

		#endregion

		#region string ShipCity

		private string _shipCity;
		[DebuggerNonUserCode]
		[Column(Storage = "_shipCity", Name = "\"ShipCity\"", DbType = "VARCHAR2")]
		public string ShipCity
		{
			get
			{
				return _shipCity;
			}
			set
			{
				if (value != _shipCity)
				{
					_shipCity = value;
				}
			}
		}

		#endregion

		#region string ShipCountry

		private string _shipCountry;
		[DebuggerNonUserCode]
		[Column(Storage = "_shipCountry", Name = "\"ShipCountry\"", DbType = "VARCHAR2")]
		public string ShipCountry
		{
			get
			{
				return _shipCountry;
			}
			set
			{
				if (value != _shipCountry)
				{
					_shipCountry = value;
				}
			}
		}

		#endregion

		#region string ShipName

		private string _shipName;
		[DebuggerNonUserCode]
		[Column(Storage = "_shipName", Name = "\"ShipName\"", DbType = "VARCHAR2")]
		public string ShipName
		{
			get
			{
				return _shipName;
			}
			set
			{
				if (value != _shipName)
				{
					_shipName = value;
				}
			}
		}

		#endregion

		#region System.DateTime? ShippedDate

		private System.DateTime? _shippedDate;
		[DebuggerNonUserCode]
		[Column(Storage = "_shippedDate", Name = "\"ShippedDate\"", DbType = "DATE")]
		public System.DateTime? ShippedDate
		{
			get
			{
				return _shippedDate;
			}
			set
			{
				if (value != _shippedDate)
				{
					_shippedDate = value;
				}
			}
		}

		#endregion

		#region string ShipPostalCode

		private string _shipPostalCode;
		[DebuggerNonUserCode]
		[Column(Storage = "_shipPostalCode", Name = "\"ShipPostalCode\"", DbType = "VARCHAR2")]
		public string ShipPostalCode
		{
			get
			{
				return _shipPostalCode;
			}
			set
			{
				if (value != _shipPostalCode)
				{
					_shipPostalCode = value;
				}
			}
		}

		#endregion

		#region string ShipRegion

		private string _shipRegion;
		[DebuggerNonUserCode]
		[Column(Storage = "_shipRegion", Name = "\"ShipRegion\"", DbType = "VARCHAR2")]
		public string ShipRegion
		{
			get
			{
				return _shipRegion;
			}
			set
			{
				if (value != _shipRegion)
				{
					_shipRegion = value;
				}
			}
		}

		#endregion

		#region decimal? ShipVia

		private decimal? _shipVia;
		[DebuggerNonUserCode]
		[Column(Storage = "_shipVia", Name = "\"ShipVia\"", DbType = "NUMBER")]
		public decimal? ShipVia
		{
			get
			{
				return _shipVia;
			}
			set
			{
				if (value != _shipVia)
				{
					_shipVia = value;
				}
			}
		}

		#endregion

		#region Children

		[Association(Storage = null, OtherKey = "OrderID", Name = "SYS_C005597")]
		[DebuggerNonUserCode]
		public EntitySet<OrderDetail> OrderDetails
		{
			get
			{
				// L212 - child data available only when part of query
				return null;
			}
		}


		#endregion

		#region Parents

		private DbLinq.Data.Linq.EntityRef<Employee> _employee;
		[Association(Storage = "_employee", ThisKey = "EmployeeID", Name = "SYS_C005590", IsForeignKey = true)]
		[DebuggerNonUserCode]
		public Employee Employee
		{
			get
			{
				return _employee.Entity;
			}
			set
			{
				_employee.Entity = value;
			}
		}

		private DbLinq.Data.Linq.EntityRef<Customer> _customer;
		[Association(Storage = "_customer", ThisKey = "CustomerID", Name = "SYS_C005589", IsForeignKey = true)]
		[DebuggerNonUserCode]
		public Customer Customer
		{
			get
			{
				return _customer.Entity;
			}
			set
			{
				_customer.Entity = value;
			}
		}


		#endregion

	}

	[Table(Name = "NORTHWIND.\"OrderDetails\"")]
	public partial class OrderDetail
	{
		#region float Discount

		private float _discount;
		[DebuggerNonUserCode]
		[Column(Storage = "_discount", Name = "\"Discount\"", DbType = "FLOAT", CanBeNull = false)]
		public float Discount
		{
			get
			{
				return _discount;
			}
			set
			{
				if (value != _discount)
				{
					_discount = value;
				}
			}
		}

		#endregion

		#region decimal OrderID

		private decimal _orderID;
		[DebuggerNonUserCode]
		[Column(Storage = "_orderID", Name = "\"OrderID\"", DbType = "NUMBER", IsPrimaryKey = true, CanBeNull = false)]
		public decimal OrderID
		{
			get
			{
				return _orderID;
			}
			set
			{
				if (value != _orderID)
				{
					_orderID = value;
				}
			}
		}

		#endregion

		#region decimal ProductID

		private decimal _productID;
		[DebuggerNonUserCode]
		[Column(Storage = "_productID", Name = "\"ProductID\"", DbType = "NUMBER", IsPrimaryKey = true, CanBeNull = false)]
		public decimal ProductID
		{
			get
			{
				return _productID;
			}
			set
			{
				if (value != _productID)
				{
					_productID = value;
				}
			}
		}

		#endregion

		#region decimal Quantity

		private decimal _quantity;
		[DebuggerNonUserCode]
		[Column(Storage = "_quantity", Name = "\"Quantity\"", DbType = "NUMBER", CanBeNull = false)]
		public decimal Quantity
		{
			get
			{
				return _quantity;
			}
			set
			{
				if (value != _quantity)
				{
					_quantity = value;
				}
			}
		}

		#endregion

		#region decimal UnitPrice

		private decimal _unitPrice;
		[DebuggerNonUserCode]
		[Column(Storage = "_unitPrice", Name = "\"UnitPrice\"", DbType = "NUMBER", CanBeNull = false)]
		public decimal UnitPrice
		{
			get
			{
				return _unitPrice;
			}
			set
			{
				if (value != _unitPrice)
				{
					_unitPrice = value;
				}
			}
		}

		#endregion

		#region Parents

		private DbLinq.Data.Linq.EntityRef<Product> _product;
		[Association(Storage = "_product", ThisKey = "ProductID", Name = "SYS_C005598", IsForeignKey = true)]
		[DebuggerNonUserCode]
		public Product Product
		{
			get
			{
				return _product.Entity;
			}
			set
			{
				_product.Entity = value;
			}
		}

		private DbLinq.Data.Linq.EntityRef<Order> _order;
		[Association(Storage = "_order", ThisKey = "OrderID", Name = "SYS_C005597", IsForeignKey = true)]
		[DebuggerNonUserCode]
		public Order Order
		{
			get
			{
				return _order.Entity;
			}
			set
			{
				_order.Entity = value;
			}
		}


		#endregion

	}

	[Table(Name = "NORTHWIND.\"Products\"")]
	public partial class Product
	{
		#region decimal? CategoryID

		private decimal? _categoryID;
		[DebuggerNonUserCode]
		[Column(Storage = "_categoryID", Name = "\"CategoryID\"", DbType = "NUMBER")]
		public decimal? CategoryID
		{
			get
			{
				return _categoryID;
			}
			set
			{
				if (value != _categoryID)
				{
					_categoryID = value;
				}
			}
		}

		#endregion

		#region bool Discontinued

		private bool _discontinued;
		[DebuggerNonUserCode]
		[Column(Storage = "_discontinued", Name = "\"Discontinued\"", DbType = "NUMBER", CanBeNull = false)]
		public bool Discontinued
		{
			get
			{
				return _discontinued;
			}
			set
			{
				if (value != _discontinued)
				{
					_discontinued = value;
				}
			}
		}

		#endregion

		#region decimal ProductID

		private decimal _productID;
		[DebuggerNonUserCode]
		[Column(Storage = "_productID", Name = "\"ProductID\"", DbType = "NUMBER", IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false, Expression = "Products_seq.NEXTVAL")]
		public decimal ProductID
		{
			get
			{
				return _productID;
			}
			set
			{
				if (value != _productID)
				{
					_productID = value;
				}
			}
		}

		#endregion

		#region string ProductName

		private string _productName;
		[DebuggerNonUserCode]
		[Column(Storage = "_productName", Name = "\"ProductName\"", DbType = "VARCHAR2", CanBeNull = false)]
		public string ProductName
		{
			get
			{
				return _productName;
			}
			set
			{
				if (value != _productName)
				{
					_productName = value;
				}
			}
		}

		#endregion

		#region string QuantityPerUnit

		private string _quantityPerUnit;
		[DebuggerNonUserCode]
		[Column(Storage = "_quantityPerUnit", Name = "\"QuantityPerUnit\"", DbType = "VARCHAR2")]
		public string QuantityPerUnit
		{
			get
			{
				return _quantityPerUnit;
			}
			set
			{
				if (value != _quantityPerUnit)
				{
					_quantityPerUnit = value;
				}
			}
		}

		#endregion

		#region decimal? ReorderLevel

		private decimal? _reorderLevel;
		[DebuggerNonUserCode]
		[Column(Storage = "_reorderLevel", Name = "\"ReorderLevel\"", DbType = "NUMBER")]
		public decimal? ReorderLevel
		{
			get
			{
				return _reorderLevel;
			}
			set
			{
				if (value != _reorderLevel)
				{
					_reorderLevel = value;
				}
			}
		}

		#endregion

		#region decimal? SupplierID

		private decimal? _supplierID;
		[DebuggerNonUserCode]
		[Column(Storage = "_supplierID", Name = "\"SupplierID\"", DbType = "NUMBER")]
		public decimal? SupplierID
		{
			get
			{
				return _supplierID;
			}
			set
			{
				if (value != _supplierID)
				{
					_supplierID = value;
				}
			}
		}

		#endregion

		#region decimal? UnitPrice

		private decimal? _unitPrice;
		[DebuggerNonUserCode]
		[Column(Storage = "_unitPrice", Name = "\"UnitPrice\"", DbType = "NUMBER")]
		public decimal? UnitPrice
		{
			get
			{
				return _unitPrice;
			}
			set
			{
				if (value != _unitPrice)
				{
					_unitPrice = value;
				}
			}
		}

		#endregion

		#region decimal? UnitsInStock

		private decimal? _unitsInStock;
		[DebuggerNonUserCode]
		[Column(Storage = "_unitsInStock", Name = "\"UnitsInStock\"", DbType = "NUMBER")]
		public decimal? UnitsInStock
		{
			get
			{
				return _unitsInStock;
			}
			set
			{
				if (value != _unitsInStock)
				{
					_unitsInStock = value;
				}
			}
		}

		#endregion

		#region decimal? UnitsOnOrder

		private decimal? _unitsOnOrder;
		[DebuggerNonUserCode]
		[Column(Storage = "_unitsOnOrder", Name = "\"UnitsOnOrder\"", DbType = "NUMBER")]
		public decimal? UnitsOnOrder
		{
			get
			{
				return _unitsOnOrder;
			}
			set
			{
				if (value != _unitsOnOrder)
				{
					_unitsOnOrder = value;
				}
			}
		}

		#endregion

		#region Children

		[Association(Storage = null, OtherKey = "ProductID", Name = "SYS_C005598")]
		[DebuggerNonUserCode]
		public EntitySet<OrderDetail> OrderDetails
		{
			get
			{
				// L212 - child data available only when part of query
				return null;
			}
		}


		#endregion

		#region Parents

		private DbLinq.Data.Linq.EntityRef<Supplier> _supplier;
		[Association(Storage = "_supplier", ThisKey = "SupplierID", Name = "SYS_C005573", IsForeignKey = true)]
		[DebuggerNonUserCode]
		public Supplier Supplier
		{
			get
			{
				return _supplier.Entity;
			}
			set
			{
				_supplier.Entity = value;
			}
		}

		private DbLinq.Data.Linq.EntityRef<Category> _category;
		[Association(Storage = "_category", ThisKey = "CategoryID", Name = "SYS_C005572", IsForeignKey = true)]
		[DebuggerNonUserCode]
		public Category Category
		{
			get
			{
				return _category.Entity;
			}
			set
			{
				_category.Entity = value;
			}
		}


		#endregion

	}

	[Table(Name = "NORTHWIND.\"Region\"")]
	public partial class Region
	{
		#region string RegionDescription

		private string _regionDescription;
		[DebuggerNonUserCode]
		[Column(Storage = "_regionDescription", Name = "\"RegionDescription\"", DbType = "VARCHAR2", CanBeNull = false)]
		public string RegionDescription
		{
			get
			{
				return _regionDescription;
			}
			set
			{
				if (value != _regionDescription)
				{
					_regionDescription = value;
				}
			}
		}

		#endregion

		#region decimal RegionID

		private decimal _regionID;
		[DebuggerNonUserCode]
		[Column(Storage = "_regionID", Name = "\"RegionID\"", DbType = "NUMBER", IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false, Expression = "Region_seq.NEXTVAL")]
		public decimal RegionID
		{
			get
			{
				return _regionID;
			}
			set
			{
				if (value != _regionID)
				{
					_regionID = value;
				}
			}
		}

		#endregion

		#region Children

		[Association(Storage = null, OtherKey = "RegionID", Name = "SYS_C005561")]
		[DebuggerNonUserCode]
		public EntitySet<Territory> Territories
		{
			get
			{
				// L212 - child data available only when part of query
				return null;
			}
		}


		#endregion

	}

	[Table(Name = "NORTHWIND.\"Suppliers\"")]
	public partial class Supplier
	{
		#region string Address

		private string _address;
		[DebuggerNonUserCode]
		[Column(Storage = "_address", Name = "\"Address\"", DbType = "VARCHAR2")]
		public string Address
		{
			get
			{
				return _address;
			}
			set
			{
				if (value != _address)
				{
					_address = value;
				}
			}
		}

		#endregion

		#region string City

		private string _city;
		[DebuggerNonUserCode]
		[Column(Storage = "_city", Name = "\"City\"", DbType = "VARCHAR2")]
		public string City
		{
			get
			{
				return _city;
			}
			set
			{
				if (value != _city)
				{
					_city = value;
				}
			}
		}

		#endregion

		#region string CompanyName

		private string _companyName;
		[DebuggerNonUserCode]
		[Column(Storage = "_companyName", Name = "\"CompanyName\"", DbType = "VARCHAR2", CanBeNull = false)]
		public string CompanyName
		{
			get
			{
				return _companyName;
			}
			set
			{
				if (value != _companyName)
				{
					_companyName = value;
				}
			}
		}

		#endregion

		#region string ContactName

		private string _contactName;
		[DebuggerNonUserCode]
		[Column(Storage = "_contactName", Name = "\"ContactName\"", DbType = "VARCHAR2")]
		public string ContactName
		{
			get
			{
				return _contactName;
			}
			set
			{
				if (value != _contactName)
				{
					_contactName = value;
				}
			}
		}

		#endregion

		#region string ContactTitle

		private string _contactTitle;
		[DebuggerNonUserCode]
		[Column(Storage = "_contactTitle", Name = "\"ContactTitle\"", DbType = "VARCHAR2")]
		public string ContactTitle
		{
			get
			{
				return _contactTitle;
			}
			set
			{
				if (value != _contactTitle)
				{
					_contactTitle = value;
				}
			}
		}

		#endregion

		#region string Country

		private string _country;
		[DebuggerNonUserCode]
		[Column(Storage = "_country", Name = "\"Country\"", DbType = "VARCHAR2")]
		public string Country
		{
			get
			{
				return _country;
			}
			set
			{
				if (value != _country)
				{
					_country = value;
				}
			}
		}

		#endregion

		#region string Fax

		private string _fax;
		[DebuggerNonUserCode]
		[Column(Storage = "_fax", Name = "\"Fax\"", DbType = "VARCHAR2")]
		public string Fax
		{
			get
			{
				return _fax;
			}
			set
			{
				if (value != _fax)
				{
					_fax = value;
				}
			}
		}

		#endregion

		#region string Phone

		private string _phone;
		[DebuggerNonUserCode]
		[Column(Storage = "_phone", Name = "\"Phone\"", DbType = "VARCHAR2")]
		public string Phone
		{
			get
			{
				return _phone;
			}
			set
			{
				if (value != _phone)
				{
					_phone = value;
				}
			}
		}

		#endregion

		#region string PostalCode

		private string _postalCode;
		[DebuggerNonUserCode]
		[Column(Storage = "_postalCode", Name = "\"PostalCode\"", DbType = "VARCHAR2")]
		public string PostalCode
		{
			get
			{
				return _postalCode;
			}
			set
			{
				if (value != _postalCode)
				{
					_postalCode = value;
				}
			}
		}

		#endregion

		#region string Region

		private string _region;
		[DebuggerNonUserCode]
		[Column(Storage = "_region", Name = "\"Region\"", DbType = "VARCHAR2")]
		public string Region
		{
			get
			{
				return _region;
			}
			set
			{
				if (value != _region)
				{
					_region = value;
				}
			}
		}

		#endregion

		#region decimal SupplierID

		private decimal _supplierID;
		[DebuggerNonUserCode]
		[Column(Storage = "_supplierID", Name = "\"SupplierID\"", DbType = "NUMBER", IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false, Expression = "Suppliers_seq.NEXTVAL")]
		public decimal SupplierID
		{
			get
			{
				return _supplierID;
			}
			set
			{
				if (value != _supplierID)
				{
					_supplierID = value;
				}
			}
		}

		#endregion

		#region Children

		[Association(Storage = null, OtherKey = "SupplierID", Name = "SYS_C005573")]
		[DebuggerNonUserCode]
		public EntitySet<Product> Products
		{
			get
			{
				// L212 - child data available only when part of query
				return null;
			}
		}


		#endregion

	}

	[Table(Name = "NORTHWIND.\"Territories\"")]
	public partial class Territory
	{
		#region decimal RegionID

		private decimal _regionID;
		[DebuggerNonUserCode]
		[Column(Storage = "_regionID", Name = "\"RegionID\"", DbType = "NUMBER", CanBeNull = false)]
		public decimal RegionID
		{
			get
			{
				return _regionID;
			}
			set
			{
				if (value != _regionID)
				{
					_regionID = value;
				}
			}
		}

		#endregion

		#region string TerritoryDescription

		private string _territoryDescription;
		[DebuggerNonUserCode]
		[Column(Storage = "_territoryDescription", Name = "\"TerritoryDescription\"", DbType = "VARCHAR2", CanBeNull = false)]
		public string TerritoryDescription
		{
			get
			{
				return _territoryDescription;
			}
			set
			{
				if (value != _territoryDescription)
				{
					_territoryDescription = value;
				}
			}
		}

		#endregion

		#region string TerritoryID

		private string _territoryID;
		[DebuggerNonUserCode]
		[Column(Storage = "_territoryID", Name = "\"TerritoryID\"", DbType = "VARCHAR2", IsPrimaryKey = true, CanBeNull = false)]
		public string TerritoryID
		{
			get
			{
				return _territoryID;
			}
			set
			{
				if (value != _territoryID)
				{
					_territoryID = value;
				}
			}
		}

		#endregion

		#region Children

		[Association(Storage = null, OtherKey = "TerritoryID", Name = "SYS_C005586")]
		[DebuggerNonUserCode]
		public EntitySet<EmployeeTerritory> EmployeeTerritories
		{
			get
			{
				// L212 - child data available only when part of query
				return null;
			}
		}


		#endregion

		#region Parents

		private DbLinq.Data.Linq.EntityRef<Region> _region;
		[Association(Storage = "_region", ThisKey = "RegionID", Name = "SYS_C005561", IsForeignKey = true)]
		[DebuggerNonUserCode]
		public Region Region
		{
			get
			{
				return _region.Entity;
			}
			set
			{
				_region.Entity = value;
			}
		}


		#endregion

	}
}

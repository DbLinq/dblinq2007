#region Auto-generated classes for "Northwind" database on 2008-06-23 01:24:35Z

//
//  ____  _     __  __      _        _
// |  _ \| |__ |  \/  | ___| |_ __ _| |
// | | | | '_ \| |\/| |/ _ \ __/ _` | |
// | |_| | |_) | |  | |  __/ || (_| | |
// |____/|_.__/|_|  |_|\___|\__\__,_|_|
//
// Auto-generated from "Northwind" on 2008-06-23 01:24:35Z
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

		[AutoGenId]
		private decimal categoryID;
		[DebuggerNonUserCode]
		[Column(Storage = "categoryID", Name = "\"CategoryID\"", DbType = "NUMBER", IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false, Expression = "Categories_seq.NEXTVAL")]
		public decimal CategoryID
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
				}
			}
		}

		#endregion

		#region string CategoryName

		private string categoryName;
		[DebuggerNonUserCode]
		[Column(Storage = "categoryName", Name = "\"CategoryName\"", DbType = "VARCHAR2", CanBeNull = false)]
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
				}
			}
		}

		#endregion

		#region string Description

		private string description;
		[DebuggerNonUserCode]
		[Column(Storage = "description", Name = "\"Description\"", DbType = "VARCHAR2")]
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
				}
			}
		}

		#endregion

		#region System.Byte[] Picture

		private System.Byte[] picture;
		[DebuggerNonUserCode]
		[Column(Storage = "picture", Name = "\"Picture\"", DbType = "BLOB")]
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
				}
			}
		}

		#endregion

		#region Children

		[Association(Storage = null, OtherKey = "CategoryID", Name = "SYS_C005572")]
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

	[Table(Name = "NORTHWIND.\"Customers\"")]
	public partial class Customer
	{
		#region string Address

		private string address;
		[DebuggerNonUserCode]
		[Column(Storage = "address", Name = "\"Address\"", DbType = "VARCHAR2")]
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
				}
			}
		}

		#endregion

		#region string City

		private string city;
		[DebuggerNonUserCode]
		[Column(Storage = "city", Name = "\"City\"", DbType = "VARCHAR2")]
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
				}
			}
		}

		#endregion

		#region string CompanyName

		private string companyName;
		[DebuggerNonUserCode]
		[Column(Storage = "companyName", Name = "\"CompanyName\"", DbType = "VARCHAR2", CanBeNull = false)]
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
				}
			}
		}

		#endregion

		#region string ContactName

		private string contactName;
		[DebuggerNonUserCode]
		[Column(Storage = "contactName", Name = "\"ContactName\"", DbType = "VARCHAR2")]
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
				}
			}
		}

		#endregion

		#region string ContactTitle

		private string contactTitle;
		[DebuggerNonUserCode]
		[Column(Storage = "contactTitle", Name = "\"ContactTitle\"", DbType = "VARCHAR2")]
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
				}
			}
		}

		#endregion

		#region string Country

		private string country;
		[DebuggerNonUserCode]
		[Column(Storage = "country", Name = "\"Country\"", DbType = "VARCHAR2")]
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
				}
			}
		}

		#endregion

		#region string CustomerID

		private string customerID;
		[DebuggerNonUserCode]
		[Column(Storage = "customerID", Name = "\"CustomerID\"", DbType = "VARCHAR2", IsPrimaryKey = true, CanBeNull = false)]
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
				}
			}
		}

		#endregion

		#region string Fax

		private string fax;
		[DebuggerNonUserCode]
		[Column(Storage = "fax", Name = "\"Fax\"", DbType = "VARCHAR2")]
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
				}
			}
		}

		#endregion

		#region string Phone

		private string phone;
		[DebuggerNonUserCode]
		[Column(Storage = "phone", Name = "\"Phone\"", DbType = "VARCHAR2")]
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
				}
			}
		}

		#endregion

		#region string PostalCode

		private string postalCode;
		[DebuggerNonUserCode]
		[Column(Storage = "postalCode", Name = "\"PostalCode\"", DbType = "VARCHAR2")]
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
				}
			}
		}

		#endregion

		#region string Region

		private string region;
		[DebuggerNonUserCode]
		[Column(Storage = "region", Name = "\"Region\"", DbType = "VARCHAR2")]
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
				}
			}
		}

		#endregion

		#region Children

		[Association(Storage = null, OtherKey = "CustomerID", Name = "SYS_C005589")]
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

	[Table(Name = "NORTHWIND.\"Employees\"")]
	public partial class Employee
	{
		#region string Address

		private string address;
		[DebuggerNonUserCode]
		[Column(Storage = "address", Name = "\"Address\"", DbType = "VARCHAR2")]
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
				}
			}
		}

		#endregion

		#region System.DateTime? BirthDate

		private System.DateTime? birthDate;
		[DebuggerNonUserCode]
		[Column(Storage = "birthDate", Name = "\"BirthDate\"", DbType = "DATE")]
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
				}
			}
		}

		#endregion

		#region string City

		private string city;
		[DebuggerNonUserCode]
		[Column(Storage = "city", Name = "\"City\"", DbType = "VARCHAR2")]
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
				}
			}
		}

		#endregion

		#region string Country

		private string country;
		[DebuggerNonUserCode]
		[Column(Storage = "country", Name = "\"Country\"", DbType = "VARCHAR2")]
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
				}
			}
		}

		#endregion

		#region decimal EmployeeID

		[AutoGenId]
		private decimal employeeID;
		[DebuggerNonUserCode]
		[Column(Storage = "employeeID", Name = "\"EmployeeID\"", DbType = "NUMBER", IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false, Expression = "Employees_seq.NEXTVAL")]
		public decimal EmployeeID
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
				}
			}
		}

		#endregion

		#region string FirstName

		private string firstName;
		[DebuggerNonUserCode]
		[Column(Storage = "firstName", Name = "\"FirstName\"", DbType = "VARCHAR2", CanBeNull = false)]
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
				}
			}
		}

		#endregion

		#region System.DateTime? HireDate

		private System.DateTime? hireDate;
		[DebuggerNonUserCode]
		[Column(Storage = "hireDate", Name = "\"HireDate\"", DbType = "DATE")]
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
				}
			}
		}

		#endregion

		#region string HomePhone

		private string homePhone;
		[DebuggerNonUserCode]
		[Column(Storage = "homePhone", Name = "\"HomePhone\"", DbType = "VARCHAR2")]
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
				}
			}
		}

		#endregion

		#region string LastName

		private string lastName;
		[DebuggerNonUserCode]
		[Column(Storage = "lastName", Name = "\"LastName\"", DbType = "VARCHAR2", CanBeNull = false)]
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
				}
			}
		}

		#endregion

		#region string Notes

		private string notes;
		[DebuggerNonUserCode]
		[Column(Storage = "notes", Name = "\"Notes\"", DbType = "VARCHAR2")]
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
				}
			}
		}

		#endregion

		#region System.Byte[] Photo

		private System.Byte[] photo;
		[DebuggerNonUserCode]
		[Column(Storage = "photo", Name = "\"Photo\"", DbType = "BLOB")]
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
				}
			}
		}

		#endregion

		#region string PostalCode

		private string postalCode;
		[DebuggerNonUserCode]
		[Column(Storage = "postalCode", Name = "\"PostalCode\"", DbType = "VARCHAR2")]
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
				}
			}
		}

		#endregion

		#region string Region

		private string region;
		[DebuggerNonUserCode]
		[Column(Storage = "region", Name = "\"Region\"", DbType = "VARCHAR2")]
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
				}
			}
		}

		#endregion

		#region decimal? ReportsTo

		private decimal? reportsTo;
		[DebuggerNonUserCode]
		[Column(Storage = "reportsTo", Name = "\"ReportsTo\"", DbType = "NUMBER")]
		public decimal? ReportsTo
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
				}
			}
		}

		#endregion

		#region string Title

		private string title;
		[DebuggerNonUserCode]
		[Column(Storage = "title", Name = "\"Title\"", DbType = "VARCHAR2")]
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
				}
			}
		}

		#endregion

		#region Children

		[Association(Storage = null, OtherKey = "EmployeeID", Name = "SYS_C005581")]
		[DebuggerNonUserCode]
		public EntityMSet<Employee> Employees
		{
			get
			{
				// L212 - child data available only when part of query
				return null;
			}
		}

		[Association(Storage = null, OtherKey = "EmployeeID", Name = "SYS_C005585")]
		[DebuggerNonUserCode]
		public EntityMSet<EmployeeTerritory> EmployeeTerritories
		{
			get
			{
				// L212 - child data available only when part of query
				return null;
			}
		}

		[Association(Storage = null, OtherKey = "EmployeeID", Name = "SYS_C005590")]
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

		private System.Data.Linq.EntityRef<Employee> reportsToEmployee;
		[Association(Storage = "reportsToEmployee", ThisKey = "ReportsTo", Name = "SYS_C005581")]
		[DebuggerNonUserCode]
		public Employee ReportsToEmployee
		{
			get
			{
				return reportsToEmployee.Entity;
			}
			set
			{
				reportsToEmployee.Entity = value;
			}
		}


		#endregion

	}

	[Table(Name = "NORTHWIND.\"EmployeeTerritories\"")]
	public partial class EmployeeTerritory
	{
		#region decimal EmployeeID

		private decimal employeeID;
		[DebuggerNonUserCode]
		[Column(Storage = "employeeID", Name = "\"EmployeeID\"", DbType = "NUMBER", IsPrimaryKey = true, CanBeNull = false)]
		public decimal EmployeeID
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
				}
			}
		}

		#endregion

		#region string TerritoryID

		private string territoryID;
		[DebuggerNonUserCode]
		[Column(Storage = "territoryID", Name = "\"TerritoryID\"", DbType = "VARCHAR2", IsPrimaryKey = true, CanBeNull = false)]
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
				}
			}
		}

		#endregion

		#region Parents

		private System.Data.Linq.EntityRef<Territory> territory;
		[Association(Storage = "territory", ThisKey = "TerritoryID", Name = "SYS_C005586")]
		[DebuggerNonUserCode]
		public Territory Territory
		{
			get
			{
				return territory.Entity;
			}
			set
			{
				territory.Entity = value;
			}
		}

		private System.Data.Linq.EntityRef<Employee> employee;
		[Association(Storage = "employee", ThisKey = "EmployeeID", Name = "SYS_C005585")]
		[DebuggerNonUserCode]
		public Employee Employee
		{
			get
			{
				return employee.Entity;
			}
			set
			{
				employee.Entity = value;
			}
		}


		#endregion

	}

	[Table(Name = "NORTHWIND.\"Orders\"")]
	public partial class Order
	{
		#region string CustomerID

		private string customerID;
		[DebuggerNonUserCode]
		[Column(Storage = "customerID", Name = "\"CustomerID\"", DbType = "VARCHAR2")]
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
				}
			}
		}

		#endregion

		#region decimal? EmployeeID

		private decimal? employeeID;
		[DebuggerNonUserCode]
		[Column(Storage = "employeeID", Name = "\"EmployeeID\"", DbType = "NUMBER")]
		public decimal? EmployeeID
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
				}
			}
		}

		#endregion

		#region decimal? Freight

		private decimal? freight;
		[DebuggerNonUserCode]
		[Column(Storage = "freight", Name = "\"Freight\"", DbType = "NUMBER")]
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
				}
			}
		}

		#endregion

		#region System.DateTime? OrderDate

		private System.DateTime? orderDate;
		[DebuggerNonUserCode]
		[Column(Storage = "orderDate", Name = "\"OrderDate\"", DbType = "DATE")]
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
				}
			}
		}

		#endregion

		#region decimal OrderID

		[AutoGenId]
		private decimal orderID;
		[DebuggerNonUserCode]
		[Column(Storage = "orderID", Name = "\"OrderID\"", DbType = "NUMBER", IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false, Expression = "Orders_seq.NEXTVAL")]
		public decimal OrderID
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
				}
			}
		}

		#endregion

		#region System.DateTime? RequiredDate

		private System.DateTime? requiredDate;
		[DebuggerNonUserCode]
		[Column(Storage = "requiredDate", Name = "\"RequiredDate\"", DbType = "DATE")]
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
				}
			}
		}

		#endregion

		#region string ShipAddress

		private string shipAddress;
		[DebuggerNonUserCode]
		[Column(Storage = "shipAddress", Name = "\"ShipAddress\"", DbType = "VARCHAR2")]
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
				}
			}
		}

		#endregion

		#region string ShipCity

		private string shipCity;
		[DebuggerNonUserCode]
		[Column(Storage = "shipCity", Name = "\"ShipCity\"", DbType = "VARCHAR2")]
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
				}
			}
		}

		#endregion

		#region string ShipCountry

		private string shipCountry;
		[DebuggerNonUserCode]
		[Column(Storage = "shipCountry", Name = "\"ShipCountry\"", DbType = "VARCHAR2")]
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
				}
			}
		}

		#endregion

		#region string ShipName

		private string shipName;
		[DebuggerNonUserCode]
		[Column(Storage = "shipName", Name = "\"ShipName\"", DbType = "VARCHAR2")]
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
				}
			}
		}

		#endregion

		#region System.DateTime? ShippedDate

		private System.DateTime? shippedDate;
		[DebuggerNonUserCode]
		[Column(Storage = "shippedDate", Name = "\"ShippedDate\"", DbType = "DATE")]
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
				}
			}
		}

		#endregion

		#region string ShipPostalCode

		private string shipPostalCode;
		[DebuggerNonUserCode]
		[Column(Storage = "shipPostalCode", Name = "\"ShipPostalCode\"", DbType = "VARCHAR2")]
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
				}
			}
		}

		#endregion

		#region string ShipRegion

		private string shipRegion;
		[DebuggerNonUserCode]
		[Column(Storage = "shipRegion", Name = "\"ShipRegion\"", DbType = "VARCHAR2")]
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
				}
			}
		}

		#endregion

		#region decimal? ShipVia

		private decimal? shipVia;
		[DebuggerNonUserCode]
		[Column(Storage = "shipVia", Name = "\"ShipVia\"", DbType = "NUMBER")]
		public decimal? ShipVia
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
				}
			}
		}

		#endregion

		#region Children

		[Association(Storage = null, OtherKey = "OrderID", Name = "SYS_C005597")]
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

		private System.Data.Linq.EntityRef<Employee> employee;
		[Association(Storage = "employee", ThisKey = "EmployeeID", Name = "SYS_C005590")]
		[DebuggerNonUserCode]
		public Employee Employee
		{
			get
			{
				return employee.Entity;
			}
			set
			{
				employee.Entity = value;
			}
		}

		private System.Data.Linq.EntityRef<Customer> customer;
		[Association(Storage = "customer", ThisKey = "CustomerID", Name = "SYS_C005589")]
		[DebuggerNonUserCode]
		public Customer Customer
		{
			get
			{
				return customer.Entity;
			}
			set
			{
				customer.Entity = value;
			}
		}


		#endregion

	}

	[Table(Name = "NORTHWIND.\"OrderDetails\"")]
	public partial class OrderDetail
	{
		#region float Discount

		private float discount;
		[DebuggerNonUserCode]
		[Column(Storage = "discount", Name = "\"Discount\"", DbType = "FLOAT", CanBeNull = false)]
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
				}
			}
		}

		#endregion

		#region decimal OrderID

		private decimal orderID;
		[DebuggerNonUserCode]
		[Column(Storage = "orderID", Name = "\"OrderID\"", DbType = "NUMBER", IsPrimaryKey = true, CanBeNull = false)]
		public decimal OrderID
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
				}
			}
		}

		#endregion

		#region decimal ProductID

		private decimal productID;
		[DebuggerNonUserCode]
		[Column(Storage = "productID", Name = "\"ProductID\"", DbType = "NUMBER", IsPrimaryKey = true, CanBeNull = false)]
		public decimal ProductID
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
				}
			}
		}

		#endregion

		#region decimal Quantity

		private decimal quantity;
		[DebuggerNonUserCode]
		[Column(Storage = "quantity", Name = "\"Quantity\"", DbType = "NUMBER", CanBeNull = false)]
		public decimal Quantity
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
				}
			}
		}

		#endregion

		#region decimal UnitPrice

		private decimal unitPrice;
		[DebuggerNonUserCode]
		[Column(Storage = "unitPrice", Name = "\"UnitPrice\"", DbType = "NUMBER", CanBeNull = false)]
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
				}
			}
		}

		#endregion

		#region Parents

		private System.Data.Linq.EntityRef<Product> product;
		[Association(Storage = "product", ThisKey = "ProductID", Name = "SYS_C005598")]
		[DebuggerNonUserCode]
		public Product Product
		{
			get
			{
				return product.Entity;
			}
			set
			{
				product.Entity = value;
			}
		}

		private System.Data.Linq.EntityRef<Order> order;
		[Association(Storage = "order", ThisKey = "OrderID", Name = "SYS_C005597")]
		[DebuggerNonUserCode]
		public Order Order
		{
			get
			{
				return order.Entity;
			}
			set
			{
				order.Entity = value;
			}
		}


		#endregion

	}

	[Table(Name = "NORTHWIND.\"Products\"")]
	public partial class Product
	{
		#region decimal? CategoryID

		private decimal? categoryID;
		[DebuggerNonUserCode]
		[Column(Storage = "categoryID", Name = "\"CategoryID\"", DbType = "NUMBER")]
		public decimal? CategoryID
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
				}
			}
		}

		#endregion

		#region bool Discontinued

		private bool discontinued;
		[DebuggerNonUserCode]
		[Column(Storage = "discontinued", Name = "\"Discontinued\"", DbType = "NUMBER", CanBeNull = false)]
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
				}
			}
		}

		#endregion

		#region decimal ProductID

		[AutoGenId]
		private decimal productID;
		[DebuggerNonUserCode]
		[Column(Storage = "productID", Name = "\"ProductID\"", DbType = "NUMBER", IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false, Expression = "Products_seq.NEXTVAL")]
		public decimal ProductID
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
				}
			}
		}

		#endregion

		#region string ProductName

		private string productName;
		[DebuggerNonUserCode]
		[Column(Storage = "productName", Name = "\"ProductName\"", DbType = "VARCHAR2", CanBeNull = false)]
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
				}
			}
		}

		#endregion

		#region string QuantityPerUnit

		private string quantityPerUnit;
		[DebuggerNonUserCode]
		[Column(Storage = "quantityPerUnit", Name = "\"QuantityPerUnit\"", DbType = "VARCHAR2")]
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
				}
			}
		}

		#endregion

		#region decimal? ReorderLevel

		private decimal? reorderLevel;
		[DebuggerNonUserCode]
		[Column(Storage = "reorderLevel", Name = "\"ReorderLevel\"", DbType = "NUMBER")]
		public decimal? ReorderLevel
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
				}
			}
		}

		#endregion

		#region decimal? SupplierID

		private decimal? supplierID;
		[DebuggerNonUserCode]
		[Column(Storage = "supplierID", Name = "\"SupplierID\"", DbType = "NUMBER")]
		public decimal? SupplierID
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
				}
			}
		}

		#endregion

		#region decimal? UnitPrice

		private decimal? unitPrice;
		[DebuggerNonUserCode]
		[Column(Storage = "unitPrice", Name = "\"UnitPrice\"", DbType = "NUMBER")]
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
				}
			}
		}

		#endregion

		#region decimal? UnitsInStock

		private decimal? unitsInStock;
		[DebuggerNonUserCode]
		[Column(Storage = "unitsInStock", Name = "\"UnitsInStock\"", DbType = "NUMBER")]
		public decimal? UnitsInStock
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
				}
			}
		}

		#endregion

		#region decimal? UnitsOnOrder

		private decimal? unitsOnOrder;
		[DebuggerNonUserCode]
		[Column(Storage = "unitsOnOrder", Name = "\"UnitsOnOrder\"", DbType = "NUMBER")]
		public decimal? UnitsOnOrder
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
				}
			}
		}

		#endregion

		#region Children

		[Association(Storage = null, OtherKey = "ProductID", Name = "SYS_C005598")]
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

		private System.Data.Linq.EntityRef<Supplier> supplier;
		[Association(Storage = "supplier", ThisKey = "SupplierID", Name = "SYS_C005573")]
		[DebuggerNonUserCode]
		public Supplier Supplier
		{
			get
			{
				return supplier.Entity;
			}
			set
			{
				supplier.Entity = value;
			}
		}

		private System.Data.Linq.EntityRef<Category> category;
		[Association(Storage = "category", ThisKey = "CategoryID", Name = "SYS_C005572")]
		[DebuggerNonUserCode]
		public Category Category
		{
			get
			{
				return category.Entity;
			}
			set
			{
				category.Entity = value;
			}
		}


		#endregion

	}

	[Table(Name = "NORTHWIND.\"Region\"")]
	public partial class Region
	{
		#region string RegionDescription

		private string regionDescription;
		[DebuggerNonUserCode]
		[Column(Storage = "regionDescription", Name = "\"RegionDescription\"", DbType = "VARCHAR2", CanBeNull = false)]
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
				}
			}
		}

		#endregion

		#region decimal RegionID

		[AutoGenId]
		private decimal regionID;
		[DebuggerNonUserCode]
		[Column(Storage = "regionID", Name = "\"RegionID\"", DbType = "NUMBER", IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false, Expression = "Region_seq.NEXTVAL")]
		public decimal RegionID
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
				}
			}
		}

		#endregion

		#region Children

		[Association(Storage = null, OtherKey = "RegionID", Name = "SYS_C005561")]
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

	[Table(Name = "NORTHWIND.\"Suppliers\"")]
	public partial class Supplier
	{
		#region string Address

		private string address;
		[DebuggerNonUserCode]
		[Column(Storage = "address", Name = "\"Address\"", DbType = "VARCHAR2")]
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
				}
			}
		}

		#endregion

		#region string City

		private string city;
		[DebuggerNonUserCode]
		[Column(Storage = "city", Name = "\"City\"", DbType = "VARCHAR2")]
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
				}
			}
		}

		#endregion

		#region string CompanyName

		private string companyName;
		[DebuggerNonUserCode]
		[Column(Storage = "companyName", Name = "\"CompanyName\"", DbType = "VARCHAR2", CanBeNull = false)]
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
				}
			}
		}

		#endregion

		#region string ContactName

		private string contactName;
		[DebuggerNonUserCode]
		[Column(Storage = "contactName", Name = "\"ContactName\"", DbType = "VARCHAR2")]
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
				}
			}
		}

		#endregion

		#region string ContactTitle

		private string contactTitle;
		[DebuggerNonUserCode]
		[Column(Storage = "contactTitle", Name = "\"ContactTitle\"", DbType = "VARCHAR2")]
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
				}
			}
		}

		#endregion

		#region string Country

		private string country;
		[DebuggerNonUserCode]
		[Column(Storage = "country", Name = "\"Country\"", DbType = "VARCHAR2")]
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
				}
			}
		}

		#endregion

		#region string Fax

		private string fax;
		[DebuggerNonUserCode]
		[Column(Storage = "fax", Name = "\"Fax\"", DbType = "VARCHAR2")]
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
				}
			}
		}

		#endregion

		#region string Phone

		private string phone;
		[DebuggerNonUserCode]
		[Column(Storage = "phone", Name = "\"Phone\"", DbType = "VARCHAR2")]
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
				}
			}
		}

		#endregion

		#region string PostalCode

		private string postalCode;
		[DebuggerNonUserCode]
		[Column(Storage = "postalCode", Name = "\"PostalCode\"", DbType = "VARCHAR2")]
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
				}
			}
		}

		#endregion

		#region string Region

		private string region;
		[DebuggerNonUserCode]
		[Column(Storage = "region", Name = "\"Region\"", DbType = "VARCHAR2")]
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
				}
			}
		}

		#endregion

		#region decimal SupplierID

		[AutoGenId]
		private decimal supplierID;
		[DebuggerNonUserCode]
		[Column(Storage = "supplierID", Name = "\"SupplierID\"", DbType = "NUMBER", IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false, Expression = "Suppliers_seq.NEXTVAL")]
		public decimal SupplierID
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
				}
			}
		}

		#endregion

		#region Children

		[Association(Storage = null, OtherKey = "SupplierID", Name = "SYS_C005573")]
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

	[Table(Name = "NORTHWIND.\"Territories\"")]
	public partial class Territory
	{
		#region decimal RegionID

		private decimal regionID;
		[DebuggerNonUserCode]
		[Column(Storage = "regionID", Name = "\"RegionID\"", DbType = "NUMBER", CanBeNull = false)]
		public decimal RegionID
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
				}
			}
		}

		#endregion

		#region string TerritoryDescription

		private string territoryDescription;
		[DebuggerNonUserCode]
		[Column(Storage = "territoryDescription", Name = "\"TerritoryDescription\"", DbType = "VARCHAR2", CanBeNull = false)]
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
				}
			}
		}

		#endregion

		#region string TerritoryID

		private string territoryID;
		[DebuggerNonUserCode]
		[Column(Storage = "territoryID", Name = "\"TerritoryID\"", DbType = "VARCHAR2", IsPrimaryKey = true, CanBeNull = false)]
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
				}
			}
		}

		#endregion

		#region Children

		[Association(Storage = null, OtherKey = "TerritoryID", Name = "SYS_C005586")]
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

		private System.Data.Linq.EntityRef<Region> region;
		[Association(Storage = "region", ThisKey = "RegionID", Name = "SYS_C005561")]
		[DebuggerNonUserCode]
		public Region Region
		{
			get
			{
				return region.Entity;
			}
			set
			{
				region.Entity = value;
			}
		}


		#endregion

	}
}

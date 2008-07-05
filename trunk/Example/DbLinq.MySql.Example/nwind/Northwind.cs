#region Auto-generated classes for Northwind database on 2008-06-23 01:24:39Z

//
//  ____  _     __  __      _        _
// |  _ \| |__ |  \/  | ___| |_ __ _| |
// | | | | '_ \| |\/| |/ _ \ __/ _` | |
// | |_| | |_) | |  | |  __/ || (_| | |
// |____/|_.__/|_|  |_|\___|\__\__,_|_|
//
// Auto-generated from Northwind on 2008-06-23 01:24:39Z
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
		: base(connection, new DbLinq.MySql.MySqlVendor())
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
		public Table<Shipper> Shippers { get { return GetTable<Shipper>(); } }
		public Table<Supplier> Suppliers { get { return GetTable<Supplier>(); } }
		public Table<Territory> Territories { get { return GetTable<Territory>(); } }

		[Function(Name = "northwind.getOrderCount", IsComposable = true)]
		public int GetOrderCount([Parameter(Name = "custId", DbType = "VARCHAR(5)")] string custId)
		{
			var result = ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod(), custId);
			return (int)result.ReturnValue;
		}

		[Function(Name = "northwind.hello0", IsComposable = true)]
		public string Hello0()
		{
			var result = ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod());
			return (string)result.ReturnValue;
		}

		[Function(Name = "northwind.hello1", IsComposable = true)]
		public string Hello1([Parameter(Name = "s", DbType = "CHAR(20)")] string s)
		{
			var result = ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod(), s);
			return (string)result.ReturnValue;
		}

		[Function(Name = "northwind.hello2", IsComposable = true)]
		public string Hello2([Parameter(Name = "s", DbType = "CHAR(20)")] string s, [Parameter(Name = "s2", DbType = "int")] int s2)
		{
			var result = ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod(), s, s2);
			return (string)result.ReturnValue;
		}

		[Function(Name = "northwind.sp_selOrders", IsComposable = false)]
		public System.Data.DataSet SpSelOrders([Parameter(Name = "s", DbType = "CHAR(20)")] string s, [Parameter(Name = "s2", DbType = "int")] out int s2)
		{
			var result = ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod(), s);
			s2 = (System.Int32)result.GetParameterValue(1);
			return (System.Data.DataSet)result.ReturnValue;
		}

		[Function(Name = "northwind.sp_updOrders", IsComposable = false)]
		public void SpUpdOrders([Parameter(Name = "custID", DbType = "int")] int custID, [Parameter(Name = "prodId", DbType = "int")] int prodId)
		{
			var result = ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod(), custID, prodId);
		}

	}

	[Table(Name = "northwind.categories")]
	public partial class Category
	{
		#region int CategoryID

		[AutoGenId]
		private int categoryID;
		[DebuggerNonUserCode]
		[Column(Storage = "categoryID", Name = "CategoryID", DbType = "int", IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false)]
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
				}
			}
		}

		#endregion

		#region string CategoryName

		private string categoryName;
		[DebuggerNonUserCode]
		[Column(Storage = "categoryName", Name = "CategoryName", DbType = "varchar(15)", CanBeNull = false)]
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
		[Column(Storage = "description", Name = "Description", DbType = "text")]
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
		[Column(Storage = "picture", Name = "Picture", DbType = "blob")]
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

		[Association(Storage = null, OtherKey = "CategoryID", Name = "products_ibfk_1")]
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

	[Table(Name = "northwind.customers")]
	public partial class Customer
	{
		#region string Address

		private string address;
		[DebuggerNonUserCode]
		[Column(Storage = "address", Name = "Address", DbType = "varchar(60)")]
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
		[Column(Storage = "city", Name = "City", DbType = "varchar(15)")]
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
		[Column(Storage = "companyName", Name = "CompanyName", DbType = "varchar(40)", CanBeNull = false)]
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
		[Column(Storage = "contactName", Name = "ContactName", DbType = "varchar(30)")]
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
		[Column(Storage = "contactTitle", Name = "ContactTitle", DbType = "varchar(30)")]
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
		[Column(Storage = "country", Name = "Country", DbType = "varchar(15)")]
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
		[Column(Storage = "customerID", Name = "CustomerID", DbType = "varchar(5)", IsPrimaryKey = true, CanBeNull = false)]
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
		[Column(Storage = "fax", Name = "Fax", DbType = "varchar(24)")]
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
		[Column(Storage = "phone", Name = "Phone", DbType = "varchar(24)")]
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
		[Column(Storage = "postalCode", Name = "PostalCode", DbType = "varchar(10)")]
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
		[Column(Storage = "region", Name = "Region", DbType = "varchar(15)")]
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

		[Association(Storage = null, OtherKey = "CustomerID", Name = "orders_ibfk_1")]
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

	[Table(Name = "northwind.employees")]
	public partial class Employee
	{
		#region string Address

		private string address;
		[DebuggerNonUserCode]
		[Column(Storage = "address", Name = "Address", DbType = "varchar(60)")]
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
		[Column(Storage = "birthDate", Name = "BirthDate", DbType = "datetime")]
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
		[Column(Storage = "city", Name = "City", DbType = "varchar(15)")]
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
		[Column(Storage = "country", Name = "Country", DbType = "varchar(15)")]
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

		#region int EmployeeID

		[AutoGenId]
		private int employeeID;
		[DebuggerNonUserCode]
		[Column(Storage = "employeeID", Name = "EmployeeID", DbType = "int", IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false)]
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
				}
			}
		}

		#endregion

		#region string FirstName

		private string firstName;
		[DebuggerNonUserCode]
		[Column(Storage = "firstName", Name = "FirstName", DbType = "varchar(10)", CanBeNull = false)]
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
		[Column(Storage = "hireDate", Name = "HireDate", DbType = "datetime")]
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
		[Column(Storage = "homePhone", Name = "HomePhone", DbType = "varchar(24)")]
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
		[Column(Storage = "lastName", Name = "LastName", DbType = "varchar(20)", CanBeNull = false)]
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
		[Column(Storage = "notes", Name = "Notes", DbType = "text")]
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
		[Column(Storage = "photo", Name = "Photo", DbType = "blob")]
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
		[Column(Storage = "postalCode", Name = "PostalCode", DbType = "varchar(10)")]
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
		[Column(Storage = "region", Name = "Region", DbType = "varchar(15)")]
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

		#region int? ReportsTo

		private int? reportsTo;
		[DebuggerNonUserCode]
		[Column(Storage = "reportsTo", Name = "ReportsTo", DbType = "int")]
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
				}
			}
		}

		#endregion

		#region string Title

		private string title;
		[DebuggerNonUserCode]
		[Column(Storage = "title", Name = "Title", DbType = "varchar(30)")]
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

		[Association(Storage = null, OtherKey = "EmployeeID", Name = "employees_ibfk_1")]
		[DebuggerNonUserCode]
		public EntitySet<Employee> Employees
		{
			get
			{
				// L212 - child data available only when part of query
				return null;
			}
		}

		[Association(Storage = null, OtherKey = "EmployeeID", Name = "employeeterritories_ibfk_1")]
		[DebuggerNonUserCode]
		public EntitySet<EmployeeTerritory> EmployeeTerritories
		{
			get
			{
				// L212 - child data available only when part of query
				return null;
			}
		}

		[Association(Storage = null, OtherKey = "EmployeeID", Name = "orders_ibfk_2")]
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

		private EntityRef<Employee> reportsToEmployee;
		[Association(Storage = "reportsToEmployee", ThisKey = "ReportsTo", Name = "employees_ibfk_1")]
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

	[Table(Name = "northwind.employeeterritories")]
	public partial class EmployeeTerritory
	{
		#region int EmployeeID

		private int employeeID;
		[DebuggerNonUserCode]
		[Column(Storage = "employeeID", Name = "EmployeeID", DbType = "int", IsPrimaryKey = true, CanBeNull = false)]
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
				}
			}
		}

		#endregion

		#region string TerritoryID

		private string territoryID;
		[DebuggerNonUserCode]
		[Column(Storage = "territoryID", Name = "TerritoryID", DbType = "varchar(20)", IsPrimaryKey = true, CanBeNull = false)]
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

		private EntityRef<Employee> employee;
		[Association(Storage = "employee", ThisKey = "EmployeeID", Name = "employeeterritories_ibfk_1")]
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

		private EntityRef<Territory> territory;
		[Association(Storage = "territory", ThisKey = "TerritoryID", Name = "employeeterritories_ibfk_2")]
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


		#endregion

	}

	[Table(Name = "northwind.orders")]
	public partial class Order
	{
		#region string CustomerID

		private string customerID;
		[DebuggerNonUserCode]
		[Column(Storage = "customerID", Name = "CustomerID", DbType = "varchar(5)")]
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

		#region int? EmployeeID

		private int? employeeID;
		[DebuggerNonUserCode]
		[Column(Storage = "employeeID", Name = "EmployeeID", DbType = "int")]
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
				}
			}
		}

		#endregion

		#region decimal? Freight

		private decimal? freight;
		[DebuggerNonUserCode]
		[Column(Storage = "freight", Name = "Freight", DbType = "decimal")]
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
		[Column(Storage = "orderDate", Name = "OrderDate", DbType = "datetime")]
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

		#region int OrderID

		[AutoGenId]
		private int orderID;
		[DebuggerNonUserCode]
		[Column(Storage = "orderID", Name = "OrderID", DbType = "int", IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false)]
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
				}
			}
		}

		#endregion

		#region System.DateTime? RequiredDate

		private System.DateTime? requiredDate;
		[DebuggerNonUserCode]
		[Column(Storage = "requiredDate", Name = "RequiredDate", DbType = "datetime")]
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
		[Column(Storage = "shipAddress", Name = "ShipAddress", DbType = "varchar(60)")]
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
		[Column(Storage = "shipCity", Name = "ShipCity", DbType = "varchar(15)")]
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
		[Column(Storage = "shipCountry", Name = "ShipCountry", DbType = "varchar(15)")]
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
		[Column(Storage = "shipName", Name = "ShipName", DbType = "varchar(40)")]
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
		[Column(Storage = "shippedDate", Name = "ShippedDate", DbType = "datetime")]
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
		[Column(Storage = "shipPostalCode", Name = "ShipPostalCode", DbType = "varchar(10)")]
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
		[Column(Storage = "shipRegion", Name = "ShipRegion", DbType = "varchar(15)")]
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

		#region int? ShipVia

		private int? shipVia;
		[DebuggerNonUserCode]
		[Column(Storage = "shipVia", Name = "ShipVia", DbType = "int")]
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
				}
			}
		}

		#endregion

		#region Children

		[Association(Storage = null, OtherKey = "OrderID", Name = "`order details_ibfk_1`")]
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

		private EntityRef<Customer> customer;
		[Association(Storage = "customer", ThisKey = "CustomerID", Name = "orders_ibfk_1")]
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

		private EntityRef<Employee> employee;
		[Association(Storage = "employee", ThisKey = "EmployeeID", Name = "orders_ibfk_2")]
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

		private EntityRef<Shipper> shipper;
		[Association(Storage = "shipper", ThisKey = "ShipVia", Name = "orders_ibfk_3")]
		[DebuggerNonUserCode]
		public Shipper Shipper
		{
			get
			{
				return shipper.Entity;
			}
			set
			{
				shipper.Entity = value;
			}
		}


		#endregion

	}

	[Table(Name = "northwind.`order details`")]
	public partial class OrderDetail
	{
		#region float Discount

		private float discount;
		[DebuggerNonUserCode]
		[Column(Storage = "discount", Name = "Discount", DbType = "float", CanBeNull = false)]
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

		#region int OrderID

		private int orderID;
		[DebuggerNonUserCode]
		[Column(Storage = "orderID", Name = "OrderID", DbType = "int", IsPrimaryKey = true, CanBeNull = false)]
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
				}
			}
		}

		#endregion

		#region int ProductID

		private int productID;
		[DebuggerNonUserCode]
		[Column(Storage = "productID", Name = "ProductID", DbType = "int", IsPrimaryKey = true, CanBeNull = false)]
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
				}
			}
		}

		#endregion

		#region short Quantity

		private short quantity;
		[DebuggerNonUserCode]
		[Column(Storage = "quantity", Name = "Quantity", DbType = "smallint(6)", CanBeNull = false)]
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
				}
			}
		}

		#endregion

		#region decimal UnitPrice

		private decimal unitPrice;
		[DebuggerNonUserCode]
		[Column(Storage = "unitPrice", Name = "UnitPrice", DbType = "decimal", CanBeNull = false)]
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

		private EntityRef<Order> order;
		[Association(Storage = "order", ThisKey = "OrderID", Name = "`order details_ibfk_1`")]
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

		private EntityRef<Product> product;
		[Association(Storage = "product", ThisKey = "ProductID", Name = "`order details_ibfk_2`")]
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


		#endregion

	}

	[Table(Name = "northwind.products")]
	public partial class Product
	{
		#region int? CategoryID

		private int? categoryID;
		[DebuggerNonUserCode]
		[Column(Storage = "categoryID", Name = "CategoryID", DbType = "int")]
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
				}
			}
		}

		#endregion

		#region bool Discontinued

		private bool discontinued;
		[DebuggerNonUserCode]
		[Column(Storage = "discontinued", Name = "Discontinued", DbType = "bit(1)", CanBeNull = false)]
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

		#region int ProductID

		[AutoGenId]
		private int productID;
		[DebuggerNonUserCode]
		[Column(Storage = "productID", Name = "ProductID", DbType = "int", IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false)]
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
				}
			}
		}

		#endregion

		#region string ProductName

		private string productName;
		[DebuggerNonUserCode]
		[Column(Storage = "productName", Name = "ProductName", DbType = "varchar(40)", CanBeNull = false)]
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
		[Column(Storage = "quantityPerUnit", Name = "QuantityPerUnit", DbType = "varchar(20)")]
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

		#region short? ReorderLevel

		private short? reorderLevel;
		[DebuggerNonUserCode]
		[Column(Storage = "reorderLevel", Name = "ReorderLevel", DbType = "smallint(6)")]
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
				}
			}
		}

		#endregion

		#region int? SupplierID

		private int? supplierID;
		[DebuggerNonUserCode]
		[Column(Storage = "supplierID", Name = "SupplierID", DbType = "int")]
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
				}
			}
		}

		#endregion

		#region decimal? UnitPrice

		private decimal? unitPrice;
		[DebuggerNonUserCode]
		[Column(Storage = "unitPrice", Name = "UnitPrice", DbType = "decimal")]
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

		#region short? UnitsInStock

		private short? unitsInStock;
		[DebuggerNonUserCode]
		[Column(Storage = "unitsInStock", Name = "UnitsInStock", DbType = "smallint(6)")]
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
				}
			}
		}

		#endregion

		#region short? UnitsOnOrder

		private short? unitsOnOrder;
		[DebuggerNonUserCode]
		[Column(Storage = "unitsOnOrder", Name = "UnitsOnOrder", DbType = "smallint(6)")]
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
				}
			}
		}

		#endregion

		#region Children

		[Association(Storage = null, OtherKey = "ProductID", Name = "`order details_ibfk_2`")]
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

		private EntityRef<Category> category;
		[Association(Storage = "category", ThisKey = "CategoryID", Name = "products_ibfk_1")]
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

		private EntityRef<Supplier> supplier;
		[Association(Storage = "supplier", ThisKey = "SupplierID", Name = "products_ibfk_2")]
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


		#endregion

	}

	[Table(Name = "northwind.region")]
	public partial class Region
	{
		#region string RegionDescription

		private string regionDescription;
		[DebuggerNonUserCode]
		[Column(Storage = "regionDescription", Name = "RegionDescription", DbType = "varchar(50)", CanBeNull = false)]
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

		#region int RegionID

		[AutoGenId]
		private int regionID;
		[DebuggerNonUserCode]
		[Column(Storage = "regionID", Name = "RegionID", DbType = "int", IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false)]
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
				}
			}
		}

		#endregion

		#region Children

		[Association(Storage = null, OtherKey = "RegionID", Name = "territories_ibfk_1")]
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

	[Table(Name = "northwind.shippers")]
	public partial class Shipper
	{
		#region string CompanyName

		private string companyName;
		[DebuggerNonUserCode]
		[Column(Storage = "companyName", Name = "CompanyName", DbType = "varchar(40)", CanBeNull = false)]
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

		#region string Phone

		private string phone;
		[DebuggerNonUserCode]
		[Column(Storage = "phone", Name = "Phone", DbType = "varchar(24)")]
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

		#region int ShipperID

		[AutoGenId]
		private int shipperID;
		[DebuggerNonUserCode]
		[Column(Storage = "shipperID", Name = "ShipperID", DbType = "int", IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false)]
		public int ShipperID
		{
			get
			{
				return shipperID;
			}
			set
			{
				if (value != shipperID)
				{
					shipperID = value;
				}
			}
		}

		#endregion

		#region Children

		[Association(Storage = null, OtherKey = "ShipperID", Name = "orders_ibfk_3")]
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

	[Table(Name = "northwind.suppliers")]
	public partial class Supplier
	{
		#region string Address

		private string address;
		[DebuggerNonUserCode]
		[Column(Storage = "address", Name = "Address", DbType = "varchar(60)")]
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
		[Column(Storage = "city", Name = "City", DbType = "varchar(15)")]
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
		[Column(Storage = "companyName", Name = "CompanyName", DbType = "varchar(40)", CanBeNull = false)]
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
		[Column(Storage = "contactName", Name = "ContactName", DbType = "varchar(30)")]
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
		[Column(Storage = "contactTitle", Name = "ContactTitle", DbType = "varchar(30)")]
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
		[Column(Storage = "country", Name = "Country", DbType = "varchar(15)")]
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
		[Column(Storage = "fax", Name = "Fax", DbType = "varchar(24)")]
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
		[Column(Storage = "phone", Name = "Phone", DbType = "varchar(24)")]
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
		[Column(Storage = "postalCode", Name = "PostalCode", DbType = "varchar(10)")]
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
		[Column(Storage = "region", Name = "Region", DbType = "varchar(15)")]
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

		#region int SupplierID

		[AutoGenId]
		private int supplierID;
		[DebuggerNonUserCode]
		[Column(Storage = "supplierID", Name = "SupplierID", DbType = "int", IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false)]
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
				}
			}
		}

		#endregion

		#region Children

		[Association(Storage = null, OtherKey = "SupplierID", Name = "products_ibfk_2")]
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

	[Table(Name = "northwind.territories")]
	public partial class Territory
	{
		#region int RegionID

		private int regionID;
		[DebuggerNonUserCode]
		[Column(Storage = "regionID", Name = "RegionID", DbType = "int", CanBeNull = false)]
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
				}
			}
		}

		#endregion

		#region string TerritoryDescription

		private string territoryDescription;
		[DebuggerNonUserCode]
		[Column(Storage = "territoryDescription", Name = "TerritoryDescription", DbType = "varchar(50)", CanBeNull = false)]
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
		[Column(Storage = "territoryID", Name = "TerritoryID", DbType = "varchar(20)", IsPrimaryKey = true, CanBeNull = false)]
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

		[Association(Storage = null, OtherKey = "TerritoryID", Name = "employeeterritories_ibfk_2")]
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

		private EntityRef<Region> region;
		[Association(Storage = "region", ThisKey = "RegionID", Name = "territories_ibfk_1")]
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

#region Auto-generated classes for "Northwind" database on 2008-05-12 16:51:00Z

//
//  ____  _     __  __      _        _
// |  _ \| |__ |  \/  | ___| |_ __ _| |
// | | | | '_ \| |\/| |/ _ \ __/ _` | |
// | |_| | |_) | |  | |  __/ || (_| | |
// |____/|_.__/|_|  |_|\___|\__\__,_|_|
//
// Auto-generated from "Northwind" on 2008-05-12 16:51:00Z
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
	public partial class Northwind : DbLinq.Linq.DataContext
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

	[Table(Name = "NORTHWIND.CATEGORIES")]
	public partial class Category
	{
		#region decimal CategoryID

		[AutoGenId]
		private decimal categoryID;
		[DebuggerNonUserCode]
		[Column(Storage = "categoryID", Name = "CATEGORYID", DbType = "NUMBER", IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false, Expression = null)]
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
		[Column(Storage = "categoryName", Name = "CATEGORYNAME", DbType = "VARCHAR2", CanBeNull = false, Expression = null)]
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
		[Column(Storage = "description", Name = "DESCRIPTION", DbType = "VARCHAR2", Expression = null)]
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
		[Column(Storage = "picture", Name = "PICTURE", DbType = "BLOB", Expression = null)]
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

		[Association(Storage = null, OtherKey = "CategoryID", Name = "SYS_C004394")]
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

	[Table(Name = "NORTHWIND.CUSTOMERS")]
	public partial class Customer
	{
		#region string Address

		private string address;
		[DebuggerNonUserCode]
		[Column(Storage = "address", Name = "ADDRESS", DbType = "VARCHAR2", Expression = null)]
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
		[Column(Storage = "city", Name = "CITY", DbType = "VARCHAR2", Expression = null)]
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
		[Column(Storage = "companyName", Name = "COMPANYNAME", DbType = "VARCHAR2", CanBeNull = false, Expression = null)]
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
		[Column(Storage = "contactName", Name = "CONTACTNAME", DbType = "VARCHAR2", Expression = null)]
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
		[Column(Storage = "contactTitle", Name = "CONTACTTITLE", DbType = "VARCHAR2", Expression = null)]
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
		[Column(Storage = "country", Name = "COUNTRY", DbType = "VARCHAR2", Expression = null)]
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
		[Column(Storage = "customerID", Name = "CUSTOMERID", DbType = "VARCHAR2", IsPrimaryKey = true, CanBeNull = false, Expression = null)]
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
		[Column(Storage = "fax", Name = "FAX", DbType = "VARCHAR2", Expression = null)]
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
		[Column(Storage = "phone", Name = "PHONE", DbType = "VARCHAR2", Expression = null)]
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
		[Column(Storage = "postalCode", Name = "POSTALCODE", DbType = "VARCHAR2", Expression = null)]
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
		[Column(Storage = "region", Name = "REGION", DbType = "VARCHAR2", Expression = null)]
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

		[Association(Storage = null, OtherKey = "CustomerID", Name = "SYS_C004411")]
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

	[Table(Name = "NORTHWIND.EMPLOYEES")]
	public partial class Employee
	{
		#region string Address

		private string address;
		[DebuggerNonUserCode]
		[Column(Storage = "address", Name = "ADDRESS", DbType = "VARCHAR2", Expression = null)]
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
		[Column(Storage = "birthDate", Name = "BIRTHDATE", DbType = "DATE", Expression = null)]
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
		[Column(Storage = "city", Name = "CITY", DbType = "VARCHAR2", Expression = null)]
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
		[Column(Storage = "country", Name = "COUNTRY", DbType = "VARCHAR2", Expression = null)]
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
		[Column(Storage = "employeeID", Name = "EMPLOYEEID", DbType = "NUMBER", IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false, Expression = null)]
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
		[Column(Storage = "firstName", Name = "FIRSTNAME", DbType = "VARCHAR2", CanBeNull = false, Expression = null)]
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
		[Column(Storage = "hireDate", Name = "HIREDATE", DbType = "DATE", Expression = null)]
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
		[Column(Storage = "homePhone", Name = "HOMEPHONE", DbType = "VARCHAR2", Expression = null)]
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
		[Column(Storage = "lastName", Name = "LASTNAME", DbType = "VARCHAR2", CanBeNull = false, Expression = null)]
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
		[Column(Storage = "notes", Name = "NOTES", DbType = "VARCHAR2", Expression = null)]
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
		[Column(Storage = "photo", Name = "PHOTO", DbType = "BLOB", Expression = null)]
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
		[Column(Storage = "postalCode", Name = "POSTALCODE", DbType = "VARCHAR2", Expression = null)]
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
		[Column(Storage = "region", Name = "REGION", DbType = "VARCHAR2", Expression = null)]
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
		[Column(Storage = "reportsTo", Name = "REPORTSTO", DbType = "NUMBER", Expression = null)]
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
		[Column(Storage = "title", Name = "TITLE", DbType = "VARCHAR2", Expression = null)]
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

		[Association(Storage = null, OtherKey = "ReportsTo", Name = "SYS_C004403")]
		[DebuggerNonUserCode]
		public EntityMSet<Employee> Employees
		{
			get
			{
				// L212 - child data available only when part of query
				return null;
			}
		}

		[Association(Storage = null, OtherKey = "EmployeeID", Name = "SYS_C004407")]
		[DebuggerNonUserCode]
		public EntityMSet<EmployeeTerritory> EmployeeTerritories
		{
			get
			{
				// L212 - child data available only when part of query
				return null;
			}
		}

		[Association(Storage = null, OtherKey = "EmployeeID", Name = "SYS_C004412")]
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
		[Association(Storage = "reportsToEmployee", ThisKey = "ReportsTo", Name = "SYS_C004403")]
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

	[Table(Name = "NORTHWIND.EMPLOYEETERRITORIES")]
	public partial class EmployeeTerritory
	{
		#region decimal EmployeeID

		private decimal employeeID;
		[DebuggerNonUserCode]
		[Column(Storage = "employeeID", Name = "EMPLOYEEID", DbType = "NUMBER", IsPrimaryKey = true, CanBeNull = false, Expression = null)]
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
		[Column(Storage = "territoryID", Name = "TERRITORYID", DbType = "VARCHAR2", IsPrimaryKey = true, CanBeNull = false, Expression = null)]
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
		[Association(Storage = "territory", ThisKey = "TerritoryID", Name = "SYS_C004408")]
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
		[Association(Storage = "employee", ThisKey = "EmployeeID", Name = "SYS_C004407")]
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

	[Table(Name = "NORTHWIND.ORDERS")]
	public partial class Order
	{
		#region string CustomerID

		private string customerID;
		[DebuggerNonUserCode]
		[Column(Storage = "customerID", Name = "CUSTOMERID", DbType = "VARCHAR2", Expression = null)]
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
		[Column(Storage = "employeeID", Name = "EMPLOYEEID", DbType = "NUMBER", Expression = null)]
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
		[Column(Storage = "freight", Name = "FREIGHT", DbType = "NUMBER", Expression = null)]
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
		[Column(Storage = "orderDate", Name = "ORDERDATE", DbType = "DATE", Expression = null)]
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
		[Column(Storage = "orderID", Name = "ORDERID", DbType = "NUMBER", IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false, Expression = null)]
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
		[Column(Storage = "requiredDate", Name = "REQUIREDDATE", DbType = "DATE", Expression = null)]
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
		[Column(Storage = "shipAddress", Name = "SHIPADDRESS", DbType = "VARCHAR2", Expression = null)]
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
		[Column(Storage = "shipCity", Name = "SHIPCITY", DbType = "VARCHAR2", Expression = null)]
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
		[Column(Storage = "shipCountry", Name = "SHIPCOUNTRY", DbType = "VARCHAR2", Expression = null)]
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
		[Column(Storage = "shipName", Name = "SHIPNAME", DbType = "VARCHAR2", Expression = null)]
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
		[Column(Storage = "shippedDate", Name = "SHIPPEDDATE", DbType = "DATE", Expression = null)]
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
		[Column(Storage = "shipPostalCode", Name = "SHIPPOSTALCODE", DbType = "VARCHAR2", Expression = null)]
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
		[Column(Storage = "shipRegion", Name = "SHIPREGION", DbType = "VARCHAR2", Expression = null)]
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
		[Column(Storage = "shipVia", Name = "SHIPVIA", DbType = "NUMBER", Expression = null)]
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

		[Association(Storage = null, OtherKey = "OrderID", Name = "SYS_C004419")]
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
		[Association(Storage = "employee", ThisKey = "EmployeeID", Name = "SYS_C004412")]
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
		[Association(Storage = "customer", ThisKey = "CustomerID", Name = "SYS_C004411")]
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

	[Table(Name = "NORTHWIND.ORDERDETAILS")]
	public partial class OrderDetail
	{
		#region float Discount

		private float discount;
		[DebuggerNonUserCode]
		[Column(Storage = "discount", Name = "DISCOUNT", DbType = "FLOAT", CanBeNull = false, Expression = null)]
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
		[Column(Storage = "orderID", Name = "ORDERID", DbType = "NUMBER", IsPrimaryKey = true, CanBeNull = false, Expression = null)]
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
		[Column(Storage = "productID", Name = "PRODUCTID", DbType = "NUMBER", IsPrimaryKey = true, CanBeNull = false, Expression = null)]
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
		[Column(Storage = "quantity", Name = "QUANTITY", DbType = "NUMBER", CanBeNull = false, Expression = null)]
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
		[Column(Storage = "unitPrice", Name = "UNITPRICE", DbType = "NUMBER", CanBeNull = false, Expression = null)]
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
		[Association(Storage = "product", ThisKey = "ProductID", Name = "SYS_C004420")]
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
		[Association(Storage = "order", ThisKey = "OrderID", Name = "SYS_C004419")]
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

	[Table(Name = "NORTHWIND.PRODUCTS")]
	public partial class Product
	{
		#region decimal? CategoryID

		private decimal? categoryID;
		[DebuggerNonUserCode]
		[Column(Storage = "categoryID", Name = "CATEGORYID", DbType = "NUMBER", Expression = null)]
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
		[Column(Storage = "discontinued", Name = "DISCONTINUED", DbType = "NUMBER", CanBeNull = false, Expression = null)]
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
		[Column(Storage = "productID", Name = "PRODUCTID", DbType = "NUMBER", IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false, Expression = null)]
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
		[Column(Storage = "productName", Name = "PRODUCTNAME", DbType = "VARCHAR2", CanBeNull = false, Expression = null)]
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
		[Column(Storage = "quantityPerUnit", Name = "QUANTITYPERUNIT", DbType = "VARCHAR2", Expression = null)]
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
		[Column(Storage = "reorderLevel", Name = "REORDERLEVEL", DbType = "NUMBER", Expression = null)]
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
		[Column(Storage = "supplierID", Name = "SUPPLIERID", DbType = "NUMBER", Expression = null)]
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
		[Column(Storage = "unitPrice", Name = "UNITPRICE", DbType = "NUMBER", Expression = null)]
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
		[Column(Storage = "unitsInStock", Name = "UNITSINSTOCK", DbType = "NUMBER", Expression = null)]
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
		[Column(Storage = "unitsOnOrder", Name = "UNITSONORDER", DbType = "NUMBER", Expression = null)]
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

		[Association(Storage = null, OtherKey = "ProductID", Name = "SYS_C004420")]
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
		[Association(Storage = "supplier", ThisKey = "SupplierID", Name = "SYS_C004395")]
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
		[Association(Storage = "category", ThisKey = "CategoryID", Name = "SYS_C004394")]
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

	[Table(Name = "NORTHWIND.REGION")]
	public partial class Region
	{
		#region string RegionDescription

		private string regionDescription;
		[DebuggerNonUserCode]
		[Column(Storage = "regionDescription", Name = "REGIONDESCRIPTION", DbType = "VARCHAR2", CanBeNull = false, Expression = null)]
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
		[Column(Storage = "regionID", Name = "REGIONID", DbType = "NUMBER", IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false, Expression = null)]
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

		[Association(Storage = null, OtherKey = "RegionID", Name = "SYS_C004383")]
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

	[Table(Name = "NORTHWIND.SUPPLIERS")]
	public partial class Supplier
	{
		#region string Address

		private string address;
		[DebuggerNonUserCode]
		[Column(Storage = "address", Name = "ADDRESS", DbType = "VARCHAR2", Expression = null)]
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
		[Column(Storage = "city", Name = "CITY", DbType = "VARCHAR2", Expression = null)]
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
		[Column(Storage = "companyName", Name = "COMPANYNAME", DbType = "VARCHAR2", CanBeNull = false, Expression = null)]
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
		[Column(Storage = "contactName", Name = "CONTACTNAME", DbType = "VARCHAR2", Expression = null)]
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
		[Column(Storage = "contactTitle", Name = "CONTACTTITLE", DbType = "VARCHAR2", Expression = null)]
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
		[Column(Storage = "country", Name = "COUNTRY", DbType = "VARCHAR2", Expression = null)]
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
		[Column(Storage = "fax", Name = "FAX", DbType = "VARCHAR2", Expression = null)]
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
		[Column(Storage = "phone", Name = "PHONE", DbType = "VARCHAR2", Expression = null)]
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
		[Column(Storage = "postalCode", Name = "POSTALCODE", DbType = "VARCHAR2", Expression = null)]
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
		[Column(Storage = "region", Name = "REGION", DbType = "VARCHAR2", Expression = null)]
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
		[Column(Storage = "supplierID", Name = "SUPPLIERID", DbType = "NUMBER", IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false, Expression = null)]
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

		[Association(Storage = null, OtherKey = "SupplierID", Name = "SYS_C004395")]
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

	[Table(Name = "NORTHWIND.TERRITORIES")]
	public partial class Territory
	{
		#region decimal RegionID

		private decimal regionID;
		[DebuggerNonUserCode]
		[Column(Storage = "regionID", Name = "REGIONID", DbType = "NUMBER", CanBeNull = false, Expression = null)]
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
		[Column(Storage = "territoryDescription", Name = "TERRITORYDESCRIPTION", DbType = "VARCHAR2", CanBeNull = false, Expression = null)]
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
		[Column(Storage = "territoryID", Name = "TERRITORYID", DbType = "VARCHAR2", IsPrimaryKey = true, CanBeNull = false, Expression = null)]
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

		[Association(Storage = null, OtherKey = "TerritoryID", Name = "SYS_C004408")]
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
		[Association(Storage = "region", ThisKey = "RegionID", Name = "SYS_C004383")]
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

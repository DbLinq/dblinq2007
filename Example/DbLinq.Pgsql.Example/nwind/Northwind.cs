#region Auto-generated classes for Northwind database on 2008-05-03 00:22:43Z

//
//  ____  _     __  __      _        _
// |  _ \| |__ |  \/  | ___| |_ __ _| |
// | | | | '_ \| |\/| |/ _ \ __/ _` | |
// | |_| | |_) | |  | |  __/ || (_| | |
// |____/|_.__/|_|  |_|\___|\__\__,_|_|
//
// Auto-generated from Northwind on 2008-05-03 00:22:43Z
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
		: base(connection, new DbLinq.PostgreSql.PgsqlVendor())
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

		#region stored procs
	
		
		[FunctionEx(Name = "hello0", ProcedureOrFunction = "")]	
		public string Hello0()	
		{	
			System.Data.Linq.IExecuteResult result = base.ExecuteMethodCall(this, ((MethodInfo)(MethodInfo.GetCurrentMethod())));	
			return (string)result.ReturnValue;	
		}	
		
	
		
		[FunctionEx(Name = "hello1", ProcedureOrFunction = "")]	
		public string Hello1([Parameter(Name = "name", DbType = "text")] string name)	
		{	
			System.Data.Linq.IExecuteResult result = base.ExecuteMethodCall(this, ((MethodInfo)(MethodInfo.GetCurrentMethod())), name);	
			return (string)result.ReturnValue;	
		}	
		
	
		
		[FunctionEx(Name = "hello2", ProcedureOrFunction = "")]	
		public string hello2([Parameter(Name = "name", DbType = "text")] string name
				,[Parameter(Name = "unused", DbType = "text")] string unused)	
		{	
			System.Data.Linq.IExecuteResult result = base.ExecuteMethodCall(this, ((MethodInfo)(MethodInfo.GetCurrentMethod())), name, unused);	
			return (string)result.ReturnValue;	
		}	
		
	
		
		[FunctionEx(Name = "getordercount", ProcedureOrFunction = "")]	
		public int GetOrderCount([Parameter(Name = "custid", DbType = "character varying")] string custid)	
		{	
			System.Data.Linq.IExecuteResult result = base.ExecuteMethodCall(this, ((MethodInfo)(MethodInfo.GetCurrentMethod())), custid);	
			return (int)result.ReturnValue;	
		}	
		
	
	#endregion
	}

	[Table(Name = "categories")]
	public partial class Category
	{
		#region int CategoryID

		[AutoGenId]
		private int categoryID;
		[DebuggerNonUserCode]
		[Column(Storage = "categoryID", Name = "categoryid", DbType = "integer(32,0)", IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false, Expression = "nextval('categories_categoryid_seq')")]
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
		[Column(Storage = "categoryName", Name = "categoryname", DbType = "character varying(15)", CanBeNull = false, Expression = null)]
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
		[Column(Storage = "description", Name = "description", DbType = "text", Expression = null)]
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
		[Column(Storage = "picture", Name = "picture", DbType = "oid", Expression = null)]
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

		[Association(Storage = null, OtherKey = "CategoryID", Name = "fk_prod_catg")]
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

	[Table(Name = "customers")]
	public partial class Customer
	{
		#region string Address

		private string address;
		[DebuggerNonUserCode]
		[Column(Storage = "address", Name = "address", DbType = "character varying(60)", Expression = null)]
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
		[Column(Storage = "city", Name = "city", DbType = "character varying(15)", Expression = null)]
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
		[Column(Storage = "companyName", Name = "companyname", DbType = "character varying(40)", CanBeNull = false, Expression = null)]
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
		[Column(Storage = "contactName", Name = "contactname", DbType = "character varying(30)", CanBeNull = false, Expression = null)]
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
		[Column(Storage = "contactTitle", Name = "contacttitle", DbType = "character varying(30)", Expression = null)]
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
		[Column(Storage = "country", Name = "country", DbType = "character varying(15)", Expression = null)]
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
		[Column(Storage = "customerID", Name = "customerid", DbType = "character varying(5)", IsPrimaryKey = true, CanBeNull = false, Expression = null)]
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
		[Column(Storage = "fax", Name = "fax", DbType = "character varying(24)", Expression = null)]
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
		[Column(Storage = "phone", Name = "phone", DbType = "character varying(24)", Expression = null)]
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
		[Column(Storage = "postalCode", Name = "postalcode", DbType = "character varying(10)", Expression = null)]
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
		[Column(Storage = "region", Name = "region", DbType = "character varying(15)", Expression = null)]
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

		[Association(Storage = null, OtherKey = "CustomerID", Name = "fk_order_customer")]
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

	[Table(Name = "employees")]
	public partial class Employee
	{
		#region string Address

		private string address;
		[DebuggerNonUserCode]
		[Column(Storage = "address", Name = "address", DbType = "character varying(60)", Expression = null)]
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
		[Column(Storage = "birthDate", Name = "birthdate", DbType = "date", Expression = null)]
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
		[Column(Storage = "city", Name = "city", DbType = "character varying(15)", Expression = null)]
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
		[Column(Storage = "country", Name = "country", DbType = "character varying(15)", Expression = null)]
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
		[Column(Storage = "employeeID", Name = "employeeid", DbType = "integer(32,0)", IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false, Expression = "nextval('employees_employeeid_seq')")]
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
		[Column(Storage = "firstName", Name = "firstname", DbType = "character varying(10)", CanBeNull = false, Expression = null)]
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
		[Column(Storage = "hireDate", Name = "hiredate", DbType = "timestamp without time zone", Expression = null)]
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
		[Column(Storage = "homePhone", Name = "homephone", DbType = "character varying(24)", Expression = null)]
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
		[Column(Storage = "lastName", Name = "lastname", DbType = "character varying(20)", CanBeNull = false, Expression = null)]
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
		[Column(Storage = "notes", Name = "notes", DbType = "text", Expression = null)]
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
		[Column(Storage = "photo", Name = "photo", DbType = "oid", Expression = null)]
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
		[Column(Storage = "postalCode", Name = "postalcode", DbType = "character varying(10)", Expression = null)]
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
		[Column(Storage = "region", Name = "region", DbType = "character varying(15)", Expression = null)]
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
		[Column(Storage = "reportsTo", Name = "reportsto", DbType = "integer(32,0)", Expression = null)]
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
		[Column(Storage = "title", Name = "title", DbType = "character varying(30)", Expression = null)]
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

		[Association(Storage = null, OtherKey = "ReportsTo", Name = "fk_emp_reportstoemp")]
		[DebuggerNonUserCode]
		public EntityMSet<Employee> Employees
		{
			get
			{
				// L212 - child data available only when part of query
				return null;
			}
		}

		[Association(Storage = null, OtherKey = "EmployeeID", Name = "employeeterritories_employeeid_fkey")]
		[DebuggerNonUserCode]
		public EntityMSet<EmployeeTerritory> EmployeeTerritories
		{
			get
			{
				// L212 - child data available only when part of query
				return null;
			}
		}

		[Association(Storage = null, OtherKey = "EmployeeID", Name = "fk_order_product")]
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

		private System.Data.Linq.EntityRef<Employee> fkEmPReportsToeMp;
		[Association(Storage = "fkEmPReportsToeMp", ThisKey = "ReportsTo", Name = "fk_emp_reportstoemp")]
		[DebuggerNonUserCode]
		public Employee ParentEmployee
		{
			get
			{
				return fkEmPReportsToeMp.Entity;
			}
			set
			{
				fkEmPReportsToeMp.Entity = value;
			}
		}


		#endregion

	}

	[Table(Name = "employeeterritories")]
	public partial class EmployeeTerritory
	{
		#region int EmployeeID

		private int employeeID;
		[DebuggerNonUserCode]
		[Column(Storage = "employeeID", Name = "employeeid", DbType = "integer(32,0)", IsPrimaryKey = true, CanBeNull = false, Expression = null)]
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
		[Column(Storage = "territoryID", Name = "territoryid", DbType = "character varying(20)", IsPrimaryKey = true, CanBeNull = false, Expression = null)]
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

		private System.Data.Linq.EntityRef<Territory> employeeTerritoriesTerritoryIDFkEY;
		[Association(Storage = "employeeTerritoriesTerritoryIDFkEY", ThisKey = "TerritoryID", Name = "employeeterritories_territoryid_fkey")]
		[DebuggerNonUserCode]
		public Territory Territory
		{
			get
			{
				return employeeTerritoriesTerritoryIDFkEY.Entity;
			}
			set
			{
				employeeTerritoriesTerritoryIDFkEY.Entity = value;
			}
		}

		private System.Data.Linq.EntityRef<Employee> employeeTerritoriesEmployeeIDFkEY;
		[Association(Storage = "employeeTerritoriesEmployeeIDFkEY", ThisKey = "EmployeeID", Name = "employeeterritories_employeeid_fkey")]
		[DebuggerNonUserCode]
		public Employee Employee
		{
			get
			{
				return employeeTerritoriesEmployeeIDFkEY.Entity;
			}
			set
			{
				employeeTerritoriesEmployeeIDFkEY.Entity = value;
			}
		}


		#endregion

	}

	[Table(Name = "orders")]
	public partial class Order
	{
		#region string CustomerID

		private string customerID;
		[DebuggerNonUserCode]
		[Column(Storage = "customerID", Name = "customerid", DbType = "character varying(5)", CanBeNull = false, Expression = null)]
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
		[Column(Storage = "employeeID", Name = "employeeid", DbType = "integer(32,0)", Expression = null)]
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
		[Column(Storage = "freight", Name = "freight", DbType = "numeric", Expression = null)]
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
		[Column(Storage = "orderDate", Name = "orderdate", DbType = "timestamp without time zone", Expression = null)]
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
		[Column(Storage = "orderID", Name = "orderid", DbType = "integer(32,0)", IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false, Expression = "nextval('orders_orderid_seq')")]
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
		[Column(Storage = "requiredDate", Name = "requireddate", DbType = "timestamp without time zone", Expression = null)]
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
		[Column(Storage = "shipAddress", Name = "shipaddress", DbType = "character varying(60)", Expression = null)]
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
		[Column(Storage = "shipCity", Name = "shipcity", DbType = "character varying(15)", Expression = null)]
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
		[Column(Storage = "shipCountry", Name = "shipcountry", DbType = "character varying(15)", Expression = null)]
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
		[Column(Storage = "shipName", Name = "shipname", DbType = "character varying(40)", Expression = null)]
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
		[Column(Storage = "shippedDate", Name = "shippeddate", DbType = "timestamp without time zone", Expression = null)]
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
		[Column(Storage = "shipPostalCode", Name = "shippostalcode", DbType = "character varying(10)", Expression = null)]
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
		[Column(Storage = "shipRegion", Name = "shipregion", DbType = "character varying(15)", Expression = null)]
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
		[Column(Storage = "shipVia", Name = "shipvia", DbType = "integer(32,0)", Expression = null)]
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

		[Association(Storage = null, OtherKey = "OrderID", Name = "orderdetails_orderid_fkey")]
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

		private System.Data.Linq.EntityRef<Employee> fkOrderProduct;
		[Association(Storage = "fkOrderProduct", ThisKey = "EmployeeID", Name = "fk_order_product")]
		[DebuggerNonUserCode]
		public Employee Employee
		{
			get
			{
				return fkOrderProduct.Entity;
			}
			set
			{
				fkOrderProduct.Entity = value;
			}
		}

		private System.Data.Linq.EntityRef<Customer> fkOrderCustomer;
		[Association(Storage = "fkOrderCustomer", ThisKey = "CustomerID", Name = "fk_order_customer")]
		[DebuggerNonUserCode]
		public Customer Customer
		{
			get
			{
				return fkOrderCustomer.Entity;
			}
			set
			{
				fkOrderCustomer.Entity = value;
			}
		}


		#endregion

	}

	[Table(Name = "orderdetails")]
	public partial class OrderDetail
	{
		#region double? Discount

		private double? discount;
		[DebuggerNonUserCode]
		[Column(Storage = "discount", Name = "discount", DbType = "double precision", Expression = null)]
		public double? Discount
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
		[Column(Storage = "orderID", Name = "orderid", DbType = "integer(32,0)", IsPrimaryKey = true, CanBeNull = false, Expression = null)]
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
		[Column(Storage = "productID", Name = "productid", DbType = "integer(32,0)", IsPrimaryKey = true, CanBeNull = false, Expression = null)]
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

		#region int? Quantity

		private int? quantity;
		[DebuggerNonUserCode]
		[Column(Storage = "quantity", Name = "quantity", DbType = "integer(32,0)", Expression = null)]
		public int? Quantity
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

		#region decimal? UnitPrice

		private decimal? unitPrice;
		[DebuggerNonUserCode]
		[Column(Storage = "unitPrice", Name = "unitprice", DbType = "numeric", Expression = null)]
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

		#region Parents

		private System.Data.Linq.EntityRef<Product> orderDetailsProductIDFkEY;
		[Association(Storage = "orderDetailsProductIDFkEY", ThisKey = "ProductID", Name = "orderdetails_productid_fkey")]
		[DebuggerNonUserCode]
		public Product Product
		{
			get
			{
				return orderDetailsProductIDFkEY.Entity;
			}
			set
			{
				orderDetailsProductIDFkEY.Entity = value;
			}
		}

		private System.Data.Linq.EntityRef<Order> orderDetailsOrderIDFkEY;
		[Association(Storage = "orderDetailsOrderIDFkEY", ThisKey = "OrderID", Name = "orderdetails_orderid_fkey")]
		[DebuggerNonUserCode]
		public Order Order
		{
			get
			{
				return orderDetailsOrderIDFkEY.Entity;
			}
			set
			{
				orderDetailsOrderIDFkEY.Entity = value;
			}
		}


		#endregion

	}

	[Table(Name = "products")]
	public partial class Product
	{
		#region int? CategoryID

		private int? categoryID;
		[DebuggerNonUserCode]
		[Column(Storage = "categoryID", Name = "categoryid", DbType = "integer(32,0)", Expression = null)]
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
		[Column(Storage = "discontinued", Name = "discontinued", DbType = "bit(1)", CanBeNull = false, Expression = null)]
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
		[Column(Storage = "productID", Name = "productid", DbType = "integer(32,0)", IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false, Expression = "nextval('products_productid_seq')")]
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
		[Column(Storage = "productName", Name = "productname", DbType = "character varying(40)", CanBeNull = false, Expression = null)]
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
		[Column(Storage = "quantityPerUnit", Name = "quantityperunit", DbType = "character varying(20)", Expression = null)]
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
		[Column(Storage = "reorderLevel", Name = "reorderlevel", DbType = "smallint(16,0)", Expression = null)]
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
		[Column(Storage = "supplierID", Name = "supplierid", DbType = "integer(32,0)", Expression = null)]
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
		[Column(Storage = "unitPrice", Name = "unitprice", DbType = "numeric", Expression = null)]
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
		[Column(Storage = "unitsInStock", Name = "unitsinstock", DbType = "smallint(16,0)", Expression = null)]
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
		[Column(Storage = "unitsOnOrder", Name = "unitsonorder", DbType = "smallint(16,0)", Expression = null)]
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

		[Association(Storage = null, OtherKey = "ProductID", Name = "orderdetails_productid_fkey")]
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

		private System.Data.Linq.EntityRef<Supplier> fkProdSupP;
		[Association(Storage = "fkProdSupP", ThisKey = "SupplierID", Name = "fk_prod_supp")]
		[DebuggerNonUserCode]
		public Supplier Supplier
		{
			get
			{
				return fkProdSupP.Entity;
			}
			set
			{
				fkProdSupP.Entity = value;
			}
		}

		private System.Data.Linq.EntityRef<Category> fkProdCatG;
		[Association(Storage = "fkProdCatG", ThisKey = "CategoryID", Name = "fk_prod_catg")]
		[DebuggerNonUserCode]
		public Category Category
		{
			get
			{
				return fkProdCatG.Entity;
			}
			set
			{
				fkProdCatG.Entity = value;
			}
		}


		#endregion

	}

	[Table(Name = "region")]
	public partial class Region
	{
		#region string RegionDescription

		private string regionDescription;
		[DebuggerNonUserCode]
		[Column(Storage = "regionDescription", Name = "regiondescription", DbType = "character varying(50)", CanBeNull = false, Expression = null)]
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
		[Column(Storage = "regionID", Name = "regionid", DbType = "integer(32,0)", IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false, Expression = "nextval('region_regionid_seq')")]
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

		[Association(Storage = null, OtherKey = "RegionID", Name = "fk_terr_region")]
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

	[Table(Name = "shippers")]
	public partial class Shipper
	{
		#region string CompanyName

		private string companyName;
		[DebuggerNonUserCode]
		[Column(Storage = "companyName", Name = "companyname", DbType = "character varying(40)", CanBeNull = false, Expression = null)]
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
		[Column(Storage = "phone", Name = "phone", DbType = "character varying(24)", Expression = null)]
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
		[Column(Storage = "shipperID", Name = "shipperid", DbType = "integer(32,0)", IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false, Expression = "nextval('shippers_shipperid_seq')")]
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

	}

	[Table(Name = "suppliers")]
	public partial class Supplier
	{
		#region string Address

		private string address;
		[DebuggerNonUserCode]
		[Column(Storage = "address", Name = "address", DbType = "character varying(60)", Expression = null)]
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
		[Column(Storage = "city", Name = "city", DbType = "character varying(15)", Expression = null)]
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
		[Column(Storage = "companyName", Name = "companyname", DbType = "character varying(40)", CanBeNull = false, Expression = null)]
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
		[Column(Storage = "contactName", Name = "contactname", DbType = "character varying(30)", Expression = null)]
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
		[Column(Storage = "contactTitle", Name = "contacttitle", DbType = "character varying(30)", Expression = null)]
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
		[Column(Storage = "country", Name = "country", DbType = "character varying(15)", Expression = null)]
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
		[Column(Storage = "fax", Name = "fax", DbType = "character varying(24)", Expression = null)]
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
		[Column(Storage = "phone", Name = "phone", DbType = "character varying(24)", Expression = null)]
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
		[Column(Storage = "postalCode", Name = "postalcode", DbType = "character varying(10)", Expression = null)]
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
		[Column(Storage = "region", Name = "region", DbType = "character varying(15)", Expression = null)]
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
		[Column(Storage = "supplierID", Name = "supplierid", DbType = "integer(32,0)", IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false, Expression = "nextval('suppliers_supplierid_seq')")]
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

		[Association(Storage = null, OtherKey = "SupplierID", Name = "fk_prod_supp")]
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

	[Table(Name = "territories")]
	public partial class Territory
	{
		#region int RegionID

		private int regionID;
		[DebuggerNonUserCode]
		[Column(Storage = "regionID", Name = "regionid", DbType = "integer(32,0)", CanBeNull = false, Expression = null)]
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
		[Column(Storage = "territoryDescription", Name = "territorydescription", DbType = "character varying(50)", CanBeNull = false, Expression = null)]
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
		[Column(Storage = "territoryID", Name = "territoryid", DbType = "character varying(20)", IsPrimaryKey = true, CanBeNull = false, Expression = null)]
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

		[Association(Storage = null, OtherKey = "TerritoryID", Name = "employeeterritories_territoryid_fkey")]
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

		private System.Data.Linq.EntityRef<Region> fkTeRrRegion;
		[Association(Storage = "fkTeRrRegion", ThisKey = "RegionID", Name = "fk_terr_region")]
		[DebuggerNonUserCode]
		public Region Region
		{
			get
			{
				return fkTeRrRegion.Entity;
			}
			set
			{
				fkTeRrRegion.Entity = value;
			}
		}


		#endregion

	}
}

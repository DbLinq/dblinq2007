#region Auto-generated classes for Northwind database on 2008-03-12 22:22:46Z

//
//              _        _ __                       _
//   /\/\   ___| |_ __ _| / _\ ___  __ _ _   _  ___| |
//  /    \ / _ \ __/ _` | \ \ / _ \/ _` | | | |/ _ \ |
// / /\/\ \  __/ || (_| | |\ \  __/ (_| | |_| |  __/ |
// \/    \/\___|\__\__,_|_\__/\___|\__, |\__,_|\___|_|
//                                    |_|
// Auto-generated from Northwind on 2008-03-12 22:22:46Z
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
	public partial class Northwind : DbLinq.MySql.MySqlDataContext
	{
		//public Northwind(string connectionString)
		//    : base(connectionString)
		//{
		//}

		public Northwind(IDbConnection connection)
		    : base(connection)
		{
		}

		public Table<Region> Region { get { return GetTable<Region>(); } }
		public Table<Supplier> Suppliers { get { return GetTable<Supplier>(); } }
		public Table<Shipper> Shippers { get { return GetTable<Shipper>(); } }
		public Table<Category> Categories { get { return GetTable<Category>(); } }
		public Table<Customer> Customers { get { return GetTable<Customer>(); } }
		public Table<Employee> Employees { get { return GetTable<Employee>(); } }
		public Table<Territory> Territories { get { return GetTable<Territory>(); } }
		public Table<EmployeeTerritory> EmployeeTerritories { get { return GetTable<EmployeeTerritory>(); } }
		public Table<Order> Orders { get { return GetTable<Order>(); } }
		public Table<Product> Products { get { return GetTable<Product>(); } }
		public Table<OrderDetail> OrderDetails { get { return GetTable<OrderDetail>(); } }

		[Function(Name = "getOrderCount", IsComposable = true)]
		public int GetOrderCount([Parameter(Name = "custId", DbType = "VARCHAR(5)")] string custId)
		{
			var result = ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod(), custId);
			return (int)result.ReturnValue;
		}

		[Function(Name = "hello0", IsComposable = true)]
		public string Hello0()
		{
			var result = ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod());
			return (string)result.ReturnValue;
		}

		[Function(Name = "hello1", IsComposable = true)]
		public string Hello1([Parameter(Name = "s", DbType = "CHAR(20)")] string s)
		{
			var result = ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod(), s);
			return (string)result.ReturnValue;
		}

		[Function(Name = "hello2", IsComposable = true)]
		public string Hello2([Parameter(Name = "s", DbType = "CHAR(20)")] string s, [Parameter(Name = "s2", DbType = "int")] int s2)
		{
			var result = ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod(), s, s2);
			return (string)result.ReturnValue;
		}

		[Function(Name = "sp_selOrders", IsComposable = false)]
		public System.Data.DataSet SpSelOrders([Parameter(Name = "s", DbType = "CHAR(20)")] string s, [Parameter(Name = "s2", DbType = "int")] out int s2)
		{
			var result = ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod(), s);
			s2 = (int)result.GetParameterValue(1);
			return (System.Data.DataSet)result.ReturnValue;
		}

		[Function(Name = "sp_updOrders", IsComposable = false)]
		public void SpUpdOrders([Parameter(Name = "custID", DbType = "int")] int custID, [Parameter(Name = "prodId", DbType = "int")] int prodId)
		{
			var result = ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod(), custID, prodId);
		}

	}

	[Table(Name = "region")]
	public partial class Region : IModified
	{
		// IModified backing field
		public bool IsModified{ get; set; }

		#region int RegionID

		[AutoGenId]
		private int regionID;
		[Column(Storage = "regionID", Name = "RegionID", DbType = "int", IsPrimaryKey = true, IsDbGenerated = true)]
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

		#region string RegionDescription

		private string regionDescription;
		[Column(Storage = "regionDescription", Name = "RegionDescription", DbType = "varchar(50)")]
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

		[Association(Storage = "null", OtherKey = "RegionID", Name = "territories_ibfk_1")]
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

	[Table(Name = "suppliers")]
	public partial class Supplier : IModified
	{
		// IModified backing field
		public bool IsModified{ get; set; }

		#region int SupplierID

		[AutoGenId]
		private int supplierID;
		[Column(Storage = "supplierID", Name = "SupplierID", DbType = "int", IsPrimaryKey = true, IsDbGenerated = true)]
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

		#region string CompanyName

		private string companyName;
		[Column(Storage = "companyName", Name = "CompanyName", DbType = "varchar(40)")]
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
		[Column(Storage = "contactName", Name = "ContactName", DbType = "varchar(30)", CanBeNull = true)]
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
		[Column(Storage = "contactTitle", Name = "ContactTitle", DbType = "varchar(30)", CanBeNull = true)]
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

		#region string Address

		private string address;
		[Column(Storage = "address", Name = "Address", DbType = "varchar(60)", CanBeNull = true)]
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
		[Column(Storage = "city", Name = "City", DbType = "varchar(15)", CanBeNull = true)]
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

		#region string Region

		private string region;
		[Column(Storage = "region", Name = "Region", DbType = "varchar(15)", CanBeNull = true)]
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

		#region string PostalCode

		private string postalCode;
		[Column(Storage = "postalCode", Name = "PostalCode", DbType = "varchar(10)", CanBeNull = true)]
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

		#region string Country

		private string country;
		[Column(Storage = "country", Name = "Country", DbType = "varchar(15)", CanBeNull = true)]
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

		#region string Phone

		private string phone;
		[Column(Storage = "phone", Name = "Phone", DbType = "varchar(24)", CanBeNull = true)]
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

		#region string Fax

		private string fax;
		[Column(Storage = "fax", Name = "Fax", DbType = "varchar(24)", CanBeNull = true)]
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

		[Association(Storage = "null", OtherKey = "SupplierID", Name = "products_ibfk_2")]
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

	[Table(Name = "shippers")]
	public partial class Shipper : IModified
	{
		// IModified backing field
		public bool IsModified{ get; set; }

		#region int ShipperID

		[AutoGenId]
		private int shipperID;
		[Column(Storage = "shipperID", Name = "ShipperID", DbType = "int", IsPrimaryKey = true, IsDbGenerated = true)]
		[DebuggerNonUserCode]
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
					IsModified = true;
				}
			}
		}

		#endregion

		#region string CompanyName

		private string companyName;
		[Column(Storage = "companyName", Name = "CompanyName", DbType = "varchar(40)")]
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

		#region string Phone

		private string phone;
		[Column(Storage = "phone", Name = "Phone", DbType = "varchar(24)", CanBeNull = true)]
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

		#region GetHashCode(), Equals() - uses column ShipperID to look up objects in liveObjectMap

		public override int GetHashCode()
		{
			return ShipperID.GetHashCode();
		}

		public override bool Equals(object o)
		{
			Shipper other = o as Shipper;
			if (other == null)
			{
				return false;
			}
			return ShipperID.Equals(other.ShipperID);
		}

		#endregion

		#region Children

		[Association(Storage = "null", OtherKey = "ShipperID", Name = "orders_ibfk_3")]
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

	[Table(Name = "categories")]
	public partial class Category : IModified
	{
		// IModified backing field
		public bool IsModified{ get; set; }

		#region int CategoryID

		[AutoGenId]
		private int categoryID;
		[Column(Storage = "categoryID", Name = "CategoryID", DbType = "int", IsPrimaryKey = true, IsDbGenerated = true)]
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
		[Column(Storage = "categoryName", Name = "CategoryName", DbType = "varchar(15)")]
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
		[Column(Storage = "description", Name = "Description", DbType = "text", CanBeNull = true)]
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

		private byte[] picture;
		[Column(Storage = "picture", Name = "Picture", DbType = "blob", CanBeNull = true)]
		[DebuggerNonUserCode]
		public byte[] Picture
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

		[Association(Storage = "null", OtherKey = "CategoryID", Name = "products_ibfk_1")]
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
	public partial class Customer : IModified
	{
		// IModified backing field
		public bool IsModified{ get; set; }

		#region string CustomerID

		private string customerID;
		[Column(Storage = "customerID", Name = "CustomerID", DbType = "varchar(5)", IsPrimaryKey = true)]
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

		#region string CompanyName

		private string companyName;
		[Column(Storage = "companyName", Name = "CompanyName", DbType = "varchar(40)")]
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
		[Column(Storage = "contactName", Name = "ContactName", DbType = "varchar(30)", CanBeNull = true)]
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
		[Column(Storage = "contactTitle", Name = "ContactTitle", DbType = "varchar(30)", CanBeNull = true)]
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

		#region string Address

		private string address;
		[Column(Storage = "address", Name = "Address", DbType = "varchar(60)", CanBeNull = true)]
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
		[Column(Storage = "city", Name = "City", DbType = "varchar(15)", CanBeNull = true)]
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

		#region string Region

		private string region;
		[Column(Storage = "region", Name = "Region", DbType = "varchar(15)", CanBeNull = true)]
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

		#region string PostalCode

		private string postalCode;
		[Column(Storage = "postalCode", Name = "PostalCode", DbType = "varchar(10)", CanBeNull = true)]
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

		#region string Country

		private string country;
		[Column(Storage = "country", Name = "Country", DbType = "varchar(15)", CanBeNull = true)]
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

		#region string Phone

		private string phone;
		[Column(Storage = "phone", Name = "Phone", DbType = "varchar(24)", CanBeNull = true)]
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

		#region string Fax

		private string fax;
		[Column(Storage = "fax", Name = "Fax", DbType = "varchar(24)", CanBeNull = true)]
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

		[Association(Storage = "null", OtherKey = "CustomerID", Name = "orders_ibfk_1")]
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
	public partial class Employee : IModified
	{
		// IModified backing field
		public bool IsModified{ get; set; }

		#region int EmployeeID

		[AutoGenId]
		private int employeeID;
		[Column(Storage = "employeeID", Name = "EmployeeID", DbType = "int", IsPrimaryKey = true, IsDbGenerated = true)]
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

		#region string LastName

		private string lastName;
		[Column(Storage = "lastName", Name = "LastName", DbType = "varchar(20)")]
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

		#region string FirstName

		private string firstName;
		[Column(Storage = "firstName", Name = "FirstName", DbType = "varchar(10)")]
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

		#region string Title

		private string title;
		[Column(Storage = "title", Name = "Title", DbType = "varchar(30)", CanBeNull = true)]
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

		#region System.DateTime? BirthDate

		private DateTime? birthDate;
		[Column(Storage = "birthDate", Name = "BirthDate", DbType = "datetime", CanBeNull = true)]
		[DebuggerNonUserCode]
		public DateTime? BirthDate
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

		#region System.DateTime? HireDate

		private DateTime? hiredAte;
		[Column(Storage = "hiredAte", Name = "HireDate", DbType = "datetime", CanBeNull = true)]
		[DebuggerNonUserCode]
		public DateTime? HireDate
		{
			get
			{
				return hiredAte;
			}
			set
			{
				if (value != hiredAte)
				{
					hiredAte = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region string Address

		private string address;
		[Column(Storage = "address", Name = "Address", DbType = "varchar(60)", CanBeNull = true)]
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
		[Column(Storage = "city", Name = "City", DbType = "varchar(15)", CanBeNull = true)]
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

		#region string Region

		private string region;
		[Column(Storage = "region", Name = "Region", DbType = "varchar(15)", CanBeNull = true)]
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

		#region string PostalCode

		private string postalCode;
		[Column(Storage = "postalCode", Name = "PostalCode", DbType = "varchar(10)", CanBeNull = true)]
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

		#region string Country

		private string country;
		[Column(Storage = "country", Name = "Country", DbType = "varchar(15)", CanBeNull = true)]
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

		#region string HomePhone

		private string homePhone;
		[Column(Storage = "homePhone", Name = "HomePhone", DbType = "varchar(24)", CanBeNull = true)]
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

		#region System.Byte[] Photo

		private byte[] photo;
		[Column(Storage = "photo", Name = "Photo", DbType = "blob", CanBeNull = true)]
		[DebuggerNonUserCode]
		public byte[] Photo
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

		#region string Notes

		private string notes;
		[Column(Storage = "notes", Name = "Notes", DbType = "text", CanBeNull = true)]
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

		#region int? ReportsTo

		private int? reportsTo;
		[Column(Storage = "reportsTo", Name = "ReportsTo", DbType = "int", CanBeNull = true)]
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

		[Association(Storage = "null", OtherKey = "EmployeeID", Name = "employees_ibfk_1")]
		[DebuggerNonUserCode]
		public EntityMSet<Employee> Employees
		{
			get
			{
				// L212 - child data available only when part of query
				return null;
			}
		}

		[Association(Storage = "null", OtherKey = "EmployeeID", Name = "employeeterritories_ibfk_1")]
		[DebuggerNonUserCode]
		public EntityMSet<EmployeeTerritory> EmployeeTerritories
		{
			get
			{
				// L212 - child data available only when part of query
				return null;
			}
		}

		[Association(Storage = "null", OtherKey = "EmployeeID", Name = "orders_ibfk_2")]
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

		private System.Data.Linq.EntityRef<Employee> employeesIBFK1;
		[Association(Storage = "employeesIBFK1", ThisKey = "ReportsTo", Name = "employees_ibfk_1")]
		[DebuggerNonUserCode]
		public Employee ParentEmployee
		{
			get
			{
				return employeesIBFK1.Entity;
			}
			set
			{
				employeesIBFK1.Entity = value;
			}
		}


		#endregion

	}

	[Table(Name = "territories")]
	public partial class Territory : IModified
	{
		// IModified backing field
		public bool IsModified{ get; set; }

		#region string TerritoryID

		private string territoryID;
		[Column(Storage = "territoryID", Name = "TerritoryID", DbType = "varchar(20)", IsPrimaryKey = true)]
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

		#region string TerritoryDescription

		private string territoryDescription;
		[Column(Storage = "territoryDescription", Name = "TerritoryDescription", DbType = "varchar(50)")]
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

		#region int RegionID

		private int regionID;
		[Column(Storage = "regionID", Name = "RegionID", DbType = "int")]
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

		[Association(Storage = "null", OtherKey = "TerritoryID", Name = "employeeterritories_ibfk_2")]
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

		private System.Data.Linq.EntityRef<Region> territoriesIBFK1;
		[Association(Storage = "territoriesIBFK1", ThisKey = "RegionID", Name = "territories_ibfk_1")]
		[DebuggerNonUserCode]
		public Region Region
		{
			get
			{
				return territoriesIBFK1.Entity;
			}
			set
			{
				territoriesIBFK1.Entity = value;
			}
		}


		#endregion

	}

	[Table(Name = "employeeterritories")]
	public partial class EmployeeTerritory : IModified
	{
		// IModified backing field
		public bool IsModified{ get; set; }

		#region int EmployeeID

		private int employeeID;
		[Column(Storage = "employeeID", Name = "EmployeeID", DbType = "int", IsPrimaryKey = true)]
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
		[Column(Storage = "territoryID", Name = "TerritoryID", DbType = "varchar(20)", IsPrimaryKey = true)]
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

		private System.Data.Linq.EntityRef<Employee> employeeTerritoriesIBFK1;
		[Association(Storage = "employeeTerritoriesIBFK1", ThisKey = "EmployeeID", Name = "employeeterritories_ibfk_1")]
		[DebuggerNonUserCode]
		public Employee Employee
		{
			get
			{
				return employeeTerritoriesIBFK1.Entity;
			}
			set
			{
				employeeTerritoriesIBFK1.Entity = value;
			}
		}

		private System.Data.Linq.EntityRef<Territory> employeeTerritoriesIBFK2;
		[Association(Storage = "employeeTerritoriesIBFK2", ThisKey = "TerritoryID", Name = "employeeterritories_ibfk_2")]
		[DebuggerNonUserCode]
		public Territory Territory
		{
			get
			{
				return employeeTerritoriesIBFK2.Entity;
			}
			set
			{
				employeeTerritoriesIBFK2.Entity = value;
			}
		}


		#endregion

	}

	[Table(Name = "orders")]
	public partial class Order : IModified
	{
		// IModified backing field
		public bool IsModified{ get; set; }

		#region int OrderID

		[AutoGenId]
		private int orderID;
		[Column(Storage = "orderID", Name = "OrderID", DbType = "int", IsPrimaryKey = true, IsDbGenerated = true)]
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

		#region string CustomerID

		private string customerID;
		[Column(Storage = "customerID", Name = "CustomerID", DbType = "varchar(5)", CanBeNull = true)]
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
		[Column(Storage = "employeeID", Name = "EmployeeID", DbType = "int", CanBeNull = true)]
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

		#region System.DateTime? OrderDate

		private DateTime? orderDate;
		[Column(Storage = "orderDate", Name = "OrderDate", DbType = "datetime", CanBeNull = true)]
		[DebuggerNonUserCode]
		public DateTime? OrderDate
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

		#region System.DateTime? RequiredDate

		private DateTime? requiredDate;
		[Column(Storage = "requiredDate", Name = "RequiredDate", DbType = "datetime", CanBeNull = true)]
		[DebuggerNonUserCode]
		public DateTime? RequiredDate
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

		#region System.DateTime? ShippedDate

		private DateTime? shippedDate;
		[Column(Storage = "shippedDate", Name = "ShippedDate", DbType = "datetime", CanBeNull = true)]
		[DebuggerNonUserCode]
		public DateTime? ShippedDate
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

		#region int? ShipVia

		private int? shipVia;
		[Column(Storage = "shipVia", Name = "ShipVia", DbType = "int", CanBeNull = true)]
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

		#region decimal? Freight

		private decimal? freight;
		[Column(Storage = "freight", Name = "Freight", DbType = "decimal", CanBeNull = true)]
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

		#region string ShipName

		private string shipName;
		[Column(Storage = "shipName", Name = "ShipName", DbType = "varchar(40)", CanBeNull = true)]
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

		#region string ShipAddress

		private string shipAddress;
		[Column(Storage = "shipAddress", Name = "ShipAddress", DbType = "varchar(60)", CanBeNull = true)]
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
		[Column(Storage = "shipCity", Name = "ShipCity", DbType = "varchar(15)", CanBeNull = true)]
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

		#region string ShipRegion

		private string shipRegion;
		[Column(Storage = "shipRegion", Name = "ShipRegion", DbType = "varchar(15)", CanBeNull = true)]
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

		#region string ShipPostalCode

		private string shipPostalCode;
		[Column(Storage = "shipPostalCode", Name = "ShipPostalCode", DbType = "varchar(10)", CanBeNull = true)]
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

		#region string ShipCountry

		private string shipCountry;
		[Column(Storage = "shipCountry", Name = "ShipCountry", DbType = "varchar(15)", CanBeNull = true)]
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

		[Association(Storage = "null", OtherKey = "OrderID", Name = "order@0020details_ibfk_1")]
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

		private System.Data.Linq.EntityRef<Customer> ordersIBFK1;
		[Association(Storage = "ordersIBFK1", ThisKey = "CustomerID", Name = "orders_ibfk_1")]
		[DebuggerNonUserCode]
		public Customer Customer
		{
			get
			{
				return ordersIBFK1.Entity;
			}
			set
			{
				ordersIBFK1.Entity = value;
			}
		}

		private System.Data.Linq.EntityRef<Employee> ordersIBFK2;
		[Association(Storage = "ordersIBFK2", ThisKey = "EmployeeID", Name = "orders_ibfk_2")]
		[DebuggerNonUserCode]
		public Employee Employee
		{
			get
			{
				return ordersIBFK2.Entity;
			}
			set
			{
				ordersIBFK2.Entity = value;
			}
		}

		private System.Data.Linq.EntityRef<Shipper> ordersIBFK3;
		[Association(Storage = "ordersIBFK3", ThisKey = "ShipVia", Name = "orders_ibfk_3")]
		[DebuggerNonUserCode]
		public Shipper Shipper
		{
			get
			{
				return ordersIBFK3.Entity;
			}
			set
			{
				ordersIBFK3.Entity = value;
			}
		}


		#endregion

	}

	[Table(Name = "products")]
	public partial class Product : IModified
	{
		// IModified backing field
		public bool IsModified{ get; set; }

		#region int ProductID

		[AutoGenId]
		private int productID;
		[Column(Storage = "productID", Name = "ProductID", DbType = "int", IsPrimaryKey = true, IsDbGenerated = true)]
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
		[Column(Storage = "productName", Name = "ProductName", DbType = "varchar(40)")]
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

		#region int? SupplierID

		private int? supplierID;
		[Column(Storage = "supplierID", Name = "SupplierID", DbType = "int", CanBeNull = true)]
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

		#region int? CategoryID

		private int? categoryID;
		[Column(Storage = "categoryID", Name = "CategoryID", DbType = "int", CanBeNull = true)]
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

		#region string QuantityPerUnit

		private string quantityPeruNit;
		[Column(Storage = "quantityPeruNit", Name = "QuantityPerUnit", DbType = "varchar(20)", CanBeNull = true)]
		[DebuggerNonUserCode]
		public string QuantityPerUnit
		{
			get
			{
				return quantityPeruNit;
			}
			set
			{
				if (value != quantityPeruNit)
				{
					quantityPeruNit = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region decimal? UnitPrice

		private decimal? unitPrice;
		[Column(Storage = "unitPrice", Name = "UnitPrice", DbType = "decimal", CanBeNull = true)]
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

		private short? unitsInsToCK;
		[Column(Storage = "unitsInsToCK", Name = "UnitsInStock", DbType = "smallint(6)", CanBeNull = true)]
		[DebuggerNonUserCode]
		public short? UnitsInStock
		{
			get
			{
				return unitsInsToCK;
			}
			set
			{
				if (value != unitsInsToCK)
				{
					unitsInsToCK = value;
					IsModified = true;
				}
			}
		}

		#endregion

		#region short? UnitsOnOrder

		private short? unitsOnOrder;
		[Column(Storage = "unitsOnOrder", Name = "UnitsOnOrder", DbType = "smallint(6)", CanBeNull = true)]
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

		#region short? ReorderLevel

		private short? reorderLevel;
		[Column(Storage = "reorderLevel", Name = "ReorderLevel", DbType = "smallint(6)", CanBeNull = true)]
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

		#region bool Discontinued

		private bool discontinued;
		[Column(Storage = "discontinued", Name = "Discontinued", DbType = "bit(1)")]
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

		[Association(Storage = "null", OtherKey = "ProductID", Name = "order@0020details_ibfk_2")]
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

		private System.Data.Linq.EntityRef<Category> productsIBFK1;
		[Association(Storage = "productsIBFK1", ThisKey = "CategoryID", Name = "products_ibfk_1")]
		[DebuggerNonUserCode]
		public Category Category
		{
			get
			{
				return productsIBFK1.Entity;
			}
			set
			{
				productsIBFK1.Entity = value;
			}
		}

		private System.Data.Linq.EntityRef<Supplier> productsIBFK2;
		[Association(Storage = "productsIBFK2", ThisKey = "SupplierID", Name = "products_ibfk_2")]
		[DebuggerNonUserCode]
		public Supplier Supplier
		{
			get
			{
				return productsIBFK2.Entity;
			}
			set
			{
				productsIBFK2.Entity = value;
			}
		}


		#endregion

	}

	[Table(Name = "order details")]
	public partial class OrderDetail : IModified
	{
		// IModified backing field
		public bool IsModified{ get; set; }

		#region int OrderID

		private int orderID;
		[Column(Storage = "orderID", Name = "OrderID", DbType = "int", IsPrimaryKey = true)]
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
		[Column(Storage = "productID", Name = "ProductID", DbType = "int", IsPrimaryKey = true)]
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

		#region decimal UnitPrice

		private decimal unitPrice;
		[Column(Storage = "unitPrice", Name = "UnitPrice", DbType = "decimal")]
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

		#region short Quantity

		private short quantity;
		[Column(Storage = "quantity", Name = "Quantity", DbType = "smallint(6)")]
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

		#region System.Single Discount

		private float discount;
		[Column(Storage = "discount", Name = "Discount", DbType = "float")]
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

		private System.Data.Linq.EntityRef<Order> order0020DetailsIBFK1;
		[Association(Storage = "order0020DetailsIBFK1", ThisKey = "OrderID", Name = "order@0020details_ibfk_1")]
		[DebuggerNonUserCode]
		public Order Order
		{
			get
			{
				return order0020DetailsIBFK1.Entity;
			}
			set
			{
				order0020DetailsIBFK1.Entity = value;
			}
		}

		private System.Data.Linq.EntityRef<Product> order0020DetailsIBFK2;
		[Association(Storage = "order0020DetailsIBFK2", ThisKey = "ProductID", Name = "order@0020details_ibfk_2")]
		[DebuggerNonUserCode]
		public Product Product
		{
			get
			{
				return order0020DetailsIBFK2.Entity;
			}
			set
			{
				order0020DetailsIBFK2.Entity = value;
			}
		}


		#endregion

	}
}

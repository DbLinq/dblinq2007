#region Auto-generated classes for Northwind database on 2008-03-22 13:56:09Z

//
//  ____  _     __  __      _        _
// |  _ \| |__ |  \/  | ___| |_ __ _| |
// | | | | '_ \| |\/| |/ _ \ __/ _` | |
// | |_| | |_) | |  | |  __/ || (_| | |
// |____/|_.__/|_|  |_|\___|\__\__,_|_|
//
// Auto-generated from Northwind on 2008-03-22 13:56:09Z
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
	public partial class Northwind : DbLinq.Oracle.OracleDataContext
	{
		//public Northwind(string connectionString)
		//    : base(connectionString)
		//{
		//}

		public Northwind(IDbConnection connection)
		    : base(connection)
		{
		}

		public Table<CaseTest> CaseTests { get { return GetTable<CaseTest>(); } }
		public Table<CaseTest2> CaseTest2 { get { return GetTable<CaseTest2>(); } }
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

	}

	[Table(Name = "CASETEST")]
	public partial class CaseTest : INotifyPropertyChanged
	{
		#region INotifyPropertyChanged handling

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion

		#region string NoCase

		private string noCase;
		[Column(Storage = "noCase", Name = "NOCASE", DbType = "VARCHAR2")]
		[DebuggerNonUserCode]
		public string NoCase
		{
			get
			{
				return noCase;
			}
			set
			{
				if (value != noCase)
				{
					noCase = value;
					OnPropertyChanged("NoCase");
				}
			}
		}

		#endregion

		#region string Value0

		private string value0;
		[Column(Storage = "value0", Name = "value0", DbType = "VARCHAR2")]
		[DebuggerNonUserCode]
		public string Value0
		{
			get
			{
				return value0;
			}
			set
			{
				if (value != value0)
				{
					value0 = value;
					OnPropertyChanged("Value0");
				}
			}
		}

		#endregion

		#region string WithCase

		private string withCase;
		[Column(Storage = "withCase", Name = "WithCase", DbType = "VARCHAR2")]
		[DebuggerNonUserCode]
		public string WithCase
		{
			get
			{
				return withCase;
			}
			set
			{
				if (value != withCase)
				{
					withCase = value;
					OnPropertyChanged("WithCase");
				}
			}
		}

		#endregion

		#warning L189 table CASETEST has no primary key. Multiple C# objects will refer to the same row.
	}

	[Table(Name = "CaseTest2")]
	public partial class CaseTest2 : INotifyPropertyChanged
	{
		#region INotifyPropertyChanged handling

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion

		#region string SimpleColumn

		private string simpleColumn;
		[Column(Storage = "simpleColumn", Name = "SimpleColumn", DbType = "VARCHAR2")]
		[DebuggerNonUserCode]
		public string SimpleColumn
		{
			get
			{
				return simpleColumn;
			}
			set
			{
				if (value != simpleColumn)
				{
					simpleColumn = value;
					OnPropertyChanged("SimpleColumn");
				}
			}
		}

		#endregion

		#warning L189 table CaseTest2 has no primary key. Multiple C# objects will refer to the same row.
	}

	[Table(Name = "CATEGORIES")]
	public partial class Category : INotifyPropertyChanged
	{
		#region INotifyPropertyChanged handling

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion

		#region int CategoryID

		[AutoGenId]
		private int categoryID;
		[Column(Storage = "categoryID", Name = "CATEGORYID", DbType = "NUMBER", IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false)]
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
					OnPropertyChanged("CategoryID");
				}
			}
		}

		#endregion

		#region string CategoryName

		private string categoryName;
		[Column(Storage = "categoryName", Name = "CATEGORYNAME", DbType = "VARCHAR2", CanBeNull = false)]
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
					OnPropertyChanged("CategoryName");
				}
			}
		}

		#endregion

		#region string Description

		private string description;
		[Column(Storage = "description", Name = "DESCRIPTION", DbType = "VARCHAR2")]
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
					OnPropertyChanged("Description");
				}
			}
		}

		#endregion

		#region System.Byte[] Picture

		private byte[] picture;
		[Column(Storage = "picture", Name = "PICTURE", DbType = "BLOB")]
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
					OnPropertyChanged("Picture");
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

		[Association(Storage = "null", OtherKey = "CategoryID", Name = "SYS_C004131")]
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

	[Table(Name = "CUSTOMERS")]
	public partial class Customer : INotifyPropertyChanged
	{
		#region INotifyPropertyChanged handling

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion

		#region string Address

		private string address;
		[Column(Storage = "address", Name = "ADDRESS", DbType = "VARCHAR2")]
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
					OnPropertyChanged("Address");
				}
			}
		}

		#endregion

		#region string City

		private string city;
		[Column(Storage = "city", Name = "CITY", DbType = "VARCHAR2")]
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
					OnPropertyChanged("City");
				}
			}
		}

		#endregion

		#region string CompanyName

		private string companyName;
		[Column(Storage = "companyName", Name = "COMPANYNAME", DbType = "VARCHAR2", CanBeNull = false)]
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
					OnPropertyChanged("CompanyName");
				}
			}
		}

		#endregion

		#region string ContactName

		private string contactName;
		[Column(Storage = "contactName", Name = "CONTACTNAME", DbType = "VARCHAR2")]
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
					OnPropertyChanged("ContactName");
				}
			}
		}

		#endregion

		#region string ContactTitle

		private string contactTitle;
		[Column(Storage = "contactTitle", Name = "CONTACTTITLE", DbType = "VARCHAR2")]
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
					OnPropertyChanged("ContactTitle");
				}
			}
		}

		#endregion

		#region string Country

		private string country;
		[Column(Storage = "country", Name = "COUNTRY", DbType = "VARCHAR2")]
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
					OnPropertyChanged("Country");
				}
			}
		}

		#endregion

		#region string CustomerID

		private string customerID;
		[Column(Storage = "customerID", Name = "CUSTOMERID", DbType = "VARCHAR2", IsPrimaryKey = true, CanBeNull = false)]
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
					OnPropertyChanged("CustomerID");
				}
			}
		}

		#endregion

		#region string Fax

		private string fax;
		[Column(Storage = "fax", Name = "FAX", DbType = "VARCHAR2")]
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
					OnPropertyChanged("Fax");
				}
			}
		}

		#endregion

		#region string Phone

		private string phone;
		[Column(Storage = "phone", Name = "PHONE", DbType = "VARCHAR2")]
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
					OnPropertyChanged("Phone");
				}
			}
		}

		#endregion

		#region string PostalCode

		private string postalCode;
		[Column(Storage = "postalCode", Name = "POSTALCODE", DbType = "VARCHAR2")]
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
					OnPropertyChanged("PostalCode");
				}
			}
		}

		#endregion

		#region string Region

		private string region;
		[Column(Storage = "region", Name = "REGION", DbType = "VARCHAR2")]
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
					OnPropertyChanged("Region");
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

		[Association(Storage = "null", OtherKey = "CustomerID", Name = "SYS_C004148")]
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

	[Table(Name = "EMPLOYEES")]
	public partial class Employee : INotifyPropertyChanged
	{
		#region INotifyPropertyChanged handling

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion

		#region string Address

		private string address;
		[Column(Storage = "address", Name = "ADDRESS", DbType = "VARCHAR2")]
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
					OnPropertyChanged("Address");
				}
			}
		}

		#endregion

		#region System.DateTime? BirthDate

		private DateTime? birthDate;
		[Column(Storage = "birthDate", Name = "BIRTHDATE", DbType = "DATE")]
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
					OnPropertyChanged("BirthDate");
				}
			}
		}

		#endregion

		#region string City

		private string city;
		[Column(Storage = "city", Name = "CITY", DbType = "VARCHAR2")]
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
					OnPropertyChanged("City");
				}
			}
		}

		#endregion

		#region string Country

		private string country;
		[Column(Storage = "country", Name = "COUNTRY", DbType = "VARCHAR2")]
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
					OnPropertyChanged("Country");
				}
			}
		}

		#endregion

		#region int EmployeeID

		[AutoGenId]
		private int employeeID;
		[Column(Storage = "employeeID", Name = "EMPLOYEEID", DbType = "NUMBER", IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false)]
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
					OnPropertyChanged("EmployeeID");
				}
			}
		}

		#endregion

		#region string FirstName

		private string firstName;
		[Column(Storage = "firstName", Name = "FIRSTNAME", DbType = "VARCHAR2", CanBeNull = false)]
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
					OnPropertyChanged("FirstName");
				}
			}
		}

		#endregion

		#region System.DateTime? HiredAte

		private DateTime? hiredAte;
		[Column(Storage = "hiredAte", Name = "HIREDATE", DbType = "DATE")]
		[DebuggerNonUserCode]
		public DateTime? HiredAte
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
					OnPropertyChanged("HiredAte");
				}
			}
		}

		#endregion

		#region string HomePhone

		private string homePhone;
		[Column(Storage = "homePhone", Name = "HOMEPHONE", DbType = "VARCHAR2")]
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
					OnPropertyChanged("HomePhone");
				}
			}
		}

		#endregion

		#region string LastName

		private string lastName;
		[Column(Storage = "lastName", Name = "LASTNAME", DbType = "VARCHAR2", CanBeNull = false)]
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
					OnPropertyChanged("LastName");
				}
			}
		}

		#endregion

		#region string Notes

		private string notes;
		[Column(Storage = "notes", Name = "NOTES", DbType = "VARCHAR2")]
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
					OnPropertyChanged("Notes");
				}
			}
		}

		#endregion

		#region System.Byte[] Photo

		private byte[] photo;
		[Column(Storage = "photo", Name = "PHOTO", DbType = "BLOB")]
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
					OnPropertyChanged("Photo");
				}
			}
		}

		#endregion

		#region string PostalCode

		private string postalCode;
		[Column(Storage = "postalCode", Name = "POSTALCODE", DbType = "VARCHAR2")]
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
					OnPropertyChanged("PostalCode");
				}
			}
		}

		#endregion

		#region string Region

		private string region;
		[Column(Storage = "region", Name = "REGION", DbType = "VARCHAR2")]
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
					OnPropertyChanged("Region");
				}
			}
		}

		#endregion

		#region int? ReportsTo

		private int? reportsTo;
		[Column(Storage = "reportsTo", Name = "REPORTSTO", DbType = "NUMBER")]
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
					OnPropertyChanged("ReportsTo");
				}
			}
		}

		#endregion

		#region string Title

		private string title;
		[Column(Storage = "title", Name = "TITLE", DbType = "VARCHAR2")]
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
					OnPropertyChanged("Title");
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

		[Association(Storage = "null", OtherKey = "EmployeeID", Name = "SYS_C004140")]
		[DebuggerNonUserCode]
		public EntityMSet<Employee> Employees
		{
			get
			{
				// L212 - child data available only when part of query
				return null;
			}
		}

		[Association(Storage = "null", OtherKey = "EmployeeID", Name = "SYS_C004144")]
		[DebuggerNonUserCode]
		public EntityMSet<EmployeeTerritory> EmployeeTerritories
		{
			get
			{
				// L212 - child data available only when part of query
				return null;
			}
		}

		[Association(Storage = "null", OtherKey = "EmployeeID", Name = "SYS_C004149")]
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

		private System.Data.Linq.EntityRef<Employee> sysC004140;
		[Association(Storage = "sysC004140", ThisKey = "ReportsTo", Name = "SYS_C004140")]
		[DebuggerNonUserCode]
		public Employee ParentEmployee
		{
			get
			{
				return sysC004140.Entity;
			}
			set
			{
				sysC004140.Entity = value;
			}
		}


		#endregion

	}

	[Table(Name = "EMPLOYEETERRITORIES")]
	public partial class EmployeeTerritory : INotifyPropertyChanged
	{
		#region INotifyPropertyChanged handling

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion

		#region int EmployeeID

		private int employeeID;
		[Column(Storage = "employeeID", Name = "EMPLOYEEID", DbType = "NUMBER", IsPrimaryKey = true, CanBeNull = false)]
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
					OnPropertyChanged("EmployeeID");
				}
			}
		}

		#endregion

		#region string TerritoryID

		private string territoryID;
		[Column(Storage = "territoryID", Name = "TERRITORYID", DbType = "VARCHAR2", IsPrimaryKey = true, CanBeNull = false)]
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
					OnPropertyChanged("TerritoryID");
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

		private System.Data.Linq.EntityRef<Territory> sysC004145;
		[Association(Storage = "sysC004145", ThisKey = "TerritoryID", Name = "SYS_C004145")]
		[DebuggerNonUserCode]
		public Territory Territory
		{
			get
			{
				return sysC004145.Entity;
			}
			set
			{
				sysC004145.Entity = value;
			}
		}

		private System.Data.Linq.EntityRef<Employee> sysC004144;
		[Association(Storage = "sysC004144", ThisKey = "EmployeeID", Name = "SYS_C004144")]
		[DebuggerNonUserCode]
		public Employee Employee
		{
			get
			{
				return sysC004144.Entity;
			}
			set
			{
				sysC004144.Entity = value;
			}
		}


		#endregion

	}

	[Table(Name = "ORDERS")]
	public partial class Order : INotifyPropertyChanged
	{
		#region INotifyPropertyChanged handling

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion

		#region string CustomerID

		private string customerID;
		[Column(Storage = "customerID", Name = "CUSTOMERID", DbType = "VARCHAR2")]
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
					OnPropertyChanged("CustomerID");
				}
			}
		}

		#endregion

		#region int? EmployeeID

		private int? employeeID;
		[Column(Storage = "employeeID", Name = "EMPLOYEEID", DbType = "NUMBER")]
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
					OnPropertyChanged("EmployeeID");
				}
			}
		}

		#endregion

		#region int? Freight

		private int? freight;
		[Column(Storage = "freight", Name = "FREIGHT", DbType = "NUMBER")]
		[DebuggerNonUserCode]
		public int? Freight
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
					OnPropertyChanged("Freight");
				}
			}
		}

		#endregion

		#region System.DateTime? OrderDate

		private DateTime? orderDate;
		[Column(Storage = "orderDate", Name = "ORDERDATE", DbType = "DATE")]
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
					OnPropertyChanged("OrderDate");
				}
			}
		}

		#endregion

		#region int OrderID

		[AutoGenId]
		private int orderID;
		[Column(Storage = "orderID", Name = "ORDERID", DbType = "NUMBER", IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false)]
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
					OnPropertyChanged("OrderID");
				}
			}
		}

		#endregion

		#region System.DateTime? RequiredDate

		private DateTime? requiredDate;
		[Column(Storage = "requiredDate", Name = "REQUIREDDATE", DbType = "DATE")]
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
					OnPropertyChanged("RequiredDate");
				}
			}
		}

		#endregion

		#region string ShipAddress

		private string shipAddress;
		[Column(Storage = "shipAddress", Name = "SHIPADDRESS", DbType = "VARCHAR2")]
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
					OnPropertyChanged("ShipAddress");
				}
			}
		}

		#endregion

		#region string ShipCity

		private string shipCity;
		[Column(Storage = "shipCity", Name = "SHIPCITY", DbType = "VARCHAR2")]
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
					OnPropertyChanged("ShipCity");
				}
			}
		}

		#endregion

		#region string ShipCountry

		private string shipCountry;
		[Column(Storage = "shipCountry", Name = "SHIPCOUNTRY", DbType = "VARCHAR2")]
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
					OnPropertyChanged("ShipCountry");
				}
			}
		}

		#endregion

		#region string ShipName

		private string shipName;
		[Column(Storage = "shipName", Name = "SHIPNAME", DbType = "VARCHAR2")]
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
					OnPropertyChanged("ShipName");
				}
			}
		}

		#endregion

		#region System.DateTime? ShippedDate

		private DateTime? shippedDate;
		[Column(Storage = "shippedDate", Name = "SHIPPEDDATE", DbType = "DATE")]
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
					OnPropertyChanged("ShippedDate");
				}
			}
		}

		#endregion

		#region string ShipPostalCode

		private string shipPostalCode;
		[Column(Storage = "shipPostalCode", Name = "SHIPPOSTALCODE", DbType = "VARCHAR2")]
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
					OnPropertyChanged("ShipPostalCode");
				}
			}
		}

		#endregion

		#region string ShipRegion

		private string shipRegion;
		[Column(Storage = "shipRegion", Name = "SHIPREGION", DbType = "VARCHAR2")]
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
					OnPropertyChanged("ShipRegion");
				}
			}
		}

		#endregion

		#region int? ShipVia

		private int? shipVia;
		[Column(Storage = "shipVia", Name = "SHIPVIA", DbType = "NUMBER")]
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
					OnPropertyChanged("ShipVia");
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

		[Association(Storage = "null", OtherKey = "OrderID", Name = "SYS_C004156")]
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

		private System.Data.Linq.EntityRef<Employee> sysC004149;
		[Association(Storage = "sysC004149", ThisKey = "EmployeeID", Name = "SYS_C004149")]
		[DebuggerNonUserCode]
		public Employee Employee
		{
			get
			{
				return sysC004149.Entity;
			}
			set
			{
				sysC004149.Entity = value;
			}
		}

		private System.Data.Linq.EntityRef<Customer> sysC004148;
		[Association(Storage = "sysC004148", ThisKey = "CustomerID", Name = "SYS_C004148")]
		[DebuggerNonUserCode]
		public Customer Customer
		{
			get
			{
				return sysC004148.Entity;
			}
			set
			{
				sysC004148.Entity = value;
			}
		}


		#endregion

	}

	[Table(Name = "ORDERDETAILS")]
	public partial class OrderDetail : INotifyPropertyChanged
	{
		#region INotifyPropertyChanged handling

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion

		#region double Discount

		private double discount;
		[Column(Storage = "discount", Name = "DISCOUNT", DbType = "FLOAT", CanBeNull = false)]
		[DebuggerNonUserCode]
		public double Discount
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
					OnPropertyChanged("Discount");
				}
			}
		}

		#endregion

		#region int OrderID

		private int orderID;
		[Column(Storage = "orderID", Name = "ORDERID", DbType = "NUMBER", IsPrimaryKey = true, CanBeNull = false)]
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
					OnPropertyChanged("OrderID");
				}
			}
		}

		#endregion

		#region int ProductID

		private int productID;
		[Column(Storage = "productID", Name = "PRODUCTID", DbType = "NUMBER", IsPrimaryKey = true, CanBeNull = false)]
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
					OnPropertyChanged("ProductID");
				}
			}
		}

		#endregion

		#region int Quantity

		private int quantity;
		[Column(Storage = "quantity", Name = "QUANTITY", DbType = "NUMBER", CanBeNull = false)]
		[DebuggerNonUserCode]
		public int Quantity
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
					OnPropertyChanged("Quantity");
				}
			}
		}

		#endregion

		#region int UnitPrice

		private int unitPrice;
		[Column(Storage = "unitPrice", Name = "UNITPRICE", DbType = "NUMBER", CanBeNull = false)]
		[DebuggerNonUserCode]
		public int UnitPrice
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
					OnPropertyChanged("UnitPrice");
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

		private System.Data.Linq.EntityRef<Product> sysC004157;
		[Association(Storage = "sysC004157", ThisKey = "ProductID", Name = "SYS_C004157")]
		[DebuggerNonUserCode]
		public Product Product
		{
			get
			{
				return sysC004157.Entity;
			}
			set
			{
				sysC004157.Entity = value;
			}
		}

		private System.Data.Linq.EntityRef<Order> sysC004156;
		[Association(Storage = "sysC004156", ThisKey = "OrderID", Name = "SYS_C004156")]
		[DebuggerNonUserCode]
		public Order Order
		{
			get
			{
				return sysC004156.Entity;
			}
			set
			{
				sysC004156.Entity = value;
			}
		}


		#endregion

	}

	[Table(Name = "PRODUCTS")]
	public partial class Product : INotifyPropertyChanged
	{
		#region INotifyPropertyChanged handling

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion

		#region int? CategoryID

		private int? categoryID;
		[Column(Storage = "categoryID", Name = "CATEGORYID", DbType = "NUMBER")]
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
					OnPropertyChanged("CategoryID");
				}
			}
		}

		#endregion

		#region bool Discontinued

		private bool discontinued;
		[Column(Storage = "discontinued", Name = "DISCONTINUED", DbType = "NUMBER", CanBeNull = false)]
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
					OnPropertyChanged("Discontinued");
				}
			}
		}

		#endregion

		#region int ProductID

		[AutoGenId]
		private int productID;
		[Column(Storage = "productID", Name = "PRODUCTID", DbType = "NUMBER", IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false)]
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
					OnPropertyChanged("ProductID");
				}
			}
		}

		#endregion

		#region string ProductName

		private string productName;
		[Column(Storage = "productName", Name = "PRODUCTNAME", DbType = "VARCHAR2", CanBeNull = false)]
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
					OnPropertyChanged("ProductName");
				}
			}
		}

		#endregion

		#region string QuantityPeruNit

		private string quantityPeruNit;
		[Column(Storage = "quantityPeruNit", Name = "QUANTITYPERUNIT", DbType = "VARCHAR2")]
		[DebuggerNonUserCode]
		public string QuantityPeruNit
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
					OnPropertyChanged("QuantityPeruNit");
				}
			}
		}

		#endregion

		#region int? ReorderLevel

		private int? reorderLevel;
		[Column(Storage = "reorderLevel", Name = "REORDERLEVEL", DbType = "NUMBER")]
		[DebuggerNonUserCode]
		public int? ReorderLevel
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
					OnPropertyChanged("ReorderLevel");
				}
			}
		}

		#endregion

		#region int? SupplierID

		private int? supplierID;
		[Column(Storage = "supplierID", Name = "SUPPLIERID", DbType = "NUMBER")]
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
					OnPropertyChanged("SupplierID");
				}
			}
		}

		#endregion

		#region int? UnitPrice

		private int? unitPrice;
		[Column(Storage = "unitPrice", Name = "UNITPRICE", DbType = "NUMBER")]
		[DebuggerNonUserCode]
		public int? UnitPrice
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
					OnPropertyChanged("UnitPrice");
				}
			}
		}

		#endregion

		#region int? UnitsInsToCK

		private int? unitsInsToCK;
		[Column(Storage = "unitsInsToCK", Name = "UNITSINSTOCK", DbType = "NUMBER")]
		[DebuggerNonUserCode]
		public int? UnitsInsToCK
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
					OnPropertyChanged("UnitsInsToCK");
				}
			}
		}

		#endregion

		#region int? UnitsOnOrder

		private int? unitsOnOrder;
		[Column(Storage = "unitsOnOrder", Name = "UNITSONORDER", DbType = "NUMBER")]
		[DebuggerNonUserCode]
		public int? UnitsOnOrder
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
					OnPropertyChanged("UnitsOnOrder");
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

		[Association(Storage = "null", OtherKey = "ProductID", Name = "SYS_C004157")]
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

		private System.Data.Linq.EntityRef<Supplier> sysC004132;
		[Association(Storage = "sysC004132", ThisKey = "SupplierID", Name = "SYS_C004132")]
		[DebuggerNonUserCode]
		public Supplier Supplier
		{
			get
			{
				return sysC004132.Entity;
			}
			set
			{
				sysC004132.Entity = value;
			}
		}

		private System.Data.Linq.EntityRef<Category> sysC004131;
		[Association(Storage = "sysC004131", ThisKey = "CategoryID", Name = "SYS_C004131")]
		[DebuggerNonUserCode]
		public Category Category
		{
			get
			{
				return sysC004131.Entity;
			}
			set
			{
				sysC004131.Entity = value;
			}
		}


		#endregion

	}

	[Table(Name = "REGION")]
	public partial class Region : INotifyPropertyChanged
	{
		#region INotifyPropertyChanged handling

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion

		#region string RegionDescription

		private string regionDescription;
		[Column(Storage = "regionDescription", Name = "REGIONDESCRIPTION", DbType = "VARCHAR2", CanBeNull = false)]
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
					OnPropertyChanged("RegionDescription");
				}
			}
		}

		#endregion

		#region int RegionID

		[AutoGenId]
		private int regionID;
		[Column(Storage = "regionID", Name = "REGIONID", DbType = "NUMBER", IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false)]
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
					OnPropertyChanged("RegionID");
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

		[Association(Storage = "null", OtherKey = "RegionID", Name = "SYS_C004120")]
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

	[Table(Name = "SUPPLIERS")]
	public partial class Supplier : INotifyPropertyChanged
	{
		#region INotifyPropertyChanged handling

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion

		#region string Address

		private string address;
		[Column(Storage = "address", Name = "ADDRESS", DbType = "VARCHAR2")]
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
					OnPropertyChanged("Address");
				}
			}
		}

		#endregion

		#region string City

		private string city;
		[Column(Storage = "city", Name = "CITY", DbType = "VARCHAR2")]
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
					OnPropertyChanged("City");
				}
			}
		}

		#endregion

		#region string CompanyName

		private string companyName;
		[Column(Storage = "companyName", Name = "COMPANYNAME", DbType = "VARCHAR2", CanBeNull = false)]
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
					OnPropertyChanged("CompanyName");
				}
			}
		}

		#endregion

		#region string ContactName

		private string contactName;
		[Column(Storage = "contactName", Name = "CONTACTNAME", DbType = "VARCHAR2")]
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
					OnPropertyChanged("ContactName");
				}
			}
		}

		#endregion

		#region string ContactTitle

		private string contactTitle;
		[Column(Storage = "contactTitle", Name = "CONTACTTITLE", DbType = "VARCHAR2")]
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
					OnPropertyChanged("ContactTitle");
				}
			}
		}

		#endregion

		#region string Country

		private string country;
		[Column(Storage = "country", Name = "COUNTRY", DbType = "VARCHAR2")]
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
					OnPropertyChanged("Country");
				}
			}
		}

		#endregion

		#region string Fax

		private string fax;
		[Column(Storage = "fax", Name = "FAX", DbType = "VARCHAR2")]
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
					OnPropertyChanged("Fax");
				}
			}
		}

		#endregion

		#region string Phone

		private string phone;
		[Column(Storage = "phone", Name = "PHONE", DbType = "VARCHAR2")]
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
					OnPropertyChanged("Phone");
				}
			}
		}

		#endregion

		#region string PostalCode

		private string postalCode;
		[Column(Storage = "postalCode", Name = "POSTALCODE", DbType = "VARCHAR2")]
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
					OnPropertyChanged("PostalCode");
				}
			}
		}

		#endregion

		#region string Region

		private string region;
		[Column(Storage = "region", Name = "REGION", DbType = "VARCHAR2")]
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
					OnPropertyChanged("Region");
				}
			}
		}

		#endregion

		#region int SupplierID

		[AutoGenId]
		private int supplierID;
		[Column(Storage = "supplierID", Name = "SUPPLIERID", DbType = "NUMBER", IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false)]
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
					OnPropertyChanged("SupplierID");
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

		[Association(Storage = "null", OtherKey = "SupplierID", Name = "SYS_C004132")]
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

	[Table(Name = "TERRITORIES")]
	public partial class Territory : INotifyPropertyChanged
	{
		#region INotifyPropertyChanged handling

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion

		#region int RegionID

		private int regionID;
		[Column(Storage = "regionID", Name = "REGIONID", DbType = "NUMBER", CanBeNull = false)]
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
					OnPropertyChanged("RegionID");
				}
			}
		}

		#endregion

		#region string TerritoryDescription

		private string territoryDescription;
		[Column(Storage = "territoryDescription", Name = "TERRITORYDESCRIPTION", DbType = "VARCHAR2", CanBeNull = false)]
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
					OnPropertyChanged("TerritoryDescription");
				}
			}
		}

		#endregion

		#region string TerritoryID

		private string territoryID;
		[Column(Storage = "territoryID", Name = "TERRITORYID", DbType = "VARCHAR2", IsPrimaryKey = true, CanBeNull = false)]
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
					OnPropertyChanged("TerritoryID");
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

		[Association(Storage = "null", OtherKey = "TerritoryID", Name = "SYS_C004145")]
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

		private System.Data.Linq.EntityRef<Region> sysC004120;
		[Association(Storage = "sysC004120", ThisKey = "RegionID", Name = "SYS_C004120")]
		[DebuggerNonUserCode]
		public Region Region
		{
			get
			{
				return sysC004120.Entity;
			}
			set
			{
				sysC004120.Entity = value;
			}
		}


		#endregion

	}
}

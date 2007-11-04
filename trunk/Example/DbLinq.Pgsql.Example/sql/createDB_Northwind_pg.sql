--DROP DATABASE IF EXISTS Northwind;
--CREATE DATABASE Northwind WITH OWNER = "LinqUser";
--USE Northwind;
--\connect Northwind;
--Carramba! http://www.postgresqlforums.com/forums/viewtopic.php?f=33&t=10
--Re: switch database connection in jdbc Postby wagnerch on Tue Jan 02, 2007 3:05 pm 
--You would need to disconnect and reconnect to the other database, or hold two connections to each database. 
--I am not aware of an option in PostgreSQL for switching databases, like MySQL has. 
--PostgreSQL has a similar feature, but it is called a schema.

--####################################################################
--## create tables
--####################################################################
CREATE TABLE Region (
  RegionID SERIAL NOT NULL,
  RegionDescription VARCHAR(50) NOT NULL,
  PRIMARY KEY(RegionID)
);

CREATE TABLE Territories (
  TerritoryID VARCHAR(20) NOT NULL,
  TerritoryDescription VARCHAR(50) NOT NULL,
  RegionID INTEGER NOT NULL,
  PRIMARY KEY(TerritoryID),
  CONSTRAINT FK_Terr_Region FOREIGN KEY (RegionID) REFERENCES Region(RegionID)
);

--####################################################################
CREATE TABLE Categories (
  CategoryID SERIAL NOT NULL,
  CategoryName VARCHAR(15) NOT NULL,
  Description TEXT NULL,
  Picture OID NULL, --BLOB type is called OID?
  PRIMARY KEY(CategoryID)
);

CREATE TABLE Suppliers (
  SupplierID SERIAL NOT NULL,
  CompanyName VARCHAR(40) NOT NULL DEFAULT '',
  ContactName VARCHAR(30) NULL,
  ContactTitle VARCHAR(30) NULL,
  Address VARCHAR(60) NULL,
  City VARCHAR(15) NULL,
  Region VARCHAR(15) NULL,
  PostalCode VARCHAR(10) NULL,
  Country VARCHAR(15) NULL,
  Phone VARCHAR(24) NULL,
  Fax VARCHAR(24) NULL,
  PRIMARY KEY(SupplierID)
);
--####################################################################

CREATE TABLE Products (
  ProductID SERIAL NOT NULL,
  ProductName VARCHAR(40) NOT NULL DEFAULT '',
  SupplierID INTEGER NULL,
  CategoryID INTEGER NULL,
  QuantityPerUnit VARCHAR(20) NULL,
  UnitPrice DECIMAL NULL,
  UnitsInStock SMALLINT NULL,
  UnitsOnOrder SMALLINT NULL,
  ReorderLevel SMALLINT NULL,
  Discontinued BIT NULL,
  PRIMARY KEY(ProductID),
  CONSTRAINT FK_prod_catg FOREIGN KEY (CategoryID) REFERENCES Categories(CategoryID),
  CONSTRAINT FK_prod_supp FOREIGN KEY (SupplierID) REFERENCES Suppliers(SupplierID)
  );
  
CREATE TABLE Customers (
  CustomerID VARCHAR(5) NOT NULL,
  CompanyName VARCHAR(40) NOT NULL,
  ContactName VARCHAR(30) NOT NULL ,
  ContactTitle VARCHAR(30) NULL,
  Address VARCHAR(60) NULL,
  City VARCHAR(15) NULL,
  Region VARCHAR(15) NULL,
  PostalCode VARCHAR(10) NULL,
  Country VARCHAR(15) NULL,
  Phone VARCHAR(24) NULL,
  Fax VARCHAR(24) NULL,
  PRIMARY KEY(CustomerID)
);  
  
--####################################################################
CREATE TABLE Shippers (
  ShipperID SERIAL NOT NULL,
  CompanyName VARCHAR(40) NOT NULL,
  Phone VARCHAR(24) NULL,
  PRIMARY KEY(ShipperID)
);

--####################################################################
CREATE TABLE Employees (
  EmployeeID SERIAL NOT NULL,
  LastName VARCHAR(20) NOT NULL,
  FirstName VARCHAR(10) NOT NULL,
  Title VARCHAR(30) NULL,
  BirthDate DATE NULL,
  HireDate TIMESTAMP NULL,
  Address VARCHAR(60) NULL,
  City VARCHAR(15) NULL,
  Region VARCHAR(15) NULL,
  PostalCode VARCHAR(10) NULL,
  Country VARCHAR(15) NULL,
  HomePhone VARCHAR(24) NULL,
  Photo OID NULL, --'BLOB'
  Notes TEXT NULL,
  ReportsTo INTEGER NULL,
  CONSTRAINT FK_Emp_ReportsToEmp FOREIGN KEY (ReportsTo) REFERENCES Employees(EmployeeID),
  PRIMARY KEY(EmployeeID)
);

CREATE TABLE Orders (
  OrderID SERIAL NOT NULL,
  CustomerID VARCHAR(5) NOT NULL,
  EmployeeID INTEGER NULL,
  OrderDate TIMESTAMP NULL,
  RequiredDate TIMESTAMP NULL,
  ShippedDate TIMESTAMP NULL,
  ShipVia INT NULL,
  Freight DECIMAL NULL,
  ShipName VARCHAR(40) NULL,
  ShipAddress VARCHAR(60) NULL,
  ShipCity VARCHAR(15) NULL,
  ShipRegion VARCHAR(15) NULL,
  ShipPostalCode VARCHAR(10) NULL,
  ShipCountry VARCHAR(15) NULL,
  PRIMARY KEY(OrderID),
  CONSTRAINT fk_order_customer FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerID),
  CONSTRAINT fk_order_product FOREIGN KEY (EmployeeID) REFERENCES Employees(EmployeeID)
);
 
--####################################################################
--## populate tables with seed data
--####################################################################
truncate table Categories CASCADE;
Insert INTO Categories (CategoryName,Description)
values ('Beverages',	'Soft drinks, coffees, teas, beers, and ales');
Insert INTO Categories (CategoryName,Description)
values ('Condiments','Sweet and savory sauces, relishes, spreads, and seasonings');

truncate table Orders; -- must be truncated before Customer
truncate table Customers CASCADE;

insert INTO Customers (CustomerID, CompanyName,ContactName,Country,PostalCode,City)
values ('AIRBU', 'airbus','jacques','France','10000','Paris');
insert INTO Customers (CustomerID, CompanyName,ContactName,Country,PostalCode,City)
values ('BT___','BT','graeme','U.K.','E14','London');

insert INTO Customers (CustomerID, CompanyName,ContactName,Country,PostalCode,City)
values ('ATT__','ATT','bob','USA','10021','New York');
insert INTO Customers (CustomerID, CompanyName,ContactName,Country,PostalCode,City)
values ('UKMOD', 'MOD','(secret)','U.K.','E14','London');

insert INTO Customers (CustomerID, CompanyName,ContactName, ContactTitle, Country,PostalCode,City, Phone)
values ('ALFKI', 'Alfreds Futterkiste','Maria Anders','Sales Representative','Germany','12209','Berlin','030-0074321');

insert INTO Customers (CustomerID, CompanyName,ContactName, ContactTitle, Country,PostalCode,City, Phone)
values ('WARTH', 'Wartian Herkku','Pirkko Koskitalo','Accounting Manager','Finland','90110','Oulu','981-443655');


truncate table Employees CASCADE;
insert INTO Employees (LastName,FirstName,Title,BirthDate,HireDate,Address,City,ReportsTo)
VALUES ('Fuller','Andrew','Vice President, Sales','19540101','19890101', '908 W. Capital Way','Tacoma',NULL);

insert INTO Employees (LastName,FirstName,Title,BirthDate,HireDate,Address,City,ReportsTo)
VALUES ('Davolio','Nancy','Sales Representative','19640101','19940101','507 - 20th Ave. E.  Apt. 2A','Seattle',1);

--truncate table Orders; -- must be truncated before Products
truncate table Products CASCADE;

insert INTO Products (ProductName,QuantityPerUnit,UnitsInStock,UnitsOnOrder,Discontinued)
VALUES ('Pen',10,     12, 2,  false);
insert INTO Products (ProductName,QuantityPerUnit,UnitsInStock,UnitsOnOrder,Discontinued)
VALUES ('Bicycle',1,  6, 0,  false);
insert INTO Products (ProductName,QuantityPerUnit,UnitsInStock,UnitsOnOrder,Discontinued)
VALUES ('Phone',3,    7, 0,  false);
insert INTO Products (ProductName,QuantityPerUnit,UnitsInStock,UnitsOnOrder,Discontinued)
VALUES ('SAM',1,      51, 11, false);
insert INTO Products (ProductName,QuantityPerUnit,UnitsInStock,UnitsOnOrder,Discontinued)
VALUES ('iPod',0,     11, 0, false);
insert INTO Products (ProductName,QuantityPerUnit,UnitsInStock,UnitsOnOrder,Discontinued)
VALUES ('Toilet Paper',2,  0, 3, true);
insert INTO Products (ProductName,QuantityPerUnit,UnitsInStock,UnitsOnOrder,Discontinued)
VALUES ('Fork',5,   111, 0, false);

truncate table Orders;
insert INTO Orders (CustomerID, EmployeeID, OrderDate, Freight)
Values ('AIRBU', 1, now(), 21.3);

insert INTO Orders (CustomerID, EmployeeID, OrderDate, Freight)
Values ('BT___', 1, now(), 11.1);

insert INTO Orders (CustomerID, EmployeeID, OrderDate, Freight)
Values ('BT___', 1, now(), 11.5);

insert INTO Orders (CustomerID, EmployeeID, OrderDate, Freight)
Values ('UKMOD', 1, now(), 32.5);


-- please match fieldnames and types to MySql AllTypes table 
-- (DbLinq.Mysql.Example\sql\create_LinqTestDB_pg.sql)
CREATE TABLE AllTypes
(
  int SERIAL NOT NULL,
  intN integer NULL,
  double float NOT NULL,
  doubleN float,
  decimal DECIMAL NOT NULL,
  decimalN DECIMAL,
  DateTime TIMESTAMP NOT NULL,
  DateTimeN TIMESTAMP,
  PRIMARY KEY(int)
);


CREATE FUNCTION hello0() RETURNS text AS $$ 
  BEGIN RETURN 'hello0'; END;
$$ LANGUAGE plpgsql;

-- contatenates strings. test case: select hello2('aa','bb')
CREATE OR REPLACE FUNCTION hello1(name text) RETURNS text AS $$ 
  BEGIN RETURN 'hello,' || name || '!'; END;
$$ LANGUAGE plpgsql;
CREATE OR REPLACE FUNCTION hello2(name text, unused text) RETURNS text AS $$ 
  BEGIN RETURN 'hello,' || name || '!'; SELECT * FROM customer; END;
$$ LANGUAGE plpgsql;


-- count orders for given CustomerID. test case: select getOrderCount(1)
CREATE OR REPLACE FUNCTION getOrderCount(custId VARCHAR) RETURNS INT AS $$
DECLARE
  count1 INTEGER;
BEGIN
  SELECT COUNT(*) INTO count1 FROM Orders WHERE CustomerID=custId;
  RETURN count1;
END;
$$ LANGUAGE plpgsql;

COMMIT;





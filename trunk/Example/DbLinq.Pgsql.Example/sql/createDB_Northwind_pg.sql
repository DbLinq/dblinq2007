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
  ProductID INTEGER NOT NULL,
  OrderDate TIMESTAMP NOT NULL,
  PRIMARY KEY(OrderID),
  CONSTRAINT fk_order_customer FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerID),
  CONSTRAINT fk_order_product FOREIGN KEY (ProductID) REFERENCES Products(ProductID)
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


truncate table Employees;
insert INTO Employees (LastName,FirstName,Title) VALUES ('Davolio','Nancy','Sales Representative');

--truncate table Orders; -- must be truncated before Products
truncate table Products CASCADE;

insert INTO Products ( ProductName,CategoryID,QuantityPerUnit) VALUES ('Pen',1,10);
insert INTO Products ( ProductName,CategoryID,QuantityPerUnit) VALUES ('Bicycle',1,1);
insert INTO Products ( ProductName,CategoryID,QuantityPerUnit) VALUES ('Phone',1,3);
insert INTO Products ( ProductName,CategoryID,QuantityPerUnit) VALUES ('SAM',1,1);
insert INTO Products ( ProductName,CategoryID,QuantityPerUnit) VALUES ('iPod',1,0);
insert INTO Products ( ProductName,CategoryID,QuantityPerUnit) VALUES ('Toilet Paper',1,2);
insert INTO Products ( ProductName,CategoryID,QuantityPerUnit) VALUES ('Fork',1,5);
insert INTO Products ( ProductName,CategoryID,QuantityPerUnit) VALUES ('Spoon',1,5);

truncate table Orders;
insert INTO Orders ( CustomerID, ProductID, OrderDate)
Values ( 'AIRBU'
, (Select ProductID from Products Where ProductName='Pen')
, now());

insert INTO Orders ( CustomerID, ProductID, OrderDate)
Values ( 'BT___'
, (Select ProductID from Products Where ProductName='Phone')
, now());

insert INTO Orders ( CustomerID, ProductID, OrderDate)
Values ( 'BT___'
, (Select ProductID from Products Where ProductName='Pen')
, now());

insert INTO Orders ( CustomerID, ProductID, OrderDate)
Values ( 'UKMOD'
, (Select ProductID from Products Where ProductName='SAM')
, now());


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





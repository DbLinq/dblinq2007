--####################################################################
-- Script to create Oracle version of the Northwind test DB
--
-- this script was tested on Oracle XE - so it does not contain any 'CREATE DATABASE' statements.
--####################################################################
CREATE TABLE Region (
  RegionID INTEGER NOT NULL,
  RegionDescription VARCHAR(50) NOT NULL,
  PRIMARY KEY(RegionID)
);

CREATE TABLE Territories (
  TerritoryID VARCHAR(20) NOT NULL,
  TerritoryDescription VARCHAR(50) NOT NULL,
  RegionID INTEGER NOT NULL,
  PRIMARY KEY(TerritoryID),
  FOREIGN KEY (RegionID) REFERENCES Region (RegionID)
);

--####################################################################
CREATE TABLE Categories (
  CategoryID INTEGER  NOT NULL,
  CategoryName VARCHAR(15) NOT NULL,
  Description VARCHAR(500) NULL,
  Picture BLOB NULL,
  PRIMARY KEY(CategoryID)
);

CREATE TABLE Suppliers (
  SupplierID INTEGER  NOT NULL,
  CompanyName VARCHAR(40) NOT NULL,
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
  ProductID INTEGER NOT NULL,
  ProductName VARCHAR(40) NOT NULL,
  SupplierID INTEGER NULL,
  CategoryID INTEGER NULL,
  QuantityPerUnit VARCHAR(20) NULL,
  UnitPrice DECIMAL NULL,
  UnitsInStock SMALLINT NULL,
  UnitsOnOrder SMALLINT NULL,
  ReorderLevel SMALLINT NULL,
  Discontinued NUMBER(1) NOT NULL, --'bool' field
  PRIMARY KEY(ProductID),
  FOREIGN KEY (CategoryID) REFERENCES Categories (CategoryID),
  FOREIGN KEY (SupplierID) REFERENCES Suppliers (SupplierID)
);
  
CREATE TABLE Customers (
  CustomerID VARCHAR(5) NOT NULL,
  CompanyName VARCHAR(40) NOT NULL,
  ContactName VARCHAR(30) NULL,
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
CREATE TABLE Employees (
  EmployeeID INTEGER NOT NULL,
  LastName VARCHAR(20) NOT NULL,
  FirstName VARCHAR(10) NOT NULL,
  Title VARCHAR(30) NULL,
  BirthDate DATE NULL,
  HireDate DATE NULL,
  Address VARCHAR(60) NULL,
  City VARCHAR(15) NULL,
  Region VARCHAR(15) NULL,
  PostalCode VARCHAR(10) NULL,
  Country VARCHAR(15) NULL,
  HomePhone VARCHAR(24) NULL,
  Photo BLOB NULL,
  Notes VARCHAR(100) NULL,
  ReportsTo INTEGER NULL,
  PRIMARY KEY(EmployeeID),
  FOREIGN KEY (ReportsTo)  REFERENCES Employees (EmployeeID)
);

CREATE TABLE EmployeeTerritories (
  EmployeeID INTEGER NOT NULL,
  TerritoryID VARCHAR(20) NOT NULL,
  PRIMARY KEY(EmployeeID,TerritoryID),
  FOREIGN KEY (EmployeeID) REFERENCES Employees (EmployeeID),
  FOREIGN KEY (TerritoryID) REFERENCES Territories (TerritoryID)
);

--####################################################################
CREATE TABLE Orders (
  OrderID INTEGER NOT NULL,
  CustomerID VARCHAR(5) NULL,
  EmployeeID INTEGER NULL,
  OrderDate DATE NULL,
  RequiredDate DATE NULL,
  ShippedDate DATE NULL,
  ShipVia INT NULL,
  Freight DECIMAL NULL,
  ShipName VARCHAR(40) NULL,
  ShipAddress VARCHAR(60) NULL,
  ShipCity VARCHAR(15) NULL,
  ShipRegion VARCHAR(15) NULL,
  ShipPostalCode VARCHAR(10) NULL,
  ShipCountry VARCHAR(15) NULL,
  PRIMARY KEY(OrderID),
  FOREIGN KEY (CustomerID) REFERENCES Customers (CustomerID),
  FOREIGN KEY (EmployeeID) REFERENCES Employees (EmployeeID)
);

--####################################################################
CREATE TABLE OrderDetails (
  OrderID INTEGER NOT NULL,
  ProductID INTEGER NOT NULL,
  UnitPrice DECIMAL NOT NULL,
  Quantity SMALLINT NOT NULL,
  Discount FLOAT NOT NULL,
  PRIMARY KEY(OrderID,ProductID),
  FOREIGN KEY (OrderID) REFERENCES Orders (OrderID),
  FOREIGN KEY (ProductID) REFERENCES Products (ProductID)
);

--####################################################################
CREATE SEQUENCE Region_seq     START WITH 1    INCREMENT BY 1;
CREATE SEQUENCE Categories_seq START WITH 1    INCREMENT BY 1;
CREATE SEQUENCE Suppliers_seq  START WITH 1    INCREMENT BY 1;
CREATE SEQUENCE Products_seq   START WITH 1    INCREMENT BY 1;
CREATE SEQUENCE Orders_seq     START WITH 1000 INCREMENT BY 1;
CREATE SEQUENCE Employees_seq  START WITH 1    INCREMENT BY 1;
 

--####################################################################
Insert INTO Categories (CategoryID, CategoryName,Description)
values (Categories_seq.NextVal, 'Beverages',	'Soft drinks, coffees, teas, beers, and ales');
Insert INTO Categories (CategoryID, CategoryName,Description)
values (Categories_seq.NextVal, 'Condiments','Sweet and savory sauces, relishes, spreads, and seasonings');

--####################################################################
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
values ('BONAP', 'Bon something','Bon Boss','Sales Representative','France','11109','Paris','033-0074321');

insert INTO Customers (CustomerID, CompanyName,ContactName, ContactTitle, Country,PostalCode,City, Phone)
values ('WARTH', 'Wartian Herkku','Pirkko Koskitalo','Accounting Manager','Finland','90110','Oulu','981-443655');

--####################################################################
insert INTO Suppliers (SupplierID, CompanyName, ContactName, ContactTitle, Address, City, Region, Country)
VALUES (Suppliers_seq.Nextval, 'alles AG', 'Harald Reitmeyer', 'Prof', 'Fischergasse 8', 'Heidelberg', 'B-W', 'Germany');

insert INTO Suppliers (SupplierID, CompanyName, ContactName, ContactTitle, Address, City, Region, Country)
VALUES (Suppliers_seq.Nextval, 'Microsoft', 'Mr Allen', 'Monopolist', '1 MS', 'Redmond', 'WA', 'USA');


insert INTO Products (ProductId, ProductName,SupplierID, QuantityPerUnit,UnitsInStock,UnitsOnOrder,Discontinued)
VALUES (Products_seq.nextval, 'Pen',1, 10,     12, 2,  0);
insert INTO Products (ProductId, ProductName,SupplierID, QuantityPerUnit,UnitsInStock,UnitsOnOrder,Discontinued)
VALUES (Products_seq.nextval, 'Bicycle',1, 1,  6, 0,  0);
insert INTO Products (ProductId, ProductName,QuantityPerUnit,UnitsInStock,UnitsOnOrder,Discontinued)
VALUES (Products_seq.nextval, 'Phone',3,    7, 0,  0);
insert INTO Products (ProductId, ProductName,QuantityPerUnit,UnitsInStock,UnitsOnOrder,Discontinued)
VALUES (Products_seq.nextval, 'SAM',1,      51, 11, 0);
insert INTO Products (ProductId, ProductName,QuantityPerUnit,UnitsInStock,UnitsOnOrder,Discontinued)
VALUES (Products_seq.nextval, 'iPod',0,     11, 0, 0);
insert INTO Products (ProductId, ProductName,QuantityPerUnit,UnitsInStock,UnitsOnOrder,Discontinued)
VALUES (Products_seq.nextval, 'Toilet Paper',2,  0, 3, 1);
insert INTO Products (ProductId, ProductName,QuantityPerUnit,UnitsInStock,UnitsOnOrder,Discontinued)
VALUES (Products_seq.nextval, 'Fork',5,   111, 0, 0);
insert INTO Products (ProductId, ProductName,SupplierID, QuantityPerUnit,UnitsInStock,UnitsOnOrder,Discontinued)
VALUES (Products_seq.nextval, 'Linq Book',2, 1, 0, 26, 0);

--####################################################################
insert INTO Employees (EmployeeID, LastName,FirstName,Title,BirthDate,HireDate,Address,City,ReportsTo)
VALUES (Employees_seq.nextval, 'Fuller','Andrew','Vice President, Sales','01 Jan 1964','01 Jan 1989', '908 W. Capital Way','Tacoma',NULL);

insert INTO Employees (EmployeeID, LastName,FirstName,Title,BirthDate,HireDate,Address,City,ReportsTo)
VALUES (Employees_seq.nextval, 'Davolio','Nancy','Sales Representative','01 Jan 1964','01 Jan 1994','507 - 20th Ave. E.  Apt. 2A','Seattle',1);

insert INTO Employees (EmployeeID, LastName,FirstName,Title,BirthDate,HireDate,Address,City,ReportsTo)
VALUES (Employees_seq.nextval, 'Builder','Bob','Handyman','01 Jan 1964','01 Jan 1964','666 dark street','Seattle',2);

--####################################################################
--truncate table Orders;
--
insert INTO Orders (OrderID, CustomerID, EmployeeID, OrderDate, Freight)
Values (Orders_seq.NextVal, 'AIRBU', 1, sysdate, 21.3);

insert INTO Orders (OrderID, CustomerID, EmployeeID, OrderDate, Freight)
Values (Orders_seq.NextVal, 'BT___', 1, sysdate, 11.1);

insert INTO Orders (OrderID, CustomerID, EmployeeID, OrderDate, Freight)
Values (Orders_seq.NextVal, 'BT___', 1, sysdate, 11.5);

insert INTO Orders (OrderID, CustomerID, EmployeeID, OrderDate, Freight)
Values (Orders_seq.NextVal, 'UKMOD', 1, sysdate, 32.5);


COMMIT;





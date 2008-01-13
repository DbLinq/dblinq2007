
CREATE TABLE `Region` (
  `RegionID` INTEGER NOT NULL ,
  `RegionDescription` VARCHAR(50) NOT NULL,
  PRIMARY KEY(`RegionID`)
)


CREATE TABLE `Territories` (
  `TerritoryID` VARCHAR(20) NOT NULL,
  `TerritoryDescription` VARCHAR(50) NOT NULL,
  `RegionID` INTEGER NOT NULL,
  PRIMARY KEY(`TerritoryID`)
)



####################################################################
CREATE TABLE `Categories` (
  `CategoryID` INTEGER  NOT NULL ,
  `CategoryName` VARCHAR(15) NOT NULL,
  `Description` TEXT NULL,
  `Picture` BLOB NULL,
  PRIMARY KEY(`CategoryID`)
)


CREATE TABLE `Suppliers` (
  `SupplierID` INTEGER  NOT NULL ,
  `CompanyName` VARCHAR(40) NOT NULL DEFAULT '',
  `ContactName` VARCHAR(30) NULL,
  `ContactTitle` VARCHAR(30) NULL,
  `Address` VARCHAR(60) NULL,
  `City` VARCHAR(15) NULL,
  `Region` VARCHAR(15) NULL,
  `PostalCode` VARCHAR(10) NULL,
  `Country` VARCHAR(15) NULL,
  `Phone` VARCHAR(24) NULL,
  `Fax` VARCHAR(24) NULL,
  PRIMARY KEY(`SupplierID`)
)


####################################################################
CREATE TABLE `Products` (
  `ProductID` INTEGER NOT NULL ,
  `ProductName` VARCHAR(40) NOT NULL DEFAULT '',
  `SupplierID` INTEGER NULL,
  `CategoryID` INTEGER NULL,
  `QuantityPerUnit` VARCHAR(20) NULL,
  `UnitPrice` DECIMAL NULL,
  `UnitsInStock` SMALLINT NULL,
  `UnitsOnOrder` SMALLINT NULL,
  `ReorderLevel` SMALLINT NULL,
  `Discontinued` BIT NOT NULL,
  PRIMARY KEY(`ProductID`)
)



####################################################################
CREATE TABLE `Customers` (
  `CustomerID` VARCHAR(5) NOT NULL,
  `CompanyName` VARCHAR(40) NOT NULL DEFAULT '',
  `ContactName` VARCHAR(30) NULL,
  `ContactTitle` VARCHAR(30) NULL,
  `Address` VARCHAR(60) NULL,
  `City` VARCHAR(15) NULL,
  `Region` VARCHAR(15) NULL,
  `PostalCode` VARCHAR(10) NULL,
  `Country` VARCHAR(15) NULL,
  `Phone` VARCHAR(24) NULL,
  `Fax` VARCHAR(24) NULL,
  PRIMARY KEY(`CustomerID`)
)


####################################################################
CREATE TABLE `Shippers` (
  `ShipperID` INTEGER NOT NULL ,
  `CompanyName` VARCHAR(40) NOT NULL,
  `Phone` VARCHAR(24) NULL,
  PRIMARY KEY(`ShipperID`)
)


####################################################################
CREATE TABLE `Employees` (
  `EmployeeID` INTEGER NOT NULL ,
  `LastName` VARCHAR(20) NOT NULL,
  `FirstName` VARCHAR(10) NOT NULL,
  `Title` VARCHAR(30) NULL,
  `BirthDate` DATETIME NULL,
  `HireDate` DATETIME NULL,
  `Address` VARCHAR(60) NULL,
  `City` VARCHAR(15) NULL,
  `Region` VARCHAR(15) NULL,
  `PostalCode` VARCHAR(10) NULL,
  `Country` VARCHAR(15) NULL,
  `HomePhone` VARCHAR(24) NULL,
  `Photo` BLOB NULL,
  `Notes` TEXT NULL,
  `ReportsTo` INTEGER NULL,
  PRIMARY KEY(`EmployeeID`)
)


####################################################################
CREATE TABLE `EmployeeTerritories` (
  `EmployeeID` INTEGER NOT NULL,
  `TerritoryID` VARCHAR(20) NOT NULL,
  PRIMARY KEY(`EmployeeID`,`TerritoryID`)
)



####################################################################
CREATE TABLE `Orders` (
  `OrderID` INTEGER NOT NULL ,
  `CustomerID` VARCHAR(5) NULL,
  `EmployeeID` INTEGER NULL,
  `OrderDate` DATETIME NULL,
  `RequiredDate` DATETIME NULL,
  `ShippedDate` DATETIME NULL,
  `ShipVia` INT NULL,
  `Freight` DECIMAL NULL,
  `ShipName` VARCHAR(40) NULL,
  `ShipAddress` VARCHAR(60) NULL,
  `ShipCity` VARCHAR(15) NULL,
  `ShipRegion` VARCHAR(15) NULL,
  `ShipPostalCode` VARCHAR(10) NULL,
  `ShipCountry` VARCHAR(15) NULL,
  PRIMARY KEY(`OrderID`)
)


ALTER TABLE `orders` ADD CONSTRAINT `FK_orders_1` FOREIGN KEY `FK_orders_1` (`CustomerID`)
    REFERENCES `Customers` (`CustomerID`)
    ON DELETE RESTRICT
    ON UPDATE RESTRICT;

ALTER TABLE `orders` ADD CONSTRAINT `FK_orders_emp` FOREIGN KEY `FK_orders_emp` (`EmployeeID`)
    REFERENCES `Employees` (`EmployeeID`)
    ON DELETE RESTRICT
    ON UPDATE RESTRICT;

####################################################################
CREATE TABLE `Order Details` (
  `OrderID` INTEGER NOT NULL                 REFERENCES Orders (OrderID),
  `ProductID` INTEGER NOT NULL               REFERENCES Products (OrderID),
  `UnitPrice` DECIMAL NOT NULL,
  `Quantity` SMALLINT NOT NULL,
  `Discount` FLOAT NOT NULL,
  PRIMARY KEY(`OrderID`,`ProductID`)
)



####################################################################
CREATE TABLE `AllTypes` (
  `int` INTEGER UNSIGNED NOT NULL ,
  `intN` INTEGER UNSIGNED,
  `double` DOUBLE NOT NULL DEFAULT 0,
  `doubleN` DOUBLE,
  `decimal` DECIMAL NOT NULL DEFAULT 0,
  `decimalN` DECIMAL,
  `blob` BLOB NOT NULL,
  `blobN` BLOB,
  `boolean` BOOLEAN NOT NULL DEFAULT 0,
  `boolN` BOOLEAN,
  `byte` TINYINT UNSIGNED NOT NULL DEFAULT 0,
  `byteN` TINYINT UNSIGNED,
  `DateTime` DATETIME NOT NULL DEFAULT 0,
  `DateTimeN` DATETIME,
  `float` FLOAT NOT NULL DEFAULT 0,
  `floatN` FLOAT,
  `char` CHAR NOT NULL DEFAULT '',
  `charN` CHAR,
  `text` TEXT NOT NULL,
  `textN` TEXT,
  `short` MEDIUMINT UNSIGNED NOT NULL DEFAULT 0,
  `shortN` MEDIUMINT UNSIGNED,
  `numeric` NUMERIC NOT NULL DEFAULT 0,
  `numericN` NUMERIC,
  `real` REAL NOT NULL DEFAULT 0,
  `realN` REAL,
  `smallInt` SMALLINT UNSIGNED NOT NULL DEFAULT 0,
  `smallIntN` SMALLINT UNSIGNED,
  `DbLinq_EnumTest` SMALLINT UNSIGNED NOT NULL,
  PRIMARY KEY(`int`)
)


####################################################################
## populate tables with seed data
####################################################################
truncate table `Categories`;
Insert INTO `Categories` (CategoryName,Description)
values ('Beverages',	'Soft drinks, coffees, teas, beers, and ales')
Insert INTO `Categories` (CategoryName,Description)
values     ('Condiments','Sweet and savory sauces, relishes, spreads, and seasonings')


INSERT INTO Region (RegionDescription) VALUES ('North America')
INSERT INTO Region (RegionDescription) VALUES ('Europe')

INSERT INTO Territories (TerritoryID,TerritoryDescription, RegionID) VALUES ('US.Northwest', 'Northwest', 1)

truncate table `Orders`; -- must be truncated before Customer
truncate table `Customers`;

insert INTO `Customers` (CustomerID, CompanyName,ContactName,Country,PostalCode,City)
values ('AIRBU', 'airbus','jacques','France','10000','Paris')
insert INTO `Customers` (CustomerID, CompanyName,ContactName,Country,PostalCode,City)
values ('BT___','BT','graeme','U.K.','E14','London')

insert INTO `Customers` (CustomerID, CompanyName,ContactName,Country,PostalCode,City)
values ('ATT__','ATT','bob','USA','10021','New York')
insert INTO `Customers` (CustomerID, CompanyName,ContactName,Country,PostalCode,City)
values ('UKMOD', 'MOD','(secret)','U.K.','E14','London')

insert INTO `Customers` (CustomerID, CompanyName,ContactName, ContactTitle, Country,PostalCode,City, Phone)
values ('ALFKI', 'Alfreds Futterkiste','Maria Anders','Sales Representative','Germany','12209','Berlin','030-0074321')

insert INTO `Customers` (CustomerID, CompanyName,ContactName, ContactTitle, Country,PostalCode,City, Phone)
values ('BONAP', 'Bon something','Bon Boss','Sales Representative','France','11109','Paris','033-0074321')

insert INTO `Customers` (CustomerID, CompanyName,ContactName, ContactTitle, Country,PostalCode,City, Phone)
values ('WARTH', 'Wartian Herkku','Pirkko Koskitalo','Accounting Manager','Finland','90110','Oulu','981-443655')

truncate table `Orders`; -- must be truncated before Products
truncate table `Products`;
truncate table `Suppliers`;

insert INTO Suppliers (CompanyName, ContactName, ContactTitle, Address, City, Region, Country)
VALUES ('alles AG', 'Harald Reitmeyer', 'Prof', 'Fischergasse 8', 'Heidelberg', 'B-W', 'Germany')

insert INTO Suppliers (CompanyName, ContactName, ContactTitle, Address, City, Region, Country)
VALUES ('Microsoft', 'Mr Allen', 'Monopolist', '1 MS', 'Redmond', 'WA', 'USA')

## (OLD WARNING: this actually inserts two 'Pen' rows into Products.)
## could someone with knowledge of MySQL resolve this?
## Answer: upgrade to newer version of MySql Query Browser - the problem will go away
insert INTO `Products` (ProductName,SupplierID, QuantityPerUnit,UnitsInStock,UnitsOnOrder,Discontinued)
VALUES ('Pen',1, 10,     12, 2,  0)
insert INTO `Products` (ProductName,SupplierID, QuantityPerUnit,UnitsInStock,UnitsOnOrder,Discontinued)
VALUES ('Bicycle',1, 1,  6, 0,  0)
insert INTO `Products` (ProductName,QuantityPerUnit,UnitsInStock,UnitsOnOrder,Discontinued)
VALUES ('Phone',3,    7, 0,  0)
insert INTO `Products` (ProductName,QuantityPerUnit,UnitsInStock,UnitsOnOrder,Discontinued)
VALUES ('SAM',1,      51, 11, 0)
insert INTO `Products` (ProductName,QuantityPerUnit,UnitsInStock,UnitsOnOrder,Discontinued)
VALUES ('iPod',0,     11, 0, 0)
insert INTO `Products` (ProductName,QuantityPerUnit,UnitsInStock,UnitsOnOrder,Discontinued)
VALUES ('Toilet Paper',2,  0, 3, 1)
insert INTO `Products` (ProductName,QuantityPerUnit,UnitsInStock,UnitsOnOrder,Discontinued)
VALUES ('Fork',5,   111, 0, 0)
insert INTO `Products` (ProductName,SupplierID, QuantityPerUnit,UnitsInStock,UnitsOnOrder,Discontinued)
VALUES ('Linq Book',2, 1, 0, 26, 0)

truncate table `Employees`;

insert INTO `Employees` (LastName,FirstName,Title,BirthDate,HireDate,Address,City,ReportsTo)
VALUES ('Fuller','Andrew','Vice President, Sales','19540101','19890101', '908 W. Capital Way','Tacoma',NULL)

insert INTO `Employees` (LastName,FirstName,Title,BirthDate,HireDate,Address,City,ReportsTo)
VALUES ('Davolio','Nancy','Sales Representative','19640101','19940101','507 - 20th Ave. E.  Apt. 2A','Seattle',1)

insert INTO `Employees` (LastName,FirstName,Title,BirthDate,HireDate,Address,City,ReportsTo)
VALUES ('Builder','Bob','Handyman','19640101','19940101','666 dark street','Seattle',2)

insert into employeeTerritories (EmployeeID,TerritoryID)
values (2,'US.Northwest')

####################################################################
truncate table `Orders`;
insert INTO `Orders` (CustomerID, EmployeeID, OrderDate, Freight)
Values ('AIRBU', 1, '2007-12-14', 21.3)

insert INTO `Orders` (CustomerID, EmployeeID, OrderDate, Freight)
Values ('BT___', 1, '2007-12-15', 11.1)

insert INTO `Orders` (CustomerID, EmployeeID, OrderDate, Freight)
Values ('BT___', 1, '2007-12-16', 11.5)

insert INTO `Orders` (CustomerID, EmployeeID, OrderDate, Freight)
Values ('UKMOD', 1, '2007-12-17', 32.5)

####################################################################
INSERT INTO alltypes (
               `intN` ,
  `double` ,   `doubleN` ,
  `decimal` ,  `decimalN` ,
  `blob` ,     `blobN` ,
  `boolean` ,  `boolN` ,
  `byte` ,     `byteN` ,
  `DateTime` , `DateTimeN` ,
  `float` ,    `floatN` ,
  `char` ,     `charN` ,
  `text` ,     `textN` ,
  `short` ,    `shortN` ,
  `numeric` ,  `numericN` ,
  `real` ,     `realN` ,
  `smallInt` , `smallIntN`,
  `DbLinq_EnumTest`
)
VALUES(         null,
  2,            null, 
  3,            null,
  'aa',         null, 
  1,            null,
  8,            null, 
  '2007-12-14', null,
  4,            null, 
  'c',          null,
  'text',       null, 
  127,          null,
  999.9,        null, 
  998.9,        null,
  16000,        null, 
  1)

####################################################################
## create stored procs
####################################################################
/* we also need some functions to test the -sprocs option **/
CREATE FUNCTION hello0() RETURNS char(20) RETURN 'hello0';
CREATE FUNCTION hello1(s CHAR(20)) RETURNS char(30) RETURN CONCAT('Hello, ',s,'!');
CREATE FUNCTION `hello2`(s CHAR(20),s2 int) RETURNS char(30) RETURN CONCAT('Hello, ',s,'!');

DELIMITER $$

DROP FUNCTION IF EXISTS `getOrderCount` $$
CREATE FUNCTION `getOrderCount`(custId VARCHAR(5)) RETURNS INT
BEGIN
DECLARE count1 int;
SELECT COUNT(*) INTO count1 FROM Orders WHERE CustomerID=custId;
RETURN count1;
END $$

DROP PROCEDURE IF EXISTS `sp_selOrders` $$
CREATE PROCEDURE `sp_selOrders`(s CHAR(20), OUT s2 int)
BEGIN
set s2 = 22;
select * from orders;
END $$

DROP PROCEDURE IF EXISTS `sp_updOrders` $$
CREATE PROCEDURE `sp_updOrders`(custID int, prodId int)
BEGIN
UPDATE Orders
SET OrderDate=Now()
WHERE ProductId=prodId AND CustomerID=custId;
END $$

DELIMITER ;
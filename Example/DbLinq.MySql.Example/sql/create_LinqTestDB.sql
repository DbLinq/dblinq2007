DROP DATABASE IF EXISTS `linqtestdb`;

CREATE DATABASE `linqtestdb`;

/*DROP USER IF EXISTS 'LinqUser'@'%'; */
/*DELETE FROM `mysql`.`user` WHERE `User`='LinqUser';*/
/*DROP USER 'LinqUser'@'%';*/


/* create user LinqUser, password: 'linq2' 
CREATE USER 'LinqUser'@'%'
  IDENTIFIED BY PASSWORD '*247E8BFCE2F07F00D7FD773390A282540001077B';

/* give our new user full permissions on new database: 
GRANT ALL ON linqtestdb.*  TO 'LinqUser'@'%';
FLUSH PRIVILEGES;
*/


####################################################################
## create tables
####################################################################
CREATE TABLE `linqtestdb`.`Categories` (
  `CategoryID` INTEGER  NOT NULL AUTO_INCREMENT,
  `CategoryName` VARCHAR(15) NOT NULL,
  `Description` TEXT NULL,
  `Picture` BLOB NULL,
  PRIMARY KEY(`CategoryID`)
)
ENGINE = InnoDB;

####################################################################
CREATE TABLE `linqtestdb`.`Products` (
  `ProductID` INTEGER UNSIGNED NOT NULL AUTO_INCREMENT,
  `ProductName` VARCHAR(40) NOT NULL DEFAULT '',
  `SupplierID` INTEGER UNSIGNED NULL,
  `CategoryID` INTEGER NULL,
  `QuantityPerUnit` VARCHAR(20) NULL,
  `UnitPrice` DECIMAL NULL,
  `UnitsInStock` SMALLINT NULL,
  `UnitsOnOrder` SMALLINT NULL,
  `ReorderLevel` SMALLINT NULL,
  `Discontinued` BIT NULL,
  PRIMARY KEY(`ProductID`)
)
ENGINE = InnoDB
COMMENT = 'Holds Products';

ALTER TABLE `linqtestdb`.`Products` ADD CONSTRAINT `FK_prod_catg` FOREIGN KEY `FK_prod_catg` (`CategoryID`)
    REFERENCES `Categories` (`CategoryID`)
    ON DELETE RESTRICT
    ON UPDATE RESTRICT;


####################################################################
CREATE TABLE `linqtestdb`.`Customers` (
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
ENGINE = InnoDB;

####################################################################
CREATE TABLE `linqtestdb`.`Employees` (
  `EmployeeID` INTEGER UNSIGNED NOT NULL AUTO_INCREMENT,
  `LastName` VARCHAR(20) NOT NULL,
  `FirstName` VARCHAR(10) NOT NULL,
  `Title` VARCHAR(30) NULL,
  `HireDate` DATETIME NULL,
  `HomePhone` VARCHAR(24) NULL,
  `ReportsTo` INTEGER UNSIGNED NULL,
  PRIMARY KEY(`EmployeeID`)
)
ENGINE = InnoDB;

ALTER TABLE `linqtestdb`.`Employees` ADD CONSTRAINT `FK_Emp_ReportsToEmp` FOREIGN KEY `FK_Emp_ReportsToEmp` (`ReportsTo`)
    REFERENCES `Employees` (`EmployeeID`)
    ON DELETE RESTRICT
    ON UPDATE RESTRICT;


####################################################################
CREATE TABLE `linqtestdb`.`Orders` (
  `OrderID` INTEGER NOT NULL AUTO_INCREMENT,
  `CustomerID` VARCHAR(5) NULL,
  `EmployeeID` INTEGER UNSIGNED NULL,
  `OrderDate` DATETIME NULL,
  `Freight` DECIMAL NULL,
  `ShipName` VARCHAR(40) NULL,
  `ShipAddress` VARCHAR(60) NULL,
  `ShipCity` VARCHAR(15) NULL,
  `ShipRegion` VARCHAR(15) NULL,
  `ShipPostalCode` VARCHAR(15) NULL,
  PRIMARY KEY(`OrderID`)
)
ENGINE = InnoDB;

ALTER TABLE `linqtestdb`.`orders` ADD CONSTRAINT `FK_orders_1` FOREIGN KEY `FK_orders_1` (`CustomerID`)
    REFERENCES `Customers` (`CustomerID`)
    ON DELETE RESTRICT
    ON UPDATE RESTRICT;

ALTER TABLE `linqtestdb`.`orders` ADD CONSTRAINT `FK_orders_emp` FOREIGN KEY `FK_orders_emp` (`EmployeeID`)
    REFERENCES `Employees` (`EmployeeID`)
    ON DELETE RESTRICT
    ON UPDATE RESTRICT;

####################################################################
CREATE TABLE `linqtestdb`.`Order Details` (
  `OrderID` INTEGER NOT NULL,
  `ProductID` INTEGER UNSIGNED NOT NULL,
  `UnitPrice` DECIMAL NOT NULL,
  `Quantity` SMALLINT NOT NULL,
  `Discount` FLOAT NOT NULL,
  PRIMARY KEY(`OrderID`,`ProductID`)
)
ENGINE = InnoDB;

ALTER TABLE `linqtestdb`.`Order Details` ADD CONSTRAINT `FK_ordersDetails_Ord` FOREIGN KEY `FK_ordersDetails_Ord` (`OrderID`)
    REFERENCES `Orders` (`OrderID`)
    ON DELETE RESTRICT
    ON UPDATE RESTRICT;

ALTER TABLE `linqtestdb`.`Order Details` ADD CONSTRAINT `FK_ordersDetails_Prod` FOREIGN KEY `FK_ordersDetails_Prod` (`ProductID`)
    REFERENCES `products` (`ProductID`)
    ON DELETE RESTRICT
    ON UPDATE RESTRICT;

####################################################################
CREATE TABLE `linqtestdb`.`Region` (
  `RegionID` INTEGER NOT NULL AUTO_INCREMENT,
  `RegionDescription` VARCHAR(50) NOT NULL,
  PRIMARY KEY(`RegionID`)
)
ENGINE = InnoDB;

CREATE TABLE `linqtestdb`.`Territories` (
  `TerritoryID` VARCHAR(20) NOT NULL,
  `TerritoryDescription` VARCHAR(50) NOT NULL,
  `RegionID` INTEGER NOT NULL,
  PRIMARY KEY(`TerritoryID`)
)
ENGINE = InnoDB;

ALTER TABLE `linqtestdb`.`Territories` ADD CONSTRAINT `FK_Terr_Region` FOREIGN KEY `FK_Terr_Region` (`RegionID`)
    REFERENCES `Region` (`RegionID`)
    ON DELETE RESTRICT
    ON UPDATE RESTRICT;

####################################################################
CREATE TABLE `linqtestdb`.`Shippers` (
  `ShipperID` INTEGER NOT NULL AUTO_INCREMENT,
  `CompanyName` VARCHAR(40) NOT NULL,
  `Phone` VARCHAR(24) NULL,
  PRIMARY KEY(`ShipperID`)
)
ENGINE = InnoDB;

####################################################################
CREATE TABLE `linqtestdb`.`AllTypes` (
  `int` INTEGER UNSIGNED NOT NULL AUTO_INCREMENT,
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
ENGINE = InnoDB
COMMENT = 'Tests mapping of many MySQL types to CSharp types';

USE linqtestdb;

####################################################################
## populate tables with seed data
####################################################################
truncate table `linqtestdb`.`Orders`; -- must be truncated before Customer
truncate table `linqtestdb`.`Customers`;

insert `linqtestdb`.`Customers` (CustomerID, CompanyName,ContactName,Country,PostalCode,City)
values ('AIRBU', 'airbus','jacques','France','10000','Paris');
insert `linqtestdb`.`Customers` (CustomerID, CompanyName,ContactName,Country,PostalCode,City)
values ('BT___','BT','graeme','U.K.','E14','London');

insert `linqtestdb`.`Customers` (CustomerID, CompanyName,ContactName,Country,PostalCode,City)
values ('ATT__','ATT','bob','USA','10021','New York');
insert `linqtestdb`.`Customers` (CustomerID, CompanyName,ContactName,Country,PostalCode,City)
values ('UKMOD', 'MOD','(secret)','U.K.','E14','London');

insert `linqtestdb`.`Customers` (CustomerID, CompanyName,ContactName, ContactTitle, Country,PostalCode,City, Phone)
values ('ALFKI', 'Alfreds Futterkiste','Maria Anders','Sales Representative','Germany','12209','Berlin','030-0074321');

insert `linqtestdb`.`Customers` (CustomerID, CompanyName,ContactName, ContactTitle, Country,PostalCode,City, Phone)
values ('WARTH', 'Wartian Herkku','Pirkko Koskitalo','Accounting Manager','Finland','90110','Oulu','981-443655');

truncate table `linqtestdb`.`Orders`; -- must be truncated before Products
truncate table `linqtestdb`.`Products`;
## WARNING: this actually inserts two 'Pen' rows into Products.
## could someone with knowledge of MySQL resolve this?
## Answer: upgrade to newer version of MySql Query Browser - the problem will go away
insert `linqtestdb`.`Products` (ProductName,QuantityPerUnit) VALUES ('Pen',10);
insert `linqtestdb`.`Products` (ProductName,QuantityPerUnit) VALUES ('Bicycle',1);
insert `linqtestdb`.`Products` (ProductName,QuantityPerUnit) VALUES ('Phone',3);
insert `linqtestdb`.`Products` (ProductName,QuantityPerUnit) VALUES ('SAM',1);
insert `linqtestdb`.`Products` (ProductName,QuantityPerUnit) VALUES ('iPod',0);
insert `linqtestdb`.`Products` (ProductName,QuantityPerUnit) VALUES ('Toilet Paper',2);
insert `linqtestdb`.`Products` (ProductName,QuantityPerUnit) VALUES ('Fork',5);

truncate table `linqtestdb`.`Employees`;
insert `linqtestdb`.`Employees` (LastName,FirstName,Title) VALUES ('Davolio','Nancy','Sales Representative');

####################################################################
truncate table `linqtestdb`.`Orders`;
insert `linqtestdb`.`Orders` (CustomerID, EmployeeID, OrderDate)
Values (
  (Select CustomerID from Customers Where CompanyName='airbus')
, 1, now());

insert `linqtestdb`.`Orders` (CustomerID, EmployeeID, OrderDate)
Values (
  (Select CustomerID from Customers Where CompanyName='BT')
, 1, now());

insert `linqtestdb`.`Orders` (CustomerID, EmployeeID, OrderDate)
Values (
  (Select CustomerID from Customers Where CompanyName='BT')
, 1, now());

insert `linqtestdb`.`Orders` (CustomerID, EmployeeID, OrderDate)
Values (
  (Select CustomerID from Customers Where CompanyName='MOD')
, 1, now());

####################################################################
INSERT INTO linqtestdb.alltypes (
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
  2,            null, /*double*/
  3,            null,
  'aa',         null, /*blob*/
  true,         null,
  8,            null, /*byte*/
  now(),        null,
  4,            null, /*float*/
  'c',          null,
  'text',       null, /*text*/
  127,          null,
  999.9,        null, /*numeric*/
  998.9,        null,
  16000,        null, /*smallInt*/
  1);

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
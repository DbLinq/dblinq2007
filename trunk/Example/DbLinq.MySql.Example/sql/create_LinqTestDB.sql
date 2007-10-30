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
CREATE TABLE `linqtestdb`.`Products` (
  `ProductID` INTEGER UNSIGNED NOT NULL AUTO_INCREMENT,
  `ProductName` VARCHAR(45) NOT NULL DEFAULT '',
  `SupplierID` INTEGER UNSIGNED NOT NULL DEFAULT 0,
  `CategoryID` INTEGER UNSIGNED NOT NULL DEFAULT 0,
  `QuantityPerUnit` VARCHAR(20) NOT NULL DEFAULT '',
  `UnitPrice` DECIMAL NULL,
  `UnitsInStock` SMALLINT NULL,
  `UnitsOnOrder` SMALLINT NULL,
  `ReorderLevel` SMALLINT NULL,
  `Discontinued` BIT NULL,
  PRIMARY KEY(`ProductID`)
)
ENGINE = InnoDB
COMMENT = 'Holds Products';

CREATE TABLE `linqtestdb`.`Customers` (
  `CustomerID` INTEGER UNSIGNED NOT NULL AUTO_INCREMENT,
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
CREATE TABLE `linqtestdb`.`Orders` (
  `OrderID` INTEGER UNSIGNED NOT NULL AUTO_INCREMENT,
  `CustomerID` INTEGER UNSIGNED NOT NULL DEFAULT 0,
  `ProductID` INTEGER UNSIGNED NOT NULL DEFAULT 0,
  `OrderDate` DATETIME NOT NULL DEFAULT 0,
  PRIMARY KEY(`OrderID`)
)
ENGINE = InnoDB;

ALTER TABLE `linqtestdb`.`orders` ADD CONSTRAINT `FK_orders_1` FOREIGN KEY `FK_orders_1` (`CustomerID`)
    REFERENCES `Customers` (`CustomerID`)
    ON DELETE RESTRICT
    ON UPDATE RESTRICT;

ALTER TABLE `linqtestdb`.`orders` ADD CONSTRAINT `FK_orders_prod` FOREIGN KEY `FK_orders_prod` (`ProductID`)
    REFERENCES `products` (`ProductID`)
    ON DELETE RESTRICT
    ON UPDATE RESTRICT;

####################################################################
CREATE TABLE `linqtestdb`.`Order Details` (
  `OrderID` INTEGER NOT NULL,
  `ProductID` INTEGER NOT NULL,
  `UnitPrice` DECIMAL NOT NULL,
  `Quantity` SMALLINT NOT NULL,
  `Discount` FLOAT NOT NULL,
  PRIMARY KEY(`OrderID`,`ProductID`)
)
ENGINE = InnoDB;

/*
## Script line: 81	Can't create table 'linqtestdb.#sql-fc8_1' (errno: 150)
ALTER TABLE `linqtestdb`.`Order Details` ADD CONSTRAINT `FK_ordersDetails_Ord` FOREIGN KEY `FK_ordersDetails_Ord` (`OrderID`)
    REFERENCES `Orders` (`OrderID`)
    ON DELETE RESTRICT
    ON UPDATE RESTRICT;

ALTER TABLE `linqtestdb`.`Order Details` ADD CONSTRAINT `FK_ordersDetails_Prod` FOREIGN KEY `FK_ordersDetails_Prod` (`ProductID`)
    REFERENCES `products` (`ProductID`)
    ON DELETE RESTRICT
    ON UPDATE RESTRICT;
*/

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
truncate table Orders; -- must be truncated before Customer
truncate table Customers;

insert Customers (CompanyName,ContactName,Country,PostalCode,City)
values ('airbus','jacques','France','10000','Paris');
insert Customers (CompanyName,ContactName,Country,PostalCode,City)
values ('BT','graeme','U.K.','E14','London');

insert Customers (CompanyName,ContactName,Country,PostalCode,City)
values ('ATT','bob','USA','10021','New York');
insert Customers (CompanyName,ContactName,Country,PostalCode,City)
values ('MOD','(secret)','U.K.','E14','London');

truncate table Orders; -- must be truncated before Products
truncate table Products;
/** WARNING: this actually inserts two 'Pen' rows into Products.
 could someone with knowledge of MySQL resolve this? */
insert Products (ProductName,QuantityPerUnit) VALUES ('Pen',10);
insert Products (ProductName,QuantityPerUnit) VALUES ('Bicycle',1);
insert Products (ProductName,QuantityPerUnit) VALUES ('Phone',3);
insert Products (ProductName,QuantityPerUnit) VALUES ('SAM',1);
insert Products (ProductName,QuantityPerUnit) VALUES ('iPod',0);
insert Products (ProductName,QuantityPerUnit) VALUES ('Toilet Paper',2);
insert Products (ProductName,QuantityPerUnit) VALUES ('Fork',5);

####################################################################
truncate table Orders;
insert Orders (CustomerID, ProductID, OrderDate)
Values (
  (Select CustomerID from Customers Where CompanyName='airbus')
, (Select ProductID from Products Where ProductName='Pen')
, now());

insert Orders (CustomerID, ProductID, OrderDate)
Values (
  (Select CustomerID from Customers Where CompanyName='BT')
, (Select ProductID from Products Where ProductName='Phone')
, now());

insert Orders (CustomerID, ProductID, OrderDate)
Values (
  (Select CustomerID from Customers Where CompanyName='BT')
, (Select ProductID from Products Where ProductName='Pen')
, now());

insert Orders (CustomerID, ProductID, OrderDate)
Values (
  (Select CustomerID from Customers Where CompanyName='MOD')
, (Select ProductID from Products Where ProductName='SAM')
, now());

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
/* we also need some functions to test the -sprocs option **/
CREATE FUNCTION hello0() RETURNS char(20) RETURN 'hello0';
CREATE FUNCTION hello1(s CHAR(20)) RETURNS char(30) RETURN CONCAT('Hello, ',s,'!');
CREATE FUNCTION `hello2`(s CHAR(20),s2 int) RETURNS char(30) RETURN CONCAT('Hello, ',s,'!');

DELIMITER $$

DROP FUNCTION IF EXISTS `getOrderCount` $$
CREATE FUNCTION `getOrderCount`(custId INT UNSIGNED) RETURNS INT
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
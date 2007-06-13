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


CREATE TABLE `linqtestdb`.`Products` (
  `ProductID` INTEGER UNSIGNED NOT NULL AUTO_INCREMENT,
  `ProductName` VARCHAR(45) NOT NULL DEFAULT '',
  `SupplierID` INTEGER UNSIGNED NOT NULL DEFAULT 0,
  `CategoryID` INTEGER UNSIGNED NOT NULL DEFAULT 0,
  `QuantityPerUnit` VARCHAR(20) NOT NULL DEFAULT '',
  PRIMARY KEY(`ProductID`)
)
ENGINE = InnoDB
COMMENT = 'Holds Products';

CREATE TABLE `linqtestdb`.`Customer` (
  `CustomerID` INTEGER UNSIGNED NOT NULL AUTO_INCREMENT,
  `CompanyName` VARCHAR(45) NOT NULL DEFAULT '',
  `ContactName` VARCHAR(45) NOT NULL DEFAULT '',
  `City` VARCHAR(45) NOT NULL DEFAULT '',
  `PostalCode` VARCHAR(20) NOT NULL DEFAULT '',
  `Country` VARCHAR(45) NOT NULL DEFAULT '',
  `Phone` VARCHAR(45) NULL DEFAULT '',
  PRIMARY KEY(`CustomerID`)
)
ENGINE = InnoDB;

CREATE TABLE `linqtestdb`.`Orders` (
  `OrderID` INTEGER UNSIGNED NOT NULL AUTO_INCREMENT,
  `CustomerID` INTEGER UNSIGNED NOT NULL DEFAULT 0,
  `ProductID` INTEGER UNSIGNED NOT NULL DEFAULT 0,
  `OrderDate` DATETIME NOT NULL DEFAULT 0,
  PRIMARY KEY(`OrderID`)
)
ENGINE = InnoDB;

ALTER TABLE `linqtestdb`.`orders` ADD CONSTRAINT `FK_orders_1` FOREIGN KEY `FK_orders_1` (`CustomerID`)
    REFERENCES `customer` (`CustomerID`)
    ON DELETE RESTRICT
    ON UPDATE RESTRICT;

ALTER TABLE `linqtestdb`.`orders` ADD CONSTRAINT `FK_orders_prod` FOREIGN KEY `FK_orders_prod` (`ProductID`)
    REFERENCES `products` (`ProductID`)
    ON DELETE RESTRICT
    ON UPDATE RESTRICT;

CREATE TABLE `linqtestdb`.`AllTypes` (
  `int` INTEGER UNSIGNED NOT NULL AUTO_INCREMENT,
  `intN` INTEGER UNSIGNED,
  `double` DOUBLE NOT NULL DEFAULT 0,
  `doubleN` DOUBLE,
  `decimal` DECIMAL NOT NULL DEFAULT 0,
  `decimalN` DECIMAL,
  `blob` BLOB NOT NULL DEFAULT '',
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
  `text` TEXT NOT NULL DEFAULT '',
  `textN` TEXT,
  `short` MEDIUMINT UNSIGNED NOT NULL DEFAULT 0,
  `shortN` MEDIUMINT UNSIGNED,
  `numeric` NUMERIC NOT NULL DEFAULT 0,
  `numericN` NUMERIC,
  `real` REAL NOT NULL DEFAULT 0,
  `realN` REAL,
  `smallInt` SMALLINT UNSIGNED NOT NULL DEFAULT 0,
  `smallIntN` SMALLINT UNSIGNED,
  PRIMARY KEY(`int`)
)
ENGINE = InnoDB
COMMENT = 'Tests mapping of many MySQL types to CSharp types';

USE linqtestdb;

truncate table Orders; -- must be truncated before Customer
truncate table Customer;

insert Customer (CompanyName,ContactName,Country,PostalCode,City)
values ('airbus','jacques','France','10000','Paris');
insert Customer (CompanyName,ContactName,Country,PostalCode,City)
values ('BT','graeme','U.K.','E14','London');

insert Customer (CompanyName,ContactName,Country,PostalCode,City)
values ('ATT','bob','USA','10021','New York');
insert Customer (CompanyName,ContactName,Country,PostalCode,City)
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

truncate table Orders;
insert Orders (CustomerID, ProductID, OrderDate)
Values (
  (Select CustomerID from Customer Where CompanyName='airbus')
, (Select ProductID from Products Where ProductName='Pen')
, now());

insert Orders (CustomerID, ProductID, OrderDate)
Values (
  (Select CustomerID from Customer Where CompanyName='BT')
, (Select ProductID from Products Where ProductName='Phone')
, now());

insert Orders (CustomerID, ProductID, OrderDate)
Values (
  (Select CustomerID from Customer Where CompanyName='BT')
, (Select ProductID from Products Where ProductName='Pen')
, now());

insert Orders (CustomerID, ProductID, OrderDate)
Values (
  (Select CustomerID from Customer Where CompanyName='MOD')
, (Select ProductID from Products Where ProductName='SAM')
, now());

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
  `smallInt` , `smallIntN`
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
  16000,        null);


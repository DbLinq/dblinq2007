--CREATE DATABASE LinqTestDB;
--USE LinqTestDB;

CREATE TABLE Products (
  ProductID SERIAL NOT NULL,
  ProductName VARCHAR(45) NOT NULL,
  SupplierID INTEGER NOT NULL,
  CategoryID INTEGER NOT NULL,
  QuantityPerUnit VARCHAR(20),
  PRIMARY KEY(ProductID)
  );
  
CREATE TABLE Customer (
  CustomerID SERIAL NOT NULL,
  CompanyName VARCHAR(45) NOT NULL,
  ContactName VARCHAR(45) NOT NULL ,
  City VARCHAR(45) NOT NULL ,
  PostalCode VARCHAR(20) NOT NULL,
  Country VARCHAR(45) NOT NULL,
  Phone VARCHAR(45) NULL,
  PRIMARY KEY(CustomerID)
);  
  
CREATE TABLE Orders (
  OrderID SERIAL NOT NULL,
  CustomerID INTEGER NOT NULL,
  ProductID INTEGER NOT NULL,
  OrderDate TIMESTAMP NOT NULL,
  PRIMARY KEY(OrderID),
  CONSTRAINT fk_order_customer FOREIGN KEY (CustomerID) REFERENCES Customer(CustomerID),
  CONSTRAINT fk_order_product FOREIGN KEY (ProductID) REFERENCES Products(ProductID)
);

--CREATE SEQUENCE Products_seq START WITH  10   INCREMENT BY 1;
--CREATE SEQUENCE Customer_seq START WITH  100  INCREMENT BY 1;
--CREATE SEQUENCE Orders_seq   START WITH  1000 INCREMENT BY 1;
 

 
truncate table Orders; -- must be truncated before Customer
truncate table Customer CASCADE;

insert INTO Customer (CompanyName,ContactName,Country,PostalCode,City)
values ('airbus','jacques','France','10000','Paris');
insert INTO Customer (CompanyName,ContactName,Country,PostalCode,City)
values ('BT','graeme','U.K.','E14','London');

insert INTO Customer (CompanyName,ContactName,Country,PostalCode,City)
values ('ATT','bob','USA','10021','New York');
insert INTO Customer (CompanyName,ContactName,Country,PostalCode,City)
values ('MOD','(secret)','U.K.','E14','London');

truncate table Orders; -- must be truncated before Products
truncate table Products CASCADE;

insert INTO Products ( ProductName,SupplierID,CategoryID,QuantityPerUnit) VALUES ('Pen',0,0,10);
insert INTO Products ( ProductName,SupplierID,CategoryID,QuantityPerUnit) VALUES ('Bicycle',0,0,1);
insert INTO Products ( ProductName,SupplierID,CategoryID,QuantityPerUnit) VALUES ('Phone',0,0,3);
insert INTO Products ( ProductName,SupplierID,CategoryID,QuantityPerUnit) VALUES ('SAM',0,0,1);
insert INTO Products ( ProductName,SupplierID,CategoryID,QuantityPerUnit) VALUES ('iPod',0,0,0);
insert INTO Products ( ProductName,SupplierID,CategoryID,QuantityPerUnit) VALUES ('Toilet Paper',0,0,2);
insert INTO Products ( ProductName,SupplierID,CategoryID,QuantityPerUnit) VALUES ('Fork',0,0,5);
insert INTO Products ( ProductName,SupplierID,CategoryID,QuantityPerUnit) VALUES ('Spoon',0,0,5);

truncate table Orders;
insert INTO Orders ( CustomerID, ProductID, OrderDate)
Values ( (Select CustomerID from Customer Where CompanyName='airbus')
, (Select ProductID from Products Where ProductName='Pen')
, now());

insert INTO Orders ( CustomerID, ProductID, OrderDate)
Values ( (Select CustomerID from Customer Where CompanyName='BT')
, (Select ProductID from Products Where ProductName='Phone')
, now());

insert INTO Orders ( CustomerID, ProductID, OrderDate)
Values ( (Select CustomerID from Customer Where CompanyName='BT')
, (Select ProductID from Products Where ProductName='Pen')
, now());

insert INTO Orders ( CustomerID, ProductID, OrderDate)
Values ( (Select CustomerID from Customer Where CompanyName='MOD')
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
CREATE OR REPLACE FUNCTION getOrderCount(custId INT) RETURNS INT AS $$
DECLARE
  count1 INTEGER;
BEGIN
  SELECT COUNT(*) INTO count1 FROM Orders WHERE CustomerID=custId;
  RETURN count1;
END;
$$ LANGUAGE plpgsql;

COMMIT;





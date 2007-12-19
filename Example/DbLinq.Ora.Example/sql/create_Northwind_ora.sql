CREATE TABLE Products (
  ProductID INTEGER NOT NULL,
  ProductName VARCHAR(40) NOT NULL,
  SupplierID INTEGER NOT NULL,
  CategoryID INTEGER NOT NULL,
  QuantityPerUnit VARCHAR(20),
  UnitPrice DECIMAL NULL,
  UnitsInStock SMALLINT NULL,
  UnitsOnOrder SMALLINT NULL,
  ReorderLevel SMALLINT NULL,
  Discontinued NUMBER(1) NOT NULL, --'bool' field
  PRIMARY KEY(ProductID)
  );
  
CREATE TABLE Customer (
  CustomerID INTEGER NOT NULL,
  CompanyName VARCHAR(45) NOT NULL,
  ContactName VARCHAR(45) NOT NULL ,
  City VARCHAR(45) NOT NULL ,
  PostalCode VARCHAR(20) NOT NULL,
  Country VARCHAR(45) NOT NULL,
  Phone VARCHAR(45) NULL,
  PRIMARY KEY(CustomerID)
);  
  
CREATE TABLE Orders (
  OrderID INTEGER NOT NULL,
  CustomerID INTEGER NOT NULL,
  ProductID INTEGER NOT NULL,
  OrderDate TIMESTAMP NOT NULL,
  PRIMARY KEY(OrderID),
  CONSTRAINT fk_order_customer FOREIGN KEY (CustomerID) REFERENCES Customer(CustomerID),
  CONSTRAINT fk_order_product FOREIGN KEY (ProductID) REFERENCES Products(ProductID)
);

CREATE SEQUENCE Products_seq START WITH  10   INCREMENT BY 1;
CREATE SEQUENCE Customer_seq START WITH  100  INCREMENT BY 1;
CREATE SEQUENCE Orders_seq   START WITH  1000 INCREMENT BY 1;
 

 
-- Orders must be truncated before Customer
--truncate table Orders; 
--truncate table Customer;

insert INTO Customer (CustomerID,CompanyName,ContactName,Country,PostalCode,City)
values (customer_seq.nextval,'airbus','jacques','France','10000','Paris');
insert INTO Customer (CustomerID,CompanyName,ContactName,Country,PostalCode,City)
values (customer_seq.nextval,'BT','graeme','U.K.','E14','London');

insert INTO Customer (CustomerID,CompanyName,ContactName,Country,PostalCode,City)
values (customer_seq.nextval,'ATT','bob','USA','10021','New York');
insert INTO Customer (CustomerID,CompanyName,ContactName,Country,PostalCode,City)
values (customer_seq.nextval,'MOD','(secret)','U.K.','E14','London');

-- Orders must be truncated before Products
--truncate table Orders; 
--truncate table Products;

insert INTO Products (ProductId, ProductName,SupplierID,CategoryID,QuantityPerUnit,Discontinued) 
VALUES (Products_seq.nextval,'Pen',0,0,10,0);
insert INTO Products (ProductId, ProductName,SupplierID,CategoryID,QuantityPerUnit,Discontinued) 
VALUES (Products_seq.nextval,'Bicycle',0,0,1,0);
insert INTO Products (ProductId, ProductName,SupplierID,CategoryID,QuantityPerUnit,Discontinued) 
VALUES (Products_seq.nextval,'Phone',0,0,3,1);
insert INTO Products (ProductId, ProductName,SupplierID,CategoryID,QuantityPerUnit,Discontinued)
VALUES (Products_seq.nextval,'SAM',0,0,1,0);

truncate table Orders;
insert INTO Orders (OrderID, CustomerID, ProductID, OrderDate)
Values ( orders_seq.nextval
, (Select CustomerID from Customer Where CompanyName='airbus')
, (Select ProductID from Products Where ProductName='Pen')
, sysdate);

insert INTO Orders (OrderID, CustomerID, ProductID, OrderDate)
Values ( orders_seq.nextval
, (Select CustomerID from Customer Where CompanyName='BT')
, (Select ProductID from Products Where ProductName='Phone')
, sysdate);

insert INTO Orders (OrderID, CustomerID, ProductID, OrderDate)
Values ( orders_seq.nextval
, (Select CustomerID from Customer Where CompanyName='BT')
, (Select ProductID from Products Where ProductName='Pen')
, sysdate);

insert INTO Orders (OrderID, CustomerID, ProductID, OrderDate)
Values ( orders_seq.nextval
, (Select CustomerID from Customer Where CompanyName='MOD')
, (Select ProductID from Products Where ProductName='SAM')
, sysdate);

COMMIT;





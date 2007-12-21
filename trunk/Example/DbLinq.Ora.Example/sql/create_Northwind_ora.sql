CREATE TABLE Products (
  ProductID INTEGER NOT NULL,
  ProductName VARCHAR(40) NOT NULL,
  SupplierID INTEGER NULL,
  CategoryID INTEGER NULL,
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

CREATE SEQUENCE Products_seq START WITH  1    INCREMENT BY 1;
CREATE SEQUENCE Customer_seq START WITH  100  INCREMENT BY 1;
CREATE SEQUENCE Orders_seq   START WITH  1000 INCREMENT BY 1;
CREATE SEQUENCE Employees_seq START WITH 1    INCREMENT BY 1;
 

 
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

insert INTO Employees (EmployeeID, LastName,FirstName,Title,BirthDate,HireDate,Address,City,ReportsTo)
VALUES (Employees_seq.nextval, 'Fuller','Andrew','Vice President, Sales','01 Jan 1964','01 Jan 1989', '908 W. Capital Way','Tacoma',NULL);

insert INTO Employees (EmployeeID, LastName,FirstName,Title,BirthDate,HireDate,Address,City,ReportsTo)
VALUES (Employees_seq.nextval, 'Davolio','Nancy','Sales Representative','01 Jan 1964','01 Jan 1994','507 - 20th Ave. E.  Apt. 2A','Seattle',1);

insert INTO Employees (EmployeeID, LastName,FirstName,Title,BirthDate,HireDate,Address,City,ReportsTo)
VALUES (Employees_seq.nextval, 'Builder','Bob','Handyman','01 Jan 1964','01 Jan 1964','666 dark street','Seattle',2);


COMMIT;





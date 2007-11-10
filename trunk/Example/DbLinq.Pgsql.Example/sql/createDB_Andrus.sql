--############################################################
-- this database contains pathological cases discovered by Andrus (Tallinn, Estonia)

--####################################################################
--## create tables
--####################################################################

--# problem 1. foreign key relation generates duplicate propery if property exists.
--# problem 2. Column name 'private' causes error
CREATE TABLE t1 ( private int primary key);
CREATE TABLE t2 ( f1 int references t1,
  f2 int references t1 );

CREATE DOMAIN ebool AS bool DEFAULT false NOT NULL;

--# problem 3. GetHashCode() does not check null ids for reference types
--# problem 4. pgsql domain types are not recognized in mappings ('ebool' above)
CREATE TABLE t3 ( t3ID varchar(5) primary key, my_ebool ebool);

/*
CREATE TABLE t4 
ALTER TABLE t4 ALTER kasutaja SET NOT NULL,
ALTER t4 SET NOT NULL,
ADD constraint t4_kasutaja_firmanr_unique
UNIQUE (t4,kasutaja,firmanr);

--this causes exception in sqlmetal:
--Missing data from 'constraint_column_usage' for foreign key
--kasgrupp_kasutaja_firmanr_unique
*/

COMMIT;





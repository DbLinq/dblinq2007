CREATE USER 'LinqUser'@'%';
  SET PASSWORD FOR 'LinqUser'@'%' = PASSWORD('linq2');

--
GRANT Select, Insert, Update, Delete ON `LinqTestDB`.* TO 'LinqUser'@'%';
  FLUSH PRIVILEGES;

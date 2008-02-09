REM: note that the '-sprocs' option is turned on
REM MySqlMetal.exe -database:Northwind -server:localhost -user:LinqUser -password:linq2 -namespace:nwind -dbml:nwind_mysql.dbml -sprocs

MySqlMetal.exe -database:Northwind -server:localhost -user:LinqUser -password:linq2 -namespace:nwind -code:Northwind.cs -sprocs



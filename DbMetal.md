# Introduction #

`DbMetal.exe` is used to:

  1. Generate a `.dbml` file based on the tables within a database; or
  1. Generate source code containing entities which map to database tables.  `DbMetal` uses either a `.dbml` file or an existing database to generate the entity descriptions.

`DbMetal` is intended to be semantically similar in use and intent to the [.NET framework SqlMetal.exe tool](http://msdn.microsoft.com/en-us/library/bb386987.aspx).

# Command-line Options #

```
DbLinq Database mapping generator 2008 version 0.19
for Microsoft (R) .NET Framework version 3.5
Distributed under the MIT licence (http://linq.to/db/license)

DbMetal [OPTIONS] [<DBML INPUT FILE>]

  Generates code and mapping for DbLinq. SqlMetal can:
  - Generate source code and mapping attributes or a mapping file from a database.
  - Generate an intermediate dbml file for customization from the database.
  - Generate code and mapping attributes or mapping file from a dbml file.

  -c, --conn=CONNECTION STRING
                             Database CONNECTION STRING. Cannot be used with 
                               /server, /user or /password options.
  -u, --user=NAME            Login user NAME.
  -p, --password=PASSWORD    Login PASSWORD.
  -s, --server=NAME          Database server NAME.
  -d, --database=NAME        Database catalog NAME on server.
      --provider=PROVIDER    Specify PROVIDER. May be Ingres, MySql, Oracle, 
                               OracleODP, PostgreSql or Sqlite.
      --with-schema-loader=TYPE
                             ISchemaLoader implementation TYPE.
      --with-dbconnection=TYPE
                             IDbConnection implementation TYPE.
      --with-sql-dialect=TYPE
                             IVendor implementation TYPE.
      --code=FILE            Output as source code to FILE. Cannot be used 
                               with /dbml option.
      --dbml=FILE            Output as dbml to FILE. Cannot be used with /map 
                               option.
      --language=NAME        Language NAME for source code: C#, C#2 or VB 
                               (default: derived from extension on code file 
                               name).
      --aliases=FILE         Use mapping FILE.
      --schema               Generate schema in code files (default: enabled).
      --namespace=NAME       Namespace NAME of generated code (default: no 
                               namespace).
      --entitybase=TYPE      Base TYPE of entity classes in the generated 
                               code (default: entities have no base class).
      --member-attribute=ATTRIBUTE
                             ATTRIBUTE for entity members in the generated 
                               code, can be specified multiple times.
      --generate-type=TYPE   Generate only the TYPE selected, can be 
                               specified multiple times and does not prevent 
                               references from being generated (default: 
                               generate a DataContex subclass and all the 
                               entities in the schema).
      --generate-equals-hash Generates overrides for Equals() and 
                               GetHashCode() methods.
      --sprocs               Extract stored procedures.
      --pluralize            Automatically pluralize or singularize class and 
                               member names using specified culture rules.
      --culture=CULTURE      Specify CULTURE for word recognition and 
                               pluralization (default: "en").
      --case=STYLE           Transform names with the indicated STYLE 
                               (default: net; may be: leave, pascal, camel, 
                               net).
      --generate-timestamps  Generate timestampes in the generated code 
                               (default: enabled).
      --readline             Wait for a key to be pressed after processing.
  -h, -?, --help             Show this help
```
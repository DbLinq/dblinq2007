# Introduction #

The [DbLinq-0.19.zip](http://dblinq2007.googlecode.com/files/DbLinq-0.19.zip) file contains the DbLinq binaries such as `DbMetal.exe` and `DbLinq.dll`.  However, these are not usable by themselves.  No ADO.NET providers are distributed with DbLinq, and ADO.NET providers are needed in order for DbLinq to interact with databases.

DbLinq only uses standard ADO.NET interfaces in order to interact with databases.  An ADO.NET provider (e.g. `System.Data.SQLite.dll`) in combination with a DbLinq provider (e.g. `DbLinq.Sqlite.dll`) is needed in order for DbLinq to interact with a database.

# DbLinq provider assemblies #

DbLinq ships with provider assemblies which implement support for various SQL "dialects."  DbLinq provider assemblies may be usable by multiple different ADO.NET providers; what's important is that the the DbLinq provider is responsible for generating SQL for a given database, and may refer to various database-specific internal tables, e.g. to retrieve the primary key of a recently inserted row.

The DbLinq provider assemblies include:

  * `DbLinq.Firebird.dll`: [Firebird](http://www.firebirdsql.org/) support.
  * `DbLinq.Ingres.dll`: [Ingres](http://www.ingres.com/) support.
  * `DbLinq.MySql.dll`: [MySQL](http://www.mysql.com/) support.
  * `DbLinq.Oracle.dll`: [Oracle](http://www.oracle.com/) support, using either the System.Data.OracleClient or [Oracle.DataAccess](http://www.oracle.com/technology/software/tech/windows/odpnet/index.html) drivers.
  * `DbLinq.PostgreSql.dll`: [PostgreSQL](http://www.postgresql.org/) support.
  * `DbLinq.Sqlite.dll`: [SQLite](http://www.sqlite.org/) support, using either the [System.Data.SQLite](http://sqlite.phxsoftware.com/) or [Mono.Data.Sqlite](http://www.mono-project.com/SQLite) drivers.
  * `DbLinq.SqlServer.dll`: [Microsoft SQL Server](http://www.microsoft.com/sqlserver/) support.

# To run DbMetal #

DbMetal has two modes of operation:

  1. Connecting to a database to generate either a `.dbml` or source code containing entities, for example:
```
DbMetal /provider:Sqlite /conn "Data Source=File.db3" /dbml:File.dbml
```
  1. Generating source code containing entities contained in a `.dbml` file, for example:
```
DbMetal /code:File.cs File.dbml
```

Both of these require both an ADO.NET provider and a DbLinq provider for your database.  DbLinq provider assemblies are bundled with DbLinq, e.g. `DbLinq.Sqlite.dll`.

There are three ways that the ADO.NET provider can be used with `DbMetal.exe`:

  1. Copy the ADO.NET provider assembly into the DbLinq installation directory.  For example, for [MySQL](http://www.mysql.com/) support the [MySql.Data.dll](http://dev.mysql.com/downloads/connector/net/) assembly should be copied into the same directory as `DbMetal.exe`.
  1. If your ADO.NET provider is present within the Global Assembly Cache, you can edit `DbMetal.exe.config` (in the DbLinq installation directory) to use an assembly-qualified type name in the `/configuration/providers/provider/@databaseConnection` attribute.  For example, if you installed `System.Data.SQLite.dll` into the GAC, you could edit `DbMetal.exe.config` and change the `/configuration/providers/provider[@name='SQLite']/@databaseConnection` attribute to `System.Data.SQLite.SQLiteConnection, System.Data.SQLite, Version=1.0.61.0, Culture=neutral, PublicKeyToken=db937bc2d44ff139`:
```
<provider
    name="SQLite"
    dbLinqSchemaLoader="DbLinq.Sqlite.SqliteSchemaLoader, DbLinq.Sqlite" 
    databaseConnection="System.Data.SQLite.SQLiteConnection, System.Data.SQLite, Version=1.0.61.0, Culture=neutral, PublicKeyToken=db937bc2d44ff139" />
```
  1. You can use the `DbMetal.exe --with-dbconnection=TYPE` option, where TYPE is an assembly-qualified type name for the [IDbConnection](http://msdn.microsoft.com/en-us/library/system.data.idbconnection.aspx) implementation to use.
```
DbMetal.exe \
    --provider=Sqlite \
    --with-dbconnection="Mono.Data.Sqlite.SqliteConnection, Mono.Data.Sqlite, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756" \
    ...
```

# To use DbLinq #

Just as with DbMetal, using DbLinq within your app _also_ requires both an ADO.NET provider and a DbLinq provider.

There are three ways to create a `DataContext` instance.  Depending on how you create the `DataContext`, you may need to explicitly provide either the ADO.NET provider, the DbLinq provider, or both.

  1. **When using a _connection string_ with the `DataContext(string)` constructor**, _both_ the ADO.NET provider _and_ the DbLinq provider need to be specified.  The ADO.NET provider is provided via the `DbLinqConnectionType` key/value pair in the connection string, while the DbLinq provider is in the `DbLinqProvider` key/value pair.  If neither is listed, then DbLinq _defaults_ to using [SqlConnection](http://msdn.microsoft.com/en-us/library/system.data.sqlclient.sqlconnection.aspx) for `DbLinqConnectionType` and to using `SqlServer` for `DbLinqProvider`.  ([Type.GetType()](http://msdn.microsoft.com/en-us/library/system.type.gettype.aspx) is used to get the assembly-qualified type name used by `DbLinqConnectionType`, so a "full" assembly name complete with `PublicKeyToken` may not be necessary if the assembly is an unsigned assembly and/or is in a location that `Type.GetType()` will search for, e.g. in the same directory as the launching `.exe`.)<p>This may also be necessary if the ADO.NET connection type doesn't support "unknown" connection string keywords, as the <code>DbLinqConnectionType</code> and <code>DbLinqProvider</code> values will be removed from the connection string before the connection type is created.<br>
<pre><code>var db = new Northwind(<br>
    "DbLinqProvider=Sqlite;" +<br>
    "DbLinqConnectionType=System.Data.SQLite.SQLiteConnection, System.Data.SQLite, Version=1.0.61.0, Culture=neutral, PublicKeyToken=db937bc2d44ff139" +<br>
    "Data Source=Filename.db3"<br>
    // + other connection string key/value pairs<br>
);<br>
</code></pre>
<ol><li><b>When using an existing <code>IDbConnection</code> with the <code>DataContext(IDbConnection)</code> constructor</b>, the <code>DbLinqConnectionType</code> key/value pair isn't necessary, as the provided <code>IDbConnection</code> instance will be used.  The <code>DbLinqProvider</code> key/value pair is required (unless you want Microsoft SQL Server support), and thus must be provided to the <code>IDbConnection</code> implementation before constructing the <code>DataContext</code>.<p><i>Note:</i> This isn't an option if your ADO.NET connection type throws an exception on "unknown" connection string keywords such as <code>DbLinqProvider</code>.  If your ADO.NET connection type does this, you will need to stick to connection strings, as done in (1).<br>
<pre><code>var connection = new SQLiteConnection(<br>
    "DbLinqProvider=Sqlite; " +<br>
    "Data Source=Filename.db3"<br>
    // + other connection string key/value pairs<br>
);<br>
var db = new Northwind(connection);<br>
</code></pre>
</li><li><b>When using an existing <code>IDbConnection</code> and a provided <code>IVendor</code> implementation with the <code>DataContext(IDbConnection, IVendor)</code> constructor</b>, neither <code>DbLinqConnectionType</code> nor the <code>DbLinqProvider</code> key/value pairs need to be within the connection string.  Note: if using Mono's <code>System.Data.Linq.dll</code>, this option isn't available as the <code>DataContext(IDbConnection, IVendor)</code> constructor doesn't exist.<br>
<pre><code>var connection = new SQLiteConnection(<br>
    "Data Source=Filename.db3"<br>
    // + other connection string key/value pairs<br>
);<br>
var db = new Northwind(connection, new DbLinq.Sqlite.SqliteVendor());<br>
</code></pre></li></ol>

Note that in all three situations, the ADO.NET provider assembly and the DbLinq provider assembly must be present in locations that <code>Type.GetType()</code> can find them, e.g. the GAC, the same directory as your main <code>.exe</code> assembly, your webapp <code>bin</code> directory, etc.<br>
<br>
<h1>Why aren't ADO.NET provider assemblies bundled with DbLinq?</h1>

There are a variety of ADO.NET providers for databases.  For example, there are two ADO.NET providers for Oracle (e.g. <a href='http://msdn.microsoft.com/en-us/library/system.data.oracleclient.aspx'>System.Data.OracleClient</a> and <a href='http://www.oracle.com/technology/software/htdocs/distlic.html?url=/technology/software/tech/windows/odpnet/utilsoft.html'>Oracle.DataAccess</a>), and multiple versions of the SQLite provider.  This raises two problems:<br>
<br>
<ol><li>If DbLinq were to include an ADO.NET provider, it may be for a different version of the database you wish to use, in which case you would need to provide your own ADO.NET provider <i>anyway</i>.<br>
</li><li>The licensing for many providers is <a href='http://groups.google.com/group/dblinq/msg/2981c81492fcf32a'>unclear</a> or otherwise incompatible with DbLinq's license.</li></ol>

We don't wish to step into a legal minefield, so for simplicity and consistency no ADO.NET providers are included with the DbLinq binaries.<br>
<br>
<h1>Mono Considerations</h1>

Mono's <code>System.Data.Linq.dll</code> is built by including all of the DbLinq Provider assembly code into <code>System.Data.Linq.dll</code>, and then by making all DbLinq-specific types <code>internal</code> to the assembly.<br>
<br>
Consequently:<br>
<ol><li>When <a href='#To_run_DbMetal.md'>running</a> <code>DbMetal.exe</code> to generate your entity definitions, you must define the <code>MONO_STRICT</code> conditional compilation symbol so that DbLinq-specific types won't be referenced when compiling (e.g. by adding <code>-d:MONO_STRICT</code> to your compiler options).<br>
</li><li>When <a href='#To_use_DbLinq.md'>using</a> Mono's <code>System.Data.Linq.dll</code>, you can't use the <code>IVendor</code> interface, as this is <code>internal</code>.  You can only rely on the <code>DbLinqConnectionType</code> and <code>DbLinqProvider</code> connection string parameters (option 1) or provide an <code>IDbConnection</code> instance having a <code>DbLinqProvider</code> connection string parameter within the <a href='http://msdn.microsoft.com/en-us/library/system.data.idbconnection.connectionstring.aspx'>IDbConnection.ConnectionString</a> property value (option 2).
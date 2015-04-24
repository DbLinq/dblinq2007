# DBML format documentation and use by DbMetal #

Microsoft
[msdn documentation](http://msdn2.microsoft.com/en-us/library/bb386907.aspx) is rather scarce, so we a starting a this page.

If we depart from the official format to squeeze in new features (e.g. database-defined enums),
it will be posted here.

| **Database** | | | |
|:-------------|:|:|:|
| **Name** | **Type** | **Description** | **DbMetal use** |
|Connection|Connection|...|![http://dblinq2007.googlecode.com/svn/wiki/img/no.png](http://dblinq2007.googlecode.com/svn/wiki/img/no.png)|
|Table|Table[.md](.md)|Tables in schema|![http://dblinq2007.googlecode.com/svn/wiki/img/yes.png](http://dblinq2007.googlecode.com/svn/wiki/img/yes.png)|
|Function|Function[.md](.md)|Stored procedures and functions|![http://dblinq2007.googlecode.com/svn/wiki/img/yes.png](http://dblinq2007.googlecode.com/svn/wiki/img/yes.png)|
|Name|string|Schema name|![http://dblinq2007.googlecode.com/svn/wiki/img/yes.png](http://dblinq2007.googlecode.com/svn/wiki/img/yes.png) Placed in DBML file and in comments in code file|
|EntityNamespace|string (Namespace)|Namespace for entities|![http://dblinq2007.googlecode.com/svn/wiki/img/yes.png](http://dblinq2007.googlecode.com/svn/wiki/img/yes.png)|
|ContextNamespace|string (Namespace)|Namespace for DataContext inheritor|![http://dblinq2007.googlecode.com/svn/wiki/img/yes.png](http://dblinq2007.googlecode.com/svn/wiki/img/yes.png)|
|Class|string (Type)|DataContext inheritor name: the class used to manipulate tables|![http://dblinq2007.googlecode.com/svn/wiki/img/yes.png](http://dblinq2007.googlecode.com/svn/wiki/img/yes.png)|
|AccessModifier|AccessModifier|Access modifier for DataContext inheritor|![http://dblinq2007.googlecode.com/svn/wiki/img/yes.png](http://dblinq2007.googlecode.com/svn/wiki/img/yes.png)|
|ClassModifier|ClassModifier|Class modifier for DataContext inheritor|![http://dblinq2007.googlecode.com/svn/wiki/img/yes.png](http://dblinq2007.googlecode.com/svn/wiki/img/yes.png)|
|BaseType|string (Type)|DataContext (DbLinq.Linq.DataContext in our case)|![http://dblinq2007.googlecode.com/svn/wiki/img/yes.png](http://dblinq2007.googlecode.com/svn/wiki/img/yes.png)|
|Provider|string|Database provider (Vendor + driver)|![http://dblinq2007.googlecode.com/svn/wiki/img/yes.png](http://dblinq2007.googlecode.com/svn/wiki/img/yes.png) written in DBML file|
|ExternalMapping|string|? |![http://dblinq2007.googlecode.com/svn/wiki/img/no.png](http://dblinq2007.googlecode.com/svn/wiki/img/no.png)|
|Serialization|SerializationMode|? |![http://dblinq2007.googlecode.com/svn/wiki/img/no.png](http://dblinq2007.googlecode.com/svn/wiki/img/no.png)|
|EntityBase|string (Type)|Base class for all table classes|![http://dblinq2007.googlecode.com/svn/wiki/img/yes.png](http://dblinq2007.googlecode.com/svn/wiki/img/yes.png)|
| **Table** | | | |
| **Name** | **Type** | **Description** | **DbMetal use** |
|Type|Type|Corresponding .NET type|![http://dblinq2007.googlecode.com/svn/wiki/img/yes.png](http://dblinq2007.googlecode.com/svn/wiki/img/yes.png)|
|InsertFunction|TableFunction|? |![http://dblinq2007.googlecode.com/svn/wiki/img/no.png](http://dblinq2007.googlecode.com/svn/wiki/img/no.png)|
|UpdateFunction|TableFunction|? |![http://dblinq2007.googlecode.com/svn/wiki/img/no.png](http://dblinq2007.googlecode.com/svn/wiki/img/no.png)|
|DeleteFunction|TableFunction|? |![http://dblinq2007.googlecode.com/svn/wiki/img/no.png](http://dblinq2007.googlecode.com/svn/wiki/img/no.png)|
|Name|string|Table schema and name|![http://dblinq2007.googlecode.com/svn/wiki/img/yes.png](http://dblinq2007.googlecode.com/svn/wiki/img/yes.png)|
|Member|string|DataContext inheritor property name allowing to get the Table manipulator|![http://dblinq2007.googlecode.com/svn/wiki/img/yes.png](http://dblinq2007.googlecode.com/svn/wiki/img/yes.png)|
|AccessModifier|AccessModifier|? |![http://dblinq2007.googlecode.com/svn/wiki/img/no.png](http://dblinq2007.googlecode.com/svn/wiki/img/no.png)|
|Modifier|MemberModifier|? |![http://dblinq2007.googlecode.com/svn/wiki/img/no.png](http://dblinq2007.googlecode.com/svn/wiki/img/no.png)|
| **Type** | | | |
| **Name** | **Type** | **Description** | **DbMetal use** |
|Items|object[.md](.md) (Column or Association)|Columns and associations|![http://dblinq2007.googlecode.com/svn/wiki/img/yes.png](http://dblinq2007.googlecode.com/svn/wiki/img/yes.png)|
|Type|Type[.md](.md)|? |![http://dblinq2007.googlecode.com/svn/wiki/img/no.png](http://dblinq2007.googlecode.com/svn/wiki/img/no.png)|
|IdRef|string|? |![http://dblinq2007.googlecode.com/svn/wiki/img/no.png](http://dblinq2007.googlecode.com/svn/wiki/img/no.png)|
|Id|string|? |![http://dblinq2007.googlecode.com/svn/wiki/img/no.png](http://dblinq2007.googlecode.com/svn/wiki/img/no.png)|
|Name|string (Type)|Table as class type name|![http://dblinq2007.googlecode.com/svn/wiki/img/yes.png](http://dblinq2007.googlecode.com/svn/wiki/img/yes.png)|
|InheritanceCode|string|? |![http://dblinq2007.googlecode.com/svn/wiki/img/no.png](http://dblinq2007.googlecode.com/svn/wiki/img/no.png)|
|IsInheritanceDefault|bool|? |![http://dblinq2007.googlecode.com/svn/wiki/img/no.png](http://dblinq2007.googlecode.com/svn/wiki/img/no.png)|
|AccessModifier|AccessModifier|Access modifier for table class|![http://dblinq2007.googlecode.com/svn/wiki/img/yes.png](http://dblinq2007.googlecode.com/svn/wiki/img/yes.png)|
|Modifier|ClassModifier|Class modifier for table class|![http://dblinq2007.googlecode.com/svn/wiki/img/yes.png](http://dblinq2007.googlecode.com/svn/wiki/img/yes.png)|
| **Column** | | | |
| **Name** | **Type** | **Description** | **DbMetal use** |
|Name|string|Column database name|![http://dblinq2007.googlecode.com/svn/wiki/img/yes.png](http://dblinq2007.googlecode.com/svn/wiki/img/yes.png)|
|Member|string|Column name as class member|![http://dblinq2007.googlecode.com/svn/wiki/img/yes.png](http://dblinq2007.googlecode.com/svn/wiki/img/yes.png)|
|Storage|string|Column backing field|![http://dblinq2007.googlecode.com/svn/wiki/img/yes.png](http://dblinq2007.googlecode.com/svn/wiki/img/yes.png)|
|AccessModifier|AccessModifier|Property scope|![http://dblinq2007.googlecode.com/svn/wiki/img/yes.png](http://dblinq2007.googlecode.com/svn/wiki/img/yes.png)|
|Modifier|MemberModifier|Property modifiers|![http://dblinq2007.googlecode.com/svn/wiki/img/yes.png](http://dblinq2007.googlecode.com/svn/wiki/img/yes.png)|
|Type|string (Type)|.NET type|![http://dblinq2007.googlecode.com/svn/wiki/img/yes.png](http://dblinq2007.googlecode.com/svn/wiki/img/yes.png), ![http://dblinq2007.googlecode.com/svn/wiki/img/extension.png](http://dblinq2007.googlecode.com/svn/wiki/img/extension.png) Extension for enums: "enum A=1, B=2 ..."|
|DbType|string|Database column type|![http://dblinq2007.googlecode.com/svn/wiki/img/yes.png](http://dblinq2007.googlecode.com/svn/wiki/img/yes.png)|
|IsReadonly|bool|? |![http://dblinq2007.googlecode.com/svn/wiki/img/no.png](http://dblinq2007.googlecode.com/svn/wiki/img/no.png)|
|IsPrimaryKey|bool|Column is PK|![http://dblinq2007.googlecode.com/svn/wiki/img/yes.png](http://dblinq2007.googlecode.com/svn/wiki/img/yes.png)|
|IsDbGenerated|bool|Column is generated automatically if not specified by insert command|![http://dblinq2007.googlecode.com/svn/wiki/img/yes.png](http://dblinq2007.googlecode.com/svn/wiki/img/yes.png) Partial support by DbLinq core (may not work in all cases|
|CanBeNull|bool|Column is nullable|![http://dblinq2007.googlecode.com/svn/wiki/img/yes.png](http://dblinq2007.googlecode.com/svn/wiki/img/yes.png)|
|UpdateCheck|UpdateCheck|? |![http://dblinq2007.googlecode.com/svn/wiki/img/no.png](http://dblinq2007.googlecode.com/svn/wiki/img/no.png)|
|IsDiscriminator|bool|? |![http://dblinq2007.googlecode.com/svn/wiki/img/no.png](http://dblinq2007.googlecode.com/svn/wiki/img/no.png)|
|Expression|string|Expression used for column generation|![http://dblinq2007.googlecode.com/svn/wiki/img/yes.png](http://dblinq2007.googlecode.com/svn/wiki/img/yes.png) Export is done, but property is unused|
|IsVersion|bool|? |![http://dblinq2007.googlecode.com/svn/wiki/img/no.png](http://dblinq2007.googlecode.com/svn/wiki/img/no.png)|
|IsDelayLoaded|bool|? |![http://dblinq2007.googlecode.com/svn/wiki/img/no.png](http://dblinq2007.googlecode.com/svn/wiki/img/no.png)|
|AutoSync|AutoSync|? |![http://dblinq2007.googlecode.com/svn/wiki/img/no.png](http://dblinq2007.googlecode.com/svn/wiki/img/no.png)|
| **Association** | | | |
| **Name** | **Type** | **Description** | **DbMetal use** |
|Name|string|Association database name|![http://dblinq2007.googlecode.com/svn/wiki/img/yes.png](http://dblinq2007.googlecode.com/svn/wiki/img/yes.png)|
|Member|string|Association name as class member|![http://dblinq2007.googlecode.com/svn/wiki/img/yes.png](http://dblinq2007.googlecode.com/svn/wiki/img/yes.png)|
|Storage|string|Association backing field|![http://dblinq2007.googlecode.com/svn/wiki/img/yes.png](http://dblinq2007.googlecode.com/svn/wiki/img/yes.png)|
|AccessModifier|AccessModifier|Property scope|![http://dblinq2007.googlecode.com/svn/wiki/img/yes.png](http://dblinq2007.googlecode.com/svn/wiki/img/yes.png)|
|Modifier|MemberModifier|Property modifiers|![http://dblinq2007.googlecode.com/svn/wiki/img/yes.png](http://dblinq2007.googlecode.com/svn/wiki/img/yes.png)|
|Type|string (Type)|Related (foreign) class type name|![http://dblinq2007.googlecode.com/svn/wiki/img/yes.png](http://dblinq2007.googlecode.com/svn/wiki/img/yes.png)|
|ThisKey|string|Property name for key in this class|![http://dblinq2007.googlecode.com/svn/wiki/img/yes.png](http://dblinq2007.googlecode.com/svn/wiki/img/yes.png)|
|OtherKey|string|Property name for key in the other class (specified by Type)|![http://dblinq2007.googlecode.com/svn/wiki/img/yes.png](http://dblinq2007.googlecode.com/svn/wiki/img/yes.png)|
|IsForeignKey|bool|true is the association is a FK definition|![http://dblinq2007.googlecode.com/svn/wiki/img/yes.png](http://dblinq2007.googlecode.com/svn/wiki/img/yes.png)|
|Cardinality|Cardinality|Relation cardinality|![http://dblinq2007.googlecode.com/svn/wiki/img/no.png](http://dblinq2007.googlecode.com/svn/wiki/img/no.png)|
|DeleteRule|string|? |![http://dblinq2007.googlecode.com/svn/wiki/img/no.png](http://dblinq2007.googlecode.com/svn/wiki/img/no.png)|
|DeleteOnNull|bool|? |![http://dblinq2007.googlecode.com/svn/wiki/img/no.png](http://dblinq2007.googlecode.com/svn/wiki/img/no.png)|
| **Function** | | | |
| **Name** | **Type** | **Description** | **DbMetal use** |
|Parameter|Parameter[.md](.md)|Procedure parameters|![http://dblinq2007.googlecode.com/svn/wiki/img/yes.png](http://dblinq2007.googlecode.com/svn/wiki/img/yes.png)|
|Items|object[.md](.md) (Type or Return)|Procedure return value (and something else I don't know)|![http://dblinq2007.googlecode.com/svn/wiki/img/yes.png](http://dblinq2007.googlecode.com/svn/wiki/img/yes.png)|
|Name|string|Procedure database name|![http://dblinq2007.googlecode.com/svn/wiki/img/yes.png](http://dblinq2007.googlecode.com/svn/wiki/img/yes.png)|
|Id|string|? |![http://dblinq2007.googlecode.com/svn/wiki/img/no.png](http://dblinq2007.googlecode.com/svn/wiki/img/no.png)|
|Method|string|Procedure class name in DataContext inheritor|![http://dblinq2007.googlecode.com/svn/wiki/img/yes.png](http://dblinq2007.googlecode.com/svn/wiki/img/yes.png)|
|AccessModifier|AccessModifier|Procedure scope|![http://dblinq2007.googlecode.com/svn/wiki/img/yes.png](http://dblinq2007.googlecode.com/svn/wiki/img/yes.png)|
|Modifier|MemberModifier|Procedure modifiers|![http://dblinq2007.googlecode.com/svn/wiki/img/yes.png](http://dblinq2007.googlecode.com/svn/wiki/img/yes.png)|
|HasMultipleResults|bool|? |![http://dblinq2007.googlecode.com/svn/wiki/img/no.png](http://dblinq2007.googlecode.com/svn/wiki/img/no.png)|
|IsComposable|bool|true if the procedure is a procedure, false if it is a function|![http://dblinq2007.googlecode.com/svn/wiki/img/yes.png](http://dblinq2007.googlecode.com/svn/wiki/img/yes.png)|
| **Return** | | | |
| **Name** | **Type** | **Description** | **DbMetal use** |
|Type|string (Type)|.NET type|![http://dblinq2007.googlecode.com/svn/wiki/img/yes.png](http://dblinq2007.googlecode.com/svn/wiki/img/yes.png)|
|DbType|string|Database type|![http://dblinq2007.googlecode.com/svn/wiki/img/yes.png](http://dblinq2007.googlecode.com/svn/wiki/img/yes.png)|
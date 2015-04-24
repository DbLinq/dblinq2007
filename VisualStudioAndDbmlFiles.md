# Introduction #

Visual Studio has tooling for .NET [Linq-to-SQL](http://msdn.microsoft.com/en-us/library/bb425822.aspx) and [Entity Framework](http://msdn.microsoft.com/en-us/library/aa697427(VS.80).aspx).  Linq-to-SQL support is based around `.dbml` files.

# Details #

Visual Studio's `.dbml` support will _not_ work with DbLinq.

The problem is that Visual Studio will process the `.dbml` file with e.g. [SqlMetal](http://msdn.microsoft.com/en-us/library/bb386987.aspx), and `SqlMetal` doesn't know about DbLinq and DbMetal.  The result is that the generated `.designer.cs` file will be generated from the wrong tool, and thus won't run as expected.

# Solution #

Decent guidance would be appreciated. ;-)

For now, don't add `.dbml` files to Visual Studio (or if you do, don't build the Visual Studio-generated `.designer.cs` file).  Instead, add some pre-build logic to run DbMetal against your `.dbml` file to generate a file that is included in your build.
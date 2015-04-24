# Differences between DbLinq implementations #

This page describes the two implementations and how to contribute to them.
  * **Standalone DbLinq** is the original implementation, designed to replace missing Linq to SQL access to other databases and extend it.
  * **DbLinq for Mono** is the implementation for Mono. Its goal is to be the default `System.Data.Linq` implementation

We will refer below to **strict** code and **extended** code. The strict code is the common part, following strictly the Linq to SQL implementation, and the extended code is the non-standard code, extending this specification.

## DbLinq versions and differences ##
|Feature|Standalone DbLinq|DbLinq for Mono|
|:------|:----------------|:--------------|
|Target|Microsoft.NET and Mono|Mono|
|Specifications|`System.Data.Linq` extended|`System.Data.Linq` strict|
|Linq namespace|`DbLinq.Data.Linq`|`System.Data.Linq`|
|Predefined constant|(none)|`MONO_STRICT`|

## Contributing ##

### Files ###

Files in `DbLinq/Data/Linq` are organized as follows:
  * `<ClassOrInterfaceName>.cs` contains the strict implementation
  * `<ClassOrInterfaceName>.Extended.cs` contains DbLinq specific extensions

### Namespaces ###

Classes and interfaces are declared as `partial` in both files.
The namespace is defined as follows:
```
#if MONO_STRICT
namespace System
#else
namespace DbLinq
#endif
{
    namespace Data.Linq
    {
        public partial class SomeClass
        {
            // ...
        }
    }
}
```

Please note that this code snippet is necessary only in the strict code part.

### Calling extensions from strict code ###

  1. Define a partial method in strict code
> 2. Call it from from strict code
> 3. Implement the class in extended code
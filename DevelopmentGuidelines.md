# Development guidelines #
### Conception recommendations ###
#### Use dependencies injection (and allow extensions) ####
  * Components implement an interface (such as `DataMapper` and `IDataMapper`). User classes use interface (the default implementation is injected in the constructor)
  * Avoid `static` classes and members, since they don't allow interface implementation

### Naming ###
  * We start from .NET naming conventions at http://msdn2.microsoft.com/en-us/library/xzf533w0(vs.71).aspx
  * Use PascalCase for classes, methods and public/protected properties names
  * Use camelCase for parameters, local variables and private properties
  * Use long (explicit) variable names, and no short names (`Generated` instead of `Gen`, `Variables` instead of `Vars`, etc.)
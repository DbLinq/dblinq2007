# Sugar #

"Sugar" is the codename for the new linq to sql generation engine.
Its main goals are:
  * Simplify process (unify joins and projections)
  * Allow extensibility
  * Allow caching

### Process ###

The main process is:
> I. Parse Expressions (Sugar only)
    1. Expressions language patterns identification and replacement (VB string compare for example)
    * Expressions dispatch
      * Non-operation expressions deduction
        * Table
        * Column
        * Input parameter
        * Meta-table (a class containing table aliases)
      * Key methods (where, select...) identification and inner expressions dispatching (top-level method is handled specifically)
    * Constants reduction
  * Generate SQL (Sugar + Database IVendor)

### The cache ###

The cache keeps track of:
  * Generated SQL
  * Result value/type(code generation expression)
  * Parameters
The cache key is the linq expression reference (to be checked)
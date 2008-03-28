using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using nwind;
using Test_NUnit;

#if MYSQL
    namespace Test_NUnit_MySql
#elif ORACLE
    namespace Test_NUnit_Oracle
#elif POSTGRES
    namespace Test_NUnit_PostgreSql
#elif SQLITE
    namespace Test_NUnit_Sqlite
#elif INGRES
    namespace Test_NUnit_Ingres
#else
    #error unknown target
#endif
{
    /// <summary>
    /// TODO: write test cases which ensure that db.Employee can produce derived classes, such as HourlyEmployee.
    /// </summary>
    public class VerticalPartitioningTest : TestBase
    {
    }
}

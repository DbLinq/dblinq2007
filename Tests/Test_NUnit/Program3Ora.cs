using System;
using System.Collections.Generic;
using System.Text;

#if LINQ_PREVIEW_2006
//Visual Studio 2005 with Linq Preview May 2006 - can run on Win2000
using System.Query;
using System.Expressions;
#else
//Visual Studio Orcas - requires WinXP
using System.Linq;
#endif

namespace Test_NUnit
{
    /// <summary>
    /// when a problem crops up in NUnit, 
    /// you can convert the project from DLL into EXE, 
    /// and debug into the offending method.
    /// </summary>
    class Program2
    {
        static void Main()
        {
            //new ReadTest_Complex().F1_ProductCount();
            new ReadTest_Complex().F1_ProductCount();
            //new WriteTest().G2_DeleteTest();
            //new WriteTest().G1_InsertProduct();
        }
    }
    //class Column { public string table_name; }
    //class Table { public string table_name; }
}

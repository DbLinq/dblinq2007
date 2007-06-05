#region HEADER
using System;
using System.Collections.Generic;
using System.Text;
#if LINQ_PREVIEW_2006
//Visual Studio 2005 with Linq Preview May 2006 - can run on Win2000
using System.Query;
using System.Data.DLinq;
#else
//Visual Studio Orcas - requires WinXP
using System.Linq;
using System.Data.Linq;
#endif

#endregion

namespace Test_NUnit
{
#region HEADER
    /// <summary>
    /// when a problem crops up in NUnit, you can convert the project from DLL into EXE, 
    /// and debug into the offending method.
    /// </summary>
#endregion
    class Program2
    {
        static void Main()
        {
            //new ReadTest_GroupBy().G01_SimpleGroup_Count();
            //new ReadTest_GroupBy().G08_OrderSumByCustomerID();

            new ReadTest().A4_SelectSingleCustomer();
            //new ReadTest_GroupBy().G01_SimpleGroup();
            //new ReadTest_GroupBy().G04_SimpleGroup_WithSelector();
            //new ReadTest_GroupBy().G04_OrderSumByCustomerID();
            //ReadTest_Complex rc = new ReadTest_Complex();
            //rc.F10_DistinctCity();
            //rc.F11_ConcatString();
            //new ReadTest().D10_Products_LetterP_Desc();
            //new ReadTest().D7_OrdersFromLondon_Alt();
            //new WriteTest().G2_DeleteTest();
            //new WriteTest().G1_InsertProduct();
        }
    }

}

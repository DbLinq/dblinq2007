#region HEADER
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Data.Linq;

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

            //new ReadTest().D05_SelectOrdersForProduct();
            //new ReadTest().D07_OrdersFromLondon_Alt();
            new ReadTest_GroupBy().G01_SimpleGroup_Count();
            //new ReadTest_GroupBy().G04_SimpleGroup_WithSelector();
            //new ReadTest_GroupBy().G05_Group_Into();
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

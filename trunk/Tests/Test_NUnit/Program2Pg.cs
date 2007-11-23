#region HEADER
using System;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using System.Linq;
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
            //new ReadTest().A1_PingDatabase();
            //new ReadTest().D06_OrdersFromLondon();
            //new StoredProcTest().SP3_GetOrderCount_SelField();
            //new ReadTest_GroupBy().G01_SimpleGroup();
            //new ReadTest_GroupBy().G06_OrderCountByCustomerID();

            //new ReadTest_GroupBy().G04_OrderSumByCustomerID();
            ReadTest_Complex rc = new ReadTest_Complex();
            rc.F5_AvgProductId();
            //rc.F11_ConcatString();
            //rc.F12_ConcatString_2();
            //rc.F2_ProductCount_Clause();
            //rc.F2_ProductCount_Projected();
            //rc.F3_MaxProductId();
            //new ReadTest_Complex().F3_MaxProductId();
            //new ReadTest().D09_Products_LetterP_Take5();
            //new ReadTest().D7_OrdersFromLondon_Alt();
            //new WriteTest().G2_DeleteTest();
            //new WriteTest().G1_InsertProduct();
        }
    }
    //class Column { public string table_name; }
    //class Table { public string table_name; }
}

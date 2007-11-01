#region HEADER
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Data.Linq;
using Test_NUnit.Linq_101_Samples;
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

            //new ReadTest().C1_SelectProducts();
            //new StoredProcTest().SP3_GetOrderCount_SelField();
            //new ReadTest().D05_SelectOrdersForProduct();
            //new ReadTest_GroupBy().G01_SimpleGroup_Count();
            //new ReadTest_AllTypes().AT5_SelectEnum_();
            //new ReadTest_Operands().H1_SelectConcat();
            //new ReadTest_GroupBy().G04_SimpleGroup_WithSelector();
            //new ReadTest_GroupBy().G05_Group_Into();
            //ReadTest_Complex rc = new ReadTest_Complex();
            //rc.F11_ConcatString();
            //new WriteTest().E2_UpdateEnum();
            //new WriteTest().E2_UpdateEnum();
            //new WriteTest_BulkInsert().BI01_InsertProducts();
            new NullTest().NullableT_HasValue();
            //new Count_Sum_Min_Max_Avg().LiqnToSqlCount02();
        }
    }

}

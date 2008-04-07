#region HEADER
using System;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Test_NUnit_Ingres;
#endregion

namespace Test_NUnit_Ingres
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
            /*==========================================================================*/
            /* All failing Tests are recorded here. They will be removed, once they run */
            /*==========================================================================*/

            /*==================================================================*/
            /* The following failing tests are investigated, but not solved yet */
            /*==================================================================*/
            // DUPLICATE KEY ON INSERT
            //new CompositePK_Test().G10_DeleteTableWithCompositePK();

            // The column value has not changed for some reason, so the assertion fails
            //new CompositePK_Test().G11_UnchangedColumnShouldNotUpdated();

            // generates this SQL
            // SELECT $.categoryid, $.discontinued, $.productid, $.productname, 
            // $.quantityperunit, $.reorderlevel, $.supplierid, $.unitprice, $.unitsinstock, $.unitsonorder
            // FROM linquser.products $ WHERE $.supplierid = 1 AND $.unitsinstock > 2 ORDER BY $.productid
            // which failes as a single $ sign is not allowed for a table alias
            //new DynamicLinqTest().DL1_Products();

            // SELECT COUNT(*) FROM linquser.products p$ ORDER BY p$.productid
            // productid not found
            // I don't get this test. Why should I do a count(*) and then order it by a field
            // that is actually not in the resultset?
            //new ReadTest().C4_CountWithOrderBy();

            // No clue, what this test does
            //new ReadTest().D12_SelectChildCustomer();

            // Method All seems not to be mapped
            // in ExpressionTreeParser.cs
            //L274: Unprepared to map method All (c.Orders.Select(o => o).All(o => (o.ShipCity = c.City))) to SQL
            //new ReadTest_Complex().O1_OperatorAll();

            // SELECT ((p$.productname||?)||VARCHAR(p$.supplierid)) FROM linquser.products p$
            //new ReadTest_Operands().H1_SelectConcat();

            /*=======================================================*/
            /* These 101 Tests fail for various reasons, which look  */
            /* more DBLinq-Core related and not Ingres related       */
            /* I'm not even sure if those are supposed to be running */
            /*=======================================================*/
            //new Linq_101_Samples.AdvancedTest().LinqToSqlAdvanced06();
            //new Linq_101_Samples.Count_Sum_Min_Max_Avg().LinqToSqlCount07();
            //new Linq_101_Samples.Count_Sum_Min_Max_Avg().LiqnToSqlCount02();
            //new Linq_101_Samples.Count_Sum_Min_Max_Avg().LinqToSqlCount10();
            //new Linq_101_Samples.Join().LinqToSqlJoin03();
            //new Linq_101_Samples.Join().LinqToSqlJoin10();
            //new Linq_101_Samples.String_Date_functions().LinqToSqlString01();
            //new Linq_101_Samples.Top_Bottom().LinqToSqlTop02();
            //new Linq_101_Samples.Top_Bottom().LinqToSqlTop03_Ex_Andrus();
        }
    }
}

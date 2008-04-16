#region MIT license
// 
// Copyright (c) 2007-2008 Jiri Moudry
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion
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
            
            // The query yields exactly one result, still the exception is thrown.
            // Looks like a bug in linq
            //new ReadTest_Operands().J1_LocalFunction_DateTime_ParseExact();

            // SELECT COUNT(*) FROM linquser.products p$ ORDER BY p$.productid
            // productid not found
            // I don't get this test. Why should I do a count(*) and then order it by a field
            // that is actually not in the resultset?
            //new ReadTest().C4_CountWithOrderBy();

            // The parameters are added to the query in reverse order. Ingres can't handle this,
            // as all params are called "?"
            //new ReadTest_Complex().D2_SelectPensByLocalPropertyAndConstant();

            // Yields this SQL:
            // SELECT ((p$.productname||?)||VARCHAR(p$.supplierid)) FROM linquser.products p$
            // A parameter before the FROM crashes the Ingres driver. Known problem.
            // Both tests fail due to this
            //new ReadTest_Operands().H1_SelectConcat();
            //new Linq_101_Samples.String_Date_functions().LinqToSqlString01();

            // NullRef exception in FromClausebuilder
            //new Linq_101_Samples.Join().LinqToSqlJoin10();

            // OFFSET clause is not supported in Ingres
            // Both tests fail due to this.
            //new Linq_101_Samples.Top_Bottom().LinqToSqlTop02();
            //new Linq_101_Samples.Top_Bottom().LinqToSqlTop03_Ex_Andrus();

            // Generates faulty SQL
            //new WriteTest().G11_TwoSequencesInTable();

            // Generates faulty SQL
            //new WriteTest().G12_EmptyInsertList();
        }
    }
}

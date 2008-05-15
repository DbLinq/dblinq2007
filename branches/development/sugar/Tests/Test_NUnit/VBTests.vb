Option Strict On

Imports NUnit.Framework
Imports nwind
Imports Test_NUnit

<TestFixture()> _
Public Class VBTests
    Inherits TestBase

    <Test()> _
    Sub VB1_ConvertChecked()
        Dim nw As Northwind = CreateDB()
        Dim name = (From p In nw.Products Where p.ProductID = 1 Select p.ProductName).Single
        Assert.AreEqual(name, "Pen")
    End Sub

    <Test()> _
    Sub VB2_CompareString_Equals_0()
        Dim nw As Northwind = CreateDB()
        Dim id = (From p In nw.Products Where p.ProductName = "Pen" Select p.ProductID).Single
        Assert.AreEqual(id, 1)
    End Sub

    <Test()> _
    Sub VB3_LikeString_Equals_0()
        Dim nw As Northwind = CreateDB()
        Dim id = (From p In nw.Products Where p.ProductName Like "Pe" Select p.ProductID).Single
        Assert.AreEqual(id, 1)
    End Sub

End Class

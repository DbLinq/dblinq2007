using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using nwind;

namespace Test_NUnit
{
    public class CompositePK_Test : TestBase
    {
        [Test]
        public void G9_UpdateTableWithCompositePK()
        {
            Northwind db = CreateDB();
            var orderDetail = new OrderDetail
            {
                OrderID = 3,
                ProductID = 2,
                UnitPrice = 20
            };

            db.OrderDetails.Add(orderDetail);
            db.SubmitChanges();

            orderDetail.UnitPrice = 40;
            db.SubmitChanges();

            orderDetail = (from c in db.OrderDetails
                           where c.UnitPrice == 40
                           select c).Single();

            Assert.AreEqual(3, orderDetail.OrderID);
            Assert.AreEqual(2, orderDetail.ProductID);
            Assert.AreEqual(40, orderDetail.UnitPrice);

            db.OrderDetails.Remove(orderDetail);
            db.SubmitChanges();
        }

        [Test]
        public void G10_DeleteTableWithCompositePK()
        {
            Northwind db = CreateDB();

            var orderDetail = new OrderDetail { OrderID = 3, ProductID = 2 };
            db.OrderDetails.Add(orderDetail);
            db.SubmitChanges();

            Assert.AreEqual(db.OrderDetails.Count(), 2);
            db.OrderDetails.Remove(orderDetail);
            db.SubmitChanges();

            Assert.AreEqual(db.OrderDetails.Count(), 1);
        }

        [Test]
        public void G11_UnchangedColumnShouldNotUpdated()
        {
            Northwind db = CreateDB();
            var orderDetail = new OrderDetail { OrderID = 1, ProductID = 2 };
            db.OrderDetails.Attach(orderDetail, false);
            orderDetail.Discount = 15;
            db.SubmitChanges();
            Assert.AreEqual(db.OrderDetails.Single().UnitPrice, 33);
        }
    }
}

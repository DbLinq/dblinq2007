using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using nwind;

namespace Test_NUnit
{
    [TestFixture]
    public class CompositePK_Test : TestBase
    {
        [Test]
        public void G9_UpdateTableWithCompositePK()
        {
            Northwind db = CreateDB();
            try
            {
                db.ExecuteCommand("DELETE FROM `order details` WHERE OrderID=3 AND ProductID=2");
            }
            catch (Exception)
            {
            }

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

            OrderDetail orderDetail2 = (from c in db.OrderDetails
                                        where c.UnitPrice == 40
                                        select c).Single();

            Assert.IsTrue(object.ReferenceEquals(orderDetail, orderDetail2), "Must be same object");

            Assert.AreEqual(3, orderDetail2.OrderID);
            Assert.AreEqual(2, orderDetail2.ProductID);
            Assert.AreEqual(40, orderDetail2.UnitPrice);

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
            Random rand = new Random();

            Northwind db = CreateDB();
            var orderDetail = new OrderDetail { OrderID = 1, ProductID = 2 };
            db.OrderDetails.Attach(orderDetail, false);

            float newDiscount = 15 + (float)rand.NextDouble();
            orderDetail.Discount = newDiscount;
            db.SubmitChanges();

            var orderDetail2 = db.OrderDetails.Single();
            Assert.AreEqual(orderDetail2.Discount, newDiscount);
        }
    }
}

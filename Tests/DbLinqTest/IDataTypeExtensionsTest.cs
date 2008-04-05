using System.Collections.Generic;
using DbLinq.Schema;
using DbLinq.Schema.Dbml;
using DbLinq.Util;
using DbLinq.Vendor;
using DbLinq.Vendor.Implementation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace DbLinqTest
{


    /// <summary>
    ///This is a test class for SchemaLoaderTest and is intended
    ///to contain all SchemaLoaderTest Unit Tests
    ///</summary>
    [TestClass()]
    public class IDataTypeExtensionsTest
    {

        /// <summary>
        ///A test for UnpackDbType
        ///</summary>
        [TestMethod()]
        public void UnpackDbType1Test()
        {
            string rawType = "int";
            IDataType dataType = new SchemaLoader.DataType();
            dataType.UnpackRawDbType(rawType);
            Assert.AreEqual("int", dataType.Type);
            Assert.AreEqual(null, dataType.Length);
            Assert.AreEqual(null, dataType.Precision);
            Assert.AreEqual(null, dataType.Scale);
            //Assert.AreEqual(null, dataType.Unsigned); // irrelevant
        }

        /// <summary>
        ///A test for UnpackDbType
        ///</summary>
        [TestMethod()]
        public void UnpackDbType2Test()
        {
            string rawType = "int(12)";
            IDataType dataType = new SchemaLoader.DataType();
            dataType.UnpackRawDbType(rawType);
            Assert.AreEqual("int", dataType.Type);
            Assert.AreEqual(12, dataType.Length);
            Assert.AreEqual(12, dataType.Precision);
            Assert.AreEqual(null, dataType.Scale);
            //Assert.AreEqual(null, dataType.Unsigned); // irrelevant
        }

        /// <summary>
        ///A test for UnpackDbType
        ///</summary>
        [TestMethod()]
        public void UnpackDbType3Test()
        {
            string rawType = "number(15,5)";
            IDataType dataType = new SchemaLoader.DataType();
            dataType.UnpackRawDbType(rawType);
            Assert.AreEqual("number", dataType.Type);
            Assert.AreEqual(15, dataType.Length);
            Assert.AreEqual(15, dataType.Precision);
            Assert.AreEqual(5, dataType.Scale);
            Assert.AreEqual(false, dataType.Unsigned);
        }

        /// <summary>
        ///A test for UnpackDbType
        ///</summary>
        [TestMethod()]
        public void UnpackDbType4Test()
        {
            string rawType = "type()";
            IDataType dataType = new SchemaLoader.DataType();
            dataType.UnpackRawDbType(rawType);
            Assert.AreEqual("type", dataType.Type);
            Assert.AreEqual(null, dataType.Length);
            Assert.AreEqual(null, dataType.Precision);
            Assert.AreEqual(null, dataType.Scale);
        }

        /// <summary>
        ///A test for UnpackDbType
        ///</summary>
        [TestMethod()]
        public void UnpackDbType5Test()
        {
            string rawType = "smallint unsigned";
            IDataType dataType = new SchemaLoader.DataType();
            dataType.UnpackRawDbType(rawType);
            Assert.AreEqual("smallint", dataType.Type);
            Assert.AreEqual(null, dataType.Length);
            Assert.AreEqual(null, dataType.Precision);
            Assert.AreEqual(null, dataType.Scale);
            Assert.AreEqual(true, dataType.Unsigned);
        }
    }
}

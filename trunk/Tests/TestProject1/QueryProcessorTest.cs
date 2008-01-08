using DBLinq.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq.Expressions;

namespace TestProject1
{
    
    
    /// <summary>
    ///This is a test class for QueryProcessorTest and is intended
    ///to contain all QueryProcessorTest Unit Tests
    ///</summary>
    [TestClass()]
    public class QueryProcessorTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for processJoinClause
        ///</summary>
        [TestMethod()]
        [DeploymentItem("DbLinq.Mysql.Prototype.dll")]
        public void processJoinClauseTest()
        {
            PrivateObject param0 = null; // TODO: Initialize to an appropriate value
            QueryProcessor_Accessor target = new QueryProcessor_Accessor(param0); // TODO: Initialize to an appropriate value
            MethodCallExpression joinExpr = null; // TODO: Initialize to an appropriate value
            target.processJoinClause(joinExpr);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for processGroupJoin
        ///</summary>
        [TestMethod()]
        [DeploymentItem("DbLinq.Mysql.Prototype.dll")]
        public void processGroupJoinTest()
        {
            PrivateObject param0 = null; // TODO: Initialize to an appropriate value
            QueryProcessor_Accessor target = new QueryProcessor_Accessor(param0); // TODO: Initialize to an appropriate value
            MethodCallExpression exprCall = null; // TODO: Initialize to an appropriate value
            target.processGroupJoin(exprCall);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }
    }
}

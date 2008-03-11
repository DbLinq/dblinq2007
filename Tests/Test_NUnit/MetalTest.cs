using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Test_NUnit
{
    [TestFixture]
    public class MetalTest
    {
        static string GetSqlMetalPath()
        {
            string path = "../../../SqlMetal/bin/SqlMetal.exe";
            if (!File.Exists(path))
                throw new NUnit.Framework.IgnoreException("SqlMetal not found");
            return path;
        }

        static string GetCompilerPath()
        {
            string windowsDir = Environment.GetEnvironmentVariable("SystemRoot");
            string frameworkDir = Path.Combine(windowsDir, "Microsoft.Net/Framework/v3.5");
            if (!Directory.Exists(frameworkDir))
                throw new NUnit.Framework.IgnoreException("Framework dir not found");

            string cscExe = Path.Combine(frameworkDir, "csc.exe");
            if (!File.Exists(cscExe))
                throw new NUnit.Framework.IgnoreException("csc.exe not found in framework dir");
            return cscExe;
        }

        [Test]
        public void GenerateFromDbml()
        {
            //1. run SqlMetal
            string sqlMetal = GetSqlMetalPath();
            string currDir = Directory.GetCurrentDirectory();
            string mysqlExampleDir = "../../../Example/DbLinq.Mysql.Example/nwind";
            bool ok = Directory.Exists(mysqlExampleDir);
            string args1 = " -provider=MySql -namespace:nwind -code:Northwind.cs -sprocs Northwind_from_mysql.dbml";
            Process p = Process.Start(sqlMetal, args1);
            bool exitOk = p.WaitForExit(5000);
            Assert.IsTrue(exitOk, "Expeceted SqlMetal to exit cleanly");

            string cscExe = GetCompilerPath();
            //TODO
            //string args2 = "";
            //Process.Start()

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Globalization;
using System.IO;

namespace TestReporter
{
    class Program
    {
        static void Main(string[] args)
        {
            var testFile = args.Length > 0 ? args[0] : @"..\..\..\..\..\TestResult.xml";
            var inputFile = args.Length > 1 ? args[1] : @"..\..\..\..\..\..\wiki\Tests.wiki";
            var outputFile = args.Length > 2 ? args[2] : inputFile;
            if (!File.Exists(testFile)) throw new ArgumentException("Test file '" + testFile + "' does not exist");
            if (!File.Exists(inputFile)) throw new ArgumentException("Input file '" + testFile + "' does not exist");
            if (!File.Exists(outputFile)) throw new ArgumentException("Output file '" + testFile + "' does not exist");

            // Maps assembly file names to table names and column titles
            var fileList = new[] {
                new { File = "DbLinq.SqlServer_test_strict", Group = "vendor", Name = "Linq to SQL" },
                new { File = "DbLinq.SqlServer_test", Group = "vendor", Name = "Sql Server" },
                new { File = "DbLinq.MySql_test", Group = "vendor", Name = "MySQL" },
                new { File = "DbLinq.Oracle_test", Group = "vendor", Name = "Oracle" },
                new { File = "DbLinq.Oracle_test_odp", Group = "vendor", Name = "Oracle ODP" },
                new { File = "DbLinq.Sqlite_test", Group = "vendor", Name = "SQLite" },
                new { File = "DbLinq.Sqlite_test_mono", Group = "vendor", Name = "SQLite (Mono)" },
                new { File = "DbLinq.Sqlite_test_mono_strict", Group = "-", Name = "SQLite (Mono, strict)" }, // Disabled
                new { File = "DbLinq.PostgreSql_test", Group = "vendor", Name = "pgSQL" },
                new { File = "DbLinq.Ingres_test", Group = "vendor", Name = "Ingres" },
                new { File = "DbLinq.Firebird_test", Group = "vendor", Name = "Firebird" },
                new { File = "DbLinq_test", Group = "strict", Name = "DbLinq" },
                new { File = "DbLinq_test_ndb_strict", Group = "strict", Name = "Strict" },
                new { File = "", Group = "shared", Name = "Shared" } // Used for all other files
            };
            var files = fileList.ToDictionary(f => f.File, f => new { f.Group, f.Name });

            // Load all tests
            var d = XDocument.Load(testFile);
            var tests = d.Root
                .Descendants("test-case")
                .Select(e => new
                {
                    Name = (string)e.Attribute("name"),
                    Success = (string)e.Attribute("success"),
                    Description = (string)e.Attribute("description"),
                    //Assembly = e.Ancestors("test-suite").Select(s => (string)s.Attribute("name")).Where(n => n != null && n.StartsWith(@"Test_NUnit_")).FirstOrDefault(),
                    File = e.Ancestors("test-suite")
                    .Select(s => (string)s.Attribute("name"))
                    .Where(n => n != null && (n.Contains('\\') || n.Contains('/')) && (n.EndsWith(@".dll") || n.EndsWith(@".exe")))
                    .Select(n => n.Substring(n.LastIndexOfAny(new[] { '\\', '/' }) + 1))
                    .Select(n => n.Substring(0, n.Length - 4))
                    .FirstOrDefault(),
                })
                .Select(c => new
                {
                    c.Description,
                    File = files.ContainsKey(c.File ?? "") ? files[c.File ?? ""] : files[""],
                    Name = files.ContainsKey(c.File ?? "") ? c.Name.Substring(c.Name.IndexOf(".") + 1): c.Name,
                    c.Success,
                })
                .ToList();

            // Calculate aggregates
            var totals = files.Values.ToDictionary(k => k, k => 0);
            var failures = files.Values.ToDictionary(k => k, k => 0);
            foreach (var t in tests)
            {
                if (t.Success == "True" || t.Success == "False") totals[t.File]++;
                if (t.Success == "False") failures[t.File]++;
            }

            // Load template
            var template = "";
            using (var sr = new System.IO.StreamReader(inputFile))
                template = sr.ReadToEnd();

            // Replace tags
            template = Regex.Replace(template, @"(<wiki:comment>date</wiki:comment>)[0-9/-]*",
                new MatchEvaluator(m => m.Groups[1].Value + DateTime.Now.ToString("yyyy-MM-dd", new CultureInfo("en-US"))));

            template = Regex.Replace(template, @"(<wiki:comment>([a-z]+)-tests</wiki:comment>\r?\n)(?:\|\|.*\|\|\r?\n)*",
                new MatchEvaluator(m => {
                    var sb = new StringBuilder();
                    sb.Append(m.Groups[1].Value);
                    var group = m.Groups[2].Value;

                    var tempFiles = fileList.Where(s => s.Group == group).Select(s => files[s.File]).Distinct().ToArray();
                    var groupCount = tests.Where(t => t.File.Group == group).Select(t => t.Name).Distinct().Count();

                    sb.AppendLine("||Test||" +
                        string.Join("||", tempFiles.Select(v => v.Name).ToArray()) + "||");
                    sb.AppendLine("||Total||" +
                        string.Join("||", tempFiles.Select(v => totals[v].ToString()).ToArray()) + "||");
                    sb.AppendLine("||Failures||" +
                        string.Join("||", tempFiles.Select(v => failures[v].ToString()).ToArray()) + "||");
                    sb.AppendLine("||Untested||" +
                        string.Join("||", tempFiles.Select(v => (groupCount - totals[v]).ToString()).ToArray()) + "||");

                    foreach (var t in tests
                        .Where(t => t.File.Group == group)
                        .GroupBy(t => new { t.Name, t.Description },
                                 (t, ts) => new { t.Name, t.Description, Results = ts.Select(r => new { r.File, r.Success }).ToArray() })
                        .Where(t => t.Results.Count(r => r.Success == "True") != tempFiles.Length))
                    {
                        sb.AppendLine("||`" + t.Name + "`" +
                            (!string.IsNullOrEmpty(t.Description) ? "<br><font color=#999999 size=1>" + t.Description : "") +
                            "||" + string.Join("||", tempFiles.GroupJoin(t.Results, s => s, r => r.File, (s, rs) => FormatSucces(rs.Select(r => r.Success).SingleOrDefault())).ToArray()) + "||");
                    }
                    return sb.ToString();
                }));

            // Save result
            using (var wr = new System.IO.StreamWriter(outputFile))
                wr.Write(template);
        }

        private static string FormatSucces(string s)
        {
            return s == "True" ? "<font color=#009900>OK" :
                (s == "False" ? "<font color=#990000>FAIL" :
                (s == "" ? "<font color=#999999>-" :
                "<font color=#999999>?"));
        }
    }
}

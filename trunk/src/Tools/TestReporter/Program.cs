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
        class Column
        {
            public string Group;
            public string ColName;
        }

        class RawTest
        {
            public Column File;
            public string Name;
            public string Description;
            public string Success;
        }

        static IEnumerable<KeyValuePair<string, string>> ParseArgs(string[] args)
        {
            string param = null;
            foreach (var arg in args)
            {
                if (arg.StartsWith("/") || arg.StartsWith("-"))
                {
                    if (param != null)
                        yield return new KeyValuePair<string, string>(param, null);
                    param = arg.Substring(1);
                    continue;
                }
                if (param != null)
                    yield return new KeyValuePair<string, string>(param, arg);
                param = null;
            }
            if (param != null)
                yield return new KeyValuePair<string, string>(param, null);
        }

        /// <summary>
        /// Simple parser to convert the wiki syntax used by google to html.
        /// Only meant to make it easy to use the generated reports locally.
        /// </summary>
        private static string WikiToHtml(string s)
        {
            // * ... => <ul><li>...</li></ul>
            s = Regex.Replace(s, @"^( *)\*\s*(.*?)(\r?)$", new MatchEvaluator(m =>
                m.Groups[1].Value.Replace(" ", "<ul>") +
                "<li>" + m.Groups[2].Value + "</li>" +
                m.Groups[1].Value.Replace(" ", "</ul>") + m.Groups[3].Value), RegexOptions.Multiline);
            for (var i = 0; i < 10; i++) s = Regex.Replace(s, @"</ul>(\r?\n)<ul>", "$1");

            // ||...||...|| => <table><tr><td>...</td><td>...</td></tr></table>
            s = Regex.Replace(s, @"^\|\|(.*)\|\|(\r?)$", new MatchEvaluator(m =>
                "<table border><tr><td>"
                + m.Groups[1].Value.Replace("||", "</td><td>")
                + "</td></tr></table>" + m.Groups[2].Value), RegexOptions.Multiline);
            s = Regex.Replace(s, @"</table>(\r?\n)<table border>", "$1");

            // = ... => <h1>...</h1>
            s = Regex.Replace(s, @"^(={1,6})(?!=)(.*)(?<!=)\1(\r?)$", new MatchEvaluator(m =>
                "<h" + m.Groups[1].Value.Length + ">" +
                m.Groups[2].Value +
                "</h" + m.Groups[1].Value.Length + ">" + m.Groups[3].Value), RegexOptions.Multiline);

            s = Regex.Replace(s, @"^([ \t]*\S.*?(?:\r?\n^[ \t]*\S.*?)*)\r?$", "<p>$1</p>", RegexOptions.Multiline);

            // `...` => <tt>...</tt>, *...* => <b>...</b>, <wiki:comment>...</wiki:comment> => <!--...-->
            s = Regex.Replace(s, @"`([^`\n]*?)`", "<tt>$1</tt>");
            s = Regex.Replace(s, @"\*([^*\n]*?)\*", "<b>$1</b>");
            s = Regex.Replace(s, @"<wiki:comment>(.*?)</wiki:comment>", "<!--$1-->");

            return "<style>body,td,th{font-family:arial,sans-serif; font-size:83%;}</style>" + s;
        }

        private static string FormatSucces(string s)
        {
            return s == "True" ? "<font color=#009900>OK" :
                (s == "False" ? "<font color=#990000>FAIL" :
                (s == "" ? "<font color=#999999>-" :
                "<font color=#999999>?"));
        }

        /// <summary>
        /// Loads all tests from testFile, categorizing them based one the assembly
        /// </summary>
        private static List<RawTest> LoadTests(string testFile, Dictionary<string, Column> files)
        {
            var tests = XDocument.Load(testFile).Root
                .Descendants("test-case")
                .Select(e => new
                {
                    Name = (string)e.Attribute("name"),
                    Success = (string)e.Attribute("success") ?? "",
                    Description = (string)e.Attribute("description"),
                    //Assembly = e.Ancestors("test-suite").Select(s => (string)s.Attribute("name")).Where(n => n != null && n.StartsWith(@"Test_NUnit_")).FirstOrDefault(),
                    File = e.Ancestors("test-suite")
                    .Select(s => (string)s.Attribute("name"))
                    .Where(n => n != null && (n.Contains('\\') || n.Contains('/')) && (n.EndsWith(@".dll") || n.EndsWith(@".exe")))
                    .Select(n => n.Substring(n.LastIndexOfAny(new[] { '\\', '/' }) + 1))
                    .Select(n => n.Substring(0, n.Length - 4))
                    .FirstOrDefault(),
                })
                .Select(c => new RawTest
                {
                    File = files.ContainsKey(c.File ?? "") ? files[c.File ?? ""] : files[""],
                    Name = files.ContainsKey(c.File ?? "") ? c.Name.Substring(c.Name.IndexOf(".") + 1) : c.Name,
                    Description = c.Description,
                    Success = c.Success,
                })
                .ToList();
            return tests;
        }

        static void Main(string[] args)
        {
            string testFile = null, testFileBase = null, wikiInputFile = null, wikiOutputFile = null;
            string htmlOutputFile = null;
            bool onlyFailures = false, onlyChanges = false;
            bool ignoreMissing = false;
            foreach (var opt in ParseArgs(args))
            {
                if (opt.Key == "tr") testFile = opt.Value ?? "";
                else if (opt.Key == "trb") testFileBase = opt.Value ?? "";
                else if (opt.Key == "wi") wikiInputFile = opt.Value ?? "";
                else if (opt.Key == "wo") wikiOutputFile = opt.Value ?? "";
                else if (opt.Key == "ho") htmlOutputFile = opt.Value ?? "";
                else if (opt.Key == "of") onlyFailures = true;
                else if (opt.Key == "oc") onlyChanges = true;
                else if (opt.Key == "im") ignoreMissing = true;
                else throw new ArgumentException("Unknown argument '" + opt.Key + "'");
            }
            testFile = testFile ?? @"..\..\..\..\..\TestResult.xml";
            wikiInputFile = wikiInputFile ?? @"..\..\..\..\..\..\wiki\Tests.wiki";
            if (wikiOutputFile == null && htmlOutputFile == null) wikiOutputFile = wikiInputFile;
            if (!File.Exists(testFile)) throw new ArgumentException("Test file '" + testFile + "' does not exist");
            if (!File.Exists(wikiInputFile)) throw new ArgumentException("Input file '" + wikiInputFile + "' does not exist");

            // Maps assembly file names to table names and column titles
            var fileList = new[] {
                new { File = "DbLinq.SqlServer_test_strict", Group = "vendor", ColName = "Linq to SQL" },
                new { File = "DbLinq.SqlServer_test", Group = "vendor", ColName = "Sql Server" },
                new { File = "DbLinq.MySql_test", Group = "vendor", ColName = "MySQL" },
                new { File = "DbLinq.Oracle_test", Group = "vendor", ColName = "Oracle" },
                new { File = "DbLinq.Oracle_test_odp", Group = "vendor", ColName = "Oracle ODP" },
                new { File = "DbLinq.Sqlite_test", Group = "vendor", ColName = "SQLite" },
                new { File = "DbLinq.Sqlite_test_mono", Group = "vendor", ColName = "SQLite (Mono)" },
                new { File = "DbLinq.Sqlite_test_mono_strict", Group = "-", ColName = "SQLite (Mono, strict)" }, // Disabled
                new { File = "DbLinq.PostgreSql_test", Group = "vendor", ColName = "pgSQL" },
                new { File = "DbLinq.Ingres_test", Group = "vendor", ColName = "Ingres" },
                new { File = "DbLinq.Firebird_test", Group = "vendor", ColName = "Firebird" },
                new { File = "DbLinq_test", Group = "strict", ColName = "DbLinq" },
                new { File = "DbLinq_test_ndb_strict", Group = "strict", ColName = "Strict" },
                new { File = "", Group = "shared", ColName = "Shared" } // Used for all other files
            };
            var files = fileList.ToDictionary(f => f.File, f => new Column{ Group = f.Group, ColName = f.ColName });

            var currentTests = LoadTests(testFile, files);
            var testsBase = testFileBase != null ? LoadTests(testFileBase, files) : new List<RawTest>();

            var allTests = currentTests.Select(t => new { t.File.Group, t.File.ColName, t.Name, t.Description, t.Success, SuccessBase = (string)null })
            .Union(testsBase.Select(t => new { t.File.Group, t.File.ColName, t.Name, t.Description, Success = (string)null, SuccessBase = t.Success }))
            .GroupBy(t => t.Group, (g, ts) => new { g, ts }).ToDictionary(t => t.g, t => t.ts
                .GroupBy(t2 => t2.Name, (n, ts) => new { n, ts }).ToDictionary(t2 => t2.n, t2 =>
                    new
                    {
                        Description = t2.ts.Max(t3 => t3.Description),
                        Results = t2.ts.GroupBy(t3 => t3.ColName, (cn, ts) => new { cn, ts }).ToDictionary(t4 => t4.cn, t4 => new
                        {
                            Success = t4.ts.Select(t5 => t5.Success).Where(s => s != null).SingleOrDefault(),
                            SuccessBase = t4.ts.Select(t5 => t5.SuccessBase).Where(s => s != null).SingleOrDefault()
                        })
                    }
                )
            );
            // Calculate aggregates
            var testAgg = allTests.ToDictionary(t => t.Key, t => t.Value
                .SelectMany(t2 => t2.Value.Results.Select(t3 => t3))
                .GroupBy(t2 => t2.Key, (cn, ts) => new
            {
                ColName = cn,
                Total = ts.Count(t2 => t2.Value.Success == "True" || t2.Value.Success == "False"),
                Failures = ts.Count(t2 => t2.Value.Success == "False")
            })).SelectMany(t => t.Value, (g, t) => new { Group = g.Key, t.ColName, t.Total, t.Failures });

            // Load template
            var template = "";
            using (var sr = new System.IO.StreamReader(wikiInputFile))
                template = sr.ReadToEnd();

            // Replace tags
            template = Regex.Replace(template, @"(<wiki:comment>date</wiki:comment>)[0-9/-]*",
                new MatchEvaluator(m => m.Groups[1].Value + DateTime.Now.ToString("yyyy-MM-dd", new CultureInfo("en-US"))));

            template = Regex.Replace(template, @"(<wiki:comment>([a-z]+)-tests</wiki:comment>\r?\n)(?:\|\|.*\|\|\r?\n)*",
                new MatchEvaluator(m => {
                    var sb = new StringBuilder();
                    sb.Append(m.Groups[1].Value);
                    var group = m.Groups[2].Value;

                    var tempFiles = fileList.Where(s => s.Group == group).Select(s => files[s.File]).Distinct()
                        .GroupJoin(testAgg.Where(t => t.Group == group), f => f.ColName, t => t.ColName, (c, ts) => new
                        {
                            c.ColName,
                            Total = ts.Select(t => t.Total).SingleOrDefault(),
                            Failures = ts.Select(t => t.Failures).SingleOrDefault()
                        }).ToArray();
                    var groupCount = allTests.ContainsKey(group)? allTests[group].Count: 0;

                    sb.AppendLine("||*Test*||*" + string.Join("*||*", tempFiles.Select(v => v.ColName).ToArray()) + "*||");
                    sb.AppendLine("||*Total*||" + string.Join("||", tempFiles.Select(v => v.Total.ToString()).ToArray()) + "||");
                    sb.AppendLine("||*Failures*||" + string.Join("||", tempFiles.Select(v => v.Failures.ToString()).ToArray()) + "||");
                    sb.AppendLine("||*Untested*||" + string.Join("||", tempFiles.Select(v => (groupCount - v.Total).ToString()).ToArray()) + "||");

                    if (allTests.ContainsKey(group))
                    foreach (var t in allTests[group]
                        .Where(t => !onlyFailures || t.Value.Results.Count(r => r.Value.Success == "True") != tempFiles.Length)
                        .Where(t => !onlyChanges || (testFileBase != null && t.Value.Results.Any(r => r.Value.Success != r.Value.SuccessBase && r.Value.Success != null)))
                        .OrderBy(t => t.Key))
                    {
                        var desc = t.Value.Description;
                        sb.AppendLine("||`" + t.Key + "`" +
                            (!string.IsNullOrEmpty(desc) ? "<br><font color=#999999 size=1>" + desc : "") +
                            "||" + string.Join("||", tempFiles
                                .GroupJoin(t.Value.Results, s => s.ColName, r => r.Key, (s, rs) => new
                                {
                                    Success = rs.Select(r => r.Value.Success).SingleOrDefault(),
                                    SuccessBase = rs.Select(r => r.Value.SuccessBase).SingleOrDefault(),
                                })
                                .Select(r => testFileBase == null || r.Success == r.SuccessBase ? FormatSucces(r.Success) :
                                    (ignoreMissing && r.Success == null ? FormatSucces(r.SuccessBase) :
                                    "*" + FormatSucces(r.SuccessBase) + "</font> > " + FormatSucces(r.Success) + "*")).ToArray()) + "||");
                    }
                    return sb.ToString();
                }));

            // Save result
            if (wikiOutputFile != null)
                using (var wr = new System.IO.StreamWriter(wikiOutputFile))
                    wr.Write(template);
            if (htmlOutputFile != null)
                using (var wr = new System.IO.StreamWriter(htmlOutputFile))
                    wr.Write(WikiToHtml(template));
        }
    }
}

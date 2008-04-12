#region MIT license
////////////////////////////////////////////////////////////////////
// MIT license:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//
// Authors:
//        Jiri George Moudry
////////////////////////////////////////////////////////////////////
#endregion

using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Configuration;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using DbLinq.Util;
using DbMetal.Utility;

namespace DbMetal
{
    /// <summary>
    /// Parameters base class.
    /// Allows to specify direct switches or place switches in a file (specified with @fileName).
    /// If a file specifies several line, the parameters will allow batch processing, one per line.
    /// Parameters specified before the @ file are inherited by each @ file line
    /// </summary>
    public abstract class AbstractParameters
    {
        /// <summary>
        /// Describes a switch (/sprocs)
        /// </summary>
        public class OptionAttribute : Attribute
        {
            /// <summary>
            /// Allows to specify a group. All options in the same group are displayed together
            /// </summary>
            public int Group { get; set; }

            /// <summary>
            /// Description
            /// </summary>
            public string Text { get; set; }

            /// <summary>
            /// Value name, used for help
            /// </summary>
            public string ValueName { get; set; }

            public OptionAttribute(string text)
            {
                Text = text;
            }
        }

        /// <summary>
        /// Describes an input file
        /// </summary>
        public class FileAttribute : Attribute
        {
            /// <summary>
            /// Tells if the file is required
            /// TODO: add mandatory support in parameters check
            /// </summary>
            public bool Mandatory { get; set; }
            /// <summary>
            /// The name written in help
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// Descriptions
            /// </summary>
            public string Text { get; set; }

            public FileAttribute(string name, string text)
            {
                Name = name;
                Text = text;
            }
        }

        public class AlternateAttribute : Attribute
        {
            public string Name { get; set; }

            public AlternateAttribute(string name)
            {
                Name = name;
            }
        }

        public readonly IList<string> Extra = new List<string>();

        private bool IsParameter(string arg, string switchPrefix, out string parameterName, out string parameterValue)
        {
            bool isParameter;
            if (arg.StartsWith(switchPrefix))
            {
                isParameter = true;
                string nameValue = arg.Substring(switchPrefix.Length);
                int separator = nameValue.IndexOfAny(new char[] { ':', '=' });
                if (separator >= 0)
                {
                    parameterName = nameValue.Substring(0, separator);
                    parameterValue = nameValue.Substring(separator + 1).Trim('\"');
                }
                else if (nameValue.EndsWith("+"))
                {
                    parameterName = nameValue.Substring(0, nameValue.Length - 1);
                    parameterValue = "+";
                }
                else if (nameValue.EndsWith("-"))
                {
                    parameterName = nameValue.Substring(0, nameValue.Length - 1);
                    parameterValue = "-";
                }
                else if (nameValue.StartsWith("no-"))
                {
                    parameterName = nameValue.Substring(3);
                    parameterValue = "-";
                }
                else
                {
                    parameterName = nameValue;
                    parameterValue = null;
                }
            }
            else
            {
                isParameter = false;
                parameterName = null;
                parameterValue = null;
            }
            return isParameter;
        }

        protected bool IsParameter(string arg, out string parameterName, out string parameterValue)
        {
            return IsParameter(arg, "--", out parameterName, out parameterValue)
                   || IsParameter(arg, "-", out parameterName, out parameterValue)
                   || IsParameter(arg, "/", out parameterName, out parameterValue);
        }

        protected object GetValue(string value, Type targetType)
        {
            object typedValue;
            if (typeof(bool).IsAssignableFrom(targetType))
            {
                if (value == null || value == "+")
                    typedValue = true;
                else if (value == "-")
                    typedValue = false;
                else
                    typedValue = Convert.ToBoolean(value);
            }
            else
            {
                typedValue = Convert.ChangeType(value, targetType);
            }
            return typedValue;
        }

        /// <summary>
        /// Assigns a parameter by reflection
        /// </summary>
        /// <param name="name">parameter name (case insensitive)</param>
        /// <param name="value">parameter value</param>
        protected void SetParameter(string name, string value)
        {
            // cleanup and evaluate
            name = name.Trim();
            // evaluate
            value = value.EvaluateEnvironment();

            Type thisType = GetType();
            BindingFlags flags = BindingFlags.IgnoreCase | BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public;
            FieldInfo fieldInfo = thisType.GetField(name, flags);
            if (fieldInfo != null)
                fieldInfo.SetValue(this, GetValue(value, fieldInfo.FieldType));
            else
            {
                PropertyInfo propertyInfo = thisType.GetProperty(name, flags);
                if (propertyInfo != null)
                    propertyInfo.GetSetMethod().Invoke(this, new[] { GetValue(value, propertyInfo.PropertyType) });
                else
                    throw new ArgumentException(string.Format("Parameter {0} does not exist", name));
            }
        }

        /// <summary>
        /// Loads arguments from a given list
        /// </summary>
        /// <param name="args"></param>
        public void Load(IList<string> args)
        {
            // picrap: default values are unsafe, we should try something else here instead of conf.
            var configurationParameters = (NameValueCollection)ConfigurationManager.GetSection("parameters");
            foreach (string key in configurationParameters.AllKeys)
            {
                SetParameter(key, configurationParameters[key]);
            }
            foreach (string arg in args)
            {
                string key, value;
                if (IsParameter(arg, out key, out value))
                    SetParameter(key, value);
                else
                    Extra.Add(arg);
            }
        }

        public AbstractParameters()
        {
        }

        public AbstractParameters(IList<string> args)
        {
            Load(args);
        }

        /// <summary>
        /// Internal method allowing to extract arguments and specify quotes characters
        /// </summary>
        /// <param name="commandLine"></param>
        /// <param name="quotes"></param>
        /// <returns></returns>
        public IList<string> ExtractArguments(string commandLine, char[] quotes)
        {
            var arg = new StringBuilder();
            var args = new List<string>();
            const char zero = '\0';
            char quote = zero;
            foreach (char c in commandLine)
            {
                if (quote == zero)
                {
                    if (quotes.Contains(c))
                        quote = c;
                    else if (char.IsSeparator(c) && quote == zero)
                    {
                        if (arg.Length > 0)
                        {
                            args.Add(arg.ToString());
                            arg = new StringBuilder();
                        }
                    }
                    else
                        arg.Append(c);
                }
                else
                {
                    if (c == quote)
                        quote = zero;
                    else
                        arg.Append(c);
                }
            }
            if (arg.Length > 0)
                args.Add(arg.ToString());
            return args;
        }

        private static readonly char[] Quotes = new[] { '\'', '\"' };
        /// <summary>
        /// Extracts arguments from a full line, in a .NET compatible way
        /// (includes strange quotes trimming)
        /// </summary>
        /// <param name="commandLine">The command line</param>
        /// <returns>Arguments list</returns>
        public IList<string> ExtractArguments(string commandLine)
        {
            return ExtractArguments(commandLine, Quotes);
        }

        /// <summary>
        /// Processes different "lines" of parameters:
        /// 1. the original input parameter must be starting with @
        /// 2. all other parameters are kept as a common part
        /// </summary>
        /// <typeparam name="P"></typeparam>
        /// <param name="args"></param>
        /// <returns></returns>
        protected IList<P> GetParameterBatch<P>(IList<string> args)
            where P : AbstractParameters, new()
        {
            return GetParameterBatch<P>(args, ".");
        }

        public IList<P> GetParameterBatch<P>(IList<string> args, string argsFileDirectory)
            where P : AbstractParameters, new()
        {
            var parameters = new List<P>();
            var commonArgs = new List<string>();
            var argsFiles = new List<string>();
            foreach (var arg in args)
            {
                if (arg.StartsWith("@"))
                    argsFiles.Add(arg.Substring(1));
                else
                    commonArgs.Add(arg);
            }
            // if we specify files, we must recurse
            if (argsFiles.Count > 0)
            {
                foreach (var argsFile in argsFiles)
                {
                    parameters.AddRange(GetParameterBatchFile<P>(commonArgs, Path.Combine(argsFileDirectory, argsFile)));
                }
            }
            // if we don't, just use the args
            else if (commonArgs.Count > 0)
            {
                var p = new P();
                p.Load(commonArgs);
                parameters.Add(p);
            }
            return parameters;
        }

        private IList<P> GetParameterBatchFile<P>(IList<string> baseArgs, string argsList)
            where P : AbstractParameters, new()
        {
            var parameters = new List<P>();
            string argsFileDirectory = Path.GetDirectoryName(argsList);
            using (var textReader = File.OpenText(argsList))
            {
                while (!textReader.EndOfStream)
                {
                    string line = textReader.ReadLine();
                    if (line.StartsWith("#"))
                        continue;
                    IList<string> args = ExtractArguments(line);
                    List<string> allArgs = new List<string>(baseArgs);
                    allArgs.AddRange(args);
                    parameters.AddRange(GetParameterBatch<P>(allArgs, argsFileDirectory));
                }
            }
            return parameters;
        }

        protected int TextWidth
        {
            get { return Console.BufferWidth; }
        }

        /// <summary>
        /// Outputs a formatted string to the console.
        /// We're not using the ILogger here, since we want console output.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void Write(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        /// <summary>
        /// Outputs an empty line
        /// </summary>
        public void WriteLine()
        {
            Console.WriteLine();
        }

        /// <summary>
        /// Returns the application (assembly) name (without extension)
        /// </summary>
        protected string ApplicationName
        {
            get
            {
                return Assembly.GetEntryAssembly().GetName().Name;
            }
        }

        /// <summary>
        /// Returns the application (assembly) version
        /// </summary>
        protected Version ApplicationVersion
        {
            get
            {
                return Assembly.GetEntryAssembly().GetName().Version;
            }
        }

        private bool headerWritten;
        /// <summary>
        /// Writes the application header
        /// </summary>
        public void WriteHeader()
        {
            if (!headerWritten)
            {
                WriteHeaderContents();
                WriteLine();
                headerWritten = true;
            }
        }

        protected abstract void WriteHeaderContents();

        /// <summary>
        /// Writes a small summary
        /// </summary>
        public abstract void WriteSummary();

        /// <summary>
        /// Writes examples
        /// </summary>
        public virtual void WriteExamples()
        {
        }

        /// <summary>
        /// The "syntax" is a bried containing the application name, "[options]" and eventually files.
        /// For example: "DbMetal [options] [&lt;input file>]
        /// </summary>
        public virtual void WriteSyntax()
        {
            var syntax = new StringBuilder();
            syntax.AppendFormat("{0} [options]", ApplicationName);
            foreach (var file in GetFiles())
            {
                if (file.Description.Mandatory)
                    syntax.AppendFormat(" {0}", GetFileText(file));
                else
                    syntax.AppendFormat(" [{0}]", GetFileText(file));
            }
            Write(syntax.ToString());
        }

        /// <summary>
        /// Describes an option
        /// </summary>
        protected class Option
        {
            /// <summary>
            /// The member name (property or field)
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// The attribute used to define the member as an option
            /// </summary>
            public OptionAttribute Description { get; set; }
        }

        /// <summary>
        /// Describes an input file
        /// </summary>
        protected class FileName
        {
            /// <summary>
            /// The member name (property or field)
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// The attribute used to define the member as an input file
            /// </summary>
            public FileAttribute Description { get; set; }
        }

        /// <summary>
        /// Internal class. I wrote it because I was thinking that the .NET framework already had such a class.
        /// At second thought, I may have made a confusion with STL
        /// (interesting, isn't it?)
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <typeparam name="B"></typeparam>
        protected class Pair<A, B>
        {
            public A First { get; set; }
            public B Second { get; set; }
        }

        /// <summary>
        /// Enumerates all members (fields or properties) that have been marked with the specified attribute
        /// </summary>
        /// <typeparam name="T">The attribute type to search for</typeparam>
        /// <returns>A list of pairs with name and attribute</returns>
        protected IEnumerable<Pair<string, T>> EnumerateOptions<T>()
            where T : Attribute
        {
            Type t = GetType();
            foreach (var propertyInfo in t.GetProperties())
            {
                var descriptions = (T[])propertyInfo.GetCustomAttributes(typeof(T), true);
                if (descriptions.Length == 1)
                    yield return new Pair<string, T> { First = propertyInfo.Name, Second = descriptions[0] };
            }
            foreach (var fieldInfo in t.GetFields())
            {
                var descriptions = (T[])fieldInfo.GetCustomAttributes(typeof(T), true);
                if (descriptions.Length == 1)
                    yield return new Pair<string, T> { First = fieldInfo.Name, Second = descriptions[0] };
            }
        }

        protected IEnumerable<Option> EnumerateOptions()
        {
            foreach (var pair in EnumerateOptions<OptionAttribute>())
                yield return new Option { Name = pair.First, Description = pair.Second };
        }

        protected IEnumerable<FileName> GetFiles()
        {
            foreach (var pair in from p in EnumerateOptions<FileAttribute>() orderby p.Second.Mandatory select p)
                yield return new FileName { Name = pair.First, Description = pair.Second };
        }

        /// <summary>
        /// Returns options, grouped by group (the group number is the dictionary key)
        /// </summary>
        /// <returns></returns>
        protected IDictionary<int, IList<Option>> GetOptions()
        {
            var options = new Dictionary<int, IList<Option>>();
            foreach (var option in EnumerateOptions())
            {
                if (!options.ContainsKey(option.Description.Group))
                    options[option.Description.Group] = new List<Option>();
                options[option.Description.Group].Add(option);
            }
            return options;
        }

        /// <summary>
        /// Return a literal value based on an option
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        protected virtual string GetOptionText(Option option)
        {
            if (string.IsNullOrEmpty(option.Description.ValueName))
                return option.Name.ToLower();
            return string.Format("{0}:<{1}>",
                option.Name[0].ToString().ToLower() + option.Name.Substring(1),
                option.Description.ValueName);
        }

        /// <summary>
        /// Returns a literal value base on an input file
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        protected virtual string GetFileText(FileName fileName)
        {
            return string.Format("<{0}>", fileName.Description.Name);
        }

        /// <summary>
        /// Computes the maximum options and files length, to align all descriptions
        /// </summary>
        /// <param name="options"></param>
        /// <param name="files"></param>
        /// <returns></returns>
        private int GetMaximumLength(IDictionary<int, IList<Option>> options, IEnumerable<FileName> files)
        {
            int maxLength = 0;
            foreach (var optionsList in options.Values)
            {
                foreach (var option in optionsList)
                {
                    var optionName = GetOptionText(option);
                    int length = optionName.Length;
                    if (length > maxLength)
                        maxLength = length;
                }
            }
            foreach (var file in files)
            {
                var fileName = GetFileText(file);
                int length = fileName.Length;
                if (length > maxLength)
                    maxLength = length;
            }
            return maxLength;
        }

        protected string[] SplitText(string text, int width)
        {
            var lines = new List<string>(new[] { "" });
            var words = text.Split(' ');
            foreach (var word in words)
            {
                var line = lines.Last();
                if (line.Length == 0)
                    lines[lines.Count - 1] = word;
                else if (line.Length + word.Length + 1 < width)
                    lines[lines.Count - 1] = line + " " + word;
                else
                    lines.Add(word);
            }
            return lines.ToArray();
        }

        protected void WriteOption(string firstLine, string text)
        {
            int width = TextWidth - firstLine.Length - 2;
            var lines = SplitText(text, width);
            var padding = string.Empty.PadRight(firstLine.Length);
            for (int i = 0; i < lines.Length; i++)
            {
                Write("{0} {1}", i == 0 ? firstLine : padding, lines[i]);
            }
        }

        /// <summary>
        /// Displays all available options and files
        /// </summary>
        protected void WriteOptions()
        {
            var options = GetOptions();
            var files = GetFiles();
            int maxLength = GetMaximumLength(options, files);
            Write("Options:");
            foreach (var group in from k in options.Keys orderby k select k)
            {
                var optionsList = options[group];
                foreach (var option in from o in optionsList orderby o.Name select o)
                {
                    WriteOption(string.Format("  /{0}", GetOptionText(option).PadRight(maxLength)), option.Description.Text);
                }
                WriteLine();
            }
            foreach (var file in files)
            {
                WriteOption(string.Format("  {0}", GetFileText(file).PadRight(maxLength + 1)), file.Description.Text);
            }
        }

        /// <summary>
        /// Displays application help
        /// </summary>
        public void WriteHelp()
        {
            WriteHeader(); // includes a WriteLine()
            WriteSyntax();
            WriteLine();
            WriteSummary();
            WriteLine();
            WriteOptions();
            WriteLine();
            WriteExamples();
        }
    }
}

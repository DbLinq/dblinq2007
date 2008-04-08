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
using DbMetal.Util;

namespace DbMetal
{
    public class Parameters
    {
        public class DescriptionAttribute : Attribute
        {
            public string Text { get; set; }

            public DescriptionAttribute(string text)
            {
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

        protected delegate void InjectorDelegate(object typedValue);

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

        public Parameters()
        {
        }

        public Parameters(IList<string> args)
        {
            Load(args);
        }

        public static IList<string> GetArguments(string commandLine, char[] quotes)
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

        private static char[] Quotes = new[] { '\'', '\"' };
        public static IList<string> GetArguments(string commandLine)
        {
            return GetArguments(commandLine, Quotes);
        }

        /// <summary>
        /// Processes different "lines" of parameters:
        /// 1. the original input parameter must be starting with @
        /// 2. all other parameters are kept as a common part
        /// </summary>
        /// <typeparam name="P"></typeparam>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IList<P> GetParameterBatch<P>(IList<string> args)
            where P : Parameters, new()
        {
            return GetParameterBatch<P>(args, ".");
        }

        public static IList<P> GetParameterBatch<P>(IList<string> args, string argsFileDirectory)
            where P : Parameters, new()
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

        private static IList<P> GetParameterBatchFile<P>(IList<string> baseArgs, string argsList)
            where P : Parameters, new()
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
                    IList<string> args = GetArguments(line);
                    List<string> allArgs = new List<string>(baseArgs);
                    allArgs.AddRange(args);
                    parameters.AddRange(GetParameterBatch<P>(allArgs, argsFileDirectory));
                }
            }
            return parameters;
        }
    }
}
#region MIT License
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
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SqlMetal.Generator
{
    public abstract class CodeWriter : TextWriter
    {
        public override Encoding Encoding
        {
            get
            {
                return Encoding.UTF8;
            }
        }

        public string IndentationPattern { get; set; }

        private readonly StringBuilder buffer = new StringBuilder(10 << 10);
        private int currentindentation = 0;

        protected TextWriter TextWriter;

        protected CodeWriter(TextWriter textWriter)
        {
            IndentationPattern = "\t";
            TextWriter = textWriter;
        }

        protected bool IsFullLine()
        {
            int endIndex = buffer.Length - CoreNewLine.Length;
            if (endIndex < 0)
                return false;
            for (int i = 0; i < CoreNewLine.Length; i++)
            {
                if (buffer[endIndex + i] != CoreNewLine[i])
                    return false;
            }
            return true;
        }

        protected string GetLine()
        {
            string line = buffer.ToString();
            buffer.Remove(0, buffer.Length);
            return line;
        }

        protected abstract bool MustIndent(string line);
        protected abstract bool MustUnindent(string line);

        /// <summary>
        /// In the end, all output comes to this
        /// </summary>
        /// <param name="value"></param>
        public override void Write(char value)
        {
            buffer.Append(value);
            if (IsFullLine())
            {
                string line = GetLine();
                string rawLine = Trim(line);
                // unindent before...
                if (MustUnindent(rawLine))
                    currentindentation--;
                WriteLine(rawLine, currentindentation);
                // indent after
                if (MustIndent(rawLine))
                    currentindentation++;
            }
        }

        protected virtual string Trim(string line)
        {
            return line.Trim();
        }

        protected virtual void WriteLine(string rawLine, int indentation)
        {
            if (!string.IsNullOrEmpty(rawLine))
            {
                for (int indentationCount = 0; indentationCount < indentation; indentationCount++)
                {
                    TextWriter.Write(IndentationPattern);
                }
            }
            TextWriter.WriteLine(rawLine);
        }

        #region Code generation

        protected class NestedInstruction : IDisposable
        {
            private readonly Action endAction;

            public NestedInstruction(Action end)
            {
                endAction = end;
            }

            public void Dispose()
            {
                endAction();
            }
        }

        protected IDisposable EndAction(Action end)
        {
            return new NestedInstruction(end);
        }

        public abstract void WriteComment(string line);
        public virtual void WriteComments(string comments)
        {
            string[] commentLines = comments.Split('\n');
            foreach (string commentLine in commentLines)
            {
                WriteComment(commentLine.TrimEnd());
            }
        }

        public abstract void WriteUsingNamespace(string name);
        public abstract IDisposable WriteNamespace(string name);
        public abstract IDisposable WriteClass(Specifications specifications, string name,
                                            string baseClass, params string[] interfaces);

        public abstract IDisposable WriteRegion(string name);
        public virtual IDisposable Attribute(string name, IDictionary<string, object> members)
        {
            if (!string.IsNullOrEmpty(name))
                return WriteAttribute(new AttributeDefinition(name, members));
            return null;
        }
        public abstract IDisposable WriteAttribute(AttributeDefinition attributeDefinition);

        public abstract IDisposable WriteMethod(Specifications specifications, string name, Type returnType,
                                           params ParameterDefinition[] parameters);

        public abstract string GetGenericName(string baseName, Type type);

        public abstract IDisposable WriteProperty(Specifications specifications, string name, Type propertyType);
        public abstract IDisposable WritePropertyGet();
        public abstract IDisposable WritePropertySet();

        public abstract string GetCast(string value, Type castType, bool hardCast);

        public virtual string GetLiteralValue(object value)
        {
            if (value is string)
                return string.Format("\"{0}\"", value);
            return value.ToString();
        }

        public virtual string GetMember(string obj, string member)
        {
            return string.Format("{0}.{1}", obj, member);
        }

        public virtual string GetReturnStatement(string expression)
        {
            return GetStatement(string.Format("return {0}", expression));
        }

        public virtual string GetAssignment(string variable, string expression)
        {
            return string.Format("{0} = {1}", variable, expression);
        }

        public abstract string GetArray(string array, string literalIndex);

        public virtual string GetMethodCall(string method, params string [] literalParameters)
        {
            return string.Format("{0}({1})", method, string.Join(", ", literalParameters));
        }

        public virtual string GetStatement(string statement)
        {
            return statement;
        }

        #endregion
    }
}

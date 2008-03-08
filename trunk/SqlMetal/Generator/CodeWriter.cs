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

using System.IO;
using System.Text;

namespace SqlMetal.Generator
{
    public class CodeWriter : TextWriter
    {
        public override Encoding Encoding
        {
            get
            {
                return Encoding.UTF8;
            }
        }

        public string Indent { get; set; }
        public string Unindent { get; set; }
        public string IndentationPattern { get; set; }

        private readonly StringBuilder buffer = new StringBuilder(10 << 10);
        private int currentindentation = 0;

        protected TextWriter TextWriter;

        public CodeWriter(TextWriter textWriter)
        {
            Indent = "{";
            Unindent = "}";
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

        protected virtual bool MustIndent(string line)
        {
            return line.StartsWith(Indent);
        }

        protected virtual bool MustUnindent(string line)
        {
            return line.StartsWith(Unindent);
        }

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
                string rawLine = line.Trim();
                // unindent before...
                if (MustUnindent(rawLine))
                    currentindentation--;
                WriteLine(rawLine, currentindentation);
                // indent after
                if (MustIndent(rawLine))
                    currentindentation++;
            }
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
    }
}

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
using DbLinq.Util;

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

        #endregion

        #region Code generation - Language write

        public abstract void WriteCommentLine(string line);
        public virtual void WriteCommentLines(string comments)
        {
            string[] commentLines = comments.Split('\n');
            foreach (string commentLine in commentLines)
            {
                WriteCommentLine(commentLine.TrimEnd());
            }
        }

        public abstract void WriteUsingNamespace(string name);
        public abstract IDisposable WriteNamespace(string name);
        public abstract IDisposable WriteClass(SpecificationDefinition specificationDefinition, string name,
                                            string baseClass, params string[] interfaces);

        public abstract IDisposable WriteRegion(string name);
        public virtual IDisposable Attribute(string name, IDictionary<string, object> members)
        {
            if (!string.IsNullOrEmpty(name))
                return WriteAttribute(new AttributeDefinition(name, members));
            return null;
        }
        public abstract IDisposable WriteAttribute(AttributeDefinition attributeDefinition);

        public abstract IDisposable WriteMethod(SpecificationDefinition specificationDefinition, string name, Type returnType,
                                           params ParameterDefinition[] parameters);

        public abstract string GetGenericName(string baseName, string type);

        public abstract IDisposable WriteProperty(SpecificationDefinition specificationDefinition, string name, string propertyType);
        public abstract IDisposable WritePropertyGet();
        public abstract IDisposable WritePropertySet();

        public abstract void WritePropertyWithBackingField(SpecificationDefinition specificationDefinition, string name, string propertyType, bool privateSetter);
        public virtual void WritePropertyWithBackingField(SpecificationDefinition specificationDefinition, string name, string propertyType)
        {
            WritePropertyWithBackingField(specificationDefinition, name, propertyType, false);
        }

        public abstract void WriteField(SpecificationDefinition specificationDefinition, string name, string fieldType);

        public abstract void WriteEvent(SpecificationDefinition specificationDefinition, string name, string eventDelegate);

        public abstract IDisposable WriteIf(string expression);

        #endregion

        #region Code generation - Language construction

        public abstract string GetCastExpression(string value, string castType, bool hardCast);

        public virtual string GetLiteralValue(object value)
        {
            if (value is string)
                return string.Format("\"{0}\"", value);
            return value.ToString();
        }

        public virtual string GetLiteralType(Type type)
        {
            return type.FullName;
        }

        public virtual string GetMemberExpression(string obj, string member)
        {
            return string.Format("{0}.{1}", obj, member);
        }

        public virtual string GetReturnStatement(string expression)
        {
            if (expression == null)
                return GetStatement("return");
            return GetStatement(string.Format("return {0}", expression));
        }

        public virtual string GetNewExpression(string ctor)
        {
            return string.Format("new {0}", ctor);
        }

        public virtual string GetThisExpression()
        {
            return "this";
        }

        public virtual string GetDeclarationExpression(string variable, string type)
        {
            return string.Format("{0} {1}", type, variable);
        }

        public virtual string GetAssignmentExpression(string variable, string expression)
        {
            return string.Format("{0} = {1}", variable, expression);
        }

        public abstract string GetArray(string array, string literalIndex);

        public virtual string GetMethodCallExpression(string method, params string [] literalParameters)
        {
            return string.Format("{0}({1})", method, string.Join(", ", literalParameters));
        }

        public virtual string GetStatement(string expression)
        {
            return expression;
        }

        public abstract string GetPropertySetValueExpression();

        public abstract string GetNullExpression();

        public abstract string GetDifferentExpression(string a, string b);
        public abstract string GetEqualExpression(string a, string b);

        public abstract string GetXOrExpression(string a, string b);
        public abstract string GetAndExpression(string a, string b);

        #endregion

    }
}

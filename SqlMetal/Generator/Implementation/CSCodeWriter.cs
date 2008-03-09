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
using System.Linq;
using System.Text;
using DbLinq.Util;

namespace SqlMetal.Generator.Implementation
{
    class CSCodeWriter : CodeWriter
    {
        public string Indent { get; set; }
        public string Unindent { get; set; }

        private bool trimSpaces;

        public CSCodeWriter(TextWriter textWriter, bool trimSpaces)
            : base(textWriter)
        {
            this.trimSpaces = trimSpaces;
            Indent = "{";
            Unindent = "}";
        }

        public CSCodeWriter(TextWriter textWriter)
            : this(textWriter, true)
        {
        }

        protected override bool MustIndent(string line)
        {
            return line.StartsWith(Indent);
        }

        protected override bool MustUnindent(string line)
        {
            return line.StartsWith(Unindent);
        }

        protected override string Trim(string line)
        {
            if (trimSpaces)
                return line.Trim();
            return line.TrimEnd();
        }

        #region Code generation

        protected bool HasSpecification(Specifications specifications, Specifications test)
        {
            return (specifications & test) != 0;
        }

        protected virtual string GetSpecifications(Specifications specifications)
        {
            var literalSpecifications = new List<string>();
            if (HasSpecification(specifications, Specifications.Internal))
                literalSpecifications.Add("internal");
            if (HasSpecification(specifications, Specifications.Private))
                literalSpecifications.Add("private");
            if (HasSpecification(specifications, Specifications.Protected))
                literalSpecifications.Add("protected");

            if (HasSpecification(specifications, Specifications.Abstract))
                literalSpecifications.Add("abstract");
            if (HasSpecification(specifications, Specifications.Virtual))
                literalSpecifications.Add("virtual");
            if (HasSpecification(specifications, Specifications.Override))
                literalSpecifications.Add("override");
            if (HasSpecification(specifications, Specifications.Static))
                literalSpecifications.Add("static");

            if (HasSpecification(specifications, Specifications.Partial))
                literalSpecifications.Add("partial");

            if (HasSpecification(specifications, Specifications.Out))
            {
                if (HasSpecification(specifications, Specifications.In))
                    literalSpecifications.Add("ref");
                else
                    literalSpecifications.Add("out");
            }

            string result = string.Join(" ", literalSpecifications.ToArray());
            if (!string.IsNullOrEmpty(result))
                result += " ";
            return result;
        }

        protected virtual string GetProtectionSpecifications(Specifications specifications)
        {
            string literalSpecifications = GetSpecifications(specifications & Specifications.ProtectionClass);
            if (string.IsNullOrEmpty(literalSpecifications))
                literalSpecifications = "public ";
            return literalSpecifications;
        }

        protected virtual string GetInheritanceSpecifications(Specifications specifications)
        {
            return GetSpecifications(specifications & Specifications.InheritanceClass);
        }

        protected virtual string GetDomainSpecifications(Specifications specifications)
        {
            return GetSpecifications(specifications & Specifications.DomainClass);
        }

        protected virtual string GetDirectionSpecifications(Specifications specifications)
        {
            return GetSpecifications(specifications & Specifications.DirectionClass);
        }

        public override void WriteComment(string line)
        {
            WriteLine("// {0}", line);
        }

        protected virtual IDisposable WriteBrackets()
        {
            WriteLine("{");
            return EndAction(delegate { WriteLine("}"); });
        }

        public override void WriteUsingNamespace(string name)
        {
            WriteLine("using {0};", name);
        }

        public override IDisposable WriteNamespace(string name)
        {
            WriteLine("namespace {0}", name);
            return WriteBrackets();
        }

        public override IDisposable WriteClass(Specifications specifications, string name, string baseClass, params string[] interfaces)
        {
            var classLineBuilder = new StringBuilder(1024);

            classLineBuilder.Append(GetProtectionSpecifications(specifications));
            classLineBuilder.Append(GetDomainSpecifications(specifications));
            classLineBuilder.Append(GetInheritanceSpecifications(specifications));

            var bases = new List<string>();
            if (!string.IsNullOrEmpty(baseClass))
                bases.Add(baseClass);

            classLineBuilder.AppendFormat("class {0}", name);
            if (bases.Count > 0)
            {
                classLineBuilder.Append(": ");
                classLineBuilder.Append(string.Join(", ", bases.ToArray()));
            }
            WriteLine(classLineBuilder.ToString());

            return WriteBrackets();
        }

        public override IDisposable WriteRegion(string name)
        {
            WriteLine("#region {0}", name);
            WriteLine();
            return EndAction(delegate { WriteLine(); WriteLine("#endregion"); WriteLine(); });
        }

        protected virtual string GetAttribute(AttributeDefinition attributeDefinition)
        {
            if (attributeDefinition.Members.Count == 0)
                return string.Format("[{0}]", attributeDefinition.Name);
            var attributeLineBuilder = new StringBuilder(1024);

            attributeLineBuilder.AppendFormat("[{0}(", attributeDefinition.Name);
            var members = new List<string>();
            foreach (var keyValue in attributeDefinition.Members)
                members.Add(string.Format("{0} = {1}", keyValue.Key, GetLiteralValue(keyValue.Value)));
            attributeLineBuilder.Append(string.Join(", ", members.ToArray()));
            attributeLineBuilder.Append(")]");

            return attributeLineBuilder.ToString();
        }

        public override IDisposable WriteAttribute(AttributeDefinition attributeDefinition)
        {
            WriteLine(GetAttribute(attributeDefinition));
            return null;
        }

        public string GetLiteralType(Type type)
        {
            if (type == null)
                return null;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return CSharp.FormatType(type.GetGenericArguments()[0].FullName, true);
            }
            return CSharp.FormatType(type.FullName, false);
        }

        public override IDisposable WriteMethod(Specifications specifications, string name, Type returnType,
                                    params ParameterDefinition[] parameters)
        {
            var methodLineBuilder = new StringBuilder(1024);

            methodLineBuilder.Append(GetProtectionSpecifications(specifications));
            methodLineBuilder.Append(GetDomainSpecifications(specifications));
            methodLineBuilder.Append(GetInheritanceSpecifications(specifications));

            methodLineBuilder.AppendFormat("{0} {1}(", GetLiteralType(returnType) ?? "void", name);
            var literalParameters = new List<string>();
            foreach (var parameter in parameters)
            {
                string literalParameter = string.Format("{0}{3}{1} {2}",
                    parameter.Attribute != null ? GetAttribute(parameter.Attribute) + " " : string.Empty,
                    GetLiteralType(parameter.Type), parameter.Name,
                    GetDirectionSpecifications(parameter.Specifications));
                literalParameters.Add(literalParameter);
            }
            methodLineBuilder.AppendFormat("{0})", string.Join(", ", literalParameters.ToArray()));
            WriteLine(methodLineBuilder.ToString());
            return WriteBrackets();
        }

        public override string GetGenericName(string baseName, Type type)
        {
            return string.Format("{0}<{1}>", baseName, GetLiteralType(type));
        }

        public override IDisposable WriteProperty(Specifications specifications, string name, Type propertyType)
        {
            var methodLineBuilder = new StringBuilder(1024);

            methodLineBuilder.Append(GetProtectionSpecifications(specifications));
            methodLineBuilder.Append(GetDomainSpecifications(specifications));
            methodLineBuilder.Append(GetInheritanceSpecifications(specifications));

            methodLineBuilder.AppendFormat("{0} {1}", GetLiteralType(propertyType), name);

            return WriteBrackets();
        }

        public override IDisposable WritePropertyGet()
        {
            WriteLine("get");
            return WriteBrackets();
        }

        public override IDisposable WritePropertySet()
        {
            WriteLine("set");
            return WriteBrackets();
        }

        public override string GetCast(string value, Type castType, bool hardCast)
        {
            string format = hardCast ? "({1}){0}" : "{0} as {1}";
            string literalCast = string.Format(format, value, GetLiteralType(castType));
            return literalCast;
        }

        public override string GetLiteralValue(object value)
        {
            if (value is bool)
                return ((bool)value) ? "true" : "false";
            return base.GetLiteralValue(value);
        }

        public override string GetArray(string array, string literalIndex)
        {
            return string.Format("{0}[{1}]", array, literalIndex);
        }

        public override string GetStatement(string statement)
        {
            return statement + ";";
        }

        #endregion
    }
}


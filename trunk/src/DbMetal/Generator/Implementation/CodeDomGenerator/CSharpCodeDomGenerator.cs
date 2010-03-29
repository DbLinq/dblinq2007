#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion

using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Text;
using DbLinq.Schema.Dbml;
using Microsoft.CSharp;

namespace DbMetal.Generator.Implementation.CodeDomGenerator
{
#if !MONO_STRICT
    public
#endif
    class CSharpCodeDomGenerator: AbstractCodeDomGenerator
    {
        public override string LanguageCode { get { return "c#2"; } }
        public override string Extension { get { return ".cs2"; } }

        protected override CodeDomProvider CreateProvider()
        {
            return CodeDomProvider.CreateProvider("C#");
        }

        protected override void AddConditionalImports(System.CodeDom.CodeNamespaceImportCollection imports, string firstImport, string conditional, string[] importsIfTrue, string[] importsIfFalse, string lastImport)
        {
            // HACK HACK HACK
            // Would be better if CodeDom actually supported conditional compilation constructs...
            // This is predecated upon CSharpCodeGenerator.GenerateNamespaceImport() being implemented as:
            //      output.Write ("using ");
            //      output.Write (GetSafeName (import.Namespace));
            //      output.WriteLine (';');
            // Thus, with "crafty" execution of the namespace, we can stuff arbitrary text in there...

            var block = new StringBuilder();
            // No 'using', as GenerateNamespaceImport() writes it.
            block.Append(firstImport).Append(";").Append(Environment.NewLine);
            block.Append("#if ").Append(conditional).Append(Environment.NewLine);
            foreach (var ns in importsIfTrue)
                block.Append("    using ").Append(ns).Append(";").Append(Environment.NewLine);
            block.Append("#else   // ").Append(conditional).Append(Environment.NewLine);
            foreach (var ns in importsIfFalse)
                block.Append("    using ").Append(ns).Append(";").Append(Environment.NewLine);
            block.Append("#endif  // ").Append(conditional).Append(Environment.NewLine);
            block.Append("    using ").Append(lastImport);
            // No ';', as GenerateNamespaceImport() writes it.

            imports.Add(new CodeNamespaceImport(block.ToString()));
        }
    }
}

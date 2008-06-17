#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry
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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using DbLinq.Language.Implementation;

namespace DbLinq.Language
{
    public class EnglishWords : AbstractEndPluralWords
    {
        public override void Load()
        {
            if (WordsWeights == null)
                Load("EnglishWords.txt");
        }

        public override bool Supports(CultureInfo cultureInfo)
        {
            return cultureInfo.ThreeLetterISOLanguageName == "eng";
        }

        protected override SingularPlural[] SingularsPlurals
        {
            get { return singularsPlurals; }
        }

        // important: keep this from most specific to less specific
        private SingularPlural[] singularsPlurals =
            {
                new SingularPlural { Singular="ss", Plural="sses" },
                new SingularPlural { Singular="ch", Plural="ches" },
                new SingularPlural { Singular="sh", Plural="shes" },
                new SingularPlural { Singular="zz", Plural="zzes" },
                new SingularPlural { Singular="x", Plural="xes" },
                new SingularPlural { Singular="y", Plural="ies" },
                new SingularPlural { Singular="", Plural="s" },
            };
    }
}
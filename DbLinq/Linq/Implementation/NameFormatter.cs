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
using System.Collections.Generic;
using System.Text;
using DbLinq.Util;

namespace DbLinq.Linq.Implementation
{
    public class NameFormatter : INameFormatter
    {
        public bool Singularize { get; set; }
        public Case Case { get; set; }

        [Flags]
        protected enum Position
        {
            First = 0x01,
            Last = 0x02,
        }

        public Words Words { get; set; }

        public NameFormatter()
        {
            Words = new Words();
            Singularize = true;
            Case = Case.PascalCase;
        }

        public virtual string Format(string oldName, Case newCase, bool? singularPlural)
        {
            StringBuilder result = new StringBuilder();
            IList<string> parts = Words.GetWords(oldName);
            for (int partIndex = 0; partIndex < parts.Count; partIndex++)
            {
                Position position = 0;
                if (partIndex == 0)
                    position |= Position.First;
                if (partIndex == parts.Count - 1)
                    position |= Position.Last;
                result.Append(AdjustPart(parts[partIndex], position, newCase, singularPlural));
            }
            return result.ToString();
        }

        public string ToCamelCase(string part)
        {
            return part.ToLower();
        }

        public string ToPascalCase(string part)
        {
            part = part.Substring(0, 1).ToUpper() + part.Substring(1).ToLower();
            return part;
        }

        protected virtual string AdjustPart(string part, Position position, Case newCase, bool? singularPlural)
        {
            if (singularPlural.HasValue && (position & Position.Last) != 0)
            {
                if (singularPlural.Value)
                    part = Words.Singularize(part);
                else
                    part = Words.Pluralize(part);
            }
            if ((position & Position.First) != 0 && newCase == Case.camelCase)
                part = ToCamelCase(part);
            else if (newCase != Case.Leave)
                part = ToPascalCase(part);
            return part;
        }

        public virtual string AdjustTableName(string tableName)
        {
            return Format(tableName, Case, Singularize);
        }

        public virtual string AdjustColumnName(string columnName)
        {
            return Format(columnName, Case, null);
        }

        public virtual string AdjustColumnFieldName(string columnName)
        {
            return Format(columnName, Case.camelCase, null);
        }

        public virtual string AdjustMethodName(string methodName)
        {
            return Format(methodName, Case, null);
        }

        public virtual string AdjustOneToManyColumnName(string referencedTableName)
        {
            return Format(referencedTableName, Case, null);
        }

        public virtual string AdjustManyToOneColumnName(string referencedTableName, string thisTableName)
        {
            if (referencedTableName == thisTableName)
                return Format("Parent" + referencedTableName, Case, true);
            return Format(referencedTableName, Case, true);
        }
    }
}
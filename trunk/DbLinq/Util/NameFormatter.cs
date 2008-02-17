
using System;
using System.Collections.Generic;
using System.Text;

namespace DbLinq.Util
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

        public virtual string Format(string oldName, Case newCase, bool singularize)
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
                result.Append(AdjustPart(parts[partIndex], position, newCase, singularize));
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

        protected virtual string AdjustPart(string part, Position position, Case newCase, bool singularize)
        {
            if (singularize && (position & Position.Last) != 0)
                part = Words.Singularize(part);
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
            return Format(columnName, Case, false);
        }

        public virtual string AdjustColumnFieldName(string columnName)
        {
            return Format(columnName, Case.camelCase, false);
        }

        public virtual string AdjustMethodName(string methodName)
        {
            return Format(methodName, Case, false);
        }
    }
}

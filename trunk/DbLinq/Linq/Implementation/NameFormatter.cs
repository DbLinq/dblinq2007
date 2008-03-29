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
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using DbLinq.Factory;
using DbLinq.Schema;
using DbLinq.Util.Language;

namespace DbLinq.Linq.Implementation
{
    public class NameFormatter : INameFormatter
    {
        public bool Pluralize { get; set; }
        public Case Case { get; set; }

        [Flags]
        protected enum Position
        {
            First = 0x01,
            Last = 0x02,
        }

        private CultureInfo cultureInfo;
        public CultureInfo CultureInfo
        {
            get { return cultureInfo; }
            set 
            { 
                cultureInfo = value;
                var languages = ObjectFactory.Get<ILanguages>();
                Words = languages.Load(value);
                Words.Load();
            }
        }
        public ILanguageWords Words { get; private set; }

        public NameFormatter()
        {
            CultureInfo = new CultureInfo("en-us");
            Case = Case.PascalCase;
        }

        public virtual string Format(string oldName, Case newCase, Singularization singularization)
        {
            var parts = Words.GetWords(oldName);
            return Format(parts, newCase, singularization);
        }

        private string Format(IList<string> words, Case newCase, Singularization singularization)
        {
            var result = new StringBuilder();
            for (int partIndex = 0; partIndex < words.Count; partIndex++)
            {
                Position position = 0;
                if (partIndex == 0)
                    position |= Position.First;
                if (partIndex == words.Count - 1)
                    position |= Position.Last;
                result.Append(AdjustPart(words[partIndex], position, newCase, singularization));
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

        protected virtual string AdjustPart(string part, Position position, Case newCase, Singularization singularization)
        {
            if (singularization != Singularization.DontChange && (position & Position.Last) != 0)
            {
                if (singularization == Singularization.Singular)
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
            return Format(tableName, Case, Pluralize ? Singularization.Singular : Singularization.DontChange);
        }

        public virtual string AdjustColumnName(string columnName)
        {
            return Format(columnName, Case, Singularization.DontChange);
        }

        public virtual string AdjustColumnFieldName(string columnName)
        {
            return Format(columnName, Case.camelCase, Singularization.DontChange);
        }

        public virtual string AdjustMethodName(string methodName)
        {
            return Format(methodName, Case, Singularization.DontChange);
        }

        public virtual string AdjustOneToManyColumnName(string referencedTableName)
        {
            return Format(referencedTableName, Case, Singularization.DontChange);
        }

        public virtual string AdjustManyToOneColumnName(string referencedTableName, string thisTableName)
        {
            if (referencedTableName == thisTableName)
                return Format("Parent" + referencedTableName, Case, Pluralize ? Singularization.Singular : Singularization.DontChange);
            return Format(referencedTableName, Case, Pluralize ? Singularization.Singular : Singularization.DontChange);
        }

        private void PushWord(IList<string> words, StringBuilder currentWord)
        {
            if (currentWord.Length > 0)
            {
                words.Add(currentWord.ToString());
                currentWord.Remove(0, currentWord.Length);
            }
        }

        /// <summary>
        /// Extracts words from uppercase and _
        /// A word can also be composed of several uppercase letters
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected virtual IList<string> GetWordsByCase(string name)
        {
            List<string> words = new List<string>();
            bool currentLowerCase = true;
            StringBuilder currentWord = new StringBuilder();
            for (int charIndex = 0; charIndex < name.Length; charIndex++)
            {
                char currentChar = name[charIndex];
                bool isLower = char.IsLower(currentChar);
                // we switched to uppercase
                if (!isLower && currentLowerCase)
                {
                    PushWord(words, currentWord);
                }
                else if (isLower && !currentLowerCase)
                {
                    // if the current word has several uppercase letters, it is one unique word
                    if (currentWord.Length > 1)
                        PushWord(words, currentWord);
                }
                if (char.IsLetterOrDigit(currentChar))
                    currentWord.Append(currentChar);
                currentLowerCase = isLower;
            }
            PushWord(words, currentWord);

            return words;
        }

        protected virtual IList<string> ExtractWords(string dbName, WordsExtraction extraction)
        {
            switch (extraction)
            {
            case WordsExtraction.FromCase:
                return GetWordsByCase(dbName);
            case WordsExtraction.FromDictionary:
                return Words.GetWords(dbName);
            default:
                throw new ArgumentOutOfRangeException("extraction");
            }
        }

        protected virtual Singularization GetSingularization(Singularization singularization)
        {
            if (!Pluralize)
                return Singularization.DontChange;
            return singularization;
        }

        public SchemaName GetSchemaName(string dbName, WordsExtraction extraction)
        {
            var schemaName = new SchemaName { DbName = dbName };
            schemaName.NameWords = ExtractWords(dbName, extraction);
            schemaName.ClassName = Format(schemaName.NameWords, Case, Singularization.DontChange);
            return schemaName;
        }

        public ProcedureName GetProcedureName(string dbName, WordsExtraction extraction)
        {
            var procedureName = new ProcedureName { DbName = dbName };
            procedureName.NameWords = ExtractWords(dbName, extraction);
            procedureName.MethodName = Format(procedureName.NameWords, Case, Singularization.DontChange);
            return procedureName;
        }

        public TableName GetTableName(string dbName, WordsExtraction extraction)
        {
            var tableName = new TableName { DbName = dbName };
            tableName.NameWords = ExtractWords(dbName, extraction);
            tableName.ClassName = Format(tableName.NameWords, Case, GetSingularization(Singularization.Singular));
            tableName.MemberName = Format(tableName.NameWords, Case, GetSingularization(Singularization.Plural));
            return tableName;
        }

        public ColumnName GetColumnName(string dbName, WordsExtraction extraction)
        {
            var columnName = new ColumnName { DbName = dbName };
            columnName.NameWords = ExtractWords(dbName, extraction);
            columnName.PropertyName = Format(columnName.NameWords, Case, Singularization.DontChange);
            columnName.StorageFieldName = Format(columnName.NameWords, Case.camelCase, Singularization.DontChange);
            if (columnName.StorageFieldName == columnName.PropertyName)
                columnName.StorageFieldName = columnName.StorageFieldName + "Field";
            return columnName;
        }

        public AssociationName GetAssociationName(string dbManyName, string dbOneName, string dbConstraintName, WordsExtraction extraction)
        {
            var associationName = new AssociationName { DbName = dbManyName };
            associationName.NameWords = ExtractWords(dbManyName, extraction);
            associationName.ManyToOneMemberName = Format(dbOneName, Case, GetSingularization(Singularization.Singular));
            // TODO: this works only for PascalCase
            if (dbManyName == dbOneName)
                associationName.ManyToOneMemberName = "Parent" + associationName.ManyToOneMemberName;
            // TODO: support new extraction
            associationName.OneToManyMemberName = Format(dbManyName, Case, GetSingularization(Singularization.Plural));
            associationName.ForeignKeyStorageFieldName = Format(dbConstraintName, Case.camelCase, Singularization.DontChange);
            return associationName;
        }
    }
}

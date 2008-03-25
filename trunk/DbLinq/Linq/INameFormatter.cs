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

using DbLinq.Linq.Implementation;
using DbLinq.Schema;

namespace DbLinq.Linq
{
    public enum Case
    {
        Leave,
        camelCase,
        PascalCase
    }

    public enum WordsExtraction
    {
        FromCase,
        FromDictionary,
    }

    public enum Singularization
    {
        DontChange,
        Singular,
        Plural,
    }

    public interface INameFormatter
    {
        bool Pluralize { get; set; }
        Case Case { get; set; }

        string GetFullDbName(string name, string schema);
        SchemaName GetSchemaName(string dbName, WordsExtraction extraction);
        ProcedureName GetProcedureName(string dbName, string dbSchema, WordsExtraction extraction);
        TableName GetTableName(string dbName, string dbSchema, WordsExtraction extraction);
        ColumnName GetColumnName(string dbName, WordsExtraction extraction);
        AssociationName GetAssociationName(string dbManyName, string dbManySchema, string dbOneName, string dbOneSchema, string dbConstraintName, WordsExtraction extraction);

        string AdjustTableName(string tableName);
        string AdjustColumnName(string columnName);
        string AdjustColumnFieldName(string columnName);
        string AdjustMethodName(string methodName);
        string AdjustOneToManyColumnName(string referencedTableName);
        string AdjustManyToOneColumnName(string referencedTableName, string thisTableName);
    }
}

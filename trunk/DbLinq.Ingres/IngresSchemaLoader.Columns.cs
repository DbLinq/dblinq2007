#region MIT license
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
using System.Collections.Generic;
using System.Data;
using DbLinq.Util;
using DbLinq.Vendor;

namespace DbLinq.Ingres
{
    partial class IngresSchemaLoader
    {
        protected virtual string GetFullType(string columnType, object columnLength, object columnPrecision, object columnScale)
        {
            switch (columnType.ToLower())
            {
            case "c":
            case "char":
            case "nchar":
            case "varchar":
            case "nvarchar":
            case "long varchar":
            case "text":
            //case "integer":
                return columnType + "(" + columnLength.ToString() + ")";

            case "decimal":
                return columnType + "(" + columnPrecision.ToString() + ", " + columnScale.ToString() + ")";

            default:
                return columnType;
            }
        }

        protected override IList<IDataTableColumn> ReadColumns(IDbConnection connectionString, string databaseName)
        {
            List<IDataTableColumn> result = new List<IDataTableColumn>();

            DataTable tab = (DataTable)connectionString
                .GetType()
                .GetMethod("GetSchema", new System.Type[] { typeof(string) })
                .Invoke(connectionString, new string[] { "Columns" });

            foreach (DataRow table in tab.Rows)
            {
                string colTableSchema = table["TABLE_SCHEMA"].ToString();
                if (colTableSchema == "$ingres") continue;
                DataTableColumn col = new DataTableColumn();
                col.TableSchema = colTableSchema;
                col.TableName = table["TABLE_NAME"].ToString();
                col.ColumnName = table["COLUMN_NAME"].ToString();
                col.Type = table["DATA_TYPE"].ToString();
                col.DefaultValue = table["COLUMN_DEFAULT"].ToString();
                col.Length = (table["CHARACTER_MAXIMUM_LENGTH"] is System.DBNull ? null : (long?)(int)table["CHARACTER_MAXIMUM_LENGTH"]);
                col.Scale = (table["NUMERIC_SCALE"] is System.DBNull ? null : (int?)table["NUMERIC_SCALE"]);
                col.Precision = (table["NUMERIC_PRECISION"] is System.DBNull ? null : (int?)(byte?)table["NUMERIC_PRECISION"]);
                col.FullType = GetFullType(col.Type, col.Length, col.Precision, col.Scale);
                col.Generated = col.DefaultValue != null && col.DefaultValue.StartsWith("next value for");
                col.Nullable = table["IS_NULLABLE"].ToString() == "YES";
                result.Add(col);
            }

            return result;
        }
    }
}

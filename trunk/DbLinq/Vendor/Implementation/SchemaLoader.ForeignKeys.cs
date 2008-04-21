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

using System.Data;
using System.Linq;
using DbLinq.Linq;
using DbLinq.Logging;
using DbLinq.Schema;
using DbLinq.Schema.Dbml;

namespace DbLinq.Vendor.Implementation
{
    partial class SchemaLoader
    {
        protected abstract void LoadConstraints(Database schema, SchemaName schemaName, IDbConnection conn, NameFormat nameFormat, Names names);

        protected virtual void LoadForeignKey(Database schema, Table table, string columnName, string tableName, string tableSchema,
            string referencedColumnName, string referencedTableName, string referencedTableSchema,
            string constraintName,
            NameFormat nameFormat, Names names)
        {
            var associationName = CreateAssociationName(tableName, tableSchema,
                referencedTableName, referencedTableSchema, constraintName, nameFormat);

            var foreignKey = names.ColumnsNames[tableName][columnName].PropertyName;
            var reverseForeignKey = names.ColumnsNames[referencedTableName][referencedColumnName].PropertyName;

            //both parent and child table get an [Association]
            var assoc = new Association();
            assoc.IsForeignKey = true;
            assoc.Name = constraintName;
            assoc.Type = null;
            assoc.ThisKey = foreignKey;
            assoc.OtherKey = reverseForeignKey;
            assoc.Member = associationName.ManyToOneMemberName;
            assoc.Storage = associationName.ForeignKeyStorageFieldName;
            table.Type.Associations.Add(assoc);

            //and insert the reverse association:
            var reverseAssociation = new Association();
            reverseAssociation.Name = constraintName;
            reverseAssociation.Type = table.Type.Name;
            reverseAssociation.Member = associationName.OneToManyMemberName;
            reverseAssociation.ThisKey = reverseForeignKey;
            reverseAssociation.OtherKey = foreignKey;

            string referencedFullDbName = GetFullCaseSafeDbName(referencedTableName, referencedTableSchema);
            var referencedTable = schema.Tables.FirstOrDefault(t => referencedFullDbName == t.Name);
            if (referencedTable == null)
                Logger.Write(Level.Error, "ERROR L151: parent table not found: " + referencedTableName);
            else
            {
                referencedTable.Type.Associations.Add(reverseAssociation);
                assoc.Type = referencedTable.Type.Name;
            }
        }
    }
}

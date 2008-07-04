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
using System.Linq;
using DbLinq.Schema;
using DbLinq.Schema.Dbml;

namespace DbLinq.Vendor.Implementation
{
    partial class SchemaLoader
    {
        protected virtual void CheckNamesCaseSafety(Database schema)
        {
            schema.Name = Vendor.GetSqlFieldSafeName(schema.Name);
            foreach (var table in schema.Table)
            {
                table.Name = Vendor.GetSqlFieldSafeName(table.Name);
                foreach (var column in table.Type.Columns)
                {
                    column.Name = Vendor.GetSqlFieldSafeName(column.Name);
                }
                foreach (var association in table.Type.Associations)
                {
                    association.Name = Vendor.GetSqlFieldSafeName(association.Name);
                }
            }
            foreach (var storedProcedure in schema.Functions)
            {
                storedProcedure.Name = Vendor.GetSqlFieldSafeName(storedProcedure.Name);
            }
        }

        protected virtual void CheckNames(Database schema,
                                      Func<Column, string> tableNamedColumnRenamer,
                                      Func<Association, string> tableNamedAssociationRenamer,
                                      Func<Association, string> columnNamedAssociationRenamer)
        {
            foreach (var table in schema.Tables)
            {
                foreach (var column in table.Type.Columns)
                {
                    if (column.Member == table.Type.Name)
                        column.Member = tableNamedColumnRenamer(column);
                }
                foreach (var association in table.Type.Associations)
                {
                    if (association.Member == table.Type.Name)
                        association.Member = tableNamedAssociationRenamer(association);
                    else if ((from column in table.Type.Columns where column.Member == association.Member select column).FirstOrDefault() != null)
                    {
                        association.Member = columnNamedAssociationRenamer(association);
                    }
                }
            }
        }

        protected virtual void CheckNames(Database schema)
        {
            CheckNames(schema,
                       column => "Contents",
                       association => association.ThisKey + association.Member,
                       association => association.Member + association.Type);
        }

        protected virtual void GenerateStorageFields(Database schema, Func<string, string> storageGenerator)
        {
            foreach (var table in schema.Tables)
            {
                foreach (var column in table.Type.Columns)
                {
                    column.Storage = storageGenerator(column.Member);
                }
                foreach (var association in table.Type.Associations)
                {
                    association.Storage = storageGenerator(association.Member);
                }
            }
        }

        protected virtual void GenerateStorageFields(Database schema)
        {
            GenerateStorageFields(schema, delegate(string name)
                                              {
                                                  //jgm 2008June: pre-pended underscore to have same storage format as MS
                                                  var storage = "_" + NameFormatter.Format(name, Case.camelCase);
                                                  if (storage == name)
                                                      storage = "_" + storage;
                                                  return storage;
                                              });
        }
    }
}

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

using System;
using System.Linq;
using DbLinq.Schema.Dbml;

namespace DbLinq.Util
{
    public static class DbmlSchemaExtensions
    {
        public static Association FindReverse(this Database schema, Association association)
        {
            if (string.IsNullOrEmpty(association.Type))
                return null;
            var table = (from t in schema.Tables where t.Type.Name == association.Type select t).SingleOrDefault();
            if (table != null)
            {
                var reverseAssociation = (from a in table.Type.Associations where a.ThisKey == association.OtherKey select a).
                                            SingleOrDefault();
                return reverseAssociation;
            }
            return null;
        }

        public static void CheckNames(this Database schema,
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

        public static void CheckNames(this Database schema)
        {
            CheckNames(schema,
                       column => "Contents",
                       association => association.ThisKey + association.Member,
                       association => association.Member + association.Type);
        }
    }
}

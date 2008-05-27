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

namespace DbLinq.Linq.Data.Sugar.Implementation
{
    public class QueryRunner : IQueryRunner
    {
        /// <summary>
        /// Enumerates all records return by SQL request
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public IEnumerable<T> GetEnumerator<T>(Query query)
        {
            var rowObjectCreator = query.GetRowObjectCreator<T>();
            using (query.DataContext.DatabaseContext.OpenConnection())
            using (var dbCommand = Createcommand(query))
            using (var dbDataReader = dbCommand.ExecuteReader())
            {
                while (dbDataReader.Read())
                {
                    // someone told me one day this could happen (in SQLite)
                    if (dbDataReader.FieldCount == 0)
                        continue;

                    var row = rowObjectCreator(dbDataReader, query.DataContext.MappingContext);
                    row = (T)query.DataContext.GetOrRegisterEntity(row);
                    // TODO: place updates in DataContext
                    //_vars.Table.CheckAttachment(current); // registers the object to be watched for updates
                    query.DataContext.ModificationHandler.Register(row);

                    yield return row;
                }
            }
        }

        protected virtual IDbCommand Createcommand(Query query)
        {
            var dbCommand = query.DataContext.DatabaseContext.Connection.CreateCommand();
            dbCommand.CommandText = query.Sql;
            foreach (var parameter in query.Parameters)
            {
                var dbParameter = dbCommand.CreateParameter();
                dbParameter.ParameterName = query.DataContext.Vendor.SqlProvider.GetParameterName(parameter.Alias);
                dbParameter.Value = parameter.GetValue();
                dbCommand.Parameters.Add(dbParameter);
            }
            return dbCommand;
        }
    }
}

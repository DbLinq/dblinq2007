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
using System.Collections.Generic;
using System.Data;
using DbLinq.Data.Linq.Sugar;

namespace DbLinq.Data.Linq.Sugar.Implementation
{
    public class QueryRunner : IQueryRunner
    {
        /// <summary>
        /// Enumerates all records return by SQL request
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="selectQuery"></param>
        /// <returns></returns>
        public virtual IEnumerable<T> GetEnumerator<T>(SelectQuery selectQuery)
        {
            var rowObjectCreator = selectQuery.GetRowObjectCreator<T>();

            // handle the special case where the query is empty, meaning we don't need the DB
            if (string.IsNullOrEmpty(selectQuery.Sql))
            {
                yield return rowObjectCreator(null, null);
                yield break;
            }

            using (selectQuery.DataContext.DatabaseContext.OpenConnection())
            using (var dbCommand = Createcommand(selectQuery))
            using (var dbDataReader = dbCommand.ExecuteReader())
            {
                while (dbDataReader.Read())
                {
                    // someone told me one day this could happen (in SQLite)
                    if (dbDataReader.FieldCount == 0)
                        continue;

                    var row = rowObjectCreator(dbDataReader, selectQuery.DataContext.MappingContext);
                    if (row != null)
                    {
                        row = (T) selectQuery.DataContext.GetOrRegisterEntity(row);
                        // TODO: place updates in DataContext
                        //_vars.Table.CheckAttachment(current); // registers the object to be watched for updates
                        selectQuery.DataContext.ModificationHandler.Register(row);
                    }

                    yield return row;
                }
            }
        }

        protected virtual IDbCommand Createcommand(SelectQuery selectQuery)
        {
            var dbCommand = selectQuery.DataContext.DatabaseContext.Connection.CreateCommand();
            dbCommand.CommandText = selectQuery.Sql;
            foreach (var parameter in selectQuery.InputParameters)
            {
                var dbParameter = dbCommand.CreateParameter();
                dbParameter.ParameterName = selectQuery.DataContext.Vendor.SqlProvider.GetParameterName(parameter.Alias);
                dbParameter.Value = parameter.GetValue();
                dbCommand.Parameters.Add(dbParameter);
            }
            return dbCommand;
        }

        public virtual S Execute<S>(SelectQuery selectQuery)
        {
            switch (selectQuery.ExecuteMethodName)
            {
                case null: // some calls, like Count() generate SQL and the resulting projection method name is null (never initialized)
                    return ExecuteSingle<S>(selectQuery, false); // Single() for safety, but First() should work
                case "First":
                    return ExecuteFirst<S>(selectQuery, false);
                case "FirstOrDefault":
                    return ExecuteFirst<S>(selectQuery, true);
                case "Single":
                    return ExecuteSingle<S>(selectQuery, false);
                case "SingleOrDefault":
                    return ExecuteSingle<S>(selectQuery, true);
                case "Last":
                    return ExecuteLast<S>(selectQuery, false);
            }
            throw Error.BadArgument("S0077: Unhandled method '{0}'", selectQuery.ExecuteMethodName);
        }

        /// <summary>
        /// Returns first item in query.
        /// If no row is found then if default allowed returns default(S), throws exception otherwise
        /// </summary>
        /// <typeparam name="S"></typeparam>
        /// <param name="selectQuery"></param>
        /// <param name="allowDefault"></param>
        /// <returns></returns>
        protected virtual S ExecuteFirst<S>(SelectQuery selectQuery, bool allowDefault)
        {
            foreach (var row in GetEnumerator<S>(selectQuery))
                return row;
            if (!allowDefault)
                throw new InvalidOperationException();
            return default(S);
        }

        /// <summary>
        /// Returns single item in query
        /// If more than one item is found, throws an exception
        /// If no row is found then if default allowed returns default(S), throws exception otherwise
        /// </summary>
        /// <typeparam name="S"></typeparam>
        /// <param name="selectQuery"></param>
        /// <param name="allowDefault"></param>
        /// <returns></returns>
        protected virtual S ExecuteSingle<S>(SelectQuery selectQuery, bool allowDefault)
        {
            S firstRow = default(S);
            int rowCount = 0;
            foreach (var row in GetEnumerator<S>(selectQuery))
            {
                if (rowCount > 1)
                    throw new InvalidOperationException();
                firstRow = row;
                rowCount++;
            }
            if (!allowDefault && rowCount == 0)
                throw new InvalidOperationException();
            return firstRow;
        }

        /// <summary>
        /// Returns last item in query
        /// </summary>
        /// <typeparam name="S"></typeparam>
        /// <param name="selectQuery"></param>
        /// <param name="allowDefault"></param>
        /// <returns></returns>
        protected virtual S ExecuteLast<S>(SelectQuery selectQuery, bool allowDefault)
        {
            S lastRow = default(S);
            int rowCount = 0;
            foreach (var row in GetEnumerator<S>(selectQuery))
            {
                lastRow = row;
                rowCount++;
            }
            if (!allowDefault && rowCount == 0)
                throw new InvalidOperationException();
            return lastRow;
        }
    }
}
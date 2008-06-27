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
using System.Reflection;
using DbLinq.Util;

#if MONO_STRICT
using System.Data.Linq.Sugar;
using System.Data.Linq.Sugar.Expressions;
#else
using DbLinq.Data.Linq.Sugar;
using DbLinq.Data.Linq.Sugar.Expressions;
#endif



#if MONO_STRICT
namespace System.Data.Linq.Sugar.Implementation
#else
namespace DbLinq.Data.Linq.Sugar.Implementation
#endif
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
                    // the conditions to register and watch an entity are:
                    // - not null (can this happen?)
                    // - registered in the model
                    if (row != null && selectQuery.DataContext.Mapping.GetTable(row.GetType()) != null)
                    {
                        row = (T)selectQuery.DataContext.GetOrRegisterEntity(row);
                        // TODO: place updates in DataContext
                        //_vars.Table.CheckAttachment(current); // registers the object to be watched for updates
                        selectQuery.DataContext.MemberModificationHandler.Register(row, selectQuery.DataContext.Mapping);
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

        /// <summary>
        /// Runs an InsertQuery on a provided object
        /// </summary>
        /// <param name="target"></param>
        /// <param name="insertQuery"></param>
        public void Insert(object target, UpsertQuery insertQuery)
        {
            Upsert(target, insertQuery);
        }

        private void Upsert(object target, UpsertQuery insertQuery)
        {
            var sqlProvider = insertQuery.DataContext.Vendor.SqlProvider;
            using (var dbTransaction = insertQuery.DataContext.DatabaseContext.Transaction())
            using (var dbCommand = insertQuery.DataContext.DatabaseContext.Connection.CreateCommand())
            {
                dbCommand.CommandText = insertQuery.Sql;
                dbCommand.Transaction = dbTransaction.Transaction;
                foreach (var inputParameter in insertQuery.InputParameters)
                {
                    var dbParameter = dbCommand.CreateParameter();
                    dbParameter.ParameterName = sqlProvider.GetParameterName(inputParameter.Alias);
                    dbParameter.SetValue(inputParameter.GetValue(target), inputParameter.ValueType);
                    dbCommand.Parameters.Add(dbParameter);
                }
                if (insertQuery.DataContext.Vendor.SupportsOutputParameter)
                {
                    int outputStart = insertQuery.InputParameters.Count;
                    foreach (var outputParameter in insertQuery.OutputParameters)
                    {
                        var dbParameter = dbCommand.CreateParameter();
                        dbParameter.ParameterName = sqlProvider.GetParameterName(outputParameter.Alias);
                        // Oracle is lost if output variables are unitialized. Another winner story.
                        dbParameter.SetValue(null, outputParameter.ValueType);
                        dbParameter.Size = 100;
                        dbParameter.Direction = /*outputParameter.IsReturn ? ParameterDirection.ReturnValue :*/ ParameterDirection.Output;
                        dbCommand.Parameters.Add(dbParameter);
                    }
                    int rowsCount = dbCommand.ExecuteNonQuery();
                    for (int outputParameterIndex = 0;
                         outputParameterIndex < insertQuery.OutputParameters.Count;
                         outputParameterIndex++)
                    {
                        var outputParameter = insertQuery.OutputParameters[outputParameterIndex];
                        var outputDbParameter =
                            (IDbDataParameter)dbCommand.Parameters[outputParameterIndex + outputStart];
                        SetOutputParameterValue(target, outputParameter, outputDbParameter.Value);
                    }
                }
                else
                {
                    object result = dbCommand.ExecuteScalar();
                    if (insertQuery.OutputParameters.Count > 1)
                        throw new ArgumentException();
                    if (insertQuery.OutputParameters.Count == 1)
                        SetOutputParameterValue(target, insertQuery.OutputParameters[0], result);
                }
            }
        }

        protected virtual void SetOutputParameterValue(object target, ObjectOutputParameterExpression outputParameter, object value)
        {
            if (value is DBNull)
                outputParameter.SetValue(target, null);
            else
                outputParameter.SetValue(target, TypeConvert.To(value, outputParameter.ValueType));
        }

        /// <summary>
        /// Performs an update
        /// </summary>
        /// <param name="target">Entity to be flushed</param>
        /// <param name="updateQuery">SQL update query</param>
        /// <param name="modifiedMembers">List of modified members, or null to update all members</param>
        public void Update(object target, UpsertQuery updateQuery, IList<MemberInfo> modifiedMembers)
        {
            Upsert(target, updateQuery);
        }

        /// <summary>
        /// Performs a delete
        /// </summary>
        /// <param name="target">Entity to be deleted</param>
        /// <param name="deleteQuery">SQL delete query</param>
        public void Delete(object target, DeleteQuery deleteQuery)
        {
            var sqlProvider = deleteQuery.DataContext.Vendor.SqlProvider;
            using (var dbTransaction = deleteQuery.DataContext.DatabaseContext.Transaction())
            using (var dbCommand = deleteQuery.DataContext.DatabaseContext.Connection.CreateCommand())
            {
                dbCommand.CommandText = deleteQuery.Sql;
                dbCommand.Transaction = dbTransaction.Transaction;
                foreach (var inputParameter in deleteQuery.InputParameters)
                {
                    var dbParameter = dbCommand.CreateParameter();
                    dbParameter.ParameterName = sqlProvider.GetParameterName(inputParameter.Alias);
                    dbParameter.SetValue(inputParameter.GetValue(target), inputParameter.ValueType);
                    dbCommand.Parameters.Add(dbParameter);
                }
                int rowsCount = dbCommand.ExecuteNonQuery();
            }
        }
    }
}

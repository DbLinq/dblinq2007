﻿#region MIT license
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
using System.Linq.Expressions;
using DbLinq.Data.Linq.Sugar.Expressions;
using DbLinq.Util;

namespace DbLinq.Data.Linq.Sugar.Implementation
{
    partial class QueryBuilder
    {
        // SQLite:
        // IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false, Expression = null
        // INSERT INTO main.Products (CategoryID, Discontinued, ProductName, QuantityPerUnit) 
        //                  VALUES (@P1, @P2, @P3, @P4) ;SELECT last_insert_rowid()
        //
        // Ingres:
        // IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false, 
        //                       Expression = "next value for \"linquser\".\"products_seq\"")]
        // INSERT INTO linquser.products (categoryid, discontinued, productid, productname, quantityperunit) 
        //                  VALUES ($param_000001_param$, $param_000002_param$, 
        //                          next value for "linquser"."products_seq", $param_000004_param$, $param_000005_param$) 
        //
        // Oracle:
        // IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false, Expression = null
        // BEGIN 
        // INSERT INTO NORTHWIND."Products" ("CategoryID", "Discontinued", "ProductID", "ProductName", "QuantityPerUnit") 
        //                  VALUES (:P1, :P2, NORTHWIND."Products_SEQ.NextVal, :P4, :P5)
        //               ;SELECT NORTHWIND."Products_SEQ.CurrVal INTO :P3 FROM DUAL; END;
        //
        // PostgreSQL:
        // IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false, Expression = "nextval('\"Products_ProductID_seq\"')"
        // INSERT INTO public."Products" ("CategoryID", "Discontinued", "ProductName", "QuantityPerUnit") 
        //                  VALUES (:P1, :P2, :P3, :P4) 
        //               ;SELECT currval('"Products_ProductID_seq"')
        //
        // SQL Server (bogus):
        // IsPrimaryKey = true, IsDbGenerated = true
        // INSERT INTO [dbo].[Products] (, , , ) VALUES (@P1, @P2, @P3, @P4) 
        //                  ; SELECT @@IDENTITY
        //
        // Column:               default --> use value
        //          PK: Expression !null --> use parameter (Oracle is wrong here)
        //              Expression  null --> ignore
        // SQL: wrap clause with PK information

        /// <summary>
        /// Creates a query for insertion
        /// </summary>
        /// <param name="objectToInsert"></param>
        /// <param name="queryContext"></param>
        /// <returns></returns>
        public InsertQuery GetInsertQuery(object objectToInsert, QueryContext queryContext)
        {
            var rowType = objectToInsert.GetType();
            var table = queryContext.DataContext.Mapping.GetTable(rowType);
            var sqlProvider = queryContext.DataContext.Vendor.SqlProvider;
            var inputParameters = new List<ObjectInputParameterExpression>();
            var outputParameters = new List<ObjectOutputParameterExpression>();
            var columns = new List<string>();
            var columnsValues = new List<string>();
            var outputValues = new List<string>();
            var outputExpressions = new List<string>();
            foreach (var dataMember in table.RowType.PersistentDataMembers)
            {
                var column = sqlProvider.GetColumn(dataMember.MappedName);
                // if the column is generated, we may have:
                // - an explicit generation (Expression property is not null, so we add the column)
                // - an implicit generation (Expression property is null
                // in all cases, we want to get the value back
                if (dataMember.IsDbGenerated)
                {
                    if (dataMember.Expression != null)
                    {
                        columns.Add(column);
                        columnsValues.Add(dataMember.Expression);
                    }
                    var setObjectParameter = Expression.Parameter(dataMember.Type, "obj");
                    var setValueParameter = Expression.Parameter(dataMember.Type, "value");
                    var memberInfo = dataMember.Member;
                    var setter = (Expression<Action<object, object>>)((o, v) => memberInfo.SetMemberValue(o, v));
                    var writeLambda = Expression.Lambda(setter, setObjectParameter, setValueParameter);
                    var outputParameter = new ObjectOutputParameterExpression(writeLambda, dataMember.MappedName);
                    outputValues.Add(outputParameter.Alias);
                    outputExpressions.Add(dataMember.Expression);
                }
                else // standard column
                {
                    var getObjectParameter = Expression.Parameter(dataMember.Type, "obj");
                    var memberInfo = dataMember.Member;
                    var getter = (Expression<Func<object, object>>)(o => memberInfo.GetMemberValue(o));
                    var readLambda = Expression.Lambda(getter, getObjectParameter);
                    var inputParameter = new ObjectInputParameterExpression(
                        readLambda,
                        sqlProvider.GetParameterName(dataMember.Name));
                    columns.Add(column);
                    columnsValues.Add(inputParameter.Alias);
                    inputParameters.Add(inputParameter);
                }
            }
            var insertSql = sqlProvider.GetInsert(sqlProvider.GetTable(table.TableName), columns, columnsValues, outputValues, outputExpressions);
            return new InsertQuery(queryContext.DataContext, insertSql, inputParameters, outputParameters);
        }
    }
}

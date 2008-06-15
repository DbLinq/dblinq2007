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
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using DbLinq.Data.Linq.Sugar;
using DbLinq.Factory;
using DbLinq.Linq;
using DbLinq.Linq.Clause;
using DbLinq.Logging;
using DbLinq.Vendor;

namespace DbLinq.Util
{
    /// <summary>
    /// handles scalar calls: query.First, query.Last, and query.Count
    /// </summary>
    public class RowScalar<T>
    {
        public IQueryRunner QueryRunner { get; set; }
        SessionVarsParsed _vars;
        IEnumerable<T> _parentTable;

        public ILogger Logger { get; set; }

        public RowScalar(SessionVarsParsed vars, IEnumerable<T> parentTable)
        {
            QueryRunner = ObjectFactory.Get<IQueryRunner>();
            Logger = vars.Context.Logger;
            //don't modify the parent query with any additional clauses:
            _vars = vars;
            _parentTable = parentTable;
        }

        public S GetScalar<S>(Expression expression)
        {
            if (_vars.Query != null)
                return QueryRunner.Execute<S>(_vars.Query);

            MethodCallExpression exprCall = expression as MethodCallExpression;
            if (exprCall == null)
                throw new ApplicationException("L35: GetScalar<S> only prepared for MethodCall, not " + expression.NodeType);

            switch (exprCall.Method.Name)
            {
            case "First":
                {
                    if (typeof(S) != typeof(T))
                        throw new ApplicationException("L39: Not prepared for double projection");
                    //Microsoft syntax: "SELECT TOP 1 ProductId FROM Products"
                    //MySql syntax:     "SELECT ProductId FROM Products LIMIT 1"

                    //foreach(T t in _parentTable) //call GetEnumerator
                    var rowEnum = new RowEnumerator<S>(_vars);
                    foreach (S firstS in rowEnum)
                    {
                        return firstS;
                    }
                    throw new ApplicationException("First() failed, enumeration has no entries");
                }
            case "Count":
            case "Max":
            case "Min":
            case "Average":
            case "Sum":
                {
                    bool isAlreadyProjected = _parentTable is MTable_Projected<T>;
                    if (exprCall.Method.Name == "Count" && typeof(T) != typeof(S) && !isAlreadyProjected)
                    {
                        //Count is very similar to Min/Max/Sum (below),
                        //but unlike those, it may be asked to work on TableRows.
                        //in that case, we need to project to an int.
                        _vars.SqlParts.CountClause = "COUNT"; //Count or Max
                        //string varName = _vars.GetDefaultVarName(); //'$x'
                        string varName = "x$"; //TODO - get it from QueryProcessor
                        FromClauseBuilder.SelectAllFields(_vars, _vars.SqlParts, typeof(T), varName);

                        var rowEnum = new RowEnumerator<S>(_vars);
                        foreach (S firstS in rowEnum)
                        {
                            return firstS;
                        }
                        throw new ApplicationException("RowScalar.COUNT: Unable to advance to first result");
                    }

                    //during Average(), typeof(T)=int, typeof(S)=double.
                    var rowEnumerator = new RowEnumerator<S>(_vars);
                    using (IEnumerator<S> enumerator = rowEnumerator.GetEnumerator())
                    {
                        bool hasOne = enumerator.MoveNext();
                        if (!hasOne)
                            throw new InvalidOperationException("Max/Count() called on set with zero items");
                        S firstT = enumerator.Current;
                        bool hasTwo = enumerator.MoveNext();
                        if (hasTwo)
                            throw new InvalidOperationException("Max/Count() called on set with more than one item");

                        //return (S)(object)firstT; --throws InvalidCastExc?!

                        object objT = firstT;

                        //uh this is nasty ...  needs fixing
                        //note: SELECT COUNT(x) in MySql returns type uint
                        //C# would like an int
                        if (typeof(S) == typeof(int) && typeof(T) == typeof(uint))
                        {
                            uint firstU = (uint)objT;
                            int firstI = (int)firstU; //how can I invoke this cast to int dynamically?
                            objT = firstI;
                        }
                        return (S)objT;
                    }
                    //throw new ArgumentException("L51: Unprepared for Count");
                    //break;
                }
            case "Last":
                {
                    if (typeof(S) != typeof(T))
                        throw new ApplicationException("L58: Not prepared for double projection");
                    //TODO: can I use "LIMIT" to retrieve last row? or use ORDER BY? order by what column?
                    object lastObj = null;
                    foreach (T t in _parentTable) //call GetEnumerator
                    {
                        lastObj = t;
                    }
                    S lastS = (S)lastObj;
                    return lastS;
                }

            case "Single":
            case "SingleOrDefault":
                //QueryProcessor prepared a 'LIMIT 2' query, throw InvalidOperationException occurs
                {
                    //there are two types of Sequence.Single(), one passes an extra Lambda
                    //LambdaExpression lambdaParam = expression.XMethodCall().XParam(1).XLambda();
                    //MTable<T> table2 = _parentTable as MTable<T>;
                    //IEnumerator<T> enumerator;

                    if (exprCall.Method.Name == "Single" || exprCall.Method.Name == "SingleOrDefault" || exprCall.Method.Name == "First")
                    {
                        S cachedRow;
                        if (CacheChecker.TryRetrieveFromCache(_vars, expression, out cachedRow))
                            return cachedRow;
                    }

                    var rowEnumerator = new RowEnumerator<T>(_vars);
                    using (IEnumerator<T> enumerator = rowEnumerator.GetEnumerator())
                    {
                        //_vars.LimitClause = "LIMIT 2";
                        bool hasOne = enumerator.MoveNext();
                        if (!hasOne)
                        {
                            //no data? Single() will throw, whereas SingleOrDefault() allows null return
                            if (exprCall.Method.Name == "SingleOrDefault")
                                return default(S);
                            else
                                throw new InvalidOperationException("Single() called on set with zero items");
                        }
                        T firstT = enumerator.Current;
                        bool hasTwo = enumerator.MoveNext();
                        if (hasTwo)
                            throw new InvalidOperationException("Single() called on set with more than one item");
                        return (S)(object)firstT;
                    }
                }
            //break;
            }
            throw new ApplicationException("L68: GetScalar<S> TODO: methodName=" + exprCall.Method.Name);

        }
    }
}

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

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using DbLinq.Linq;
using DbLinq.Linq.Clause;
using DbLinq.Vendor;

namespace DbLinq.Util
{
    /// <summary>
    /// handles scalar calls: query.First, query.Last, and query.Count
    /// </summary>
    class RowScalar<T>
    {
        SessionVarsParsed _vars;
        IEnumerable<T> _parentTable;
        Dictionary<T, T> _liveObjectMap;

        public RowScalar(SessionVarsParsed vars, IEnumerable<T> parentTable, Dictionary<T, T> liveObjMap)
        {
            //don't modify the parent query with any additional clauses:
            _vars = vars;
            _parentTable = parentTable;
            _liveObjectMap = liveObjMap;
        }

        public S GetScalar<S>(Expression expression)
        {
            MethodCallExpression exprCall = expression as MethodCallExpression;
            if (exprCall == null)
                throw new ApplicationException("L35: GetScalar<S> only prepared for MethodCall, not " + expression.NodeType);

            Dictionary<S, S> liveObjectMapS = null;
            if (typeof(T) == typeof(S))
            {
                liveObjectMapS = (Dictionary<S, S>)(object)_liveObjectMap;
            }

            switch (exprCall.Method.Name)
            {
                case "First":
                    if (typeof(S) != typeof(T))
                        throw new ApplicationException("L39: Not prepared for double projection");
                    //Microsoft syntax: "SELECT TOP 1 ProductId FROM Products"
                    //MySql syntax:     "SELECT ProductId FROM Products LIMIT 1"

                    //foreach(T t in _parentTable) //call GetEnumerator
                    using (RowEnumerator<S> rowEnum = new RowEnumerator<S>(_vars, liveObjectMapS))
                    {
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
                            _vars._sqlParts.CountClause = "COUNT"; //Count or Max
                            //string varName = _vars.GetDefaultVarName(); //'$x'
                            string varName = "x$"; //TODO - get it from QueryProcessor
                            FromClauseBuilder.SelectAllFields(_vars, _vars._sqlParts, typeof(T), varName);

                            using (RowEnumerator<S> rowEnum = new RowEnumerator<S>(_vars, null))
                            {
                                foreach (S firstS in rowEnum)
                                {
                                    return firstS;
                                }
                                throw new ApplicationException("RowScalar.COUNT: Unable to advance to first result");
                            }
                        }

                        //during Average(), typeof(T)=int, typeof(S)=double.
                        using (IEnumerator<S> enumerator = new RowEnumerator<S>(_vars, null)
                            .GetEnumerator())
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
                        using (IEnumerator<T> enumerator = new RowEnumerator<T>(_vars, _liveObjectMap).GetEnumerator())
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

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
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using DbLinq.Util;
using DbLinq.Util.ExprVisitor;
using DbLinq.Linq.Mapping;
using DbLinq.Vendor;
namespace DbLinq.Linq.Clause
{
    /// <summary>
    /// ExpressionTreeParser part2 - MethodCall handling
    /// </summary>
    public partial class ExpressionTreeParser
    {
        internal void AnalyzeMethodCall(RecurData recurData, MethodCallExpression expr)
        {
            string methodName = expr.Method.Name;

            string sqlOperatorName;
            if (s_csharpOperatorToSqlMap.TryGetValue(methodName, out sqlOperatorName))
            {
                //map "op_Inequality" to " != "
                AnalyzeExpression(recurData, expr.Arguments[0]);
                _result.AppendString(sqlOperatorName);
                AnalyzeExpression(recurData, expr.Arguments[1]);
                return;
            }

            if (expr.Method.DeclaringType == typeof(string))
            {
                AnalyzeMethodCall_String(recurData, expr);
                return;
            }

            if (expr.Method.DeclaringType == typeof(DateTime))
            {
                AnalyzeMethodCall_DateTime(recurData, expr);
                return;
            }

            if (expr.Method.DeclaringType == typeof(System.Linq.Queryable) || expr.Method.DeclaringType == typeof(System.Linq.Enumerable))
            {
                AnalyzeMethodCall_Queryable(recurData, expr);
                return;
            }


            //special handling
            switch (expr.Method.Name)
            {
                case "Concat":
                    {
                        //this was discontinued after Linq 20006 preview
                        throw new ApplicationException("L581 Discontinued operand: Concat");
                    }
                default:
                    //detailed error will be thrown below
                    break;
            }

            //check if the function is a stored proc:
            FunctionAttribute functionAttrib = AttribHelper.GetFunctionAttribute(expr.Method);
            if (functionAttrib != null)
            {
                //it's a stored proc in the database
                _result.AppendString(functionAttrib.Name + "(");
                string comma = "";
                foreach (Expression functionArg in expr.Arguments)
                {
                    _result.AppendString(comma); comma = ",";
                    AnalyzeExpression(recurData, functionArg);
                }
                _result.AppendString(")");
                return;
            }

            //TODO: throw for any other method - database probably cannot handle such call
            string msg2 = "L274: Unprepared to map method " + methodName + " (" + expr + ") to SQL";
            Console.WriteLine(msg2);
            throw new ApplicationException(msg2);
            //_result.AppendString(expr.Method.Name);
        }

        /// <summary>
        /// handle Sum(), Contains(), Count() and other beasts from System.Linq.Enumerable/Queryable.
        /// </summary>
        internal void AnalyzeMethodCall_Queryable(RecurData recurData, MethodCallExpression expr)
        {
            if (expr.Method.DeclaringType == typeof(System.Linq.Queryable) || expr.Method.DeclaringType == typeof(System.Linq.Enumerable))
            {
            }
            else
            {
                throw new ArgumentException("Only Enumerable or Queryable methods allowed, not: " + expr.Method);
            }


            switch (expr.Method.Name)
            {
                case "Contains":
                    {
                        #region Contains()
                        AnalyzeExpression(recurData, expr.Arguments[1]); //p.ProductID
                        _result.AppendString(" IN ( ");
                        //AnalyzeExpression(recurData, expr.Arguments[0]); //{value(<>c__DisplayClass2).ids}
                        System.Collections.IEnumerable valueArray = null;
                        if (expr.Arguments[0].NodeType == ExpressionType.MemberAccess)
                        {
                            MemberExpression array1 = expr.Arguments[0].XMember();
                            ConstantExpression const1 = array1.Expression.XConstant();
                            object valueStruct = const1.Value;
                            System.Reflection.FieldInfo memberInfo = array1.Member as System.Reflection.FieldInfo;
                            object valueObj = memberInfo.GetValue(valueStruct);
                            //TODO instead of casting to array, process directly as IEnumerable
                            valueArray = (System.Collections.IEnumerable)valueObj;
                        }
                        else if (expr.Arguments[0].NodeType == ExpressionType.NewArrayInit)
                        {
                            //handle 'where new string[] { "ALFKI", "WARTH" }.Contains(xxx)'
                            NewArrayExpression newArrayExpr = expr.Arguments[0] as NewArrayExpression;
                            List<object> valueList = new List<object>();
                            foreach (Expression part in newArrayExpr.Expressions)
                            {
                                ConstantExpression const1 = part.XConstant();
                                object valueStruct = const1.Value;
                                valueList.Add(valueStruct);
                            }
                            valueArray = valueList;
                        }

                        string separator = "";
                        foreach (object obj in valueArray)
                        {
                            string objString = (obj == null || obj is string)
                                ? (obj as string).QuoteString_Safe()
                                : obj.ToString();

                            _result.AppendString(separator + objString);
                            separator = ",";
                        }
                        _result.AppendString(" ) ");
                        return;
                        #endregion
                    }

                case "Sum":
                    {
                        //extract 'OrderID' from '{g.Sum(o => Convert(o.OrderID))}'
                        Expression sumExpr1 = expr.Arguments[1].XLambda().Body;
                        MemberExpression sumExpr2 = null;
                        switch (sumExpr1.NodeType)
                        {
                            //case ExpressionType.Cast: //Cast disappeared in Beta2?!
                            //    sumExpr2 = sumExpr1.XCastOperand().XMember(); break;
                            case ExpressionType.MemberAccess:
                                sumExpr2 = sumExpr1.XMember(); break;
                            case ExpressionType.Convert:
                                //eg. {g.Sum(o => Convert(o.OrderID))} (from G08_OrderSumByCustomerID)
                                sumExpr2 = sumExpr1.XUnary().Operand.XMember();
                                break;
                            default:
                                throw new ArgumentException("L277 Sum(lambda): unprepared for lambda expr " + sumExpr1.NodeType);
                        }
                        _result.AppendString("SUM(" + sumExpr2.Member.Name + ")");
                        return;
                    }
                case "Count":
                    {
                        //given expr='{g.Count()}', produce Count expression
                        _result.AppendString("COUNT(*)");
                        return;
                    }
                case "Concat":
                    {
                        //this was discontinued after Linq 20006 preview
                        throw new ApplicationException("L581 Discontinued operand: Concat");
                    }
                case "Any":
                    {
                        //WARNING: this is still a hack
                        //TODO: All and Any are similar and should be handled by one function
                        //at the moment, they are handled by ProcessWhereClause_All and AnalyzeMethodCall_Queryable

                        Expression arg0 = expr.Arguments[0];
                        //this._parent._vars.SqlParts = new SqlExpressionParts(_parent._vars.Context.Vendor);
                        //_parent.ProcessScalarExpression();
                        var prevTablesUsed = _result.tablesUsed;
                        _result.tablesUsed = new Dictionary<Type, string>();
                        var prevJoins = _result.joins;
                        _result.joins = new List<string>();

                        recurData.allowSelectAllFields = false;
                        AnalyzeExpression(recurData, arg0);

                        KeyValuePair<Type, string> tablesUsedKv = _result.tablesUsed.First();
                        TableAttribute tableAttrib = AttribHelper.GetTableAttrib(tablesUsedKv.Key);
                        string tablename = tableAttrib.Name;
                        string nickname = tablesUsedKv.Value;

                        string clause = "( SELECT COUNT(*) FROM " + tablename + " AS " + nickname + " WHERE " + _result.joins[0] + " ) > 0";
                        _result.AppendString(clause);
                        _result.tablesUsed = prevTablesUsed;
                        _result.joins = prevJoins;
                        //this;
                        break;
                        //_parent.ProcessQuery();
                    }
                default:
                    //detailed error will be thrown below
                    throw new NotImplementedException("L210: unprepared for method " + expr.Method);
            }
        }

        /// <summary>
        /// handle string.Length, string.StartsWith etc
        /// </summary>
        internal void AnalyzeMethodCall_String(RecurData recurData, MethodCallExpression expr)
        {
            if (expr.Method.DeclaringType != typeof(string))
                throw new ArgumentException("Only string.X method allowed, not: " + expr.Method);

            switch (expr.Method.Name)
            {
                case "StartsWith":
                case "EndsWith":
                case "Contains":
                    {
                        //turn "e.Name.StartsWith("X")" -> "e.Name LIKE 'X%'
                        //turn "e.Name.Contains("X")" -> "e.Name LIKE '%X%'
                        AnalyzeExpression(recurData, expr.Object);
                        _result.AppendString(" LIKE ");
                        AnalyzeExpression(recurData, expr.Arguments[0]);
                        string paramName = _parent.lastParamName;
                        string lastParam = _parent.paramMap[paramName] as string;
                        if (lastParam != null)
                        {
                            //modify parameter from X to X%
                            string modParam = "";
                            switch (expr.Method.Name)
                            {
                                case "StartsWith": modParam = lastParam + "%"; break;
                                case "EndsWith": modParam = "%" + lastParam; break;
                                case "Contains": modParam = "%" + lastParam + "%"; break;
                            }
                            _parent.paramMap[paramName] = modParam;
                        }
                    }
                    return;
                case "Concat":
                    {
                        //this was discontinued after Linq 20006 preview
                        throw new ApplicationException("L581 Discontinued operand: Concat");
                    }
                case "ToLower":
                case "ToUpper":
                    {
                        string sqlFctName = expr.Method.Name == "ToLower" ? "LOWER(" : "UPPER(";
                        _result.AppendString(sqlFctName);
                        AnalyzeExpression(recurData, expr.Object);
                        _result.AppendString(")");
                        return;
                    }

                default:
                    //detailed error will be thrown below
                    throw new NotImplementedException("L266: unprepared for method " + expr.Method);
            }

        }
        /// <summary>
        /// handle string.Length, string.StartsWith etc
        /// </summary>
        internal void AnalyzeMethodCall_DateTime(RecurData recurData, MethodCallExpression expr)
        {
            if (expr.Method.DeclaringType != typeof(DateTime))
                throw new ArgumentException("Only string.X method allowed, not: " + expr.Method);

            switch (expr.Method.Name)
            {
                case "FromOADate":
                    {
                        //convert double to DateTime
                        _result.AppendString("CAST(");
                        AnalyzeExpression(recurData, expr.Arguments[0]); //it's a static fct - don't pass expr.Object
                        _result.AppendString(" as smalldatetime)");
                        return;
                    }
                case "ParseExact":
                    {
                        //compile a function that accepts one argument and does the following:
                        //DateTime.ParseExact(hireDate, "yyyy.MM.dd", CultureInfo.InvariantCulture)
                        FunctionReturningObject funcReturningObj;
                        var empty = new ParameterExpression[] { };
                        UnaryExpression castToObjExpr = Expression.Convert(expr, typeof(object));
                        LambdaExpression lambda = Expression.Lambda<FunctionReturningObject>(castToObjExpr, empty);
                        Delegate delg = lambda.Compile();
                        funcReturningObj = delg as FunctionReturningObject;
                        string paramName = _parent.storeFunctionParam(funcReturningObj);
                        _result.AppendString(paramName);
                        return;
                    }
                default:
                    throw new ApplicationException("L333 Unprepared to map DateTime method " + expr.Method.Name);
            }
        }

    }
}

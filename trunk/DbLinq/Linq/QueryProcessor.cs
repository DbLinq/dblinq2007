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
using System.Linq;
using System.Linq.Expressions;
using System.Data.Linq.Mapping;
using DbLinq.Factory;
using DbLinq.Linq.Clause;
using DbLinq.Logging;
using DbLinq.Util;
using DbLinq.Util.ExprVisitor;
using DbLinq.Vendor;

namespace DbLinq.Linq
{
    /// <summary>
    /// after all Lambdas (queries) are collected, GetEnumerator() is called.
    /// This results in a call to QueryProcess.GenerateQuery (below).
    /// QueryProcessor then calls ExpressionTreeParser to build SQL expression from parts
    /// </summary>
    public partial class QueryProcessor
    {
        internal readonly SessionVarsParsed _vars;

        /// <summary>
        /// there can be more than one select
        /// </summary>
        public LambdaExpression selectExpr;

        /// <summary>
        /// given 'table.Where(x => x>2).Where(y => y<10)',
        /// we need to store the 'x' nickname and drop the 'y'.
        /// </summary>
        public readonly Dictionary<Type, string> currentVarNames = new Dictionary<Type, string>();

        /// <summary>
        /// given 'group by c.CustomerID into g', we hold memberExprNicknames[c.CustomerID]='g'
        /// </summary>
        public readonly Dictionary<MemberExpression, string> memberExprNickames = new Dictionary<MemberExpression, string>();

        /// <summary>
        /// holds SQL parameters as they are being assigned, eg. ParametersMap['P0'] = 'London'
        /// </summary>
        public readonly Dictionary<string, object> paramMap = new Dictionary<string, object>();

        /// <summary>
        /// some parameters are not specified immediately, and we need to call a function to get the value -
        /// e.g. in 'where p.ProductName==otherProduct.ProductName'
        /// </summary>
        public readonly Dictionary<string, FunctionReturningObject> paramMap2 = new Dictionary<string, FunctionReturningObject>();

        public string lastParamName;

        /// <summary>
        /// eg. 'Select' or Join
        /// </summary>
        public string LastQueryName;

        public ILogger Logger { get; set; }

        internal QueryProcessor(SessionVarsParsed vars)
        {
            Logger = ObjectFactory.Get<ILogger>();
            _vars = vars;
        }


        public void ProcessScalarExpression()
        {
            if (_vars.ScalarExpression == null)
                return;

            Expression expr = _vars.ScalarExpression;

            MethodCallExpression exprCall = expr.XMethodCall();
            string methodName = exprCall != null ? exprCall.Method.Name : "Unknown_71";
            LambdaExpression lambdaParam = exprCall.XParam(1).XLambda();

            switch (methodName)
            {
                case "Count":
                case "Max":
                case "Min":
                case "Sum":
                    if (lambdaParam != null)
                    {
                        MethodCallExpression precedingSelectCall = _vars.ExpressionChain[_vars.ExpressionChain.Count - 1];
                        LambdaExpression precedingSelect = precedingSelectCall.Arguments[1].XLambda();
                        //change 'i=>2' into 'p=>ProductID>2'
                        lambdaParam = new DbLinq.Util.ExprVisitor.CountExpressionModifier(precedingSelect).Modify(lambdaParam).XLambda();
                    }

                    _vars.SqlParts.CountClause = methodName.ToUpper();
                    break;
                case "Average":
                    _vars.SqlParts.CountClause = "AVG";
                    break;
                case "Single":
                case "SingleOrDefault":
                    _vars.SqlParts.LimitClause = 2;
                    break;
                case "First":
                case "FirstOrDefault":
                    _vars.SqlParts.LimitClause = 1;
                    break;
            }

            //there are two forms of Single, one passes in a Where clause
            //same applies to Count, Max etc:
            if (lambdaParam != null)
            {
                ProcessWhereClause(lambdaParam);
            }

        }


        /// <summary>
        /// Post-process and build SQL string.
        /// </summary>
        public string BuildSqlString(Type T)
        {
            //eg. '$p' for user query "from p in db.products"
            if (_vars.SqlParts.IsEmpty())
            {
                //occurs when there no Where or Select expression, eg. 'from p in Products select p'
                //select all fields of target type:
                string varName = GetDefaultVarName(T); //'$x'
                FromClauseBuilder.SelectAllFields(_vars, _vars.SqlParts, T, varName);
                //PS. Should SqlParts not be empty here? Debug Clone() and AnalyzeLambda()
            }

            //string sql = _vars.SqlParts.ToString();
            string sql = _vars.Context.Vendor.BuildSqlString(_vars.SqlParts);

            if (_vars.Context.Log != null)
                _vars.Context.Log.WriteLine("SQL: " + sql);

            _vars.SqlString = sql;
            return sql;
        }

        /// <summary>
        /// traverse expression and extract various selectExpr, orderByExpr, sqlParts, etc
        /// </summary>
        /// <param name="expr"></param>
        public void ProcessQuery(MethodCallExpression exprCall)
        {
            string methodName = exprCall.Method.Name;

            //LambdaExpression lambda = WhereClauseBuilder.FindLambda(expr, out methodName);
            LambdaExpression lambda = exprCall.Arguments.Count > 1
                ? exprCall.Arguments[1].XLambda()
                : null; //for Distinct(), we have no lambda (see F10_DistinctCity)

            LastQueryName = methodName;

            switch (methodName)
            {
                case "Where":
                    ProcessWhereClause(lambda);
                    return;
                case "GroupBy":
                    ProcessGroupByCall(exprCall);
                    return;
                case "GroupJoin": //occurs in LinqToSqlJoin10()
                    ProcessGroupJoin(exprCall);
                    return;
                case "Select":
                    ProcessSelectClause(lambda);
                    return;
                case "SelectMany":
                    ProcessSelectMany(exprCall);
                    return;
                case "OrderBy":
                case "ThenBy":
                    ProcessOrderByClause(lambda, null);
                    return;
                case "OrderByDescending":
                    ProcessOrderByClause(lambda, "DESC"); // TODO --> IVendor
                    return;
                case "Join":
                    ProcessJoinClause(exprCall);
                    return;
                case "Take":
                case "Skip":
                    {
                        ConstantExpression howMany = exprCall.XParam(1).XConstant();
                        if (howMany == null)
                            throw new ArgumentException("Take(),Skip() must come with ConstExpr");
                        if (methodName == "Skip")
                            _vars.SqlParts.OffsetClause = (int)howMany.Value;
                        else
                            _vars.SqlParts.LimitClause = (int)howMany.Value;
                    }
                    return;
                case "Distinct":
                    _vars.SqlParts.DistinctClause = "DISTINCT"; // TODO --> IVendor
                    return;
                case "Union":
                    string ss = exprCall.ToString();
                    ProcessUnionClause(null);
                    return;
                default:
                    Logger.Write(Level.Error, "################# L308 TODO " + methodName);
                    throw new InvalidOperationException("L311 Unprepared for Method " + methodName);
            }
        }

        /// <summary>
        /// Look at selectExpr or whereExpr, return e.g. '$c'
        /// TODO - needs to be processed earlier, at ProcessLambda() time.
        /// </summary>
        public string GetDefaultVarName(Type t)
        {
            string sqlVarName;
            if (this.currentVarNames.TryGetValue(t, out sqlVarName))
                return sqlVarName;

            return VarName.GetSqlName("x"); //if no expressions, provide fallback
        }

        /// <summary>
        /// given 'o.Customer', return previously assigned nickname 'o$' (or join$)
        /// </summary>
        public string NicknameRequest(MemberExpression memberExpr)
        {
            string nick;
            if (memberExprNickames.TryGetValue(memberExpr, out nick))
                return nick;
            return "join";
        }

        public string NicknameRequest(Expression expr, AssociationAttribute assoc1)
        {
            //TODO this needs fixing
            return "join";
        }

        public string storeParam(string value)
        {
            int count = paramMap.Count + paramMap2.Count;
            string paramName = _vars.Context.Vendor.GetParameterName(count);
            paramMap[paramName] = value;
            lastParamName = paramName;
            return paramName;
        }

        /// <summary>
        /// store a reference to 'localProduct.ProductName'.
        /// Before calling SQL, we will call a delegate 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string storeFunctionParam(FunctionReturningObject funcReturningObject)
        {
            int count = paramMap.Count + paramMap2.Count;
            string paramName = _vars.Context.Vendor.GetParameterName(count);
            paramMap2[paramName] = funcReturningObject;
            lastParamName = paramName;
            return paramName;
        }

        /// <summary>
        /// replace {g.Key} with GroupByExpression={o.CustomerID} --if key was not composite
        /// replace {g.Key.CustomerID} with GroupByExpression={o.CustomerID} --if key had multiple fields
        /// </summary>
        /// <returns></returns>
        public Expression SubstitueGroupKeyExpression(MemberExpression memberExpr)
        {
            if (memberExpr.Expression.NodeType == ExpressionType.MemberAccess)
            {
                //handle case {g.Key.CustomerID} - test case G03_DoubleKey()
                NewExpression newEx = _vars.GroupByExpression.Body.XNew(); //{new <>f__AnonymousTypef`2(CustomerID = o.CustomerID, EmployeeID = o.EmployeeID)}
                foreach (Expression ex1 in newEx.Arguments)
                {
                    if (ex1.NodeType != ExpressionType.MemberAccess)
                        continue;
                    MemberExpression ex1Member = ex1.XMember();
                    //if (ex1Member.Member == memberExpr.Member)
                    //    return ex1Member; //oops cannot compare directly - declaring type differs
                    if (ex1Member.Member.Name == memberExpr.Member.Name)
                        return ex1Member;
                }
                throw new ApplicationException("L294 Cannot find Member " + memberExpr.Member + " in groupBy clause");
                //ParameterExpression paramexpr = _vars.GroupByExpression.Body.XNew().XMember().XParam(); //{o}
                //MemberExpression memberExpr2 = Expression.MakeMemberAccess(paramexpr, memberExpr.Member);
                //return memberExpr2; //return part of composite key {o.CustomerID}
            }
            else
            {
                //handle case {g.Key}
                return _vars.GroupByExpression.Body; //return non composite key {o.CustomerID}
            }
        }

    }
}

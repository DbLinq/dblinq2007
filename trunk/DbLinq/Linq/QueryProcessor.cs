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
using DBLinq.Linq.clause;
using DBLinq.util;
using DBLinq.vendor;

namespace DBLinq.Linq
{
    /// <summary>
    /// after all Lambdas (queries) are collected, GetEnumerator() is called.
    /// This results in a call to QueryProcess.ProcessLambdas (below).
    /// QueryProcessor then calls ExpressionTreeParser to build SQL expression from parts
    /// </summary>
    public partial class QueryProcessor
    {
        static IVendor s_vendor = VendorFactory.Make();

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
        /// holds SQL parameters as they are being assigned, eg. paramMap['P0'] = 'London'
        /// </summary>
        public readonly Dictionary<string, object> paramMap = new Dictionary<string, object>();

        public string lastParamName;

        /// <summary>
        /// eg. 'Select' or Join
        /// </summary>
        public string lastQueryName;

        private QueryProcessor(SessionVarsParsed vars)
        {
            _vars = vars;
        }


        /// <summary>
        /// main method, which processes expressions, compiles, and puts together our SQL string.
        /// </summary>
        /// <param name="vars"></param>
        public static SessionVarsParsed ProcessLambdas(SessionVars vars, Type T)
        {
            SessionVarsParsed varsFin = new SessionVarsParsed(vars);
            QueryProcessor qp = new QueryProcessor(varsFin); //TODO

            foreach (MethodCallExpression expr in vars.expressionChain)
            {
                qp.processQuery(expr);
            }

            if (qp.lastQueryName == "GroupBy")
                throw new InvalidOperationException("L98 GroupBy must by followed by an aggregate expression, such as Count or Max");

            qp.processScalarExpression();

            qp.build_SQL_string(T);

            return varsFin;
        }

        void processScalarExpression()
        {
            if (_vars.scalarExpression == null)
                return;

            Expression expr = _vars.scalarExpression;

            MethodCallExpression exprCall = expr.XMethodCall();
            string methodName = exprCall != null ? exprCall.Method.Name : "Unknown_71";
            
            switch (methodName)
            {
                case "Count":
                case "Max":
                case "Min":
                case "Sum":
                    _vars._sqlParts.countClause = methodName.ToUpper();
                    break;
                case "Average":
                    _vars._sqlParts.countClause = "AVG";
                    break;
                case "Single":
                case "SingleOrDefault":
                    _vars._sqlParts.limitClause = 2;
                    break;
                case "First":
                case "FirstOrDefault":
                    _vars._sqlParts.limitClause = 1;
                    break;
            }

            //there are two forms of Single, one passes in a Where clause
            //same applies to Count, Max etc:
            LambdaExpression lambdaParam = exprCall.XParam(1).XLambda();
            if (lambdaParam != null)
            {
                processWhereClause(lambdaParam);
            }

        }


        /// <summary>
        /// Post-process and build SQL string.
        /// </summary>
        string build_SQL_string(Type T)
        {
            //eg. '$p' for user query "from p in db.products"
            if (_vars._sqlParts.IsEmpty())
            {
                //occurs when there no Where or Select expression, eg. 'from p in Products select p'
                //select all fields of target type:
                string varName = GetDefaultVarName(T); //'$x'
                FromClauseBuilder.SelectAllFields(_vars, _vars._sqlParts, T, varName);
                //PS. Should _sqlParts not be empty here? Debug Clone() and AnalyzeLambda()
            }

            string sql = _vars._sqlParts.ToString();

            if (_vars.context.Log!=null)
                _vars.context.Log.WriteLine("SQL: " + sql);

            _vars.sqlString = sql;
            return sql;
        }

        /// <summary>
        /// traverse expression and extract various selectExpr, orderByExpr, sqlParts, etc
        /// </summary>
        /// <param name="expr"></param>
        public void processQuery(MethodCallExpression exprCall)
        {
            string methodName = exprCall.Method.Name;

            //LambdaExpression lambda = WhereClauseBuilder.FindLambda(expr, out methodName);
            LambdaExpression lambda = exprCall.Arguments.Count > 1
                ? exprCall.Arguments[1].XLambda()
                : null; //for Distinct(), we have no lambda (see F10_DistinctCity)
            
            lastQueryName = methodName;

            switch (methodName)
            {
                case "Where":
                    processWhereClause(lambda);
                    return;
                case "GroupBy":
                    processGroupByCall(exprCall);
                    return;
                case "GroupJoin": //occurs in LinqToSqlJoin10()
                    processGroupJoin(exprCall);
                    return;
                case "Select":
                    processSelectClause(lambda);
                    return;
                case "SelectMany":
                    processSelectMany(exprCall);
                    return;
                case "OrderBy":
                case "ThenBy":
                    processOrderByClause(lambda, null); 
                    return;
                case "OrderByDescending":
                    processOrderByClause(lambda, "DESC"); 
                    return;
                case "Join":
                    processJoinClause(exprCall); 
                    return;
                case "Take":
                case "Skip":
                    {
                        ConstantExpression howMany = exprCall.XParam(1).XConstant();
                        if (howMany == null)
                            throw new ArgumentException("Take(),Skip() must come with ConstExpr");
                        if (methodName == "Skip")
                            _vars._sqlParts.offsetClause = (int)howMany.Value;
                        else
                            _vars._sqlParts.limitClause = (int)howMany.Value;
                    }
                    return;
                case "Distinct":
                    _vars._sqlParts.distinctClause = "DISTINCT";
                    return;
                default:
                    Console.WriteLine("################# L308 TODO " + methodName);
                    Console.WriteLine("################# L308 TODO " + methodName);
                    throw new InvalidOperationException("L311 Unprepared for Method "+methodName);
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
            int count = paramMap.Count;
            string paramName = s_vendor.ParamName(count);
            paramMap[paramName] = value;
            lastParamName = paramName;
            return paramName;
        }

        /// <summary>
        /// replace {g.Key} with groupByExpr={o.CustomerID} --if key was not composite
        /// replace {g.Key.CustomerID} with groupByExpr={o.CustomerID} --if key had multiple fields
        /// </summary>
        /// <returns></returns>
        public Expression SubstitueGroupKeyExpression(MemberExpression memberExpr)
        {
            if (memberExpr.Expression.NodeType == ExpressionType.MemberAccess)
            {
                //handle case {g.Key.CustomerID} - test case G03_DoubleKey()
                NewExpression newEx = _vars.groupByExpr.Body.XNew(); //{new <>f__AnonymousTypef`2(CustomerID = o.CustomerID, EmployeeID = o.EmployeeID)}
                foreach (Expression ex1 in newEx.Arguments)
                {
                    if(ex1.NodeType!=ExpressionType.MemberAccess) 
                        continue;
                    MemberExpression ex1Member = ex1.XMember();
                    //if (ex1Member.Member == memberExpr.Member)
                    //    return ex1Member; //oops cannot compare directly - declaring type differs
                    if (ex1Member.Member.Name == memberExpr.Member.Name)
                        return ex1Member;
                }
                throw new ApplicationException("L294 Cannot find Member " + memberExpr.Member + " in groupBy clause");
                //ParameterExpression paramexpr = _vars.groupByExpr.Body.XNew().XMember().XParam(); //{o}
                //MemberExpression memberExpr2 = Expression.MakeMemberAccess(paramexpr, memberExpr.Member);
                //return memberExpr2; //return part of composite key {o.CustomerID}
            }
            else
            {
                //handle case {g.Key}
                return _vars.groupByExpr.Body; //return non composite key {o.CustomerID}
            }
        }

    }
}

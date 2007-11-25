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
using DBLinq.Linq.clause;
using DBLinq.util;

namespace DBLinq.Linq
{
    /// <summary>
    /// after all Lambdas are collected, and GetEnumerator() is called:
    /// QueryProcessor calls ExpressionTreeParser to build SQL expression from parts
    /// </summary>
    public partial class QueryProcessor
    {
        readonly SessionVarsParsed _vars;

        /// <summary>
        /// there can be more than one select
        /// </summary>
        public LambdaExpression selectExpr;

        /// <summary>
        /// given 'table.Where(x => x>2).Where(y => y<10)',
        /// we need to store the 'x' nickname and drop the 'y'.
        /// </summary>
        public Dictionary<Type, string> currentVarNames = new Dictionary<Type, string>();


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
            //if (vars.sqlString != null)
            //    return null; //we have already processed expressions (perhaps via GetQueryText)
            SessionVarsParsed varsFin = new SessionVarsParsed(vars);
            QueryProcessor qp = new QueryProcessor(varsFin); //TODO

            foreach (MethodCallExpression expr in vars.expressionChain)
            {
                qp.processQuery(expr);
            }

            qp.processScalarExpression();

            qp.build_SQL_string(T);

            return varsFin; //TODO
        }

        void processScalarExpression()
        {
            if (_vars.scalarExpression == null)
                return;

            Expression previousExpr = _vars.expressionChain.Count==0 
                ? null
                :_vars.expressionChain[_vars.expressionChain.Count - 1];

            string previousExprName = previousExpr.XLambdaName();

            Expression expr = _vars.scalarExpression;

            MethodCallExpression exprCall = expr.XMethodCall();
            string methodName = exprCall != null ? exprCall.Method.Name : "Unknown_71";
            
            if (previousExprName == "GroupBy")
                throw new InvalidOperationException("GroupBy must by followed by Select, not " + methodName);

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
                //StoreLambda("Where", lambdaParam);
                if(lambdaParam.Parameters.Count>0 && _vars.expressionChain.Count>0)
                {
                    Expression lastEx = _vars.expressionChain[_vars.expressionChain.Count-1];
                    string exprCatg = previousExpr.XLambdaName();
                }
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
            //huh - in case of "(db.Products).Take(5)", there is no lambda?
            //same for "(db.Products).Distinct()", there is no lambda.
            string methodName = exprCall.Method.Name;

            //LambdaExpression lambda = WhereClauseBuilder.FindLambda(expr, out methodName);
            LambdaExpression lambda = exprCall.Arguments.Count > 1
                ? exprCall.Arguments[1].XLambda()
                : null; //for Distinct(), we have no lambda (see F10_DistinctCity)

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

            //if(selectExpr!=null)
            //    return VarName.GetSqlName(selectExpr.Parameters[0].Name);
            //if (whereExpr.Count > 0)
            //    return VarName.GetSqlName(whereExpr[0].Parameters[0].Name);
            //if (orderByExpr.Count > 0)
            //    return VarName.GetSqlName(orderByExpr[0].Parameters[0].Name);
            return VarName.GetSqlName("x"); //if no expressions, provide fallback
        }

    }
}

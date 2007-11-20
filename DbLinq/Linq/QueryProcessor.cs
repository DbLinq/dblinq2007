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
        readonly WhereClauseBuilder _whereBuilder; // = new WhereClauseBuilder();

        /// <summary>
        /// there can be more than one select
        /// </summary>
        public LambdaExpression selectExpr;

#if REMOVED
        /// <summary>
        /// there can be more than one Where clause
        /// </summary>
        public List<LambdaExpression> whereExpr = new List<LambdaExpression>();
#endif
        /// <summary>
        /// given 'table.Where(x => x>2).Where(y => y<10)',
        /// we need to store the 'x' nickname and drop the 'y'.
        /// </summary>
        public Dictionary<Type, string> currentVarNames = new Dictionary<Type, string>();

#if REMOVED
        /// <summary>
        /// OrderBy goes first, ThenBy next
        /// </summary>
        public List<LambdaExpression> orderByExpr = new List<LambdaExpression>();
        public string orderBy_desc;
#endif

        private QueryProcessor(SessionVarsParsed vars)
        {
            _vars = vars;
            //TODO - pass in either vars or a delegate which allows asking for nickname for 'o.Customer'
            _whereBuilder = new WhereClauseBuilder(vars._sqlParts);
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

            foreach (Expression expr in vars.expressionChain)
            {
                qp.StoreQuery(expr);
            }

            qp.processScalarExpression();

            qp.processExpressions();
            qp.build_SQL_string(T);

            return varsFin; //TODO
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
                    string exprCatg = lastEx.XLambdaName();
                }
                processWhereClause(lambdaParam);
            }

        }

        void processExpressions()
        {
            ParseResult result = null;
#if REMOVED
            foreach(LambdaExpression lambda in whereExpr)
            {
                ParseInputs inputs = new ParseInputs(result);
                result = ExpressionTreeParser.Parse(lambda.Body, inputs);

                if (GroupHelper.IsGrouping(lambda.Parameters[0].Type))
                {
                    _vars._sqlParts.AddHaving(result.columns);
                }
                else
                {
                    _vars._sqlParts.AddWhere(result.columns);
                }

                result.CopyInto(_vars._sqlParts); //transfer params and tablesUsed
            }
#endif

            //Note: processing of groupByExpr populates SELECT columns.
            //make sure they are not added twice, when selectExpr is processed.
            if(_vars.groupByExpr!=null)
            {
                ParseInputs inputs = new ParseInputs(result);
                //inputs.groupByExpr = _vars.groupByExpr;
                result = ExpressionTreeParser.Parse(this, _vars.groupByExpr.Body, inputs);
                string groupByFields = string.Join(",", result.columns.ToArray());
                _vars._sqlParts.groupByList.Add(groupByFields);

                if (selectExpr == null //&& _vars.groupByNewExpr==null
                    )
                {
                    //manually add "SELECT c.City"
                    _vars._sqlParts.AddSelect(result.columns);

                    result.CopyInto(_vars._sqlParts); //transfer params and tablesUsed
                }

                if(_vars.groupByNewExpr==null && selectExpr==null)
                {
                    //eg. 'db.Customers.GroupBy( c=>c.City )' - select entire Customer
                    ParameterExpression paramEx = _vars.groupByExpr.Parameters[0];
                    FromClauseBuilder.SelectAllFields(_vars,_vars._sqlParts,paramEx.Type,VarName.GetSqlName(paramEx.Name));
                }
                else  if (_vars.groupByNewExpr != null)
                {
                    inputs = new ParseInputs(result);
                    //inputs.groupByExpr = _vars.groupByExpr;
                    result = ExpressionTreeParser.Parse(this, _vars.groupByNewExpr.Body, inputs);
                    _vars._sqlParts.AddSelect(result.columns);
                    result.CopyInto(_vars._sqlParts); //transfer params and tablesUsed
                }
            }

#if REMOVED
            foreach(LambdaExpression orderByExpr_ in orderByExpr)
            {
                ParseInputs inputs = new ParseInputs(result);
                result = ExpressionTreeParser.Parse(this, orderByExpr_.Body, inputs);
                string orderByFields = string.Join(",", result.columns.ToArray());
                _vars._sqlParts.orderByList.Add(orderByFields);
                _vars._sqlParts.orderBy_desc = orderBy_desc; //copy 'DESC' specifier
            }

            if(selectExpr!=null)
            {
                ParseInputs inputs = new ParseInputs(result);
                inputs.groupByExpr = _vars.groupByExpr;
                if (selectExpr.Body.NodeType == ExpressionType.Parameter)
                {
                    //'from p in Products select p' - do nothing, will result in SelectAllFields() later
                }
                else
                {
                    result = ExpressionTreeParser.Parse(this, selectExpr.Body, inputs);
                    _vars._sqlParts.AddSelect(result.columns);
                    result.CopyInto(_vars._sqlParts); //transfer params and tablesUsed
                }
            }
#endif

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
        public void StoreQuery(Expression expr)
        {
            //huh - in case of "(db.Products).Take(5)", there is no lambda?
            //same for "(db.Products).Distinct()", there is no lambda.
            string methodName;
            MethodCallExpression exprCall = expr.XMethodCall();
            if (exprCall != null && exprCall.Method.Name == "GroupBy")
            {
                //special case: GroupBy can come with 2 or 3 params
                switch (exprCall.Arguments.Count)
                {
                    case 2: //'group o by o.CustomerID into g'
                        StoreLambda("GroupBy", exprCall.Arguments[1].XLambda());
                        return;
                    case 3: //'group new {c.PostalCode, c.ContactName} by c.City into g'
                        StoreLambda("GroupBy", exprCall.Arguments[1].XLambda());
                        _vars.groupByNewExpr = exprCall.Arguments[2].XLambda();
                        return;
                    default:
                        throw new ApplicationException("StoreQuery L117: Prepared only for 2 or 3 param GroupBys");
                }
            }
            if (exprCall != null && exprCall.Method.Name == "SelectMany")
            {
                //special case: GroupBy can come with 2 or 3 params
                switch (exprCall.Arguments.Count)
                {
                    case 2: //???
                        StoreLambda("SelectMany", exprCall.Arguments[1].XLambda());
                        return;
                    case 3: //'from c in db.Customers from o in c.Orders where c.City == "London" select new { c, o }'
                        //ignore arg[0]: MTable<>
                        //ignore arg[1]: c=>c.Orders

                        LambdaExpression lambda1 = exprCall.Arguments[1].XLambda();
                        LambdaExpression lambda2 = exprCall.Arguments[2].XLambda();
                        {
                            MemberExpression memberExpression = lambda1.Body as MemberExpression;
                            ParameterExpression paramExpression = lambda2.Parameters[1];
                            ParseResult result = new ParseResult(null);
                            JoinBuilder.AddJoin1(memberExpression, paramExpression, result);
                            result.CopyInto(_vars._sqlParts);
                        }

                        //StoreLambda("Select", lambda2);
                        processSelectClause(lambda2);
                        return;
                    default:
                        throw new ApplicationException("StoreQuery L117: Prepared only for 2 or 3 param GroupBys");
                }
            }


            LambdaExpression lambda = WhereClauseBuilder.FindLambda(expr, out methodName);

            switch (methodName)
            {
                case "Where":
                    processWhereClause(lambda); return;
                case "Select":
                    processSelectClause(lambda); return;
                case "OrderBy":
                case "ThenBy":
                    processOrderByClause(lambda, null); return;
                case "OrderByDescending":
                    processOrderByClause(lambda, "DESC"); return;
            }

            //first, handle special cases, which have no lambdas: Take,Skip, Distinct
            if (methodName == "Take")
            {
                Expression howMany = expr.XMethodCall().XParam(1);
                if (!(howMany is ConstantExpression))
                    throw new ArgumentException("Take() must come with ConstExpr");
                ConstantExpression howMany2 = (ConstantExpression)howMany;
                _vars._sqlParts.limitClause = (int)howMany2.Value;
            }
            else if (methodName == "Skip")
            {
                Expression howMany = expr.XMethodCall().XParam(1);
                if (!(howMany is ConstantExpression))
                    throw new ArgumentException("Skip() must come with ConstExpr");
                ConstantExpression howMany2 = (ConstantExpression)howMany;
                _vars._sqlParts.offsetClause = (int)howMany2.Value;
            }
            else if (methodName == "Distinct")
            {
                _vars._sqlParts.distinctClause = "DISTINCT";
            }
            else
            {
                //then, handle regular clauses containing lambdas
                StoreLambda(methodName, lambda);
            }
        }

        void StoreLambda(string methodName, LambdaExpression lambda)
        {
            //From the C# spec: Specifically, query expressions are translated 
            //into invocations of methods named 
            //  Where, Select, SelectMany, Join, GroupJoin, OrderBy, OrderByDescending, 
            //  ThenBy, ThenByDescending, GroupBy, and Cast
            switch (methodName)
            {
#if REMOVED
                case "Where":
                    whereExpr.Add(lambda); break;
                case "Select":
                    selectExpr = lambda;
                    //necesary for projections?
                    if (_vars.groupByExpr == null)
                    {
                        _vars.projectionData = ProjectionData.FromSelectExpr(selectExpr);
                    }
                    else
                    {
                        _vars.projectionData = ProjectionData.FromSelectGroupByExpr(selectExpr, _vars.groupByExpr, _vars._sqlParts);
                    }
                    break;
#endif
                case "SelectMany":
                    //break the SelectMany beast into constituents
                    StoreSelectManyLambda(lambda);
                    break;
#if REMOVED
                case "OrderBy":
                    orderByExpr.Add(lambda); orderBy_desc = null; break;
                case "OrderByDescending":
                    orderByExpr.Add(lambda); orderBy_desc = "DESC"; break;
                case "ThenBy":
                    orderByExpr.Add(lambda); orderBy_desc = null; break;
#endif
                case "GroupBy":
                    _vars.groupByExpr = lambda;
                    break;

                //experiment:
                case "Take":
                    break;

                //2007-Nov: Scalar expressions now enter here
                case "Count":
                    break;
                case "Single":
                    //attempt to retrieve 2 rows, in GetScalar() we will throw if more than one arrives
                    _vars._sqlParts.limitClause = 2; 
                    break;

                default:
                    throw new ApplicationException("L109: method " + methodName + " not supported yet");
            }
        }

        private void StoreSelectManyLambda(LambdaExpression lambda)
        {
            selectExpr = lambda;
            //there is an inner Where and inner Select
            Expression exInner = lambda.Body.XCastOperand();
            //exInner={c.Orders.Where(o => op_Equality(c.City, "London")).Select(o => new {c = c, o = o})}
            MethodCallExpression exInnFct = exInner.XMethodCall();
            if (exInnFct == null)
                throw new ArgumentException("StoreSelMany L257 bad args");
            //each parameter is a lambda
            foreach (Expression innerLambda in exInnFct.Arguments)
            {
                if (innerLambda.NodeType == ExpressionType.Lambda)
                {
                    //eg. {o => new {c = c, o = o}}'
                    selectExpr = innerLambda as LambdaExpression;
                }
                else
                {
                    //eg. '{c.Orders.Where(o => op_Equality(c.City, "London"))}'
                    MethodCallExpression whereCall = innerLambda.XMethodCall();
                    string methodName2 = whereCall.Method.Name;

                    MemberExpression memberExpressionCache = null; //'c.Orders', needed below for nicknames
                    ParameterExpression paramExpressionCache = null; //'o', for nicknames

                    foreach (Expression whereCallParam in whereCall.Arguments)
                    {
                        if (whereCallParam.NodeType == ExpressionType.Convert)
                        {
                            //eg. '{c.Orders}'
                            //CHANGED: - join now handled below
                            Expression memberEx = whereCallParam.XCastOperand();
                            memberExpressionCache = memberEx.XMember();
                            //ParameterExpression memExParm = memberExpressionCache.Expression.XParam(); //'c'

                            ////cook up delgType: c=>MSet<Order>
                            //Type funcType1 = typeof(System.Query.Func<int,int>);
                            //Type funcType2 = funcType1.GetGenericTypeDefinition();
                            //Type funcType3 = funcType2.MakeGenericType(memExParm.Type, memberEx.Type);
                            //Type delgType = funcType3;

                            //List<ParameterExpression> fakeParam = new List<ParameterExpression>
                            //{ memExParm };
                            //LambdaExpression fakeJoinLambda = Expression.Lambda(delgType, memberEx, fakeParam);
                            //this.whereExpr.Add(fakeJoinLambda);
                        }
                        else if (whereCallParam.NodeType == ExpressionType.Lambda)
                        {
                            //{o => op_Equality(c.City, "London")}
                            LambdaExpression innerWhereLambda = whereCallParam as LambdaExpression;
                            StoreLambda(methodName2, innerWhereLambda);
                            paramExpressionCache = innerWhereLambda.Parameters[0];
                        }
                        else
                        {
                            //StoreLambda(methodName2, null);
                        }
                    }

                    //assign nikname mapping c.Orders=o
                    if (memberExpressionCache != null && paramExpressionCache != null)
                    {
                        //memberExprNickames[memberExpressionCache] = paramExpressionCache.Name;
                        ParseResult result = new ParseResult(null);
                        JoinBuilder.AddJoin1(memberExpressionCache, paramExpressionCache, result);
                        result.CopyInto(_vars._sqlParts);
                    }

                }
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

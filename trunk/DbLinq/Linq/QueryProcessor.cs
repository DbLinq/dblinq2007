////////////////////////////////////////////////////////////////////
//Initial author: Jiri George Moudry, 2006.
//License: LGPL. (Visit http://www.gnu.org)
//Commercial code may call into this library, if it's in a different module (DLL)
////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using DBLinq.Linq.clause;
using DBLinq.util;

namespace DBLinq.Linq
{
    /// <summary>
    /// after all Lambdas are collected: 
    /// 1) build SQL expression from parts, 
    /// and 2) compile row enum reader
    /// </summary>
    class QueryProcessor
    {
        readonly SessionVars _vars;
        readonly WhereClauseBuilder _whereBuilder; // = new WhereClauseBuilder();

        private QueryProcessor(SessionVars vars)
        {
            _vars = vars;
            //TODO - pass in either vars or a delegate which allows asking for nickname for 'o.Customer'
            _whereBuilder = new WhereClauseBuilder(vars._sqlParts);
        }


        /// <summary>
        /// we call 'go' method, which puts together our SQL string.
        /// </summary>
        /// <param name="vars"></param>
        public static void ProcessLambdas(SessionVars vars, Type T)
        {
            if (vars.sqlString != null)
                return; //we have already processed expressions (perhaps via GetQueryText)
            QueryProcessor qp = new QueryProcessor(vars);
            qp.processExpressions();
            qp.build_SQL_string(T);
        }

        void processExpressions()
        {
            ParseResult result = null;
            foreach(LambdaExpression lambda in _vars.whereExpr)
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

            //Note: processing of groupByExpr populates SELECT columns.
            //make sure they are not added twice, when selectExpr is processed.
            if(_vars.groupByExpr!=null)
            {
                ParseInputs inputs = new ParseInputs(result);
                //inputs.groupByExpr = _vars.groupByExpr;
                result = ExpressionTreeParser.Parse(_vars.groupByExpr.Body, inputs);
                string groupByFields = string.Join(",", result.columns.ToArray());
                _vars._sqlParts.groupByList.Add(groupByFields);

                if (_vars.selectExpr == null //&& _vars.groupByNewExpr==null
                    )
                {
                    //manually add "SELECT c.City"
                    _vars._sqlParts.AddSelect(result.columns);

                    result.CopyInto(_vars._sqlParts); //transfer params and tablesUsed
                }

                if(_vars.groupByNewExpr==null && _vars.selectExpr==null)
                {
                    //eg. 'db.Customers.GroupBy( c=>c.City )' - select entire Customer
                    ParameterExpression paramEx = _vars.groupByExpr.Parameters[0];
                    FromClauseBuilder.SelectAllFields(_vars,_vars._sqlParts,paramEx.Type,VarName.GetSqlName(paramEx.Name));
                }
                else if(_vars.groupByNewExpr!=null)
                {
                    inputs = new ParseInputs(result);
                    //inputs.groupByExpr = _vars.groupByExpr;
                    result = ExpressionTreeParser.Parse(_vars.groupByNewExpr.Body, inputs);
                    _vars._sqlParts.AddSelect(result.columns);
                    result.CopyInto(_vars._sqlParts); //transfer params and tablesUsed
                }
            }

            if(_vars.selectExpr!=null)
            {
                ParseInputs inputs = new ParseInputs(result);
                inputs.groupByExpr = _vars.groupByExpr;
                if (_vars.selectExpr.Body.NodeType == ExpressionType.Parameter)
                {
                    //'from p in Products select p' - do nothing, will result in SelectAllFields() later
                }
                else
                {
                    result = ExpressionTreeParser.Parse(_vars.selectExpr.Body, inputs);
                    _vars._sqlParts.AddSelect(result.columns);
                    result.CopyInto(_vars._sqlParts); //transfer params and tablesUsed
                }
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
                string varName = _vars.GetDefaultVarName(); //'$x'
                FromClauseBuilder.SelectAllFields(_vars, _vars._sqlParts, T, varName);
                //PS. Should _sqlParts not be empty here? Debug Clone() and AnalyzeLambda()
            }

            string sql = _vars._sqlParts.ToString();

            if (_vars.orderByExpr.Count>0)
            {
                //TODO: don't look at C# field name, retrieve SQL field name from attrib
                sql += "\n ORDER BY "; // +member.Member.Name;
                string separator = " ";
                foreach (LambdaExpression orderByExpr in _vars.orderByExpr)
                {
                    Expression body = orderByExpr.Body;
                    MemberExpression member = body as MemberExpression;
                    if (member != null)
                    {
                        sql += separator + member.Member.Name;
                        separator = ",";
                    }
                }
                if (_vars.orderBy_desc != null)
                {
                    sql += " " + _vars.orderBy_desc;
                }
            }

            if (_vars.limitClause != null)
            {
                sql += " " + _vars.limitClause;
            }

            if(_vars.log!=null)
                _vars.log.WriteLine("SQL: " + sql);

            _vars.sqlString = sql;
            return sql;
        }
    }
}

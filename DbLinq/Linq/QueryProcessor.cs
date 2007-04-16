////////////////////////////////////////////////////////////////////
//Initial author: Jiri George Moudry, 2006.
//License: LGPL. (Visit http://www.gnu.org)
//Commercial code may call into this library, if it's in a different module (DLL)
////////////////////////////////////////////////////////////////////

using System;
using System.Expressions;
using System.Collections.Generic;
using System.Text;
using System.Data.DLinq;
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
        public static void ProcessLambdas(SessionVars vars)
        {
            new QueryProcessor(vars).go();
        }

        void go()
        {
            ParseResult result = null;
            foreach(LambdaExpression lambda in _vars.whereExpr)
            {
                ParseInputs inputs = new ParseInputs(result);
                result = ExpressionTreeParser.Parse(lambda.Body, inputs);
                _vars._sqlParts.AddWhere(result.columns);
                result.CopyInto(_vars._sqlParts); //transfer params and tablesUsed
            }

            if(_vars.groupByExpr!=null)
            {
                ParseInputs inputs = new ParseInputs(result);
                //inputs.groupByExpr = _vars.groupByExpr;

                if(_vars.selectExpr==null //&& _vars.groupByNewExpr==null
                    )
                {
                    //manually add "SELECT c.City"
                    result = ExpressionTreeParser.Parse(_vars.groupByExpr.Body, inputs);
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
                result = ExpressionTreeParser.Parse(_vars.selectExpr.Body, inputs);
                _vars._sqlParts.AddSelect(result.columns);
                result.CopyInto(_vars._sqlParts); //transfer params and tablesUsed
            }

        }
    }
}

////////////////////////////////////////////////////////////////////
//Initial author: Jiri George Moudry, 2006.
//License: LGPL. (Visit http://www.gnu.org)
//Commercial code may call into this library, if it's in a different module (DLL)
////////////////////////////////////////////////////////////////////

using System;
using System.Expressions;
using System.Collections.Generic;
using System.Text;
using DBLinq.Linq;
using DBLinq.Linq.clause;

namespace DBLinq.util
{
    /// <summary>
    /// handles scalar calls: query.First, query.Last, and query.Count
    /// </summary>
    class RowScalar<T>
    {
        SessionVars _vars;
        IEnumerable<T> _parentTable;

        public RowScalar(SessionVars vars, IEnumerable<T> parentTable)
        {
            //don't modify the parent query with any additional clauses:
            _vars = vars.Clone();

            _parentTable = parentTable;
        }

        public S GetScalar<S>(Expression expression)
        {

            MethodCallExpression exprCall = expression as MethodCallExpression;
            if(exprCall==null)
                throw new ApplicationException("L35: GetScalar<S> only prepared for MethodCall, not "+expression.NodeType);
            switch(exprCall.Method.Name)
            {
                case "First":
                    if(typeof(S)!=typeof(T))
                        throw new ApplicationException("L39: Not prepared for double projection");
                    //Microsoft syntax: "SELECT TOP 1 ProductId FROM Products"
                    //MySql syntax:     "SELECT ProductId FROM Products LIMIT 1"
                    _vars.limitClause = "LIMIT 1";
                    foreach(T t in _parentTable) //call GetEnumerator
                    {
                        object firstObj = t; 
                        S firstS = (S)firstObj; //huh? T can be cast to S?!
                        return firstS;
                    }
                    break;
                case "Count":
                case "Max":
                case "Min":
                case "Sum":
                    {
                        bool isAlreadyProjected = _parentTable is MTable_Projected<T>;
                        if(exprCall.Method.Name=="Count"  && typeof(T)!=typeof(S) && !isAlreadyProjected)
                        {
                            //Count is very similar to Min/Max/Sum (below),
                            //but unlike those, it may be asked to work on TableRows.
                            //in that case, we need to project to an int.
                            _vars._sqlParts.countClause = "COUNT"; //Count or Max
                            string varName = _vars.GetDefaultVarName(); //'$x'
                            FromClauseBuilder.SelectAllFields(_vars, _vars._sqlParts,typeof(T),varName);
                            QueryProcessor.ProcessLambdas(_vars, typeof(S));
                            using(RowEnumerator<S> rowEnum = new RowEnumerator<S>(_vars,null))
                            {
                                //rowEnum.ExecuteSqlCommand();
                                //if(!rowEnum.MoveNext())
                                //    throw new ApplicationException("RowScalar.COUNT: Unable to advance to first result");
                                //S firstS = (S)rowEnum.Current;
                                foreach(S firstS in rowEnum){
                                    return firstS;
                                }
                                throw new ApplicationException("RowScalar.COUNT: Unable to advance to first result");
                            }
                        }
                        //MTable<T> table2 = _parentTable as MTable<T>;
                        //TODO: fir Count(), may need to add projection manually
                        IGetModifiedEnumerator<T> table2 = _parentTable as IGetModifiedEnumerator<T>;
                        //MTable_Projected<S> table2 = _parentTable as MTable_Projected<S>;
                        using(IEnumerator<T> enumerator = table2.GetModifiedEnumerator( 
                            delegate(SessionVars vars)
                            { 
                                vars._sqlParts.countClause = exprCall.Method.Name.ToUpper(); //COUNT or MAX
                            }
                        ).GetEnumerator())
                        {
                            bool hasOne = enumerator.MoveNext();
                            if(!hasOne)
                                throw new InvalidOperationException("Max/Count() called on set with zero items");
                            T firstT = enumerator.Current;
                            bool hasTwo = enumerator.MoveNext();
                            if(hasTwo)
                                throw new InvalidOperationException("Max/Count() called on set with more than one item");
                            
                            //return (S)(object)firstT; --throws InvalidCastExc?!

                            object objT = firstT;
                            
                            //uh this is nasty ...  needs fixing
                            //note: SELECT COUNT(x) in MySql returns type uint
                            //C# would like an int
                            if(typeof(S)==typeof(int) && typeof(T)==typeof(uint))
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
                    if(typeof(S)!=typeof(T))
                        throw new ApplicationException("L58: Not prepared for double projection");
                    //TODO: can I use "LIMIT" to retrieve last row? or use ORDER BY? order by what column?
                    object lastObj = null;
                    foreach(T t in _parentTable) //call GetEnumerator
                    {
                        lastObj = t;
                    }
                    S lastS = (S)lastObj;
                    return lastS;
                    }
        
                case "Single":
                    //do a LIMIT 2 query, throw InvalidOperationException occurs
                    {
                        //there are two types of Sequence.Single(), one passes an extra Lambda
                        LambdaExpression lambdaParam = expression.XMethodCall().XParam(1).XLambda();
                        MTable<T> table2 = _parentTable as MTable<T>;
                        IEnumerator<T> enumerator;
                        if(lambdaParam!=null && table2!=null)
                        {
                            //we have to pass extra Where clause into the parentTable
                            //var v1 = table2.CreateQuery(lambdaParam);
                            //get SessionVars, append our whereClause, append "LIMIT2"
                            enumerator = table2.GetModifiedEnumerator( 
                                delegate(SessionVars vars)
                                { 
                                    vars.limitClause = "LIMIT 2"; 
                                    vars.StoreLambda("Where", lambdaParam);
                                }
                            ).GetEnumerator();
                        }
                        else
                        {
                            enumerator = table2.GetEnumerator();
                        }
                        
                        using(enumerator)
                        {
                            //_vars.limitClause = "LIMIT 2";
                            bool hasOne = enumerator.MoveNext();
                            if(!hasOne)
                                throw new InvalidOperationException("Single() called on set with zero items");
                            T firstT = enumerator.Current;
                            bool hasTwo = enumerator.MoveNext();
                            if(hasTwo)
                                throw new InvalidOperationException("Single() called on set with more than one item");
                            return (S)(object)firstT;
                        }
                    }
                    //break;
            }
            throw new ApplicationException("L68: GetScalar<S> TODO: methodName="+exprCall.Method.Name);

        }
    }
}

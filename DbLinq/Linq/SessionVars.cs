////////////////////////////////////////////////////////////////////
//Initial author: Jiri George Moudry, 2006.
//License: LGPL. (Visit http://www.gnu.org)
//Commercial code may call into this library, if it's in a different module (DLL)
////////////////////////////////////////////////////////////////////

using System;
#if LINQ_PREVIEW_2006
//Visual Studio 2005 with Linq Preview May 2006 - can run on Win2000
using System.Query;
using System.Expressions;
#else
//Visual Studio Orcas - requires WinXP
using System.Linq;
using System.Linq.Expressions;
#endif

using System.Collections.Generic;
using System.Text;
//using System.Data.DLinq;
using DBLinq.Linq.clause;
using DBLinq.util;

namespace DBLinq.Linq
{
    /// <summary>
    /// holds variables that are generated 
    /// during expression parsing and querying.
    /// They are passed from MTable to MTable_Projected to RowEnumerator.
    /// </summary>
    public class SessionVars
    {
        static int              s_serial = 0;
        public //readonly 
               int              _serial = s_serial++;

        public Expression       createQueryExpr;
        public MContext         context;

        /// <summary>
        /// type of Table from which this query originated, eg. typeof(Customer).
        /// </summary>
        public Type             sourceType;

        public SqlExpressionParts _sqlParts = new SqlExpressionParts();

        /// <summary>
        /// todo: add all top-level expressions into this collection.
        /// </summary>
        public readonly List<LambdaExpression> lambdasInOrder = new List<LambdaExpression>();
        
        /// <summary>
        /// there can be more than one Where clause
        /// </summary>
        public List<LambdaExpression> whereExpr = new List<LambdaExpression>();

        /// <summary>
        /// there can be more than one select
        /// </summary>
        public LambdaExpression selectExpr;

        public LambdaExpression selectManyExpr;


        /// <summary>
        /// OrderBy goes first, ThenBy next
        /// </summary>
        public List<LambdaExpression> orderByExpr = new List<LambdaExpression>();
        public string           orderBy_desc;

        public LambdaExpression groupByExpr;
        public LambdaExpression groupByNewExpr;

        public ProjectionData   projectionData;
        public string           limitClause;

        /// <summary>
        /// in SelectMany, there is mapping c.Orders => o
        /// </summary>
        public Dictionary<MemberExpression,string> memberExprNickames = new Dictionary<MemberExpression,string>();

        /// <summary>
        /// every time the framework calls CreateQuery to further project our query, 
        /// record it in this list
        /// </summary>
        public List<Type>       createQueryList = new List<Type>();

        /// <summary>
        /// created by post-processing in QueryProcessor.build_SQL_string(), used in RowEnumerator
        /// </summary>
        public string sqlString;

        /// <summary>
        /// debug output stream
        /// </summary>
        public System.IO.TextWriter log;


        /// <summary>
        /// Look at selectExpr or whereExpr, return e.g. '$c'
        /// </summary>
        public string GetDefaultVarName()
        {
            if(selectExpr!=null)
                return VarName.GetSqlName(selectExpr.Parameters[0].Name);
            if(whereExpr.Count>0)
                return VarName.GetSqlName(whereExpr[0].Parameters[0].Name);
            return VarName.GetSqlName("x"); //if no expressions, provide fallback
        }

        /// <summary>
        /// We don't want subsequent queries (e.g. Count()) to modify early one (eg. Where)
        /// </summary>
        public SessionVars Clone()
        {
            SessionVars clone = (SessionVars) base.MemberwiseClone();
            clone._serial = s_serial++; //strange - MemberwiseClone ignores readonly attrib
            //Console.WriteLine("  SessionVars cloned " + _serial + " -> " + clone._serial);
            clone._sqlParts = _sqlParts.Clone();
            clone.createQueryList = new List<Type>(this.createQueryList);
            clone.whereExpr = new List<LambdaExpression>(this.whereExpr);
            clone.lambdasInOrder.AddRange(this.lambdasInOrder);
            return clone;
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
            if(exprCall!=null && exprCall.Method.Name=="GroupBy")
            {
                //special case: GroupBy can come with 2 or 3 params
                switch(exprCall.Arguments.Count)
                {
                    case 2: //'group o by o.CustomerID into g'
                        StoreLambda("GroupBy", exprCall.Arguments[1].XLambda()); 
                        return;
                    case 3: //'group new {c.PostalCode, c.ContactName} by c.City into g'
                        StoreLambda("GroupBy", exprCall.Arguments[1].XLambda());
                        //StoreLambda("Select", exprCall.Arguments[2].XLambda());
                        this.groupByNewExpr = exprCall.Arguments[2].XLambda();
                        return;
                    default:
                        throw new ApplicationException("StoreQuery L117: Prepared only for 2 or 3 param GroupBys");
                }
            }

            LambdaExpression lambda = WhereClauseBuilder.FindLambda(expr,out methodName);
            if(methodName=="Take")
            {
                Expression howMany = expr.XMethodCall().XParam(1);
                if( !(howMany is ConstantExpression) )
                    throw new ArgumentException("Take must come with ConstExpr");
                ConstantExpression howMany2 = (ConstantExpression)howMany;
                this.limitClause = "LIMIT "+howMany2.Value; 
            }
            else if(methodName=="Distinct")
            {
                this._sqlParts.distinctClause = "DISTINCT";
            }
            else
            {
                StoreLambda(methodName, lambda);
            }
        }

        public void StoreLambda(string methodName, LambdaExpression lambda)
        {
            //From the C# spec: Specifically, query expressions are translated 
            //into invocations of methods named 
            //  Where, Select, SelectMany, Join, GroupJoin, OrderBy, OrderByDescending, 
            //  ThenBy, ThenByDescending, GroupBy, and Cast
            switch(methodName)
            {
                case "Where":  
                    whereExpr.Add(lambda); break;
                case "Select": 
                    selectExpr = lambda; 
                    //necesary for projections?
                    if(this.groupByExpr==null){
                        projectionData = ProjectionData.FromSelectExpr(selectExpr);
                    }
                    else {
                        projectionData = ProjectionData.FromSelectGroupByExpr(selectExpr, this.groupByExpr, this._sqlParts);
                    }
                    break;
                case "SelectMany":
                    //break the SelectMany beast into constituents
                    StoreSelectManyLambda(lambda);
                    break;
                case "OrderBy": 
                    orderByExpr.Add(lambda); orderBy_desc = null; break;
                case "OrderByDescending": 
                    orderByExpr.Add(lambda); orderBy_desc = "DESC"; break;
                case "ThenBy":
                    orderByExpr.Add(lambda); orderBy_desc = null; break;

                case "GroupBy": 
                    groupByExpr = lambda;
                    break;

                //experiment:
                case "Take":
                    break;

                default: 
                    throw new ApplicationException("L109: method "+methodName+" not supported yet");
            }
        }

        private void StoreSelectManyLambda(LambdaExpression lambda)
        {
            selectExpr = lambda; 
            //there is an inner Where and inner Select
            Expression exInner = lambda.Body.XCastOperand();
            //exInner={c.Orders.Where(o => op_Equality(c.City, "London")).Select(o => new {c = c, o = o})}
            MethodCallExpression exInnFct = exInner.XMethodCall();
            if(exInnFct==null)
                throw new ArgumentException("StoreSelMany L257 bad args");
            //each parameter is a lambda
            foreach(Expression innerLambda in exInnFct.Arguments)
            {
                if(innerLambda.NodeType==ExpressionType.Lambda)
                {
                    //eg. {o => new {c = c, o = o}}'
                    this.selectExpr = innerLambda as LambdaExpression;
                }
                else
                {
                    //eg. '{c.Orders.Where(o => op_Equality(c.City, "London"))}'
                    MethodCallExpression whereCall = innerLambda.XMethodCall();
                    string methodName2 = whereCall.Method.Name;

                    MemberExpression memberExpressionCache = null; //'c.Orders', needed below for nicknames
                    ParameterExpression paramExpressionCache = null; //'o', for nicknames

                    foreach(Expression whereCallParam in whereCall.Arguments)
                    {
                        if(whereCallParam.NodeType==ExpressionType.Convert)
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
                        else if(whereCallParam.NodeType==ExpressionType.Lambda)
                        {
                            //{o => op_Equality(c.City, "London")}
                            LambdaExpression innerWhereLambda = whereCallParam as LambdaExpression;
                            StoreLambda(methodName2,innerWhereLambda);
                            paramExpressionCache = innerWhereLambda.Parameters[0];
                        }
                        else
                        {
                            //StoreLambda(methodName2, null);
                        }
                    }

                    //assign nikname mapping c.Orders=o
                    if(memberExpressionCache!=null && paramExpressionCache!=null)
                    {
                        //memberExprNickames[memberExpressionCache] = paramExpressionCache.Name;
                        ParseResult result = new ParseResult(null);
                        JoinBuilder.AddJoin1(memberExpressionCache,paramExpressionCache,result);
                        result.CopyInto(this._sqlParts);
                    }

                }
            }
        }
    }
}

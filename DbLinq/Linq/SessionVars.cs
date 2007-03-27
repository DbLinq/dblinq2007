////////////////////////////////////////////////////////////////////
//Initial author: Jiri George Moudry, 2006.
//License: LGPL. (Visit http://www.gnu.org)
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

        //public WhereClauses _whereClauses;
        public SqlExpressionParts _sqlParts;

        /// <summary>
        /// todo: add all top-level expressions into this collection.
        /// </summary>
        public readonly List<LambdaExpression> lambdasInOrder = new List<LambdaExpression>();
        
        /// <summary>
        /// there can be more than one Where clause
        /// </summary>
        public readonly List<LambdaExpression> whereExpr = new List<LambdaExpression>();

        /// <summary>
        /// there can be more than one select
        /// </summary>
        public LambdaExpression selectExpr;

        public LambdaExpression selectManyExpr;


        public LambdaExpression orderByExpr;
        public string           orderBy_desc;

        public LambdaExpression groupByExpr;
        public LambdaExpression groupByNewExpr;

        public ProjectionData   projectionData;
        public string           limitClause;

        /// <summary>
        /// every time the framework calls CreateQuery to further project our query, 
        /// record it in this list
        /// </summary>
        public List<Type>       createQueryList = new List<Type>();


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
            clone._serial = s_serial++; //strange - MemberwiseClone assigns readonly field without the s_ser++
            clone._sqlParts = _sqlParts.Clone();
            clone.createQueryList = new List<Type>(this.createQueryList);
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
            string methodName;
            MethodCallExpression exprCall = expr.XMethodCall();
            if(exprCall!=null && exprCall.Method.Name=="GroupBy")
            {
                //special case: GroupBy can come with 2 or 3 params
                switch(exprCall.Parameters.Count)
                {
                    case 2: //'group o by o.CustomerID into g'
                        StoreLambda("GroupBy", exprCall.Parameters[1].XLambda()); 
                        return;
                    case 3: //'group new {c.PostalCode, c.ContactName} by c.City into g'
                        StoreLambda("GroupBy", exprCall.Parameters[1].XLambda());
                        //StoreLambda("Select", exprCall.Parameters[2].XLambda());
                        this.groupByNewExpr = exprCall.Parameters[2].XLambda();
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
                    selectExpr = lambda; 
                    LambdaExpression lambda2 = WhereClauseBuilder.FindSelectManyLambda(lambda,out methodName);
                    if(methodName=="Where")
                    {
                        whereExpr.Add(lambda2);
                    }
                    break;
                case "OrderBy": 
                    orderByExpr = lambda; orderBy_desc = null; break;
                case "OrderByDescending": 
                    orderByExpr = lambda; orderBy_desc = "DESC"; break;

                case "GroupBy": 
                    groupByExpr = lambda;

                    //FIXME: This needs to ask the attribute what's the SQL fieldname
                    MemberExpression grpMemberExpr = groupByExpr.Body.XMember();
                    if(grpMemberExpr==null)
                        throw new ArgumentException("L145 Unexpected GroupBy");

                    string fieldName = grpMemberExpr.Member.Name;

                    //first, build projectionData to select all fields ...
                    Type srcTableType = groupByExpr.Parameters[0].Type;
                    //this.projectionData = ProjectionData.FromDbType(srcTableType);
                    ////...and then remove fields to only keep 'fieldName'
                    //this.projectionData.fields.RemoveAll( p=>p.propInfo.Name!=fieldName );
                    ////...and change the type being built from 'Customer' (later, to 'Projection')
                    //this.projectionData.type = typeof(void);


                    //First attempt: 
                    //LambdaExpression fakeLambda = Expression.Lambda(srcTableType, groupByExpr,null);
                    //FromClauseBuilder.GetClause_Projected(_sqlParts,srcTableType,"nick2", fakeLambda);

                    //Second attempt:
                    //string nick = grpMemberExpr.Expression.XParam().Name + "$";
                    //TableAttribute tAttrib = AttribHelper.GetTableAttrib(srcTableType);
                    //string tableName2 = tAttrib.Name+" "+nick; //eg. "Employee $e"
                    //_sqlParts.AddFrom(tableName2); //2B||!2B?!
                    //string s3 = FromClauseBuilder.FormatMemberExpression(grpMemberExpr);
                    //_sqlParts.selectFieldList.Add(s3); //append '$e.ID'


                    this._sqlParts.groupByList.Add(fieldName);
                    break;

                //experiment:
                case "Take":
                    break;

                default: 
                    throw new ApplicationException("L109: method "+methodName+" not supported yet");
            }
        }
    }
}

////////////////////////////////////////////////////////////////////
//Initial author: Jiri George Moudry, 2006.
//License: LGPL. (Visit http://www.gnu.org)
//Commercial code may call into this library, if it's in a different module (DLL)
////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Text;

//Visual Studio Orcas - requires WinXP:
using System.Linq;
using System.Linq.Expressions;
using System.Data.Linq;

using DBLinq.util;
using DBLinq.vendor;

namespace DBLinq.Linq.clause
{
    /// <summary>
    /// given an expression such as 'o.Customer', 
    /// call to see if there is a nickname for it, such as '$c'
    /// </summary>
    public delegate string AskNicknameHandler(Expression ex, AssociationAttribute assoc);

    /// <summary>
    /// this class is now nearly obsolete. 
    /// </summary>
    public class WhereClauseBuilder
    {
        readonly SqlExpressionParts _parts;

        public WhereClauseBuilder(SqlExpressionParts parts)
        {
            _parts = parts;
        }

        /// <summary>
        /// given entire where clause (starts with MethodCall 'where'),
        /// find the lambda
        /// </summary>
        public static LambdaExpression FindLambda(Expression expr,out string methodName)
        {
            if (expr == null || expr.NodeType != ExpressionType.Call) //MethodCall
            {
                //expr.NodeType==Cast when we enter via EntitySet or EntityMSet
                throw new ApplicationException("FindLambda: L25 failure");
            }

            MethodCallExpression methodCall = (MethodCallExpression)expr;
            methodName = methodCall.Method.Name;
            
            if(methodName=="Take" || methodName=="Distinct")
            {
                //this is a special case - there is no lambda anywhere?
                return null;
            }

            //if(methodName=="GroupBy")
            //    return null; //huh, we have 2 different lambdas here...
#if LINQ_2006_PREVIEW
            //used to work differently in 2006
#else
            if (methodName == "SelectMany" && methodCall.Arguments.Count != 2)
            {

            }
#endif

            if(methodCall.Arguments.Count!=2)
            {
                //This happens for GroupBy, which has 3 params
                throw new ApplicationException("FindLambda: L28 failure - Lambda does not have 2 params");
                //return null; //"MethodCallExpr: expected 2 params";
            }

            //param0 is const-type
            Expression param1 = methodCall.Arguments[1];
            
            if(methodName=="Including")
            {
                //if(param1.NodeType==ExpressionType.NewArrayInit)...
                throw new ApplicationException("FindLambda: L38 '"+methodName+"' clause not yet supported");
            }

            //'ExpressionType.Quote' is new in Orcas Beta1. e.g. 'orderby p=>p.ProductName' is a quote
            if (param1.NodeType == ExpressionType.Quote)
            {
                param1 = param1.XUnary().Operand; //inside the quote there will be a Lambda
            }

            if(param1.NodeType!=ExpressionType.Lambda)
                throw new ApplicationException("FindLambda: L41 failure");
                //return null;
            return (LambdaExpression)param1;
        }

    }

}

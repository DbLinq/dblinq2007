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

using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Data.Linq;
using System.Data.Linq.Mapping;
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
            if (methodName == "SelectMany" && methodCall.Arguments.Count != 2)
            {
                //re-visit
            }

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

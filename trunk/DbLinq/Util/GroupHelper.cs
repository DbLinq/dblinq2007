#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using DbLinq.Linq;

namespace DbLinq.Util
{
    static class GroupHelper
    {
        /// <summary>
        /// given {g.Key}, or {g.Key.Length}, return true.
        /// </summary>
        public static bool IsGrouping(MemberExpression me)
        {
            if (me == null)
                return false;
            while (me.Expression.NodeType == ExpressionType.MemberAccess)
            {
                //given {g.Key.Length}, extract {g.Key}
                me = me.Expression as MemberExpression;
            }
            return IsGrouping(me.Expression.Type);
        }

        /// <summary>
        /// given T, check if it's IGrouping`2
        /// </summary>
        public static bool IsGrouping(Type t)
        {
            if (!t.IsGenericType)
                return false;
            //bool isGrp = t.Name == "IGrouping`2";
            Type genBaseType = t.GetGenericTypeDefinition();
            bool isGrp = genBaseType == typeof(System.Linq.IGrouping<,>);
            return isGrp;
        }
    }

    public class GroupHelper2<T>
    {
        /// <summary>
        /// when user is selecting 'new {g.Key,g}', we need to build bindings for the fields so that a row lambda can be created.
        /// Called from BuildProjectedRowLambda().
        /// </summary>
        /// <param name="projFld"></param>
        /// <returns></returns>
        public static MemberAssignment BuildProjFieldBinding(SessionVarsParsed vars, ProjectionData.ProjectionField projFld,
            ParameterExpression rdr, ParameterExpression mappingContext, ref int fieldID)
        {
            PropertyInfo[] igroupies = projFld.FieldType.GetProperties();
            ConstructorInfo[] ictos = projFld.FieldType.GetConstructors();
            ProjectionData projInner = ProjectionData.FromSelectGroupByExpr(vars.GroupByNewExpression, vars.GroupByExpression, vars.SqlParts);
            //ProjectionData projInner = ProjectionData.FromSelectGroupByExpr(vars.groupByNewExpr,vars.GroupByExpression,vars.SqlParts);
            LambdaExpression innerLambda = RowEnumeratorCompiler<T>.BuildProjectedRowLambda(vars, projInner, rdr, mappingContext, ref fieldID);
            MemberAssignment binding = projFld.BuildMemberAssignment(innerLambda.Body);

            return binding;
        }
    }
}

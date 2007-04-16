////////////////////////////////////////////////////////////////////
//Initial author: Jiri George Moudry, 2006.
//License: LGPL. (Visit http://www.gnu.org)
//Commercial code may call into this library, if it's in a different module (DLL)
////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.Expressions;
using DBLinq.Linq;

namespace DBLinq.util
{
    static class GroupHelper
    {
        /// <summary>
        /// given {g.Key}, or {g.Key.Length}, return true.
        /// </summary>
        public static bool IsGrouping(MemberExpression me)
        {
            if(me==null)
                return false;
            while(me.Expression.NodeType==ExpressionType.MemberAccess)
            {
                //given {g.Key.Length}, extract {g.Key}
                me = me.Expression as MemberExpression;
            }
            Type meExType = me.Expression.Type;
            bool isGrp = meExType.Name=="IGrouping`2";
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
        public static MemberAssignment BuildProjFieldBinding(SessionVars vars, ProjectionData.ProjectionField projFld, ParameterExpression rdr, ref int fieldID)
        {
            PropertyInfo[] igroupies = projFld.propInfo.PropertyType.GetProperties();
            ConstructorInfo[] ictos = projFld.propInfo.PropertyType.GetConstructors();
            ProjectionData projInner = ProjectionData.FromSelectGroupByExpr(vars.groupByNewExpr,vars.groupByExpr,vars._sqlParts);
            //ProjectionData projInner = ProjectionData.FromSelectGroupByExpr(vars.groupByNewExpr,vars.groupByExpr,vars._sqlParts);
            LambdaExpression innerLambda = RowEnumeratorCompiler<T>.BuildProjectedRowLambda(vars, projInner, rdr, ref fieldID);
            Type t1 = innerLambda.Body.Type;
            Type t2 = projFld.propInfo.PropertyType;
            //bool same  = t1==t2;
            MemberAssignment binding = Expression.Bind(projFld.propInfo, innerLambda.Body);
            return binding;
        }
    }
}

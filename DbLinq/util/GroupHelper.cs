////////////////////////////////////////////////////////////////////
//Initial author: Jiri George Moudry, 2006.
//License: LGPL. (Visit http://www.gnu.org)
//Commercial code may call into this library, if it's in a different module (DLL)
////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
#if LINQ_PREVIEW_2006
//Visual Studio 2005 with Linq Preview May 2006 - can run on Win2000
using System.Expressions;
#else
//Visual Studio Orcas - requires WinXP
using System.Linq.Expressions;
#endif
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
            PropertyInfo[] igroupies = projFld.FieldType.GetProperties();
            ConstructorInfo[] ictos = projFld.FieldType.GetConstructors();
            ProjectionData projInner = ProjectionData.FromSelectGroupByExpr(vars.groupByNewExpr,vars.groupByExpr,vars._sqlParts);
            //ProjectionData projInner = ProjectionData.FromSelectGroupByExpr(vars.groupByNewExpr,vars.groupByExpr,vars._sqlParts);
            LambdaExpression innerLambda = RowEnumeratorCompiler<T>.BuildProjectedRowLambda(vars, projInner, rdr, ref fieldID);
            MemberAssignment binding = projFld.BuildMemberAssignment(innerLambda.Body);
            
            return binding;
        }
    }
}

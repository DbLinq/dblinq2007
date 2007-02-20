using System;
using System.Data.DLinq;
using System.Reflection;
using System.Expressions;
using System.Collections.Generic;
using System.Text;
using DBLinq.util;

namespace DBLinq.Linq
{
    /// <summary>
    /// holds ctor and list of fields.
    /// comes from 'Select'
    /// </summary>
    public class ProjectionData
    {
        public Type type;
        public ConstructorInfo ctor;
        public ConstructorInfo ctor2;
        public TableAttribute tableAttribute;
        public List <ProjectionField> fields = new List<ProjectionField>();
        /// <summary>
        /// this field must be populated after an INSERT.
        /// </summary>
        public FieldInfo autoGenField;

        /// <summary>
        /// key column, eg. 'productID', used for deletion
        /// </summary>
        public string keyColumnName;

        public class ProjectionField
        {
            public PropertyInfo propInfo;
            public MemberExpression expr1; //holds e.g. {e.ID}
            public Type type; //eg. int

            /// <summary>
            /// is this a primitive type, a DB column, or a projection?
            /// </summary>
            public TypeEnum typeEnum;

            public ColumnAttribute columnAttribute;
        }

        public ProjectionData()
        {
        }

        public static ProjectionData FromType(Type t)
        {
            return AttribHelper.GetProjectionData(t);
        }

        public static ProjectionData FromSelectExpr(LambdaExpression selectExpr)
        {
            ProjectionData proj = new ProjectionData();
            //when selecting just a string, body is not MemberInitExpr, but MemberExpr
            MemberInitExpression memberInit = selectExpr.Body as MemberInitExpression;
            if(memberInit==null){
                Console.WriteLine("Select is not a projection");
                return null;
            }
            NewExpression newExpr = memberInit.NewExpression;
            proj.ctor = newExpr.Constructor;
            foreach(MemberAssignment memberAssign in memberInit.Bindings)
            {
                ProjectionField projField = new ProjectionField();
                projField.propInfo = memberAssign.Member as PropertyInfo;
                projField.type = projField.propInfo.PropertyType;
                if(projField.type==null || projField.propInfo==null)
                    throw new ApplicationException("ProjectionData L36: unknown type of memberInit");

                switch(memberAssign.Expression.NodeType)
                {
                    case ExpressionType.MemberAccess:
                        //occurs during 'select new {e.ID,e.Name}'
                        projField.expr1 = memberAssign.Expression as MemberExpression;
                        projField.type = projField.expr1.Type;
                        projField.typeEnum = CSharp.CategorizeType(projField.type);
                        break;
                    case ExpressionType.Parameter:
                        //occurs during 'from c ... from o ... select new {c,o}'
                        ParameterExpression paramEx = memberAssign.Expression as ParameterExpression;
                        projField.type = paramEx.Type;
                        projField.typeEnum = CSharp.CategorizeType(projField.type);
                        break;
                    default:
                        throw new ArgumentException("L86: Unprepared for "+memberAssign.Expression.NodeType);
                }
                proj.fields.Add(projField);
            }

            //object xx = newExpr.Args; //args.Count=0
            return proj;
        }

        /// <summary>
        /// this path is taken during          
        ///   'from c in db.customers from o in c.Orders 
        ///    where c.City == "London" select new { c, o };'
        /// </summary>
        public static ProjectionData FromSelectManyExpr(LambdaExpression selectExpr)
        {
            //ProjectionData proj = new ProjectionData();
            if(selectExpr.Body.NodeType!=ExpressionType.Cast)
                throw new ArgumentOutOfRangeException("FromSelectMany needs Cast");
            UnaryExpression unEx = (UnaryExpression)selectExpr.Body;
            if(unEx.Operand.NodeType!=ExpressionType.MethodCall)
                throw new ArgumentOutOfRangeException("FromSelectMany needs Cast-MethodCall");
            MethodCallExpression methEx = (MethodCallExpression)unEx.Operand;
            foreach(Expression exParm in methEx.Parameters)
            {
                ExpressionType et = exParm.NodeType;
                if(et==ExpressionType.Lambda)
                {
                    LambdaExpression lambda2 = (LambdaExpression)exParm;
                    ProjectionData proj = FromSelectExpr(lambda2);
                    if(proj!=null)
                        return proj;
                }
            }
            return null;
        }

    }
}

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
using System.Query;
using System.Expressions;
using System.Data.DLinq;
#else
//Visual Studio Orcas - requires WinXP
using System.Linq;
using System.Linq.Expressions;
using System.Data.Linq;
#endif

using DBLinq.util;
using DBLinq.Linq.clause;

namespace DBLinq.Linq
{
    /// <summary>
    /// holds ctor and list of fields.
    /// comes from 'Select'. Used to build select statement and sql read method.
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
            public static readonly object[] s_emptyIndices = new object[0];

            /// <summary>
            /// TODO: propInfo and fieldInfo should be handled together
            /// </summary>
            PropertyInfo propInfo;
            FieldInfo fieldInfo; //we can assign Properties or Fields in memberInit

            public MemberExpression expr1; //holds e.g. {e.ID}
            public Type type; //eg. int

            /// <summary>
            /// is this a primitive type, a DB column, or a projection?
            /// </summary>
            public TypeEnum typeEnum;

            public ColumnAttribute columnAttribute;

            public ProjectionField(MemberInfo memberInfo)
            {
                propInfo = memberInfo as PropertyInfo;
                if (propInfo == null)
                    fieldInfo = memberInfo as FieldInfo;
                if (propInfo==null && fieldInfo == null)
                    throw new ArgumentException("Bad mInfo:" + memberInfo);
            }
            public Type FieldType
            {
                get
                {
                    if (propInfo != null)
                        return propInfo.PropertyType;
                    else
                        return fieldInfo.FieldType;
                }
            }
            public object GetFieldValue(object parentObj)
            {
                object paramValue;
                if (propInfo != null)
                    paramValue = propInfo.GetValue(parentObj, s_emptyIndices);
                else
                    paramValue = fieldInfo.GetValue(parentObj);
                return paramValue;
            }

            public MemberInfo MemberInfo 
            {
                get { return (MemberInfo)propInfo ?? fieldInfo; }
            }

            public override string ToString()
            {
                return "ProjFld "+propInfo.Name+" "+expr1+" "+typeEnum;
            }

            public bool IsValid(out string error)
            {
                if (type == null) { error = "Missing type"; return false; }
                if (propInfo == null && fieldInfo==null) { error = "Missing propInfo/fieldInfo"; return false; }
                error = null;
                return true;
            }
            public MemberAssignment BuildMemberAssignment(Expression innerExpr)
            {
                //binding = Expression.Bind(projFld.propInfo, innerLambda.Body);
                MemberAssignment binding = Expression.Bind(MemberInfo, innerExpr);
                return binding;
            }

        }

        public ProjectionData()
        {
        }

        /// <summary>
        /// walk our type's properties with ColumnAttributes, 
        /// build ProjData that contains all sql fields.
        /// </summary>
        public static ProjectionData FromDbType(Type t)
        {
            return AttribHelper.GetProjectionData(t);
        }

        /// <summary>
        /// in a group-by clause, we know tha table name, but columns are computed.
        /// This is a stripped-down version of FromDbType().
        /// </summary>
        public static ProjectionData FromReflectedType(Type t)
        {
            ProjectionData projData = new ProjectionData();
            projData.type = t;
            projData.ctor = t.GetConstructor(new Type[0]);
            ConstructorInfo[] ctors = t.GetConstructors();
            if(ctors.Length==2 && ctors[0]==projData.ctor)
            {
                //classes generated by MysqlMetal have ctor2 with multiple params:
                projData.ctor2 = ctors[1]; 
            }

            projData.tableAttribute = AttribHelper.GetTableAttrib(t);
            //List<ColumnAttribute> lst = new List<ColumnAttribute>();
            PropertyInfo[] props = t.GetProperties();
            foreach(PropertyInfo prop in props)
            {
                ProjectionData.ProjectionField projField = new ProjectionData.ProjectionField(prop);
                projField.type = prop.PropertyType;
                //projField.propInfo = prop;
                projField.typeEnum = CSharp.CategorizeType(projField.type);
                projData.fields.Add(projField);
            }
            return projData;
        }

        /// <summary>
        /// given a selectExpression lambda, examine all fields, construct ProjectionData
        /// </summary>
        public static ProjectionData FromSelectExpr(LambdaExpression selectExpr)
        {
            //when selecting just a string, body is not MemberInitExpr, but MemberExpr
            MemberInitExpression memberInit = selectExpr.Body as MemberInitExpression;
            if(memberInit==null){
                Console.WriteLine("  Select is not a projection - just a single field");
                return null;
            }

            ProjectionData proj = new ProjectionData();
            NewExpression newExpr = memberInit.NewExpression;
            proj.ctor = newExpr.Constructor;
#if NEVER
            foreach(MemberAssignment memberAssign in memberInit.Bindings)
            {
                ProjectionField projField = new ProjectionField(memberAssign.Member);

                //projField.type = projField.propInfo.PropertyType; //fails for o.Product.ProductID
                projField.type = memberAssign.Expression.Type;

                string err;
                if ( ! projField.IsValid(out err))
                    throw new ApplicationException("ProjectionData L36: error on field: " + memberAssign.Expression + ": " + err);

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
                    case ExpressionType.MethodCall:
                        //occurs during 'from c ... group o by o.CustomerID into g ... 
                        //select new { g.Key , OrderCount = g.Count() };
                        MethodCallExpression callEx = memberAssign.Expression.XMethodCall();
                        //projField.expr1 = memberAssign.Expression as MemberExpression;
                        projField.type = callEx.Type;
                        projField.typeEnum = CSharp.CategorizeType(projField.type);
                        break;
                    default:
                        throw new ArgumentException("L205: Unprepared for "+memberAssign.Expression.NodeType);
                }
                proj.fields.Add(projField);
            }
#endif
            LoopOverBindings(proj, memberInit);

            //object xx = newExpr.Args; //args.Count=0
            return proj;
        }

        /// <summary>
        /// given a selectExpression lambda, examine all fields, construct ProjectionData
        /// (special case for GroupBy selects - replaces 'g.Key' with 'Order')
        /// </summary>
        public static ProjectionData FromSelectGroupByExpr(LambdaExpression selectExpr, LambdaExpression groupByExpr, SqlExpressionParts sqlParts)
        {
            //when selecting just a string, body is not MemberInitExpr, but MemberExpr
            MemberInitExpression memberInit = selectExpr.Body as MemberInitExpression;
            if (memberInit==null && groupByExpr.Body.NodeType == ExpressionType.MemberInit)
            {
                //use groupByExpr rather than selectExpr
                memberInit = groupByExpr.Body as MemberInitExpression;
            }
            if(memberInit==null){
                Console.WriteLine("Select is not a projection (L159)");
                return null;
            }

            ProjectionData proj = new ProjectionData();
            NewExpression newExpr = memberInit.NewExpression;
            proj.ctor = newExpr.Constructor;
            proj.type = newExpr.Type;
#if NEVER
            foreach(MemberAssignment memberAssign in memberInit.Bindings)
            {
                ProjectionField projField = new ProjectionField(memberAssign.Member);
                projField.type = projField.FieldType;
                //if(projField.type==null)
                //    throw new ApplicationException("ProjectionData L36: unknown type of memberInit");

                switch(memberAssign.Expression.NodeType)
                {
                    case ExpressionType.MemberAccess:
                        //occurs during 'select new {e.ID,e.Name}'
                        projField.expr1 = memberAssign.Expression as MemberExpression;
                        projField.type = projField.expr1.Type;
                        projField.typeEnum = CSharp.CategorizeType(projField.type);

                        //Now handled in ExpressionTreeParser
                        ////TODO: for GroupBy selects, replace 'g.Key' with 'o.CustomerID':
                        //if(projField.expr1.Member.Name=="Key")
                        //{
                        //    projField.expr1 = groupByExpr.Body as MemberExpression;
                        //    sqlParts.selectFieldList.Add(projField.expr1.Member.Name);
                        //}
                        break;
                    case ExpressionType.Parameter:
                        //occurs during 'from c ... from o ... select new {c,o}'
                        ParameterExpression paramEx = memberAssign.Expression as ParameterExpression;
                        projField.type = paramEx.Type;
                        projField.typeEnum = CSharp.CategorizeType(projField.type);
                        break;
                    case ExpressionType.MethodCallVirtual:
                        //occurs in 'select o.Name.ToLower()'
                        break;
                    case ExpressionType.MethodCall:
                        //occurs during 'from c ... group o by o.CustomerID into g ... 
                        //select new { g.Key , OrderCount = g.Count() };
                        MethodCallExpression callEx = memberAssign.Expression.XMethodCall();
                        //projField.expr1 = memberAssign.Expression as MemberExpression;
                        //projField.type = callEx.Type;
                        //projField.typeEnum = CSharp.CategorizeType(projField.type);
                        break;
                    default:
                        throw new ArgumentException("L274: Unprepared for "+memberAssign.Expression.NodeType);
                }
                proj.fields.Add(projField);
            }
#endif
            LoopOverBindings(proj, memberInit);

            //object xx = newExpr.Args; //args.Count=0
            return proj;
        }

        private static void LoopOverBindings(ProjectionData proj, MemberInitExpression memberInit)
        {
            foreach(MemberAssignment memberAssign in memberInit.Bindings)
            {
                ProjectionField projField = new ProjectionField(memberAssign.Member);
                projField.type = projField.FieldType;
                //if(projField.type==null)
                //    throw new ApplicationException("ProjectionData L36: unknown type of memberInit");

                switch(memberAssign.Expression.NodeType)
                {
                    case ExpressionType.MemberAccess:
                        //occurs during 'select new {e.ID,e.Name}'
                        projField.expr1 = memberAssign.Expression as MemberExpression;
                        projField.type = projField.expr1.Type;
                        projField.typeEnum = CSharp.CategorizeType(projField.type);

                        //Now handled in ExpressionTreeParser
                        ////TODO: for GroupBy selects, replace 'g.Key' with 'o.CustomerID':
                        //if(projField.expr1.Member.Name=="Key")
                        //{
                        //    projField.expr1 = groupByExpr.Body as MemberExpression;
                        //    sqlParts.selectFieldList.Add(projField.expr1.Member.Name);
                        //}
                        break;
                    case ExpressionType.Parameter:
                        //occurs during 'from c ... from o ... select new {c,o}'
                        ParameterExpression paramEx = memberAssign.Expression as ParameterExpression;
                        projField.type = paramEx.Type;
                        projField.typeEnum = CSharp.CategorizeType(projField.type);
                        break;
                    case ExpressionType.CallVirtual:
                        //occurs in 'select o.Name.ToLower()'
                        break;
                    case ExpressionType.Convert:
                        //occurs in 'select (CategoryEnum)o.EmployeeCategory'
                        break;
                    case ExpressionType.Call:
                        //occurs during 'from c ... group o by o.CustomerID into g ... 
                        //select new { g.Key , OrderCount = g.Count() };
                        MethodCallExpression callEx = memberAssign.Expression.XMethodCall();
                        //projField.expr1 = memberAssign.Expression as MemberExpression;
                        //projField.type = callEx.Type;
                        //projField.typeEnum = CSharp.CategorizeType(projField.type);
                        break;
                    default:
                        throw new ArgumentException("L274: Unprepared for "+memberAssign.Expression.NodeType);
                }
                proj.fields.Add(projField);
            }        
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
            if(unEx.Operand.NodeType!=ExpressionType.Call)
                throw new ArgumentOutOfRangeException("FromSelectMany needs Cast-MethodCall");
            MethodCallExpression methEx = (MethodCallExpression)unEx.Operand;
            foreach(Expression exParm in methEx.Arguments)
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

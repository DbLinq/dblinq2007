////////////////////////////////////////////////////////////////////
//Initial author: Jiri George Moudry, 2006.
//License: LGPL. (Visit http://www.gnu.org)
//Commercial code may call into this library, if it's in a different module (DLL)
////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using DBLinq.util;
using DBLinq.Linq.clause;

namespace DBLinq.Linq
{
    /// <summary>
    /// Represents a new-object expression (eg. 'new {ProductId=p.ProductID') from a user's query.
    /// This will be subsequently compiled into another expression - 'new f__anonType(reader.GetInt32())'
    /// 
    /// In case of db.Products.ToList(), we use reflection to retrieve all fields of Product.
    /// 
    /// Internally, this class holds ctor and list of fields.
    /// comes from 'Select'. Used to build select statement and sql read method.
    /// 
    /// e.g. user code 'select new {ProductId=p.ProductID}' 
    /// would create ProjectionData containing one ProjectionField, pointing to ProductId column. 
    /// </summary>
    public class ProjectionData
    {
        public Type type;

        /// <summary>
        /// default ctor reference. As of Beta2, they seem to have eliminated use of default ctors in "<>f__AnonymousType"
        /// </summary>
        public ConstructorInfo ctor;
        
        /// <summary>
        /// ctor2 with multiple params (non-default)
        /// </summary>
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

        /// <summary>
        /// nested class which knows which SQL column each field refers to.
        /// Field can be either a Property or Field.
        /// e.g. 'select new {ProductId=p.ProductID}' would create one ProjectionField, pointing to ProductId column. 
        /// </summary>
        public class ProjectionField
        {
            #region ProjectionField
            public static readonly object[] s_emptyIndices = new object[0];

            /// <summary>
            /// TODO: propInfo and fieldInfo should be handled together
            /// </summary>
            PropertyInfo propInfo;
            FieldInfo fieldInfo; //we can assign Properties or Fields in memberInit

            public MemberExpression expr1; //holds e.g. {e.ID}
            //public Type type; //eg. int
            Type fieldType_NoBind;

            /// <summary>
            /// is this a primitive type, a DB column, or a projection?
            /// </summary>
            public TypeEnum typeEnum;

            public ColumnAttribute columnAttribute;

            public ProjectionField(Type fieldType)
            {
                fieldType_NoBind = fieldType;
                //projField.type = projField.expr1.Type;
                typeEnum = CSharp.CategorizeType(fieldType);
            }

            public ProjectionField(MemberInfo memberInfo)
            {
                if (memberInfo.MemberType == MemberTypes.Method && memberInfo.Name.StartsWith("get_"))
                {
                    //Orcas Beta2: we got passed '{UInt32 get_ProductId()}' instead of 'ProductId'
                    string propName = memberInfo.Name.Substring(4);
                    PropertyInfo propInfo2 = memberInfo.DeclaringType.GetProperty(propName);
                    if (propInfo2 != null)
                    {
                        this.propInfo = propInfo2; //looked up 'ProductId' from '{UInt32 get_ProductId()}'
                        typeEnum = CSharp.CategorizeType(this.FieldType);
                        return;
                    }
                }

                propInfo = memberInfo as PropertyInfo;
                if (propInfo == null)
                    fieldInfo = memberInfo as FieldInfo;
                if (propInfo==null && fieldInfo == null)
                    throw new ArgumentException("Bad mInfo:" + memberInfo);

                typeEnum = CSharp.CategorizeType(this.FieldType);
            }

            public Type FieldType
            {
                get
                {
                    if (fieldType_NoBind != null)
                        return fieldType_NoBind;
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
                //if (type == null) { error = "Missing type"; return false; }
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
            #endregion
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
                //classes generated by MysqlMetal (and immutable anonymous types) have ctor2 with multiple params:
                projData.ctor2 = ctors[1]; 
            }

            if (ctors.Length == 1 && projData.ctor == null)
            {
                //immutable anonymous types have one (non-default) ctor
                projData.ctor = ctors[0];
            }

            projData.tableAttribute = AttribHelper.GetTableAttrib(t);
            //List<ColumnAttribute> lst = new List<ColumnAttribute>();
            PropertyInfo[] props = t.GetProperties();
            foreach(PropertyInfo prop in props)
            {
                ProjectionData.ProjectionField projField = new ProjectionData.ProjectionField(prop);
                //projField.type = prop.PropertyType;
                //projField.propInfo = prop;
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
            NewExpression newExpr1 = selectExpr.Body as NewExpression;
            if (newExpr1 != null)
            {
                //OrcasBeta2: we now receive a NewExpression instead of a MemberInitExpression
                ProjectionData proj1 = new ProjectionData();
                proj1.ctor = newExpr1.Constructor;
                LoopOverBindings_OrcasB2(proj1, newExpr1);
                return proj1;
            }

            MemberInitExpression memberInit = selectExpr.Body as MemberInitExpression;
            if(memberInit==null){
                Console.WriteLine("  Select is not a projection - just a single field");
                return null;
            }

            ProjectionData proj = new ProjectionData();
            NewExpression newExpr = memberInit.NewExpression;
            proj.ctor = newExpr.Constructor;
            proj.type = newExpr.Type;

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

            LoopOverBindings(proj, memberInit);

            //object xx = newExpr.Args; //args.Count=0
            return proj;
        }

        private static void LoopOverBindings(ProjectionData proj, MemberInitExpression memberInit)
        {
            foreach(MemberAssignment memberAssign in memberInit.Bindings)
            {
                ProjectionField projField = new ProjectionField(memberAssign.Member);
                //projField.type = projField.FieldType;
                //if(projField.type==null)
                //    throw new ApplicationException("ProjectionData L36: unknown type of memberInit");

                switch(memberAssign.Expression.NodeType)
                {
                    case ExpressionType.MemberAccess:
                        //occurs during 'select new {e.ID,e.Name}'
                        projField.expr1 = memberAssign.Expression as MemberExpression;
                        //projField.type = projField.expr1.Type;
                        //projField.typeEnum = CSharp.CategorizeType(projField.type);

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
                        //projField.type = paramEx.Type;
                        //projField.typeEnum = CSharp.CategorizeType(projField.type);
                        break;

                    //CallVirtual disappeared in Beta2?!
                    //case ExpressionType.CallVirtual:
                    //    //occurs in 'select o.Name.ToLower()'
                    //    break;

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
                        throw new ArgumentException("L325: Unprepared for "+memberAssign.Expression.NodeType);
                }
                proj.fields.Add(projField);
            }        
        }


        private static void LoopOverBindings_OrcasB2(ProjectionData proj, NewExpression newExpr)
        {
            //if (newExpr.Members == null)
            //    return; //eg. "{new ProductWrapper3(p.ProductID, p.SupplierID)}"

            int i = 0;
            foreach (Expression argExpr in newExpr.Arguments)
            {
                ProjectionField projField = null;
                if (newExpr.Members == null)
                {
                    //eg. "{new ProductWrapper3(p.ProductID, p.SupplierID)}"
                    projField = new ProjectionField(argExpr.Type);
                }
                else
                {
                    MemberInfo memberInfo = newExpr.Members[i++];
                    projField = new ProjectionField(memberInfo);
                }
                //if(projField.type==null)
                //    throw new ApplicationException("ProjectionData L36: unknown type of memberInit");

                switch (argExpr.NodeType)
                {
                    case ExpressionType.MemberAccess:
                        //occurs during 'select new {e.ID,e.Name}'
                        projField.expr1 = argExpr as MemberExpression;

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
                        ParameterExpression paramEx = argExpr as ParameterExpression;
                        break;

                    //CallVirtual disappeared in Beta2?!
                    //case ExpressionType.CallVirtual:
                    //    //occurs in 'select o.Name.ToLower()'
                    //    break;

                    case ExpressionType.Convert:
                        //occurs in 'select (CategoryEnum)o.EmployeeCategory'
                        break;
                    case ExpressionType.Call:
                        //occurs during 'from c ... group o by o.CustomerID into g ... 
                        //select new { g.Key , OrderCount = g.Count() };
                        MethodCallExpression callEx = argExpr.XMethodCall();
                        //projField.expr1 = memberAssign.Expression as MemberExpression;
                        //projField.type = callEx.Type;
                        //projField.typeEnum = CSharp.CategorizeType(projField.type);
                        break;
                    default:
                        throw new ArgumentException("L390: Unprepared for " + argExpr.NodeType);
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
            //Cast disappeared in Beta2?!
            //if(selectExpr.Body.NodeType!=ExpressionType.Cast)
            //    throw new ArgumentOutOfRangeException("FromSelectMany needs Cast");

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

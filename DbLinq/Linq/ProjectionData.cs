#region MIT license
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
#endregion

using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using DbLinq.Util;

namespace DbLinq.Linq
{
    /// <summary>
    /// Represents a new-object expression (eg. 'new {ProductId=p.ProductID') from a user's query.
    /// This will be subsequently compiled into another expression - 'new f__anonType(reader.GetInt32())'
    /// Internally, this class holds ctor and list of fields, 
    /// which comes from 'Select' query. 
    /// 
    /// Example: user code 'select new {ProductId=p.ProductID}' 
    /// would create ProjectionData containing one ProjectionField, pointing to ProductId column. 
    /// 
    /// In the special case of 'db.Products.ToList()', 
    /// we use reflection to retrieve all the fields to select - all fields of Product.
    /// </summary>
    public class ProjectionData
    {
        public Type type;

        /// <summary>
        /// default ctor reference. As of Beta2, they seem to have eliminated use of default ctors in "<>f__AnonymousType"
        /// </summary>
        public ConstructorInfo ctor;

        public TableAttribute tableAttribute;

        public readonly List<InheritanceMappingAttribute> inheritanceAttributes = new List<InheritanceMappingAttribute>();

        public List<ProjectionField> fields = new List<ProjectionField>();

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
                if (propInfo == null && fieldInfo == null)
                    throw new ArgumentException("Bad mInfo:" + memberInfo);
                if (propInfo != null)
                {
                    object[] colAtts = propInfo.GetCustomAttributes(typeof(ColumnAttribute), false);
                    if (colAtts.Length == 1)
                    {
                        this.columnAttribute = colAtts[0] as ColumnAttribute;
                    }
                }

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
                return "ProjFld " + propInfo.Name + " " + expr1 + " " + typeEnum;
            }

            public bool IsValid(out string error)
            {
                //if (type == null) { error = "Missing type"; return false; }
                if (propInfo == null && fieldInfo == null) { error = "Missing propInfo/fieldInfo"; return false; }
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
        /// in a group-by clause, we know the table name, but columns are computed.
        /// This is a stripped-down version of FromDbType().
        /// </summary>
        public static ProjectionData FromReflectedType(Type t)
        {
            ProjectionData projData = new ProjectionData();
            projData.type = t;
            projData.ctor = t.GetConstructor(new Type[0]);
            ConstructorInfo[] ctors = t.GetConstructors();
            if (ctors.Length == 2 && ctors[0] == projData.ctor)
            {
                //classes generated by MysqlMetal (and immutable anonymous types) have ctor2 with multiple params:
                //projData.ctor2 = ctors[1]; 
            }

            if (ctors.Length == 1 && projData.ctor == null)
            {
                //immutable anonymous types have one (non-default) ctor
                projData.ctor = ctors[0];
            }

            projData.tableAttribute = AttribHelper.GetTableAttrib(t);
            PropertyInfo[] props = t.GetProperties();
            foreach (PropertyInfo prop in props)
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
            switch (selectExpr.Body.NodeType)
            {
                case ExpressionType.New:
                    {
                        NewExpression newExpr1 = selectExpr.Body as NewExpression;
                        //OrcasBeta2: we now receive a NewExpression instead of a MemberInitExpression
                        ProjectionData proj1 = new ProjectionData();
                        proj1.ctor = newExpr1.Constructor;
                        LoopOverBindings_OrcasB2(proj1, newExpr1);
                        return proj1;
                    }
                case ExpressionType.MemberAccess:
                case ExpressionType.Parameter:
                    {
                        //MemberAccess example: in LinqToSqlJoin01(), we get "{<>h__TransparentIdentifier0.o}"
                        //Parameter example: in C1_SelectProducts(), we get Products.Select(p => p)
                        //that means we wish to select entire 'Order' row
                        return FromDbType(selectExpr.Body.Type);
                    }
                case ExpressionType.MemberInit:
                    {
                        MemberInitExpression memberInit = selectExpr.Body as MemberInitExpression;
                        if (memberInit == null)
                        {
                            Console.WriteLine("  Select is not a projection - just a single field");
                            return null;
                        }

                        ProjectionData proj = new ProjectionData();
                        NewExpression newExpr = memberInit.NewExpression;
                        proj.ctor = newExpr.Constructor;
                        proj.type = newExpr.Type;

                        LoopOverBindings(proj, memberInit);
                        return proj;
                    }
                default:
                    //throw new ApplicationException("L270 ProjData.FromSelectExpr: unprepared for " + selectExpr.Body.NodeType + " in " + selectExpr.Body);
                    return null; //eg contatenated strings
            }
        }

        /// <summary>
        /// given a selectExpression lambda, examine all fields, construct ProjectionData
        /// (special case for GroupBy selects - replaces 'g.Key' with 'Order')
        /// </summary>
        public static ProjectionData FromSelectGroupByExpr(LambdaExpression selectExpr, LambdaExpression groupByExpr, SqlExpressionParts sqlParts)
        {
            //when selecting just a string, body is not MemberInitExpr, but MemberExpr
            MemberInitExpression memberInit = selectExpr.Body as MemberInitExpression;
            if (memberInit == null && groupByExpr.Body.NodeType == ExpressionType.MemberInit)
            {
                //use GroupByExpression rather than selectExpr
                memberInit = groupByExpr.Body as MemberInitExpression;
            }
            if (memberInit == null)
            {
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

        /// <summary>
        /// given a expression such as '{new Customer() {CustomerID = c.CustomerID}}',
        /// walk over all assignments to create ProjectionField entries
        /// </summary>
        private static void LoopOverBindings(ProjectionData proj, MemberInitExpression memberInit)
        {
            foreach (MemberAssignment memberAssign in memberInit.Bindings)
            {
                ProjectionField projField = new ProjectionField(memberAssign.Member);

                switch (memberAssign.Expression.NodeType)
                {
                    case ExpressionType.MemberAccess:
                        //occurs during 'select new {e.ID,e.Name}'
                        projField.expr1 = memberAssign.Expression as MemberExpression;
                        break;
                    case ExpressionType.Parameter:
                        //occurs during 'from c ... from o ... select new {c,o}'
                        ParameterExpression paramEx = memberAssign.Expression as ParameterExpression;
                        break;

                    case ExpressionType.Convert:
                        //occurs in 'select (CategoryEnum)o.EmployeeCategory'
                        break;
                    case ExpressionType.Call:
                        //occurs during 'from c ... group o by o.CustomerID into g ... 
                        //select new { g.Key , OrderCount = g.Count() };
                        MethodCallExpression callEx = memberAssign.Expression.XMethodCall();
                        break;
                    default:
                        throw new ArgumentException("L325: Unprepared for " + memberAssign.Expression.NodeType);
                }
                proj.fields.Add(projField);
            }
        }

        /// <summary>
        /// handle many flavors of NewExpressions.
        /// eg. {new <>f__AnonymousType16`2(CustomerID = c.CustomerID, Location = ((c.City + ", ") + c.Country))}
        /// </summary>
        /// <param name="proj"></param>
        /// <param name="newExpr"></param>
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
                        //    projField.expr1 = GroupByExpression.Body as MemberExpression;
                        //    sqlParts.SelectFieldList.Add(projField.expr1.Member.Name);
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

                    case ExpressionType.Add: //{new <>f__AnonymousType16`2(CustomerID = c.CustomerID, Location = ((c.City + ", ") + c.Country))}
                    case ExpressionType.Convert: //'select (CategoryEnum)o.EmployeeCategory'
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
            if (unEx.Operand.NodeType != ExpressionType.Call)
                throw new ArgumentOutOfRangeException("FromSelectMany needs Cast-MethodCall");
            MethodCallExpression methEx = (MethodCallExpression)unEx.Operand;
            foreach (Expression exParm in methEx.Arguments)
            {
                ExpressionType et = exParm.NodeType;
                if (et == ExpressionType.Lambda)
                {
                    LambdaExpression lambda2 = (LambdaExpression)exParm;
                    ProjectionData proj = FromSelectExpr(lambda2);
                    if (proj != null)
                        return proj;
                }
            }
            return null;
        }

    }
}

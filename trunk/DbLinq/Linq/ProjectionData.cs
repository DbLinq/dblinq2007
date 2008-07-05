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
#if MONO_STRICT
    internal
#else
    public
#endif
 class ProjectionData
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
        public MemberInfo AutoGenMember;
        public bool AutoGen { get { return AutoGenMember != null; } }

        /// <summary>
        /// key column, eg. 'productID', used for deletion
        /// </summary>
        public string keyColumnName;

        /// <summary>
        /// nested class which knows which SQL column each field refers to.
        /// Field can be either a Property or Field.
        /// e.g. 'select new {ProductId=p.ProductID}' would create one ProjectionField, pointing to ProductId column. 
        /// </summary>
#if MONO_STRICT
        internal
#else
        public
#endif
        class ProjectionField
        {
            #region ProjectionField
            public static readonly object[] s_emptyIndices = new object[0];

            public ProjectionData nestedTableProjData;
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
            public TypeCategory typeEnum;

            public ColumnAttribute columnAttribute;

            public ProjectionField(Type fieldType)
            {
                fieldType_NoBind = fieldType;
                //projField.type = projField.expr1.Type;
                typeEnum = fieldType.GetCategory();
            }

            /// <summary>
            /// see test F16_NestedObjectSelect() for example of nesting
            /// </summary>
            /// <param name="nestedTableProjData"></param>
            public ProjectionField(ProjectionData nestedTableProjData, MemberInfo memberInfo)
                : this(memberInfo)
            {
                this.nestedTableProjData = nestedTableProjData;
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
                        typeEnum = FieldType.GetCategory();
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

                typeEnum = FieldType.GetCategory();
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

    }
}

#region MIT license
// 
// Copyright (c) 2007-2008 Jiri Moudry
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
using DbLinq.Linq;
using DbLinq.Linq.Mapping;

namespace DbLinq.Util
{
    /// <summary>
    /// Helper class which does the walking over Types to analyze attributes
    /// TODO: rename this to 'ReflectionHelper'?
    /// </summary>
    public class AttribHelper
    {

        /// <summary>
        /// given type Products, find it's [TableAttribute] (or null)
        /// </summary>
        public static TableAttribute GetTableAttrib(Type t)
        {
            //object[] objs = t.GetCustomAttributes(typeof(TableAttribute), false);
            foreach (Type t2 in SelfAndBaseClasses(t))
            {
                object[] objs = t2.GetCustomAttributes(typeof(TableAttribute), true);
                if (objs.Length == 0)
                    continue;
                TableAttribute tbl = objs.OfType<TableAttribute>().FirstOrDefault();
                return tbl;
            }
            return null;
        }

        /// <summary>
        /// enumerate inheritance chain - copied from DynamicLinq
        /// </summary>
        static IEnumerable<Type> SelfAndBaseClasses(Type type)
        {
            while (type != null)
            {
                yield return type;
                type = type.BaseType;
            }
        }

        public static MemberInfo[] GetMemberFields(Type t)
        {
            List<MemberInfo> fields = new List<MemberInfo>();
            foreach (Type t2 in SelfAndBaseClasses(t))
            {
                //find member fields, includes protected members from base class
                MemberInfo[] membersInclBase = t2.FindMembers(MemberTypes.Field
                    , BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
                    , null, null);
                //now remove the base class members (to exclude duplicates):
                var membersWithoutBase = membersInclBase.Where(m => m.DeclaringType == t2);
                fields.AddRange(membersWithoutBase);
            }
            return fields.ToArray();
        }

        /// <summary>
        /// given type Employee, find it's InheritanceMapping attribs.
        /// These list derived classes, such as HourlyEmployee.
        /// See David Hayden's example:
        /// http://davidhayden.com/blog/dave/archive/2007/10/26/LINQToSQLInheritanceDiscriminatorColumnExampleInheritanceMappingTutorial.aspx
        /// </summary>
        public static List<InheritanceMappingAttribute> GetInheritanceAttribs(Type t)
        {
            object[] objs = t.GetCustomAttributes(typeof(InheritanceMappingAttribute), false);
            List<InheritanceMappingAttribute> list = objs.OfType<InheritanceMappingAttribute>().ToList();
            return list;
        }

        /// <summary>
        /// from class Employee, walk all properties, extract their [Column] attribs
        /// </summary>
        public static T[] FindPropertiesWithGivenAttrib<T>(Type t) where T : Attribute
        {
            List<T> lst = new List<T>();
            PropertyInfo[] infos = t.GetProperties();

            foreach (PropertyInfo pi in infos)
            {
                object[] objs = pi.GetCustomAttributes(typeof(T), false);
                List<T> partList = objs.OfType<T>().ToList();
                lst.AddRange(partList);
            }
            return lst.ToArray();
        }

        /// <summary>
        /// from class Employee, walk all properties with [Column] attribs, 
        /// and return array of {field,[Column]} pairs.
        /// </summary>
        public static KeyValuePair<PropertyInfo, T>[] FindPropertiesWithGivenAttrib2<T>(Type t) where T : Attribute
        {
            List<KeyValuePair<PropertyInfo, T>> lst = new List<KeyValuePair<PropertyInfo, T>>();
            PropertyInfo[] infos = t.GetProperties();
            foreach (PropertyInfo pi in infos)
            {
                object[] objs = pi.GetCustomAttributes(typeof(T), false);
                List<T> partList = objs.OfType<T>().ToList();
                //lst.AddRange(partList);
                if (partList.Count == 0)
                    continue;
                lst.Add(new KeyValuePair<PropertyInfo, T>(pi, partList[0]));
            }
            return lst.ToArray();
        }

        /// <summary>
        /// from class Employee, walk all properties, extract their [Column] attribs
        /// </summary>
        public static ColumnAttribute[] GetColumnAttribs(Type t)
        {
            return FindPropertiesWithGivenAttrib<ColumnAttribute>(t);
        }

        /// <summary>
        /// for Microsoft SQL bulk insert, we need a version of GetColumnAttribs with their PropInfo
        /// </summary>
        public static KeyValuePair<PropertyInfo, ColumnAttribute>[] GetColumnAttribs2(Type t)
        {
            return FindPropertiesWithGivenAttrib2<ColumnAttribute>(t);
        }

        /// <summary>
        /// from one column Employee.EmployeeID, extract its [Column] attrib
        /// </summary>
        public static ColumnAttribute GetColumnAttrib(MemberInfo memberInfo)
        {
            object[] colAttribs = memberInfo.GetCustomAttributes(typeof(ColumnAttribute), false);
            if (colAttribs.Length != 1)
            {
                return null;
            }
            ColumnAttribute colAtt0 = colAttribs[0] as ColumnAttribute;
            return colAtt0;
        }

        /// <summary>
        /// get name of column in SQL table, given one C# field (MemberInfo).
        /// </summary>
        public static string GetSQLColumnName(MemberInfo memberInfo)
        {
            ColumnAttribute colAtt0 = GetColumnAttrib(memberInfo);
            return colAtt0 == null ? null : colAtt0.Name;
        }

        /// <summary>
        /// prepate ProjectionData - which holds ctor and field accessors.
        /// </summary>
        public static ProjectionData GetProjectionData(Type t)
        {
            ProjectionData projData = new ProjectionData();
            projData.type = t;
            projData.ctor = t.GetConstructor(new Type[0]);
            ConstructorInfo[] ctors = t.GetConstructors();
            if (ctors.Length == 2 && ctors[0] == projData.ctor)
            {
                //classes generated by MysqlMetal have ctor2 with multiple params:
                //projData.ctor2 = ctors[1];
            }
            //if (projData.ctor2 == null && ctors.Length == 1 && ctors[0].GetParameters().Length > 0)
            //{
            //    //Beta2: "<>f__AnonymousType" ctor has >0 params
            //    //projData.ctor2 = ctors[0];
            //}

            projData.tableAttribute = GetTableAttrib(t);
            projData.inheritanceAttributes.AddRange(GetInheritanceAttribs(t));

            //warning - Andrus points out that order of properties is not guaranteed,
            //and indeed changes after an exception within Studio.
            PropertyInfo[] props = t.GetProperties();

            foreach (PropertyInfo prop in props)
            {
                object[] objs = prop.GetCustomAttributes(typeof(ColumnAttribute), false);
                List<ColumnAttribute> colAtt = objs.OfType<ColumnAttribute>().ToList();
                if (colAtt.Count == 0)
                    continue; //not a DB field
                ProjectionData.ProjectionField projField = new ProjectionData.ProjectionField(prop);
                //projField.type = prop.PropertyType;
                //projField.propInfo = prop;
                projField.columnAttribute = colAtt[0];
                if (!prop.CanWrite)
                    throw new ApplicationException("Cannot retrieve type " + t.Name + " from SQL - field " + prop.Name + " has no setter");
                projData.fields.Add(projField);

                if (colAtt[0].IsPrimaryKey)
                {
                    projData.keyColumnName = colAtt[0].Name;
                }
            }


            // picrap: we use ColumnAttribute properties first, instead of AutoGenIdAttribute
            // if we find none, we pass the hand to AutoGenId handle
            foreach (var propertyInfo in t.GetProperties())
            {
                var columnAttributes = (ColumnAttribute[]) propertyInfo.GetCustomAttributes(typeof (ColumnAttribute), true);
                if(columnAttributes.Length>0)
                {
                    var columnAttribute = columnAttributes[0];
                    if(columnAttribute.IsDbGenerated)
                    {
                        projData.AutoGenMember = propertyInfo;
                        break;
                    }
                }
            }

            if (projData.AutoGenMember == null)
            {
                //now we are looking for '[AutoGenId] protected int productId':
                //MemberInfo[] members = t.FindMembers(MemberTypes.Field, BindingFlags.Instance | BindingFlags.NonPublic, null, null);

                MemberInfo[] members = GetMemberFields(t);
                foreach (FieldInfo field in members.OfType<FieldInfo>())
                {
                    object[] objs = field.GetCustomAttributes(typeof (AutoGenIdAttribute), false);
                    List<AutoGenIdAttribute> att = objs.OfType<AutoGenIdAttribute>().ToList();
                    if (att.Count == 0)
                        continue; //not our field
                    projData.AutoGenMember = field;
                    break;
                }
            }

            return projData;
        }


        /// <summary>
        /// given expression {o.Customer} (type Order.Customer), check whether it has [Association] attribute
        /// </summary>
        public static bool IsAssociation(MemberExpression memberExpr, out AttribAndProp attribAndProp)
        {
            attribAndProp = null;
            if (memberExpr == null)
                return false;
            MemberInfo memberInfo = memberExpr.Member;
            PropertyInfo propInfo = memberInfo as PropertyInfo;
            if (propInfo == null)
                return false;
            AssociationAttribute assoc1;
            bool ok = IsAssociation(propInfo, out assoc1);
            attribAndProp = new AttribAndProp { assoc = assoc1, propInfo = propInfo };
            return ok;
        }

        /// <summary>
        /// given field Order.Customer, check whether it has [Association] attribute
        /// </summary>
        public static bool IsAssociation(PropertyInfo propInfo, out AssociationAttribute assoc)
        {
            object[] objs = propInfo.GetCustomAttributes(typeof(AssociationAttribute), false);
            assoc = objs.OfType<AssociationAttribute>().FirstOrDefault();
            //if (assoc != null && !s_knownAssocs.ContainsKey(assoc))
            //    s_knownAssocs.Add(assoc, propInfo);
            return assoc != null;
        }

        /// <summary>
        /// given type EntityMSet{X}, return X.
        /// </summary>
        public static Type ExtractTypeFromMSet(Type t)
        {
            if (t.IsGenericType && t.Name.EndsWith("Set`1"))
            {
                //EntityMSet
                Type[] t1 = t.GetGenericArguments();
                return t1[0]; //Orders
            }
            return t;
        }

        public static AssociationAttribute FindReverseAssociation(AttribAndProp attribAndProp)
        {
            //if (!s_knownAssocs.ContainsKey(assoc1))
            //    throw new ArgumentException("AZrgument assoc1 must come from previous IsAssociation call");
            //PropertyInfo propInfo = s_knownAssocs[assoc1];
            AssociationAttribute assoc1 = attribAndProp.assoc;
            PropertyInfo propInfo = attribAndProp.propInfo;

            Type parentTableType = propInfo.PropertyType;
            parentTableType = ExtractTypeFromMSet(parentTableType);

            AssociationAttribute[] assocs = FindPropertiesWithGivenAttrib<AssociationAttribute>(parentTableType);

            string assocName = assoc1.Name; //eg. 'FK_Customers_Orders'

            var q = from a in assocs
                    where a.Name == assocName && a.ThisKey != assoc1.ThisKey
                    select a;
            AssociationAttribute result = q.FirstOrDefault();
            if (result == null)
            {
                throw new ApplicationException("Failed to find reverse assoc for " + assoc1.Name + " among " + assocs.Length);
            }
            return result;
        }

        /// <summary>
        /// given type Customer, return CustomerID.
        /// </summary>
        public static KeyValuePair<PropertyInfo, ColumnAttribute>[] FindPrimaryKeys(Type tableType)
        {
            KeyValuePair<PropertyInfo, ColumnAttribute>[] columns = GetColumnAttribs2(tableType);
            return columns.Where(c => c.Value.IsPrimaryKey).ToArray();
        }

        public static FunctionAttribute GetFunctionAttribute(MethodInfo methodInfo)
        {
            object[] customerAttributes = methodInfo.GetCustomAttributes(false);
#pragma warning disable 612,618
            var functionExAttribute = customerAttributes.OfType<FunctionExAttribute>().FirstOrDefault();
#pragma warning restore 612,618
            if (functionExAttribute != null)
                return functionExAttribute.FunctionAttribute;
            FunctionAttribute functionAttribute = customerAttributes.OfType<FunctionAttribute>().FirstOrDefault();
            return functionAttribute;
        }
    }

    public class AttribAndProp
    {
        public PropertyInfo propInfo;
        public AssociationAttribute assoc;
    }
}

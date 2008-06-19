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

using DbLinq.Linq;
using DbLinq.Util;
using DbLinq.Vendor;
using System.Data;

namespace DbLinq.Util
{
    public class RowEnumeratorCompiler<T>
    {
        /// <summary>
        /// the entry point - routes your call into special cases for Projection and primitive types
        /// </summary>
        /// <returns>compiled func which loads object from SQL reader</returns>
        public static Func<IDataRecord, MappingContext, T> CompileRowDelegate(SessionVarsParsed vars, ref int fieldID)
        {
            Func<IDataRecord, MappingContext, T> objFromRow = null;

            ProjectionData projData = vars.ProjectionData;

            //which fields are we selecting? we have three categories to handle:
            //A) one field ('builtin type'):           extract object of primitive / builtin type (eg. string or int or DateTime?)
            //B) all fields of a table:                extract table object, which will be 'newed' and then tracked for changes
            //C) several fields defined by projection: extract a projection object, using default ctor and bindings, no tracking needed.
            bool isBuiltinType = typeof(T).IsPrimitive() || typeof(T).IsEnum || typeof(T) == typeof(byte[]);
            bool isTableType = typeof(T).IsTable();
            bool isProjectedType = typeof(T).IsProjection();
            Type dynamicClassType;
            bool isDynamicType = typeof(T).IsDynamicClass(out dynamicClassType);

            if (projData == null && !isBuiltinType && isTableType)
            {
                //for Table types, use attributes to determine fields
                //for projection types, return projData with only ctor assigned
                projData = ProjectionData.FromDbType(typeof(T));
            }

            if (isBuiltinType)
            {
                objFromRow = CompilePrimitiveRowDelegate(ref fieldID);
            }
            else if (isTableType)
            {
                objFromRow = CompileColumnRowDelegate_TableType(projData, ref fieldID);
            }
            //else if (isProjectedType && vars.GroupByExpression != null)
            else if (isProjectedType && projData == null)
            {
                //now we know what the GroupBy object is, 
                //and what method to use with grouping (eg Count())
                ProjectionData projData2 = ProjectionData.FromReflectedType(typeof(T));
                //and compile the sucker
                objFromRow = CompileProjectedRowDelegate(vars, projData2);
            }
            else if (isProjectedType || isDynamicType)
            {
                objFromRow = CompileProjectedRowDelegate(vars, projData);
            }
            else
            {
                throw new ApplicationException("L124: RowEnumerator can handle basic types or projected types, but not " + typeof(T));
            }
            return objFromRow;
        }

        /// <summary>
        /// given primitive type T (eg. string or int), 
        /// construct and compile a 'reader.GetString(0);' delegate (or similar).
        /// </summary>
        public static
            Func<IDataRecord, MappingContext, T>
            CompilePrimitiveRowDelegate(ref int fieldID)
        {
            #region CompilePrimitiveRowDelegate
            //compile one of these:
            // a) string GetRow(DataReader rdr){ return rdr.GetString(0); }
            // b) int    GetRow(DataReader rdr){ return rdr.GetInt32(0); }

            ParameterExpression rdr = Expression.Parameter(typeof(IDataRecord), "rdr");
            ParameterExpression mappingContext = Expression.Parameter(typeof(MappingContext), "mappingContext");

            Expression body = GetFieldMethodCall(typeof(T), rdr, mappingContext, fieldID++);

            List<ParameterExpression> paramListRdr = new List<ParameterExpression>();
            paramListRdr.Add(rdr);
            paramListRdr.Add(mappingContext);

            LambdaExpression lambda = Expression.Lambda<Func<IDataRecord, MappingContext, T>>(body, paramListRdr);
            Func<IDataRecord, MappingContext, T> func_t = (Func<IDataRecord, MappingContext, T>)lambda.Compile();

            //StringBuilder sb = new StringBuilder();
            //lambda.BuildString(sb);
            //Console.WriteLine("  RowEnumCompiler(Primitive): Compiled "+sb);
            return func_t;
            #endregion
        }

        /// <summary>
        /// given column type T (eg. Customer or Order), 
        /// construct and compile a delegate similar to:
        /// 'new Customer(){_customerID=reader.GetInt32(0),_customerName=reader.GetString(1)),...};' 
        /// 
        /// note: this used to use non-default ctor, but after exceptions, 
        /// order of args got messed up.
        /// </summary>
        public static
            Func<IDataRecord, MappingContext, T>
            CompileColumnRowDelegate_TableType(ProjectionData projData, ref int fieldID)
        {
            if (projData == null)
                throw new ArgumentException("CompileColumnRow: need projData");

            if (projData.inheritanceAttributes.Count > 0)
                return CompileColumnRowDelegate_TableType_Inheritance(projData, ref fieldID);

            ParameterExpression rdr = Expression.Parameter(typeof(IDataRecord), "rdr");
            ParameterExpression mappgingContext = Expression.Parameter(typeof(MappingContext), "mappingContext");

            MemberInitExpression newExprInit = BuildNewExpression_TableType(projData, mappgingContext, rdr, ref fieldID);

            List<ParameterExpression> paramListRdr = new List<ParameterExpression>();
            paramListRdr.Add(rdr);
            paramListRdr.Add(mappgingContext);

            LambdaExpression lambda = Expression.Lambda<Func<IDataRecord, MappingContext, T>>(newExprInit, paramListRdr);
            Func<IDataRecord, MappingContext, T> func_t = (Func<IDataRecord, MappingContext, T>)lambda.Compile();

            //lambda.BuildString(sb);
            //Console.WriteLine("  RowEnumCompiler(Column): Compiled "+sb);

            return func_t;
        }

        public static
            MemberInitExpression
            BuildNewExpression_TableType(ProjectionData projData, ParameterExpression mappgingContext
            , ParameterExpression rdr, ref int fieldID)
        {
            #region CompileColumnRowDelegate

            List<Expression> ctorArgs = new List<Expression>();
            //Andrus points out that order of projData.fields is not reliable after an exception - switch to ctor params

            //given type Customer, find protected fields: _CustomerID,_CompanyName,...
            Type t = typeof(T);
            if (projData.type != typeof(T))
                t = projData.type; //nested select?

            //MemberInfo[] fields1 = t.FindMembers(MemberTypes.Field
            //    , BindingFlags.NonPublic | BindingFlags.Instance
            //    , null, null);
            MemberInfo[] fields1 = AttribHelper.GetMemberFields(t); //this works with derived types also

            Dictionary<string, FieldInfo> fieldNameMap = fields1
                .OfType<FieldInfo>()
                .ToDictionary(mi => mi.Name);

            List<MemberAssignment> bindList = new List<MemberAssignment>();
            foreach (ProjectionData.ProjectionField projFld in projData.fields)
            {
                Type fieldType = projFld.FieldType;

                Expression arg_i;
                MemberAssignment bindEx;
                if (projFld.nestedTableProjData != null)
                {
                    //nested table type
                    arg_i = BuildNewExpression_TableType(projFld.nestedTableProjData, mappgingContext, rdr, ref fieldID);
                    PropertyInfo propInfo = projFld.MemberInfo as PropertyInfo;
                    MethodInfo methodInfo = propInfo.GetSetMethod();
                    bindEx = Expression.Bind(methodInfo, arg_i);
                }
                else
                {
                    //plain field
                    arg_i = GetFieldMethodCall(fieldType, rdr, mappgingContext, fieldID++);

                    string errorIntro = "Cannot retrieve type " + typeof(T) + " from DB, because [Column";
                    if (projFld.columnAttribute == null)
                        throw new ApplicationException("L162: " + errorIntro + "] is missing for field " + fieldID + ": " + projFld.MemberInfo.Name);
                    if (projFld.columnAttribute.Storage == null)
                        throw new ApplicationException("L164: " + errorIntro + " Storage=xx] is missing for col=" + projFld.columnAttribute.Name);

                    string storage = projFld.columnAttribute.Storage; //'_customerID'
                    
                    FieldInfo fieldInfo;
                    if (!fieldNameMap.TryGetValue(storage, out fieldInfo))
                        throw new ApplicationException("L169: " + errorIntro + "Storage=" + storage + "] refers to a non-existent field for col=" + projFld.columnAttribute.Name);

                    //bake expression: "CustomerID = rdr.GetString(0)"
                    bindEx = Expression.Bind(fieldInfo, arg_i);
                }
                bindList.Add(bindEx);
            }

            NewExpression newExpr1 = Expression.New(projData.ctor); //2008Jan: changed to default ctor

            MemberInitExpression newExprInit = Expression.MemberInit(newExpr1, bindList.ToArray());
            return newExprInit;
            #endregion
        }

        /// <summary>
        /// given column type T (eg. Employee or Customer or Order), 
        /// construct and compile a delegate similar to:
        /// 'reader.GetInt32(0)==0
        ///    ? new HourlyEmployee(){_employeeID=reader.GetInt32(0)}
        ///    : new SalariedEmployee(){_employeeID=reader.GetInt32(0)};'
        /// </summary>
        public static
            Func<IDataRecord, MappingContext, T>
            CompileColumnRowDelegate_TableType_Inheritance(ProjectionData projData, ref int fieldID)
        {
            #region CompileColumnRowDelegate
            if (projData == null)
                throw new ArgumentException("CompileColumnRow: need projData");
            if (projData.inheritanceAttributes.Count == 0)
                throw new ArgumentException("CompileColumnRow: need projData with inheritance");

            //1. order (Default derived type comes first)
            var inheritAtts = (from ia in projData.inheritanceAttributes
                               orderby ia.IsDefault descending
                               select ia)
                     .ToList();

            //2. determine integer colIndex of Discriminator column
            var discriminatorCol = (from f in projData.fields
                                    where f.columnAttribute.IsDiscriminator
                                    select f).Single();
            int discriminatorColIndex = projData.fields.IndexOf(discriminatorCol);

            ParameterExpression rdr = Expression.Parameter(typeof(IDataRecord), "rdr");
            ParameterExpression mappgingContext = Expression.Parameter(typeof(MappingContext), "mappingContext");

            List<Expression> ctorArgs = new List<Expression>();
            //Andrus points out that order of projData.fields is not reliable after an exception - switch to ctor params

            //given type Customer, find protected fields: _CustomerID,_CompanyName,...
            Type t = typeof(T);
            MemberInfo[] fields1 = t.FindMembers(MemberTypes.Field
                , BindingFlags.NonPublic | BindingFlags.Instance
                , null, null);

            Dictionary<string, FieldInfo> fieldNameMap = fields1
                .OfType<FieldInfo>()
                .ToDictionary(mi => mi.Name);

            System.Data.Linq.Mapping.InheritanceMappingAttribute defaultInheritanceAtt = inheritAtts[0];
            inheritAtts.RemoveAt(0);

            int discriminatorFieldID = fieldID + discriminatorColIndex;

            //create 'reader.GetInt32(7)'
            Expression readerGetDiscrimExpr = GetFieldMethodCall(discriminatorCol.FieldType, rdr, mappgingContext
                , discriminatorFieldID);

            int fieldID_copy = fieldID;
            Expression defaultNewExpr = TableRow_NewMemberInit(projData, fieldNameMap, defaultInheritanceAtt.Type, rdr, mappgingContext, ref fieldID);

            Expression combinedExpr = defaultNewExpr;
            while (inheritAtts.Count > 0)
            {
                System.Data.Linq.Mapping.InheritanceMappingAttribute inheritanceAtt = inheritAtts[0];
                inheritAtts.RemoveAt(0);

                int fieldID_temp = fieldID_copy;
                Expression newExpr = TableRow_NewMemberInit(projData, fieldNameMap, inheritanceAtt.Type, rdr, mappgingContext, ref fieldID_temp);

                // 'reader.GetInt32(7)==1'
                Expression testExpr = Expression.Equal(
                                            Expression.Constant(inheritanceAtt.Code)
                                            , readerGetDiscrimExpr);

                Expression iif = Expression.Condition(testExpr, newExpr, combinedExpr);
                combinedExpr = iif;
            }

            List<ParameterExpression> paramListRdr = new List<ParameterExpression>();
            paramListRdr.Add(rdr);

            LambdaExpression lambda = Expression.Lambda<Func<IDataRecord, MappingContext, T>>(combinedExpr, paramListRdr);
            Func<IDataRecord, MappingContext, T> func_t = (Func<IDataRecord, MappingContext, T>)lambda.Compile();

            //lambda.BuildString(sb);
            //Console.WriteLine("  RowEnumCompiler(Column): Compiled "+sb);
            return func_t;
            #endregion
        }

        /// <summary>
        /// bake expression such as 
        /// 'reader=>new HourlyEmployee(){_employeeID=reader.GetInt32(0)}'
        /// </summary>
        public static Expression TableRow_NewMemberInit(ProjectionData projData,
            Dictionary<string, FieldInfo> fieldNameMap,
            Type derivedType,
            ParameterExpression rdr, ParameterExpression mappingContext,
            ref int fieldID)
        {
            List<MemberAssignment> bindList = new List<MemberAssignment>();
            foreach (ProjectionData.ProjectionField projFld in projData.fields)
            {
                Type fieldType = projFld.FieldType;
                Expression arg_i = GetFieldMethodCall(fieldType, rdr, mappingContext, fieldID++);

                //bake expression: "CustomerID = rdr.GetString(0)"
                string errorIntro = "Cannot retrieve type " + typeof(T) + " from DB, because [Column";
                if (projFld.columnAttribute == null)
                    throw new ApplicationException("L303: " + errorIntro + "] is missing for field " + fieldID);
                if (projFld.columnAttribute.Storage == null)
                    throw new ApplicationException("L305: " + errorIntro + " Storage=xx] is missing for col=" + projFld.columnAttribute.Name);

                string storage = projFld.columnAttribute.Storage; //'_customerID'
                FieldInfo fieldInfo;
                if (!fieldNameMap.TryGetValue(storage, out fieldInfo))
                    throw new ApplicationException("L310: " + errorIntro + "Storage=" + storage + "] refers to a non-existent field for col=" + projFld.columnAttribute.Name);

                MemberAssignment bindEx = Expression.Bind(fieldInfo, arg_i);
                bindList.Add(bindEx);
            }

            NewExpression newExpr1 = Expression.New(derivedType); //2008Jan: changed to default ctor
            MemberInitExpression newExprInit = Expression.MemberInit(newExpr1, bindList.ToArray());

            // '(Employee) (new HourlyEmployee(){_employeeID=xxx})'
            UnaryExpression newExprInit2 = Expression.TypeAs(newExprInit, projData.type);
            return newExprInit2;
        }

        /// <summary>
        /// given column type T (eg. Customer or Order), 
        /// construct and compile a 'new Customer(reader.GetInt32(0),reader.GetString(1));' 
        /// delegate (or similar).
        /// </summary>
        public static Func<IDataRecord, MappingContext, T> CompileProjectedRowDelegate(SessionVarsParsed vars, ProjectionData projData)
        {
            ParameterExpression rdr = Expression.Parameter(typeof(IDataRecord), "rdr");
            ParameterExpression mappingContext = Expression.Parameter(typeof(MappingContext), "mappingContext");

            StringBuilder sb = new StringBuilder(500);
            int fieldID = 0;
            LambdaExpression lambda = BuildProjectedRowLambda(vars, projData, rdr, mappingContext, ref fieldID);

            //lambda.BuildString(sb);

            //if(vars.log!=null)
            //    vars.log.WriteLine("  RowEnumCompiler(Projection): Compiling "+sb);
            //error lambda not in scope?!
            Func<IDataRecord, MappingContext, T> func_t = (Func<IDataRecord, MappingContext, T>)lambda.Compile();

            return func_t;

        }

        /// <summary>
        /// given user code 'select new {ProductId=p.ProductID}', 
        /// create expression suitable for compilation:
        ///   either 'new <>F__AnonymousType7{rdr.GetUint(0)}' 
        ///   or     'new <>F__AnonymousType7{ProductId=rdr.GetUint(0)}'
        ///   
        /// On Orcas Beta2, projections are handled using the top scheme.
        /// </summary>
        public static
            LambdaExpression
            BuildProjectedRowLambda(SessionVarsParsed vars, ProjectionData projData,
            ParameterExpression rdr, ParameterExpression mappingContext,
            ref int fieldID)
        {

            #region CompileColumnRowDelegate
            bool hasCtor = projData != null && projData.ctor != null;

            if (!hasCtor)
            {
                if (projData.type.IsValueType)
                {
                    //structs have no ctors, but can be new'ed okay
                }
                else
                {
                    throw new ArgumentException("CompileColumnRow: need projData with ctor2");
                }
            }

            if (hasCtor && projData.ctor.GetParameters().Length > 0)
            {
                //Orcas Beta2: previously, used default ctor. Now, must pass params into ctor:
                return BuildProjectedRowLambda_NoBind(vars, projData, rdr, mappingContext, ref fieldID);
            }

            List<MemberBinding> bindings = new List<MemberBinding>();
            //int i=0;
            foreach (ProjectionData.ProjectionField projFld in projData.fields)
            {
                //if( ! projFld.isPrimitiveType)
                switch (projFld.typeEnum)
                {
                    case TypeCategory.Column:
                        {
                            //occurs for 'from c ... from o ... select new {c,o}'
                            //should compile into:
                            //  'new Projection{ new C(field0,field1), new O(field2,field3) }'

                            #region Ugly code to create a generic arg T for Expression.Lambda<T>
                            //simple version: (does not work since BuildProjRow needs different generic arg than our T)
                            //LambdaExpression innerLambda = BuildProjectedRowLambda(vars, projData2, rdr, ref fieldID);                            
                            //nasty version:
                            ProjectionData projData2 = AttribHelper.GetProjectionData(projFld.FieldType);
                            Type TArg2 = projFld.FieldType;
                            Type rowEnumCompilerType2 = typeof(RowEnumeratorCompiler<>).MakeGenericType(TArg2);
                            object rowCompiler2 = Activator.CreateInstance(rowEnumCompilerType2);
                            MethodInfo[] mis = rowEnumCompilerType2.GetMethods();
                            MethodInfo mi = rowEnumCompilerType2.GetMethod("BuildProjectedRowLambda");
                            object[] methodArgs = new object[] { vars, projData2, rdr, fieldID };
                            //...and call BuildProjectedRowLambda():
                            object objResult = mi.Invoke(rowCompiler2, methodArgs);
                            fieldID = (int)methodArgs[3];
                            LambdaExpression innerLambda = (LambdaExpression)objResult;
                            #endregion
                            //LambdaExpression innerLambda = BuildProjectedRowLambda(vars, projData2, rdr, ref fieldID);

                            MemberInitExpression innerInit = innerLambda.Body as MemberInitExpression;
                            //MemberAssignment binding = Expression.Bind(projFld.propInfo, innerInit);
                            MemberAssignment binding = projFld.BuildMemberAssignment(innerInit);
                            bindings.Add(binding);
                        }
                        break;
                    case TypeCategory.Primitive:
                        {
                            Type fieldType = projFld.FieldType;
                            Expression arg_i = GetFieldMethodCall(fieldType, rdr, mappingContext, fieldID++);
                            //MethodInfo accessor = null;
                            MemberAssignment binding = Expression.Bind(projFld.MemberInfo, arg_i);
                            bindings.Add(binding);
                        }
                        break;
                    case TypeCategory.Other:
                        {
                            //e.g.: "select new {g.Key,g}" - but g is also a projection
                            //Expression.MemberInit
                            if (vars.GroupByNewExpression == null)
                                throw new ApplicationException("TODO - handle other cases than groupByNewExpr");

                            MemberAssignment binding = GroupHelper2<T>.BuildProjFieldBinding(vars, projFld, rdr, mappingContext, ref fieldID);
                            bindings.Add(binding);
                        }
                        break;

                    default:
                        throw new ApplicationException("TODO - objects other than primitive and entities in CompileProjRow: " + projFld.FieldType);
                }

            }

            NewExpression newExpr = hasCtor
                ? Expression.New(projData.ctor) //for classes
                : Expression.New(projData.type); //for structs

            MemberInitExpression memberInit = Expression.MemberInit(newExpr, bindings);

            List<ParameterExpression> paramListRdr = new List<ParameterExpression>();
            paramListRdr.Add(rdr);
            paramListRdr.Add(mappingContext);

            LambdaExpression lambda = Expression.Lambda<Func<IDataRecord, MappingContext, T>>(memberInit, paramListRdr);
            return lambda;
            //StringBuilder sb = new StringBuilder(500);
            //Func<DataReader2,T> func_t = (Func<DataReader2,T>)lambda.Compile();
            //lambda.BuildString(sb);
            //Console.WriteLine("  RowEnumCompiler(Projection): Compiled "+sb);
            //return func_t;
            #endregion
        }

        /// <summary>
        /// given user code 'select new {ProductId=p.ProductID}', 
        /// create expression 'new <>F__AnonymousType7{reader.GetUint(0)}' - suitable for compilation
        /// </summary>
        public static
            LambdaExpression
            BuildProjectedRowLambda_NoBind(SessionVarsParsed vars, ProjectionData projData,
                                        ParameterExpression reader, ParameterExpression mappingContext, ref int fieldID)
        {

            #region CompileColumnRowDelegate
            if (projData == null || projData.ctor == null)
                throw new ArgumentException("BuildProjectedRowLambda_NoBind: need projData with ctor");
            if (projData.ctor.GetParameters().Length == 0)
                throw new ArgumentException("BuildProjectedRowLambda_NoBind: need projData with non-default ctor");

            List<Expression> argList = new List<Expression>();
            //List<MemberBinding> bindings = new List<MemberBinding>();

            foreach (ProjectionData.ProjectionField projFld in projData.fields)
            {
                switch (projFld.typeEnum)
                {
                    case TypeCategory.Column:
                        {
                            //occurs for 'from c ... from o ... select new {c,o}'
                            //should compile into:
                            //  'new Projection{ new C(field0,field1), new O(field2,field3) }'

                            #region Ugly code to create a generic arg T for Expression.Lambda<T>
                            //simple version: (does not work since BuildProjRow needs different generic arg than our T)
                            //LambdaExpression innerLambda = BuildProjectedRowLambda(vars, projData2, reader, ref fieldID);                            
                            //nasty version:
                            ProjectionData projData2 = AttribHelper.GetProjectionData(projFld.FieldType);
                            Type TArg2 = projFld.FieldType;
                            Type rowEnumCompilerType2 = typeof(RowEnumeratorCompiler<>).MakeGenericType(TArg2);
                            object rowCompiler2 = Activator.CreateInstance(rowEnumCompilerType2);
                            MethodInfo[] mis = rowEnumCompilerType2.GetMethods();
                            MethodInfo mi = rowEnumCompilerType2.GetMethod("BuildProjectedRowLambda");
                            object[] methodArgs = new object[] { vars, projData2, reader, mappingContext, fieldID };
                            //...and call BuildProjectedRowLambda():
                            object objResult = mi.Invoke(rowCompiler2, methodArgs);
                            fieldID = (int)methodArgs[4];
                            LambdaExpression innerLambda = (LambdaExpression)objResult;
                            #endregion
                            //LambdaExpression innerLambda = BuildProjectedRowLambda(vars, projData2, reader, ref fieldID);

                            MemberInitExpression innerInit = innerLambda.Body as MemberInitExpression;
                            //MemberAssignment binding = Expression.Bind(projFld.propInfo, innerInit);
                            //MemberAssignment binding = projFld.BuildMemberAssignment(innerInit);
                            //bindings.Add(binding);
                            argList.Add(innerInit);
                        }
                        break;
                    case TypeCategory.Primitive:
                        {
                            Type fieldType = projFld.FieldType;
                            Expression arg_i = GetFieldMethodCall(fieldType, reader, mappingContext, fieldID++);
                            //MethodInfo accessor = null;
                            //MemberAssignment binding = Expression.Bind(projFld.MemberInfo, arg_i);
                            //bindings.Add(binding);
                            argList.Add(arg_i);
                        }
                        break;
                    case TypeCategory.Other:
                        {
                            //e.g.: "select new {g.Key,g}" - but g is also a projection
                            //Expression.MemberInit
                            if (vars.GroupByNewExpression == null)
                                throw new ApplicationException("TODO - handle other cases than groupByNewExpr");

                            //MemberAssignment binding = GroupHelper2<T>.BuildProjFieldBinding(vars, projFld, reader, ref fieldID);
                            //bindings.Add(binding);
                            throw new Exception("TODO L351 - when compiling, handle type" + projFld.typeEnum);
                        }
                    //break;

                    default:
                        throw new ApplicationException("TODO - objects other than primitive and entities in CompileProjRow: " + projFld.FieldType);
                }
            }

            //List<Expression> paramZero = new List<Expression>();
            //paramZero.Add( Expression.Constant(0) ); //that's the zero in GetInt32(0)
            //MethodCallExpression body = Expression.CallVirtual(minfo,reader,paramZero);
            NewExpression newExpr = Expression.New(projData.ctor, argList);
            //MemberInitExpression memberInit = Expression.MemberInit(newExpr, bindings);

            List<ParameterExpression> paramListRdr = new List<ParameterExpression>();
            paramListRdr.Add(reader);
            paramListRdr.Add(mappingContext);

            //LambdaExpression lambda = Expression.Lambda<Func<DataReader2, T>>(memberInit, paramListRdr);
            LambdaExpression lambda = Expression.Lambda<Func<IDataRecord, MappingContext, T>>(newExpr, paramListRdr);
            return lambda;
            //StringBuilder sb = new StringBuilder(500);
            //Func<DataReader2,T> func_t = (Func<DataReader2,T>)lambda.Compile();
            //lambda.BuildString(sb);
            //Console.WriteLine("  RowEnumCompiler(Projection): Compiled "+sb);
            //return func_t;
            #endregion
        }

        /// <summary>
        /// return 'reader.GetString(0)' or 'reader.GetInt32(1)'
        /// as an Expression suitable for compilation
        /// </summary>
        private static Expression GetFieldMethodCall(Type t2, ParameterExpression reader,
                                                            ParameterExpression mappingContext, int fieldID)
        {
            Expression callExpr = GetPropertyReader(reader, mappingContext, t2, fieldID);
            return callExpr;
        }

        private static Expression GetSimplePropertyReader(Type returnType, int valueIndex,
            Expression reader, Expression mappingContext)
        {
            bool cast;
            var propertyReader = GetSimplePropertyReader(returnType, valueIndex, out cast);
            propertyReader = Expression.Invoke(propertyReader, reader, mappingContext);
            if (cast)
                propertyReader = Expression.Convert(propertyReader, returnType);
            return propertyReader;
        }

        private static string GetAsString(IDataRecord dataRecord, int columnIndex, MappingContext mappingContext)
        {
            var value = dataRecord.GetAsString(columnIndex);
            mappingContext.OnGetAsString(dataRecord, ref value, typeof(T), columnIndex);
            return value;
        }

        private static object GetAsObject(IDataRecord dataRecord, int columnIndex, MappingContext mappingContext)
        {
            var value = dataRecord.GetAsObject(columnIndex);
            mappingContext.OnGetAsObject(dataRecord, ref value, typeof(T), columnIndex);
            return value;
        }

        private static Expression GetSimplePropertyReader(Type returnType, int valueIndex,
            out bool recast)
        {
            recast = false;
            Expression propertyReader;
            if (returnType == typeof(string))
            {
                propertyReader = (Expression<Func<IDataRecord, MappingContext, string>>)((dataReader, mappingContext)
                    => GetAsString(dataReader, valueIndex, mappingContext));
            }
            else if (returnType == typeof(bool))
            {
                propertyReader = (Expression<Func<IDataRecord, MappingContext, bool>>)((dataReader, mappingContext)
                    => dataReader.GetAsBool(valueIndex));
            }
            else if (returnType == typeof(char))
            {
                propertyReader = (Expression<Func<IDataRecord, MappingContext, char>>)((dataReader, mappingContext)
                    => dataReader.GetAsChar(valueIndex));
            }
            else if (returnType == typeof(byte))
            {
                propertyReader = (Expression<Func<IDataRecord, MappingContext, byte>>)((dataReader, mappingContext)
                    => dataReader.GetAsNumeric<byte>(valueIndex));
            }
            else if (returnType == typeof(sbyte))
            {
                propertyReader = (Expression<Func<IDataRecord, MappingContext, sbyte>>)((dataReader, mappingContext)
                    => dataReader.GetAsNumeric<sbyte>(valueIndex));
            }
            else if (returnType == typeof(short))
            {
                propertyReader = (Expression<Func<IDataRecord, MappingContext, short>>)((dataReader, mappingContext)
                    => dataReader.GetAsNumeric<short>(valueIndex));
            }
            else if (returnType == typeof(ushort))
            {
                propertyReader = (Expression<Func<IDataRecord, MappingContext, ushort>>)((dataReader, mappingContext)
                    => dataReader.GetAsNumeric<ushort>(valueIndex));
            }
            else if (returnType == typeof(int))
            {
                propertyReader = (Expression<Func<IDataRecord, MappingContext, int>>)((dataReader, mappingContext)
                    => dataReader.GetAsNumeric<int>(valueIndex));
            }
            else if (returnType == typeof(uint))
            {
                propertyReader = (Expression<Func<IDataRecord, MappingContext, uint>>)((dataReader, mappingContext)
                    => dataReader.GetAsNumeric<uint>(valueIndex));
            }
            else if (returnType == typeof(long))
            {
                propertyReader = (Expression<Func<IDataRecord, MappingContext, long>>)((dataReader, mappingContext)
                    => dataReader.GetAsNumeric<long>(valueIndex));
            }
            else if (returnType == typeof(ulong))
            {
                propertyReader = (Expression<Func<IDataRecord, MappingContext, ulong>>)((dataReader, mappingContext)
                    => dataReader.GetAsNumeric<ulong>(valueIndex));
            }
            else if (returnType == typeof(float))
            {
                propertyReader = (Expression<Func<IDataRecord, MappingContext, float>>)((dataReader, mappingContext)
                    => dataReader.GetAsNumeric<float>(valueIndex));
            }
            else if (returnType == typeof(double))
            {
                propertyReader = (Expression<Func<IDataRecord, MappingContext, double>>)((dataReader, mappingContext)
                    => dataReader.GetAsNumeric<double>(valueIndex));
            }
            else if (returnType == typeof(decimal))
            {
                propertyReader = (Expression<Func<IDataRecord, MappingContext, decimal>>)((dataReader, mappingContext)
                    => dataReader.GetAsNumeric<decimal>(valueIndex));
            }
            else if (returnType == typeof(DateTime))
            {
                propertyReader = (Expression<Func<IDataRecord, MappingContext, DateTime>>)((dataReader, mappingContext)
                    => dataReader.GetDateTime(valueIndex));
            }
            else if (returnType == typeof(Guid))
            {
                propertyReader = (Expression<Func<IDataRecord, MappingContext, Guid>>)((dataReader, mappingContext)
                    => dataReader.GetGuid(valueIndex));
            }
            else if (returnType == typeof(byte[]))
            {
                propertyReader = (Expression<Func<IDataRecord, MappingContext, byte[]>>)((dataReader, mappingContext)
                    => dataReader.GetAsBytes(valueIndex));
            }
            else if (returnType.IsEnum)
            {
                recast = true;
                propertyReader = (Expression<Func<IDataRecord, MappingContext, int>>)((dataReader, mappingContext)
                    => dataReader.GetAsEnum(returnType, valueIndex));
            }
            // for polymorphic types especially for ExecuteQuery<>()
            else if (returnType == typeof(object))
            {
                propertyReader = (Expression<Func<IDataRecord, MappingContext, object>>)((dataReader, mappingContext)
                    => GetAsObject(dataReader, valueIndex, mappingContext));
            }
            else
            {
                //s_rdr.GetUInt32();
                //s_rdr.GetFloat();
                string msg = "RowEnum TODO L381: add support for type " + returnType;
                Console.WriteLine(msg);
                propertyReader = null;
                throw new ApplicationException(msg);
            }
            //if (propertyReader == null)
            //{
            //    Console.WriteLine("L298: Reference to invalid function name");
            //}
            return propertyReader;
        }

        private static Type GetNullableTypeArgument(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return type.GetGenericArguments()[0];
            }
            return null;
        }

        public static Expression GetPropertyReader(Expression reader, Expression mappingContext, Type returnType, int valueIndex)
        {
            Expression propertyReader;
            Type nullableValueType = GetNullableTypeArgument(returnType);
            if (nullableValueType != null)
            {
                Expression simplePropertyReader = Expression.Convert(
                GetSimplePropertyReader(nullableValueType, valueIndex, reader, mappingContext), returnType);
                Expression zero = Expression.Constant(null, returnType);
                propertyReader = Expression.Condition(
                    Expression.Invoke((Expression<Func<IDataRecord, MappingContext, bool>>)((dataReader, context) => dataReader.IsDBNull(valueIndex)), reader, mappingContext),
                    zero, simplePropertyReader);
            }
            else
            {
                propertyReader = GetSimplePropertyReader(returnType, valueIndex, reader, mappingContext);
            }
            return propertyReader;
        }

        /// <summary>
        /// given Employee 'e', compile a method similar to 'e.ID.ToString()',
        /// which returns the object ID as a string[].
        /// 
        /// Composite PK objects return array with multiple entries.
        /// For OrderDetails sample table, we return: 
        ///   'new string[]{OrderID.ToString(),ProductID.ToString()}'
        /// </summary>
        public static Func<T, string[]> CompileIDRetrieval(ProjectionData projData)
        {
            if (projData == null || projData.tableAttribute == null)
                throw new ArgumentNullException("CompiledIDRetrieval: needs object with [Table] attribute");

            //find the ID fields
            List<MemberInfo> minfos = (from f in projData.fields
                                       where f.columnAttribute != null && f.columnAttribute.IsPrimaryKey
                                       select f.MemberInfo).ToList();
            if (minfos.Count == 0)
                throw new ArgumentNullException("CompiledIDRetrieval: needs object with [Table] attribute and one [Column(Id=true)]");

            ParameterExpression param = Expression.Parameter(typeof(T), "obj");
            List<ParameterExpression> lambdaParams = new List<ParameterExpression>();
            lambdaParams.Add(param);

            List<Expression> toStringCalls = new List<Expression>();
            foreach (PropertyInfo minfo in minfos)
            {
                MemberExpression member = Expression.Property(param, minfo);
                MethodInfo minfo3 = minfo.PropertyType.GetMethod("ToString", new Type[0]);
                Expression body = Expression.Call(member, minfo3);
                toStringCalls.Add(body);
            }

            Expression newStringArr = Expression.NewArrayInit(typeof(string), toStringCalls.ToArray());

            LambdaExpression lambda = Expression.Lambda<Func<T, string[]>>(newStringArr, lambdaParams);
            Func<T, string[]> func_t = (Func<T, string[]>)lambda.Compile();
            return func_t;
        }

        /// <summary>
        /// helper method for reading in an int from SqlDataReader, and casting it to enum
        /// </summary>
        public static T2 GetEnum<T2>(IDataRecord reader, int field)
        {
            int i = reader.GetInt32(field);
            return (T2)Enum.ToObject(typeof(T2), i);
        }



    }

    //public static class RowConverter {

    //  internal static string OnGetString(object src, Type t, IDataRecord dr,
    //      int index) {
    //    if (GetString == null)
    //      return src.ToString();
    //    return GetString(src, t, dr, index);
    //  }
    //}

}

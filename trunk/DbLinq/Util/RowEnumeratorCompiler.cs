#region MIT License
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

using DBLinq.Linq;
using DBLinq.Vendor;
using System.Data;

namespace DBLinq.Util
{
    public class RowEnumeratorCompiler<T>
    {
        /// <summary>
        /// the entry point - routes your call into special cases for Projection and primitive types
        /// </summary>
        /// <returns>compiled func which loads object from SQL reader</returns>
        public static Func<IDataReader2, T> CompileRowDelegate(SessionVarsParsed vars, ref int fieldID)
        {
            Func<IDataReader2, T> objFromRow = null;

            ProjectionData projData = vars.projectionData;

            //which fields are we selecting? we have three categories to handle:
            //A) one field ('builtin type'):           extract object of primitive / builtin type (eg. string or int or DateTime?)
            //B) all fields of a table:                extract table object, which will be 'newed' and then tracked for changes
            //C) several fields defined by projection: extract a projection object, using default ctor and bindings, no tracking needed.
            bool isBuiltinType = CSharp.IsPrimitiveType(typeof(T)) || typeof(T).IsEnum;
            bool isTableType = CSharp.IsTableType(typeof(T));
            bool isProjectedType = CSharp.IsProjection(typeof(T));

            if (projData == null && !isBuiltinType)
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
            else if (isProjectedType && vars.groupByExpr != null)
            {
                //now we know what the GroupBy object is, 
                //and what method to use with grouping (eg Count())
                ProjectionData projData2 = ProjectionData.FromReflectedType(typeof(T));
                //and compile the sucker
                objFromRow = CompileProjectedRowDelegate(vars, projData2);
            }
            else if (isProjectedType)
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
            Func<IDataReader2, T>
            CompilePrimitiveRowDelegate(ref int fieldID)
        {
            #region CompilePrimitiveRowDelegate
            //compile one of these:
            // a) string GetRow(DataReader rdr){ return rdr.GetString(0); }
            // b) int    GetRow(DataReader rdr){ return rdr.GetInt32(0); }

            ParameterExpression rdr = Expression.Parameter(typeof(IDataReader2), "rdr");

            Expression body = GetFieldMethodCall(typeof(T), rdr, fieldID++);

            List<ParameterExpression> paramListRdr = new List<ParameterExpression>();
            paramListRdr.Add(rdr);

            LambdaExpression lambda = Expression.Lambda<Func<IDataReader2, T>>(body, paramListRdr);
            Func<IDataReader2, T> func_t = (Func<IDataReader2, T>)lambda.Compile();

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
            Func<IDataReader2, T>
            CompileColumnRowDelegate_TableType(ProjectionData projData, ref int fieldID)
        {
            #region CompileColumnRowDelegate
            if (projData == null)
                throw new ArgumentException("CompileColumnRow: need projData");

            if (projData.inheritanceAttributes.Count > 0)
                return CompileColumnRowDelegate_TableType_Inheritance(projData, ref fieldID);

            ParameterExpression rdr = Expression.Parameter(typeof(IDataReader2), "rdr");

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

            List<MemberAssignment> bindList = new List<MemberAssignment>();
            foreach (ProjectionData.ProjectionField projFld in projData.fields)
            {
                Type fieldType = projFld.FieldType;
                Expression arg_i = GetFieldMethodCall(fieldType, rdr, fieldID++);

                //bake expression: "CustomerID = rdr.GetString(0)"
                string errorIntro = "Cannot retrieve type " + typeof(T) + " from DB, because [Column";
                if (projFld.columnAttribute == null)
                    throw new ApplicationException("L162: " + errorIntro + "] is missing for field " + fieldID);
                if (projFld.columnAttribute.Storage == null)
                    throw new ApplicationException("L164: " + errorIntro + " Storage=xx] is missing for col=" + projFld.columnAttribute.Name);

                string storage = projFld.columnAttribute.Storage; //'_customerID'
                FieldInfo fieldInfo;
                if (!fieldNameMap.TryGetValue(storage, out fieldInfo))
                    throw new ApplicationException("L169: " + errorIntro + "Storage=" + storage + "] refers to a non-existent field for col=" + projFld.columnAttribute.Name);

                MemberAssignment bindEx = Expression.Bind(fieldInfo, arg_i);
                bindList.Add(bindEx);
            }

            NewExpression newExpr1 = Expression.New(projData.ctor); //2008Jan: changed to default ctor
            List<ParameterExpression> paramListRdr = new List<ParameterExpression>();
            paramListRdr.Add(rdr);

            Expression newExprInit = Expression.MemberInit(newExpr1, bindList.ToArray());


            LambdaExpression lambda = Expression.Lambda<Func<IDataReader2, T>>(newExprInit, paramListRdr);
            Func<IDataReader2, T> func_t = (Func<IDataReader2, T>)lambda.Compile();

            //lambda.BuildString(sb);
            //Console.WriteLine("  RowEnumCompiler(Column): Compiled "+sb);
            return func_t;
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
            Func<IDataReader2, T>
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

            ParameterExpression rdr = Expression.Parameter(typeof(IDataReader2), "rdr");

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
            Expression readerGetDiscrimExpr = GetFieldMethodCall(discriminatorCol.FieldType, rdr
                , discriminatorFieldID);

            int fieldID_copy = fieldID;
            Expression defaultNewExpr = TableRow_NewMemberInit(projData, fieldNameMap, defaultInheritanceAtt.Type, rdr, ref fieldID);

            Expression combinedExpr = defaultNewExpr;
            while (inheritAtts.Count > 0)
            {
                System.Data.Linq.Mapping.InheritanceMappingAttribute inheritanceAtt = inheritAtts[0];
                inheritAtts.RemoveAt(0);

                int fieldID_temp = fieldID_copy;
                Expression newExpr = TableRow_NewMemberInit(projData, fieldNameMap, inheritanceAtt.Type, rdr, ref fieldID_temp);

                // 'reader.GetInt32(7)==1'
                Expression testExpr = Expression.Equal(
                                            Expression.Constant(inheritanceAtt.Code)
                                            , readerGetDiscrimExpr);

                Expression iif = Expression.Condition(testExpr, newExpr, combinedExpr);
                combinedExpr = iif;
            }

            List<ParameterExpression> paramListRdr = new List<ParameterExpression>();
            paramListRdr.Add(rdr);

            LambdaExpression lambda = Expression.Lambda<Func<IDataReader2, T>>(combinedExpr, paramListRdr);
            Func<IDataReader2, T> func_t = (Func<IDataReader2, T>)lambda.Compile();

            //lambda.BuildString(sb);
            //Console.WriteLine("  RowEnumCompiler(Column): Compiled "+sb);
            return func_t;
            #endregion
        }

        /// <summary>
        /// bake expression such as 
        /// 'reader=>new HourlyEmployee(){_employeeID=reader.GetInt32(0)}'
        /// </summary>
        static Expression TableRow_NewMemberInit(ProjectionData projData
            , Dictionary<string, FieldInfo> fieldNameMap
            , Type derivedType
            , ParameterExpression rdr, ref int fieldID)
        {
            List<MemberAssignment> bindList = new List<MemberAssignment>();
            foreach (ProjectionData.ProjectionField projFld in projData.fields)
            {
                Type fieldType = projFld.FieldType;
                Expression arg_i = GetFieldMethodCall(fieldType, rdr, fieldID++);

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
        public static Func<IDataReader2, T> CompileProjectedRowDelegate(SessionVarsParsed vars, ProjectionData projData)
        {
            ParameterExpression rdr = Expression.Parameter(typeof(IDataReader2), "rdr");

            StringBuilder sb = new StringBuilder(500);
            int fieldID = 0;
            LambdaExpression lambda = BuildProjectedRowLambda(vars, projData, rdr, ref fieldID);

            //lambda.BuildString(sb);

            //if(vars.log!=null)
            //    vars.log.WriteLine("  RowEnumCompiler(Projection): Compiling "+sb);
            //error lambda not in scope?!
            Func<IDataReader2, T> func_t = (Func<IDataReader2, T>)lambda.Compile();

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
            BuildProjectedRowLambda(SessionVarsParsed vars, ProjectionData projData, ParameterExpression rdr, ref int fieldID)
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
                return BuildProjectedRowLambda_NoBind(vars, projData, rdr, ref fieldID);
            }

            List<MemberBinding> bindings = new List<MemberBinding>();
            //int i=0;
            foreach (ProjectionData.ProjectionField projFld in projData.fields)
            {
                //if( ! projFld.isPrimitiveType)
                switch (projFld.typeEnum)
                {
                    case TypeEnum.Column:
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
                    case TypeEnum.Primitive:
                        {
                            Type fieldType = projFld.FieldType;
                            Expression arg_i = GetFieldMethodCall(fieldType, rdr, fieldID++);
                            //MethodInfo accessor = null;
                            MemberAssignment binding = Expression.Bind(projFld.MemberInfo, arg_i);
                            bindings.Add(binding);
                        }
                        break;
                    case TypeEnum.Other:
                        {
                            //e.g.: "select new {g.Key,g}" - but g is also a projection
                            //Expression.MemberInit
                            if (vars.groupByNewExpr == null)
                                throw new ApplicationException("TODO - handle other cases than groupByNewExpr");

                            MemberAssignment binding = GroupHelper2<T>.BuildProjFieldBinding(vars, projFld, rdr, ref fieldID);
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

            LambdaExpression lambda = Expression.Lambda<Func<IDataReader2, T>>(memberInit, paramListRdr);
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
        /// create expression 'new <>F__AnonymousType7{rdr.GetUint(0)}' - suitable for compilation
        /// </summary>
        public static
            LambdaExpression
            BuildProjectedRowLambda_NoBind(SessionVarsParsed vars, ProjectionData projData, ParameterExpression rdr, ref int fieldID)
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
                    case TypeEnum.Column:
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
                            //MemberAssignment binding = projFld.BuildMemberAssignment(innerInit);
                            //bindings.Add(binding);
                            argList.Add(innerInit);
                        }
                        break;
                    case TypeEnum.Primitive:
                        {
                            Type fieldType = projFld.FieldType;
                            Expression arg_i = GetFieldMethodCall(fieldType, rdr, fieldID++);
                            //MethodInfo accessor = null;
                            //MemberAssignment binding = Expression.Bind(projFld.MemberInfo, arg_i);
                            //bindings.Add(binding);
                            argList.Add(arg_i);
                        }
                        break;
                    case TypeEnum.Other:
                        {
                            //e.g.: "select new {g.Key,g}" - but g is also a projection
                            //Expression.MemberInit
                            if (vars.groupByNewExpr == null)
                                throw new ApplicationException("TODO - handle other cases than groupByNewExpr");

                            //MemberAssignment binding = GroupHelper2<T>.BuildProjFieldBinding(vars, projFld, rdr, ref fieldID);
                            //bindings.Add(binding);
                            throw new Exception("TODO L351 - when compiling, handle type" + projFld.typeEnum);
                        }
                        break;

                    default:
                        throw new ApplicationException("TODO - objects other than primitive and entities in CompileProjRow: " + projFld.FieldType);
                }
            }

            //List<Expression> paramZero = new List<Expression>();
            //paramZero.Add( Expression.Constant(0) ); //that's the zero in GetInt32(0)
            //MethodCallExpression body = Expression.CallVirtual(minfo,rdr,paramZero);
            NewExpression newExpr = Expression.New(projData.ctor, argList);
            //MemberInitExpression memberInit = Expression.MemberInit(newExpr, bindings);

            List<ParameterExpression> paramListRdr = new List<ParameterExpression>();
            paramListRdr.Add(rdr);

            //LambdaExpression lambda = Expression.Lambda<Func<DataReader2, T>>(memberInit, paramListRdr);
            LambdaExpression lambda = Expression.Lambda<Func<IDataReader2, T>>(newExpr, paramListRdr);
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
        private static Expression GetFieldMethodCall(Type t2, ParameterExpression rdr, int fieldID)
        {
            Expression callExpr = GetPropertyReader(rdr, t2, fieldID);
            return callExpr;
        }

        private static Expression GetFieldMethodCall0(Type t2, ParameterExpression rdr, int fieldID)
        {
            MethodInfo minfo = ChooseFieldRetrievalMethod(t2);
            if (minfo == null)
            {
                string msg = "GetFieldMethodCall L180: failed to get methodInfo for type " + t2;
                Console.WriteLine(msg);
                throw new Exception(msg);
            }
            List<Expression> paramZero = new List<Expression>();
            paramZero.Add(Expression.Constant(fieldID)); //that's the zero in GetInt32(0)
            MethodCallExpression callExpr = Expression.Call(rdr, minfo, paramZero);
            return callExpr;
        }

        /// <summary>
        /// return 'reader.GetString(0)' or 'reader.GetInt32(1)'
        /// as an Expression suitable for compilation.
        /// 
        /// Note: In C# 2006MayCTP, the compiled expression 
        /// 'reader.IsDbNull(0)?null:reader.GetInt32(0)'
        /// crashes the virtual machine.
        /// Instead of using MySqlDataReader, call into a wrapper class 'DataReader2' which handles nullable types for us
        /// </summary>
        static MethodInfo ChooseFieldRetrievalMethod(Type t2)
        {
            #region ChooseFieldRetrievalMethod
            //TODO: handle Nullable<int> as well as int?
            MethodInfo minfo = null;
            BindingFlags flags = BindingFlags.FlattenHierarchy|BindingFlags.Instance|BindingFlags.Public|BindingFlags.InvokeMethod;
            if (t2 == typeof(string))
            {
                minfo = typeof(IDataReader2).GetMethod("GetString", flags);
            }
            else if (t2 == typeof(bool))
            {
                minfo = typeof(IDataReader2).GetMethod("GetBoolean", flags);
            }
            else if (t2 == typeof(bool?))
            {
                minfo = typeof(IDataReader2).GetMethod("GetBooleanN", flags);
            }
            else if (t2 == typeof(char))
            {
                minfo = typeof(IDataReader2).GetMethod("GetChar", flags);
            }
            else if (t2 == typeof(char?))
            {
                minfo = typeof(IDataReader2).GetMethod("GetCharN", flags);
            }
            else if (t2 == typeof(short))
            {
                minfo = typeof(IDataReader2).GetMethod("GetInt16", flags);
            }
            else if (t2 == typeof(short?))
            {
                minfo = typeof(IDataReader2).GetMethod("GetInt16N", flags);
            }
            else if (t2 == typeof(int))
            {
                minfo = typeof(IDataReader2).GetMethod("GetInt32", flags);
            }
            else if (t2 == typeof(int?))
            {
                minfo = typeof(IDataReader2).GetMethod("GetInt32N", flags);
            }
            else if (t2 == typeof(uint))
            {
                minfo = typeof(IDataReader2).GetMethod("GetUInt32", flags);
            }
            else if (t2 == typeof(uint?))
            {
                minfo = typeof(IDataReader2).GetMethod("GetUInt32N", flags);
            }
            else if (t2 == typeof(float))
            {
                minfo = typeof(IDataReader2).GetMethod("GetFloat", flags);
            }
            else if (t2 == typeof(float?))
            {
                minfo = typeof(IDataReader2).GetMethod("GetFloatN", flags);
            }
            else if (t2 == typeof(double))
            {
                minfo = typeof(IDataReader2).GetMethod("GetDouble", flags);
            }
            else if (t2 == typeof(double?))
            {
                minfo = typeof(IDataReader2).GetMethod("GetDoubleN", flags);
            }
            else if (t2 == typeof(decimal))
            {
                minfo = typeof(IDataReader2).GetMethod("GetDecimal", flags);
            }
            else if (t2 == typeof(decimal?))
            {
                minfo = typeof(IDataReader2).GetMethod("GetDecimalN", flags);
            }
            else if (t2 == typeof(DateTime))
            {
                minfo = typeof(IDataReader2).GetMethod("GetDateTime", flags);
            }
            else if (t2 == typeof(DateTime?))
            {
                minfo = typeof(IDataReader2).GetMethod("GetDateTimeN", flags);
            }
            else if (t2 == typeof(long))
            {
                minfo = typeof(IDataReader2).GetMethod("GetInt64", flags);
            }
            else if (t2 == typeof(long?))
            {
                minfo = typeof(IDataReader2).GetMethod("GetInt64N", flags);
            }
            else if (t2 == typeof(byte[]))
            {
                minfo = typeof(IDataReader2).GetMethod("GetBytes", flags);
            }
            else if (t2.IsEnum)
            {
                //minfo = typeof(DataReader2).GetMethod("GetInt32");
                MethodInfo genericMInfo = typeof(IDataReader2).GetMethod("GetEnum", flags);
                minfo = genericMInfo.MakeGenericMethod(t2);
                //if(minfo.isg
                //string msg = "RowEnum TODO L377: compile casting from int to enum " + t2;
                //Console.WriteLine(msg);
                //throw new ApplicationException(msg);
            }
            else if (t2 == typeof(byte))
            {
                minfo = typeof(IDataReader2).GetMethod("GetByte", flags);
            }
            else if (t2 == typeof(byte?))
            {
                minfo = typeof(IDataReader2).GetMethod("GetByteN", flags);
            }
            else
            {
                //s_rdr.GetUInt32();
                //s_rdr.GetFloat();
                string msg = "RowEnum TODO L381: add support for type " + t2;
                Console.WriteLine(msg);
                throw new ApplicationException(msg);
            }
            if (minfo == null)
            {
                Console.WriteLine("L298: Reference to invalid function name");
            }
            return minfo;
            #endregion
        }

        #region GetAs*

        // please note that sometimes (depending on driver), GetValue() returns DBNull instead of null
        // so at this level, we handle both

        private static string GetAsString(IDataReader2 dataRecord, int index)
        {
            object o = dataRecord.GetValue(index);
            if (o == null || o is DBNull)
                return null;
            return o.ToString();
        }

        private static bool GetAsBool(IDataReader2 dataRecord, int index)
        {
            object b = dataRecord.GetValue(index);
            // first check: this may be a boolean
            if (b is bool)
                return (bool)b;
            // if it is a string, we may have "T"/"F" or "True"/"False"
            if (b is string)
            {
                // regular literals
                string lb = (string)b;
                bool ob;
                if (bool.TryParse(lb, out ob))
                    return ob;
                // alternative literals
                if (lb == "T" || lb == "F")
                    return lb == "T";
            }
            return GetAsNumeric<int>(dataRecord, index) != 0;
        }

        private static char GetAsChar(IDataReader2 dataRecord, int index)
        {
            object c = dataRecord.GetValue(index);
            if (c is char)
                return (char)c;
            if (c is string)
            {
                string sc = (string)c;
                if (sc.Length == 1)
                    return sc[0];
            }
            if (c == null || c is DBNull)
                return '\0';
            throw new InvalidCastException(string.Format("Can't convert type {0} in GetAsChar()", c.GetType().Name));
        }

        private static U GetAsNumeric<U>(IDataReader2 dataRecord, int index)
        {
            return GetAsNumeric<U>(dataRecord.GetValue(index));
        }

        private static U GetAsNumeric<U>(object o)
        {
            if (o is U)
                return (U)o;
            if (o == null || o is DBNull)
                return (U)Convert.ChangeType(0, typeof(U)); // this is a trick, since I found no simple way to do the cas
            string methodName = string.Format("To{0}", typeof(U).Name);
            MethodInfo convertMethod = typeof(Convert).GetMethod(methodName, new Type[] { o.GetType() });
            if (convertMethod != null)
                return (U)convertMethod.Invoke(null, new object[] { o });
            throw new InvalidCastException(string.Format("Can't convert type {0} in Convert.{1}()", o.GetType().Name, methodName));
        }

        private static object GetAsEnum(IDataReader2 dataRecord, Type enumType, int index)
        {
            int enumAsInt = GetAsNumeric<int>(dataRecord,index);
            return enumAsInt;
        }

        #endregion

        private static Expression GetSimplePropertyReader(Type returnType, int valueIndex)
        {
            Expression propertyReader;
            if (returnType == typeof(string))
            {
                propertyReader = (Expression<Func<IDataReader2, string>>)((IDataReader2 dataReader) => GetAsString(dataReader, valueIndex));
            }
            else if (returnType == typeof(bool))
            {
                propertyReader = (Expression<Func<IDataReader2, bool>>)((IDataReader2 dataReader) => GetAsBool(dataReader, valueIndex));
            }
            else if (returnType == typeof(char))
            {
                propertyReader = (Expression<Func<IDataReader2, char>>)((IDataReader2 dataReader) => GetAsChar(dataReader, valueIndex));
            }
            else if (returnType == typeof(byte))
            {
                propertyReader = (Expression<Func<IDataReader2, byte>>)((IDataReader2 dataReader) => GetAsNumeric<byte>(dataReader, valueIndex));
            }
            else if (returnType == typeof(sbyte))
            {
                propertyReader = (Expression<Func<IDataReader2, sbyte>>)((IDataReader2 dataReader) => GetAsNumeric<sbyte>(dataReader, valueIndex));
            }
            else if (returnType == typeof(short))
            {
                propertyReader = (Expression<Func<IDataReader2, short>>)((IDataReader2 dataReader) => GetAsNumeric<short>(dataReader, valueIndex));
            }
            else if (returnType == typeof(ushort))
            {
                propertyReader = (Expression<Func<IDataReader2, ushort>>)((IDataReader2 dataReader) => GetAsNumeric<ushort>(dataReader, valueIndex));
            }
            else if (returnType == typeof(int))
            {
                propertyReader = (Expression<Func<IDataReader2, int>>)((IDataReader2 dataReader) => GetAsNumeric<int>(dataReader, valueIndex));
            }
            else if (returnType == typeof(uint))
            {
                propertyReader = (Expression<Func<IDataReader2, uint>>)((IDataReader2 dataReader) => GetAsNumeric<uint>(dataReader, valueIndex));
            }
            else if (returnType == typeof(long))
            {
                propertyReader = (Expression<Func<IDataReader2, long>>)((IDataReader2 dataReader) => GetAsNumeric<long>(dataReader, valueIndex));
            }
            else if (returnType == typeof(ulong))
            {
                propertyReader = (Expression<Func<IDataReader2, ulong>>)((IDataReader2 dataReader) => GetAsNumeric<ulong>(dataReader, valueIndex));
            }
            else if (returnType == typeof(float))
            {
                propertyReader = (Expression<Func<IDataReader2, float>>)((IDataReader2 dataReader) => GetAsNumeric<float>(dataReader, valueIndex));
            }
            else if (returnType == typeof(double))
            {
                propertyReader = (Expression<Func<IDataReader2, double>>)((IDataReader2 dataReader) => GetAsNumeric<double>(dataReader, valueIndex));
            }
            else if (returnType == typeof(decimal))
            {
                propertyReader = (Expression<Func<IDataReader2, decimal>>)((IDataReader2 dataReader) => GetAsNumeric<decimal>(dataReader, valueIndex));
            }
            else if (returnType == typeof(DateTime))
            {
                propertyReader = (Expression<Func<IDataReader2, DateTime>>)((IDataReader2 dataReader) => dataReader.GetDateTime(valueIndex));
            }
            else if (returnType == typeof(byte[]))
            {
                propertyReader = (Expression<Func<IDataReader2, byte[]>>)((IDataReader2 dataReader) => dataReader.GetBytes(valueIndex));
            }
            else if (returnType.IsEnum)
            {
                propertyReader = Expression.Convert((Expression<Func<IDataReader2, int>>)((IDataReader2 dataReader) => GetAsNumeric<int>(dataReader, valueIndex)), returnType);
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

        private static Expression GetPropertyReader(Expression reader, Type returnType, int valueIndex)
        {
            Expression propertyReader;
            Type nullableValueType = GetNullableTypeArgument(returnType);
            if (nullableValueType != null)
            {
                Expression simplePropertyReader = Expression.Convert(Expression.Invoke(GetSimplePropertyReader(nullableValueType, valueIndex), reader), returnType);
                Expression zero = Expression.Constant(null, returnType);
                propertyReader = Expression.Condition(
                    Expression.Invoke((Expression<Func<IDataReader2, bool>>)((IDataReader2 dataReader) => dataReader.IsDBNull(valueIndex)), reader),
                    zero, simplePropertyReader);
            }
            else
            {
                propertyReader = Expression.Invoke(GetSimplePropertyReader(returnType, valueIndex), reader);
            }
            return propertyReader;
        }

        /// <summary>
        /// given Employee 'e', compile a method similar to 'e.ID.ToString()',
        /// which returns the object ID as a string
        /// </summary>
        public static Func<T, string> CompileIDRetrieval(ProjectionData projData)
        {
            if (projData == null || projData.tableAttribute == null)
                throw new ArgumentNullException("CompiledIDRetrieval: needs object with [Table] attribute");
            //find the ID field
            PropertyInfo minfo = null;
            foreach (ProjectionData.ProjectionField projFld in projData.fields)
            {
                if (projFld.columnAttribute == null)
                    continue;

                //if (projFld.columnAttribute.Id)
                if (projFld.columnAttribute.IsPrimaryKey)
                {
                    //found the ID column
                    minfo = projFld.MemberInfo as PropertyInfo;
                    break;
                }
            }
            if (minfo == null)
                throw new ArgumentNullException("CompiledIDRetrieval: needs object with [Table] attribute and one [Column(Id=true)]");
            ParameterExpression param = Expression.Parameter(typeof(T), "obj");
            MemberExpression member = Expression.Property(param, minfo);
            List<ParameterExpression> lambdaParams = new List<ParameterExpression>();
            lambdaParams.Add(param);
            MethodInfo minfo3 = minfo.PropertyType.GetMethod("ToString", new Type[0]);
            Expression body = Expression.Call(member, minfo3);

            LambdaExpression lambda = Expression.Lambda<Func<T, string>>(body, lambdaParams);
            Func<T, string> func_t = (Func<T, string>)lambda.Compile();
            return func_t;
        }

        /// <summary>
        /// helper method for reading in an int from SqlDataReader, and casting it to enum
        /// </summary>
        public static T2 GetEnum<T2>(IDataReader2 reader, int field)
        {
            int i = reader.GetInt32(field);
            return (T2)Enum.ToObject(typeof(T2), i);
        }



    }
}

////////////////////////////////////////////////////////////////////
//Initial author: Jiri George Moudry, 2006.
//License: LGPL. (Visit http://www.gnu.org)
//Commercial code may call into this library, if it's in a different module (DLL)
////////////////////////////////////////////////////////////////////


using System;
using System.Reflection;
using System.Query;
using System.Expressions;
using System.Collections.Generic;
using System.Text;
using DBLinq.Linq;

namespace DBLinq.util
{
    public class RowEnumeratorCompiler<T>
    {
        /// <summary>
        /// the entry point - routes your call into special cases for Projection and primitive types
        /// </summary>
        /// <returns>compiled func which loads object from SQL reader</returns>
        public static Func<DataReader2,T> CompileRowDelegate(SessionVars vars, ref int fieldID)
        {
            Func<DataReader2,T> objFromRow = null;

            ProjectionData projData = vars.projectionData;

            //three categories to handle:
            //A) extract object of primitive / builtin type (eg. string or int)
            //B) extract column object, which will be 'newed' and then tracked for changes
            //C) extract a projection object, using default ctor and bindings, no tracking needed.
            bool isBuiltinType = CSharp.IsPrimitiveType(typeof(T));
            bool isColumnType  = CSharp.IsColumnType(typeof(T));
            bool isProjectedType = CSharp.IsProjection(typeof(T));
            //bool isGroup = IsGroupBy();

            if(projData==null && !isBuiltinType)
            {
                //for Table types, use attributes to determine fields
                //for projection types, return projData with only ctor assigned
                projData = ProjectionData.FromDbType(typeof(T));
            }

            //if(isGroup)
            //{
            //    //_objFromRow2, SQL string handled in RowEnumGroupBy
            //    return null;
            //}
            //else 
            if(isBuiltinType)
            {
                objFromRow = RowEnumeratorCompiler<T>.CompilePrimitiveRowDelegate(ref fieldID);
            }
            else if(isColumnType)
            {
                objFromRow = RowEnumeratorCompiler<T>.CompileColumnRowDelegate(projData, ref fieldID);
            }
            else if(isProjectedType && vars.groupByExpr!=null)
            {
                //now we know what the GroupBy object is, 
                //and what method to use with grouping (eg Count())
                //_projectionData.type = typeof(T);
                //vars._sqlParts.selectFieldList.Add("Count(*)");

                //ProjectionData projData2 = ProjectionData.FromReflectedType(typeof(T));
                //and compile the sucker
                //objFromRow = RowEnumeratorCompiler<T>.CompileProjectedRowDelegate(vars, projData2);
                objFromRow = RowEnumeratorCompiler<T>.CompileProjectedRowDelegate(vars, vars.projectionData);
            }
            else if(isProjectedType)
            {
                objFromRow = RowEnumeratorCompiler<T>.CompileProjectedRowDelegate(vars, projData);
            }
            else
            {
                throw new ApplicationException("L124: RowEnumerator can handle basic types or projected types, but not "+typeof(T));
            }
            return objFromRow;
        }

        /// <summary>
        /// given primitive type T (eg. string or int), 
        /// construct and compile a 'reader.GetString(0);' delegate (or similar).
        /// </summary>
        public static 
            Func<DataReader2,T> 
            CompilePrimitiveRowDelegate(ref int fieldID)
        {
            #region CompilePrimitiveRowDelegate
            //compile one of these:
            // a) string GetRow(DataReader rdr){ return rdr.GetString(0); }
            // b) int    GetRow(DataReader rdr){ return rdr.GetInt32(0); }

            ParameterExpression rdr = Expression.Parameter(typeof(DataReader2),"rdr");

            MethodCallExpression body = GetFieldMethodCall(typeof(T),rdr,fieldID++);

            List<ParameterExpression> paramListRdr = new List<ParameterExpression>();
            paramListRdr.Add(rdr);

            LambdaExpression lambda = Expression.Lambda<Func<DataReader2,T>>(body,paramListRdr);
            Func<DataReader2,T> func_t = (Func<DataReader2,T>)lambda.Compile();

            StringBuilder sb = new StringBuilder();
            lambda.BuildString(sb);
            //Console.WriteLine("  RowEnumCompiler(Primitive): Compiled "+sb);
            return func_t;
            #endregion
        }

        /// <summary>
        /// given column type T (eg. Customer or Order), 
        /// construct and compile a 'new Customer(reader.GetInt32(0),reader.GetString(1));' 
        /// delegate (or similar).
        /// </summary>
        public static 
            Func<DataReader2,T> 
            CompileColumnRowDelegate(ProjectionData projData, ref int fieldID)
        {
            #region CompileColumnRowDelegate
            if(projData==null || projData.ctor2==null)
                throw new ArgumentException("CompileColumnRow: need projData with ctor2");

            ParameterExpression rdr = Expression.Parameter(typeof(DataReader2),"rdr");
            
            List<Expression> ctorArgs = new List<Expression>();

            foreach(ProjectionData.ProjectionField projFld in projData.fields)
            {
                Type fieldType = projFld.type;
                MethodCallExpression arg_i = GetFieldMethodCall(fieldType,rdr,fieldID++);
                ctorArgs.Add(arg_i);
            }

            //List<Expression> paramZero = new List<Expression>();
            //paramZero.Add( Expression.Constant(0) ); //that's the zero in GetInt32(0)
            //MethodCallExpression body = Expression.CallVirtual(minfo,rdr,paramZero);
            NewExpression newExpr = Expression.New(projData.ctor2, ctorArgs);

            List<ParameterExpression> paramListRdr = new List<ParameterExpression>();
            paramListRdr.Add(rdr);
            StringBuilder sb = new StringBuilder(500);

            LambdaExpression lambda = Expression.Lambda<Func<DataReader2,T>>(newExpr,paramListRdr);
            Func<DataReader2,T> func_t = (Func<DataReader2,T>)lambda.Compile();

            lambda.BuildString(sb);
            //Console.WriteLine("  RowEnumCompiler(Column): Compiled "+sb);
            return func_t;
            #endregion
        }

        /// <summary>
        /// given column type T (eg. Customer or Order), 
        /// construct and compile a 'new Customer(reader.GetInt32(0),reader.GetString(1));' 
        /// delegate (or similar).
        /// </summary>
        public static Func<DataReader2,T> CompileProjectedRowDelegate(SessionVars vars, ProjectionData projData)
        {
            ParameterExpression rdr = Expression.Parameter(typeof(DataReader2),"rdr");

            StringBuilder sb = new StringBuilder(500);
            int fieldID = 0;
            LambdaExpression lambda = BuildProjectedRowLambda(vars, projData, rdr, ref fieldID);

            lambda.BuildString(sb);

            if(vars.log!=null)
                vars.log.WriteLine("  RowEnumCompiler(Projection): Compiling "+sb);
            //error lambda not in scope?!
            Func<DataReader2,T> func_t = (Func<DataReader2,T>)lambda.Compile();

            return func_t;

        }

        internal static 
            LambdaExpression
            BuildProjectedRowLambda(SessionVars vars, ProjectionData projData, ParameterExpression rdr, ref int fieldID)
        {

            #region CompileColumnRowDelegate
            if(projData==null || projData.ctor==null)
                throw new ArgumentException("CompileColumnRow: need projData with ctor2");
            
            //List<Expression> ctorArgs = new List<Expression>();
            List<Binding> bindings = new List<Binding>();
            //int i=0;
            foreach(ProjectionData.ProjectionField projFld in projData.fields)
            {
                //if( ! projFld.isPrimitiveType)
                switch(projFld.typeEnum)
                {
                    case TypeEnum.Column:
                        {
                            //occurs for 'from c ... from o ... select new {c,o}'
                            //should compile into:
                            //  'new Projection{ new C(field0,field1), new O(field2,field3) }'
                            ProjectionData projData2 = AttribHelper.GetProjectionData(projFld.type);
                            LambdaExpression innerLambda = BuildProjectedRowLambda(vars, projData2, rdr, ref fieldID);
                            MemberInitExpression innerInit = innerLambda.Body as MemberInitExpression;
                            //MemberAssignment binding = Expression.Bind(projFld.propInfo, innerInit);
                            MemberAssignment binding = projFld.BuildMemberAssignment(innerInit);
                            bindings.Add(binding);
                        }
                        break;
                    case TypeEnum.Primitive:
                        {
                            Type fieldType = projFld.type;
                            MethodCallExpression arg_i = GetFieldMethodCall(fieldType, rdr, fieldID++);
                            //MethodInfo accessor = null;
                            MemberAssignment binding = Expression.Bind(projFld.MemberInfo, arg_i);
                            bindings.Add(binding);
                        }
                        break;
                    case TypeEnum.Other:
                        {
                            //e.g.: "select new {g.Key,g}" - but g is also a projection
                            //Expression.MemberInit
                            if(vars.groupByNewExpr==null)
                                throw new ApplicationException("TODO - handle other cases than groupByNewExpr");

                            MemberAssignment binding = GroupHelper2<T>.BuildProjFieldBinding(vars, projFld, rdr, ref fieldID);
                            bindings.Add(binding);
                        }
                        break;

                    default:
                        throw new ApplicationException("TODO - objects other than primitive and entities in CompileProjRow: "+projFld.type);
                }

                //Type fieldType = projFld.type;
                //MethodCallExpression arg_i = GetFieldMethodCall(fieldType,rdr,i++);
                ////MethodInfo accessor = null;
                //MemberAssignment binding = Expression.Bind(projFld.propInfo, arg_i);
                //bindings.Add(binding);
            }

            //List<Expression> paramZero = new List<Expression>();
            //paramZero.Add( Expression.Constant(0) ); //that's the zero in GetInt32(0)
            //MethodCallExpression body = Expression.CallVirtual(minfo,rdr,paramZero);
            NewExpression newExpr = Expression.New(projData.ctor);
            MemberInitExpression memberInit = Expression.MemberInit(newExpr,bindings);

            List<ParameterExpression> paramListRdr = new List<ParameterExpression>();
            paramListRdr.Add(rdr);

            LambdaExpression lambda = Expression.Lambda<Func<DataReader2,T>>(memberInit,paramListRdr);
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
        static MethodCallExpression GetFieldMethodCall(Type t2,Expression rdr,int fieldID)
        {
            MethodInfo minfo = ChooseFieldRetrievalMethod(t2);
            if(minfo==null)
            {
                string msg = "GetFieldMethodCall L180: failed to get methodInfo for type " + t2;
                Console.WriteLine(msg);
                throw new Exception(msg);
            }
            List<Expression> paramZero = new List<Expression>();
            paramZero.Add( Expression.Constant(fieldID) ); //that's the zero in GetInt32(0)
            MethodCallExpression callExpr = Expression.CallVirtual(minfo,rdr,paramZero);
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
            if(t2==typeof(string))
            {
                minfo = typeof(DataReader2).GetMethod("GetString");
            }
            else if(t2==typeof(bool))
            {
                minfo = typeof(DataReader2).GetMethod("GetBoolean");
            }
            else if(t2==typeof(bool?))
            {
                minfo = typeof(DataReader2).GetMethod("GetBooleanN");
            }
            else if(t2==typeof(char))
            {
                minfo = typeof(DataReader2).GetMethod("GetChar");
            }
            else if(t2==typeof(char?))
            {
                minfo = typeof(DataReader2).GetMethod("GetCharN");
            }
            else if(t2==typeof(short))
            {
                minfo = typeof(DataReader2).GetMethod("GetInt16");
            }
            else if(t2==typeof(short?))
            {
                minfo = typeof(DataReader2).GetMethod("GetInt16N");
            }
            else if (t2 == typeof(int))
            {
                minfo = typeof(DataReader2).GetMethod("GetInt32");
            }
            else if (t2 == typeof(int?))
            {
                minfo = typeof(DataReader2).GetMethod("GetInt32N");
            }
            else if (t2 == typeof(uint))
            {
                minfo = typeof(DataReader2).GetMethod("GetUInt32");
            }
            else if(t2==typeof(uint?))
            {
                minfo = typeof(DataReader2).GetMethod("GetUInt32N");
            }
            else if(t2==typeof(float))
            {
                minfo = typeof(DataReader2).GetMethod("GetFloat");
            }
            else if(t2==typeof(double))
            {
                minfo = typeof(DataReader2).GetMethod("GetDouble");
            }
            else if (t2 == typeof(double?))
            {
                minfo = typeof(DataReader2).GetMethod("GetDoubleN");
            }
            else if (t2 == typeof(decimal))
            {
                minfo = typeof(DataReader2).GetMethod("GetDecimal");
            }
            else if(t2==typeof(DateTime))
            {
                minfo = typeof(DataReader2).GetMethod("GetDateTime");
            }
            else if(t2==typeof(DateTime?))
            {
                minfo = typeof(DataReader2).GetMethod("GetDateTimeN");
            }
            else if(t2==typeof(long))
            {
                minfo = typeof(DataReader2).GetMethod("GetInt64");
            }
            else if(t2==typeof(long?))
            {
                minfo = typeof(DataReader2).GetMethod("GetInt64N");
            }
            else if(t2==typeof(byte[]))
            {
                minfo = typeof(DataReader2).GetMethod("GetBytes");
            }
            else if (t2.IsEnum)
            {
                minfo = typeof(DataReader2).GetMethod("GetInt32");
                string msg = "RowEnum TODO L377: compile casting from int to enum " + t2;
                Console.WriteLine(msg);
                throw new ApplicationException(msg);
            }
            else
            {
                //s_rdr.GetUInt32();
                //s_rdr.GetFloat();
                string msg = "RowEnum TODO L381: add support for type " + t2;
                Console.WriteLine(msg);
                throw new ApplicationException(msg);
            }
            if(minfo==null){
                Console.WriteLine("L298: Reference to invalid function name");
            }
            return minfo;
            #endregion
        }

        /// <summary>
        /// given Employee 'e', compile a method similar to 'e.ID.ToString()',
        /// which returns the object ID as a string
        /// </summary>
        public static Func<T,string> CompileIDRetrieval(ProjectionData projData)
        {
            if(projData==null || projData.tableAttribute==null)
                throw new ArgumentNullException("CompiledIDRetrieval: needs object with [Table] attribute");
            //find the ID field
            PropertyInfo minfo = null;
            foreach(ProjectionData.ProjectionField projFld in projData.fields)
            {
                if(projFld.columnAttribute==null)
                    continue;
                if(projFld.columnAttribute.Id)
                {
                    //found the ID column
                    minfo = projFld.MemberInfo as PropertyInfo;
                    break;
                }
            }
            if(minfo==null)
                throw new ArgumentNullException("CompiledIDRetrieval: needs object with [Table] attribute and one [Column(Id=true)]");
            ParameterExpression param = Expression.Parameter(typeof(T),"obj");
            MemberExpression member = Expression.Property(param,minfo);
            List<ParameterExpression> lambdaParams = new List<ParameterExpression>();
            lambdaParams.Add(param);
            MethodInfo minfo3 = minfo.PropertyType.GetMethod("ToString",new Type[0]);
            Expression body = Expression.Call(minfo3, member);
            LambdaExpression lambda = Expression.Lambda<Func<T,string>>(body,lambdaParams);
            Func<T,string> func_t = (Func<T,string>)lambda.Compile();
            return func_t;
        }

    }
}

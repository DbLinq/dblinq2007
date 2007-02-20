////////////////////////////////////////////////////////////////////
//Initial author: Jiri George Moudry, 2006.
//License: LGPL. (Visit http://www.gnu.org)
//Commercial code may call into this library, if it's in a different module (DLL)
////////////////////////////////////////////////////////////////////

//#define USE_MYSQLREADER_DIRECTLY

using System;
using System.Reflection;
using System.Query;
using System.Expressions;
using System.Collections.Generic;
using System.Text;
//using MySql.Data.MySqlClient;
using DBLinq.Linq;

namespace DBLinq.util
{
    public class RowEnumeratorCompiler<T>
    {
#if USE_MYSQLREADER_DIRECTLY
        static MySqlDataReader s_rdr;
#endif

        /// <summary>
        /// given primitive type T (eg. string or int), 
        /// construct and compile a 'reader.GetString(0);' delegate (or similar).
        /// </summary>
        public static 
#if USE_MYSQLREADER_DIRECTLY
            Func<MySqlDataReader,T> 
#else
            Func<DataReader2,T> 
#endif
            CompilePrimitiveRowDelegate()
        {
            #region CompilePrimitiveRowDelegate
            //compile one of these:
            // a) string GetRow(DataReader rdr){ return rdr.GetString(0); }
            // b) int    GetRow(DataReader rdr){ return rdr.GetInt32(0); }

#if USE_MYSQLREADER_DIRECTLY
            ParameterExpression rdr = Expression.Parameter(typeof(MySqlDataReader),"rdr");
#else
            ParameterExpression rdr = Expression.Parameter(typeof(DataReader2),"rdr");
#endif

            MethodCallExpression body = GetFieldMethodCall(typeof(T),rdr,0);
            //MethodInfo minfo = ChooseFieldRetrievalMethod(typeof(T));
            //List<Expression> paramZero = new List<Expression>();
            //paramZero.Add( Expression.Constant(0) ); //that's the zero in GetInt32(0)
            //MethodCallExpression body = Expression.CallVirtual(minfo,rdr,paramZero);

            List<ParameterExpression> paramListRdr = new List<ParameterExpression>();
            paramListRdr.Add(rdr);
#if USE_MYSQLREADER_DIRECTLY
            LambdaExpression lambda = Expression.Lambda<Func<MySqlDataReader,T>>(body,paramListRdr);
            Func<MySqlDataReader,T> func_t = (Func<MySqlDataReader,T>)lambda.Compile();
#else
            LambdaExpression lambda = Expression.Lambda<Func<DataReader2,T>>(body,paramListRdr);
            Func<DataReader2,T> func_t = (Func<DataReader2,T>)lambda.Compile();
#endif
            StringBuilder sb = new StringBuilder();
            lambda.BuildString(sb);
            Console.WriteLine("  RowEnumCompiler(Primitive): Compiled "+sb);
            return func_t;
            #endregion
        }

        /// <summary>
        /// given column type T (eg. Customer or Order), 
        /// construct and compile a 'new Customer(reader.GetInt32(0),reader.GetString(1));' 
        /// delegate (or similar).
        /// </summary>
        public static 
#if USE_MYSQLREADER_DIRECTLY
            Func<MySqlDataReader,T> 
#else
            Func<DataReader2,T> 
#endif
            CompileColumnRowDelegate(ProjectionData projData)
        {
            #region CompileColumnRowDelegate
            if(projData==null || projData.ctor2==null)
                throw new ArgumentException("CompileColumnRow: need projData with ctor2");

#if USE_MYSQLREADER_DIRECTLY
            ParameterExpression rdr = Expression.Parameter(typeof(MySqlDataReader),"rdr");
#else
            ParameterExpression rdr = Expression.Parameter(typeof(DataReader2),"rdr");
#endif
            
            List<Expression> ctorArgs = new List<Expression>();
            int i=0;
            foreach(ProjectionData.ProjectionField projFld in projData.fields)
            {
                Type fieldType = projFld.type;
                MethodCallExpression arg_i = GetFieldMethodCall(fieldType,rdr,i++);
                ctorArgs.Add(arg_i);
            }

            //List<Expression> paramZero = new List<Expression>();
            //paramZero.Add( Expression.Constant(0) ); //that's the zero in GetInt32(0)
            //MethodCallExpression body = Expression.CallVirtual(minfo,rdr,paramZero);
            NewExpression newExpr = Expression.New(projData.ctor2, ctorArgs);

            List<ParameterExpression> paramListRdr = new List<ParameterExpression>();
            paramListRdr.Add(rdr);
            StringBuilder sb = new StringBuilder(500);
#if USE_MYSQLREADER_DIRECTLY
            LambdaExpression lambda = Expression.Lambda<Func<MySqlDataReader,T>>(newExpr,paramListRdr);
            Func<MySqlDataReader,T> func_t = (Func<MySqlDataReader,T>)lambda.Compile();
#else
            LambdaExpression lambda = Expression.Lambda<Func<DataReader2,T>>(newExpr,paramListRdr);
            Func<DataReader2,T> func_t = (Func<DataReader2,T>)lambda.Compile();
#endif
            lambda.BuildString(sb);
            Console.WriteLine("  RowEnumCompiler(Column): Compiled "+sb);
            return func_t;
            #endregion
        }

        /// <summary>
        /// given column type T (eg. Customer or Order), 
        /// construct and compile a 'new Customer(reader.GetInt32(0),reader.GetString(1));' 
        /// delegate (or similar).
        /// </summary>
        public static Func<DataReader2,T> CompileProjectedRowDelegate(ProjectionData projData)
        {
            ParameterExpression rdr = Expression.Parameter(typeof(DataReader2),"rdr");

            StringBuilder sb = new StringBuilder(500);
            int fieldID = 0;
            LambdaExpression lambda = BuildProjectedRowLambda(projData, rdr, ref fieldID);

            lambda.BuildString(sb);
            Console.WriteLine("  RowEnumCompiler(Projection): Compiling "+sb);
            //error lambda not in scope?!
            Func<DataReader2,T> func_t = (Func<DataReader2,T>)lambda.Compile();

            return func_t;

        }

        private static 
            //Func<DataReader2,T> 
            LambdaExpression
            BuildProjectedRowLambda(ProjectionData projData, ParameterExpression rdr, ref int fieldID)
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
                            LambdaExpression innerLambda = BuildProjectedRowLambda(projData2, rdr, ref fieldID);
                            MemberInitExpression innerInit = innerLambda.Body as MemberInitExpression;
                            MemberAssignment binding = Expression.Bind(projFld.propInfo, innerInit);
                            bindings.Add(binding);
                        }
                        break;
                    case TypeEnum.Primitive:
                        {
                            Type fieldType = projFld.type;
                            MethodCallExpression arg_i = GetFieldMethodCall(fieldType, rdr, fieldID++);
                            //MethodInfo accessor = null;
                            MemberAssignment binding = Expression.Bind(projFld.propInfo, arg_i);
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
                Console.WriteLine("GetFieldMethodCall L180: failed to get methodInfo for type "+t2);
            }
            List<Expression> paramZero = new List<Expression>();
            paramZero.Add( Expression.Constant(fieldID) ); //that's the zero in GetInt32(0)
            MethodCallExpression callExpr = Expression.CallVirtual(minfo,rdr,paramZero);
            return callExpr;
        }

#if USE_MYSQLREADER_DIRECTLY
        static MethodInfo ChooseFieldRetrievalMethod(Type t2)
        {
            //TODO: handle Nullable<int> as well as int?
            MethodInfo minfo = null;
            if(t2==typeof(string))
            {
                minfo = typeof(MySqlDataReader).GetMethod("GetString");
            }
            else if(t2==typeof(int))
            {
                minfo = typeof(MySqlDataReader).GetMethod("GetInt32");
            }
            else if(t2==typeof(uint))
            {
                minfo = typeof(MySqlDataReader).GetMethod("GetUInt32");
            }
            else if(t2==typeof(float))
            {
                minfo = typeof(MySqlDataReader).GetMethod("GetFloat");
            }
            else if(t2==typeof(DateTime))
            {
                minfo = typeof(MySqlDataReader).GetMethod("GetDateTime");
            }
            else if(t2==typeof(char))
            {
                minfo = typeof(MySqlDataReader).GetMethod("GetChar");
            }
            else
            {
                //s_rdr.GetUInt32();
                //s_rdr.GetFloat();
                string msg = "RowEnum TODO L116: add support for type "+t2;
                Console.WriteLine(msg);
                throw new ApplicationException(msg);
            }
            return minfo;
        }
#else
        /// <summary>
        /// In C# 2006MayCTP, the compiled expression 
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
            else if(t2==typeof(int))
            {
                minfo = typeof(DataReader2).GetMethod("GetInt32");
            }
            else if(t2==typeof(uint))
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
            else if(t2==typeof(decimal))
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
            else if(t2==typeof(char))
            {
                minfo = typeof(DataReader2).GetMethod("GetChar");
            }
            else if(t2==typeof(byte[]))
            {
                minfo = typeof(DataReader2).GetMethod("GetBytes");
            }
            else
            {
                //s_rdr.GetUInt32();
                //s_rdr.GetFloat();
                string msg = "RowEnum TODO L176: add support for type "+t2;
                Console.WriteLine(msg);
                throw new ApplicationException(msg);
            }
            if(minfo==null){
                Console.WriteLine("L298: Reference to invalid function name");
            }
            return minfo;
            #endregion
        }
#endif
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
                    minfo = projFld.propInfo;
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

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
using System.Data;
using System.Linq.Expressions;
using DbLinq.Util;

namespace DbLinq.Linq.Data.Sugar.Implementation
{
    public class DataRecordReader: IDataRecordReader
    {
        /// <summary>
        /// Returns a Expression reading a property from a IDataRecord, at the specified index
        /// </summary>
        /// <param name="dataRecordParameter">The IDataRecord as ParameterExpression</param>
        /// <param name="mappingContextParameter">The MappingContext, as ParameterExpression</param>
        /// <param name="returnType">The expected return type (to be mapped to the property)</param>
        /// <param name="valueIndex">Field index in IDataRecord</param>
        /// <returns>An expression returning the field value</returns>
        public virtual Expression GetPropertyReader(Expression dataRecordParameter, Expression mappingContextParameter, Type returnType, int valueIndex)
        {
            Expression propertyReader;
            Type nullableValueType = GetNullableTypeArgument(returnType);
            if (nullableValueType != null)
            {
                Expression simplePropertyReader = Expression.Convert(
                GetSimplePropertyReader(nullableValueType, valueIndex, dataRecordParameter, mappingContextParameter), returnType);
                Expression zero = Expression.Constant(null, returnType);
                propertyReader = Expression.Condition(
                    Expression.Invoke((Expression<Func<IDataRecord, MappingContext, bool>>)((dataReader, context) => dataReader.IsDBNull(valueIndex)), dataRecordParameter, mappingContextParameter),
                    zero, simplePropertyReader);
            }
            else
            {
                propertyReader = GetSimplePropertyReader(returnType, valueIndex, dataRecordParameter, mappingContextParameter);
            }
            return propertyReader;
        }

        protected virtual Type GetNullableTypeArgument(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return type.GetGenericArguments()[0];
            }
            return null;
        }

        protected virtual Expression GetSimplePropertyReader(Type returnType, int valueIndex,
            Expression reader, Expression mappingContext)
        {
            bool cast;
            var propertyReader = GetSimplePropertyReader(returnType, valueIndex, out cast);
            propertyReader = Expression.Invoke(propertyReader, reader, mappingContext);
            if (cast)
                propertyReader = Expression.Convert(propertyReader, returnType);
            return propertyReader;
        }

        protected virtual Expression GetSimplePropertyReader(Type returnType, int valueIndex,
            out bool cast)
        {
            cast = false;
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
            else if (returnType == typeof(byte[]))
            {
                propertyReader = (Expression<Func<IDataRecord, MappingContext, byte[]>>)((dataReader, mappingContext)
                    => dataReader.GetAsBytes(valueIndex));
            }
            else if (returnType.IsEnum)
            {
                cast = true;
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

        /// <summary>
        /// Wrapper to call the MappingContext
        /// </summary>
        /// <param name="dataRecord"></param>
        /// <param name="columnIndex"></param>
        /// <param name="mappingContext"></param>
        /// <returns></returns>
        protected virtual string GetAsString(IDataRecord dataRecord, int columnIndex, MappingContext mappingContext)
        {
            var value = dataRecord.GetAsString(columnIndex);
            mappingContext.OnGetAsString(dataRecord, ref value, null, columnIndex); // return type null here, expression can be a little more complex than a known type
                                                                                    // TODO: see if we keep this type
            return value;
        }

        protected virtual object GetAsObject(IDataRecord dataRecord, int columnIndex, MappingContext mappingContext)
        {
            var value = dataRecord.GetAsObject(columnIndex);
            mappingContext.OnGetAsObject(dataRecord, ref value, null, columnIndex);
            return value;
        }
    }
}

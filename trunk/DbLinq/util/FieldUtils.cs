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

using System;
using System.Collections.Generic;
using System.Text;

namespace DBLinq.util
{
    public class FieldUtils
    {
        /// <summary>
        /// assign 'ID' to a field, handles int/uint/long conversions.
        /// </summary>
        /// <param name="rowObj">the object containing the ID field</param>
        /// <param name="finfo">field info</param>
        /// <param name="id">the ID value</param>
        public static void SetObjectIdField(object rowObj, System.Reflection.FieldInfo finfo, object id)
        {
            id = CastValue(id, finfo.FieldType);
            /*
            if (id is long)
            {
                //prevent {"Object of type 'System.Int64' cannot be converted to type 'System.UInt32'."}
                //long longID = (long)id;
                bool assignable1 = finfo.FieldType.IsAssignableFrom(typeof(long));
                bool assignable2 = finfo.FieldType.IsAssignableFrom(typeof(int));
                if (finfo.FieldType == typeof(uint))
                {
                    uint uintID = (uint)(long)id;
                    id = uintID;
                }
                else if (finfo.FieldType == typeof(int))
                {
                    int intID = (int)(long)id;
                    id = intID;
                }
            }
            else if (id is decimal)
            {
                //prevent {"Object of type 'System.Int64' cannot be converted to type 'System.UInt32'."}
                //long longID = (long)id;
                bool assignable1 = finfo.FieldType.IsAssignableFrom(typeof(long));
                bool assignable2 = finfo.FieldType.IsAssignableFrom(typeof(int));
                if (finfo.FieldType == typeof(uint))
                {
                    uint uintID = (uint)(decimal)id;
                    id = uintID;
                }
                else if (finfo.FieldType == typeof(int))
                {
                    int intID = (int)(decimal)id;
                    id = intID;
                }
            }
             * */
            finfo.SetValue(rowObj, id);
        }

        /// <summary>
        /// perform cast between decimal, int, uint, and long types.
        /// </summary>
        public static object CastValue(object dbValue, Type desiredType)
        {
            if (dbValue is long)
            {
                //prevent exception {"Object of type 'System.Int64' cannot be converted to type 'System.UInt32'."}
                if (desiredType == typeof(uint))
                {
                    uint uintID = (uint)(long)dbValue;
                    return uintID;
                }
                else if (desiredType == typeof(int))
                {
                    int intID = (int)(long)dbValue;
                    return intID;
                }
            }
            else if (dbValue is decimal)
            {
                if (desiredType == typeof(uint))
                {
                    uint uintID = (uint)(decimal)dbValue;
                    return uintID;
                }
                else if (desiredType == typeof(int))
                {
                    int intID = (int)(decimal)dbValue;
                    return intID;
                }
            }
            return dbValue;
        }

        static readonly object[] emptyIndices = new object[0];

        public static object GetValue(System.Reflection.MemberInfo field, object parentObj)
        {
            System.Reflection.PropertyInfo propInfo = field as System.Reflection.PropertyInfo;
            if (propInfo != null)
            {
                object obj = propInfo.GetValue(parentObj,emptyIndices); //retrieve 'myID' from wrapper class
                return obj;
            }
            else
            {
                System.Reflection.FieldInfo fieldInfo = field as System.Reflection.FieldInfo;
                if (fieldInfo == null)
                {
                    string msg = "L55 Member is neither Property nor Field:" + field;
                    Console.WriteLine(msg);
                    throw new Exception(msg);
                }
                object obj = fieldInfo.GetValue(parentObj);
                return obj;
            }
        }
    }
}

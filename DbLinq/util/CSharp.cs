////////////////////////////////////////////////////////////////////
//Initial author: Jiri George Moudry, 2006.
//License: LGPL. (Visit http://www.gnu.org)
//Commercial code may call into this library, if it's in a different module (DLL)
////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

namespace DBLinq.util
{
    public class CSharp
    {
        /// <summary>
        /// Categorize type - this will determine further processing of retrieved types
        /// </summary>
        public static TypeEnum CategorizeType(Type t)
        {
            if(IsPrimitiveType(t))
                return TypeEnum.Primitive;
            if(AttribHelper.GetTableAttrib(t)!=null)
                return TypeEnum.Column;
            return TypeEnum.Other;
        }

        /// <summary>
        /// if T is string or int or friends, return true.
        /// </summary>
        public static bool IsPrimitiveType(Type t)
        {
            #region IsBuiltinType
            bool isBuiltinType = t==typeof(string)
                || t==typeof(short)
                || t==typeof(ushort)
                || t==typeof(int)
                || t==typeof(uint)
                || t==typeof(long)
                || t==typeof(ulong)
                || t==typeof(float)
                || t==typeof(double)
                || t==typeof(decimal)
                || t==typeof(char)
                || t==typeof(byte)
                || t==typeof(bool);
            return isBuiltinType;
            #endregion
        }

    }
}

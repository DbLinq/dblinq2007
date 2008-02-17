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
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using DbLinq.Linq;

namespace DbLinq.Util
{
    static class RowEnumFactory<T>
    {
        /// <summary>
        /// factory method to create either RowEnum class, or derived RowEnumGroupBy class
        /// </summary>
        public static RowEnumerator<T> Create(SessionVarsParsed vars, Dictionary<T,T> rowCache)
        {
            Type groupType;
            bool hasGroup = IsOrHasGroupField(vars, out groupType); 
            if(!hasGroup)
            {
                return new RowEnumerator<T>(vars,rowCache);
            }

            //for GroupBy queries, determine type TKey, TVal, and read funcs
            Type[] tGenArgs = groupType.GetGenericArguments();
            Type tGrpKey = tGenArgs[0];
            Type tGrpVal = tGenArgs[1];

            //now construct RowEnumeratorGroupBy<X,Y>
            Type tRowEnumGrp1 = typeof(RowEnumeratorGroupBy<,,>);
            Type tRowEnumGrp2 = tRowEnumGrp1.MakeGenericType(typeof(T), tGrpKey, tGrpVal);

            try
            {
                object obj = Activator.CreateInstance(tRowEnumGrp2, vars);
                RowEnumerator<T> ret = (RowEnumerator<T>)obj;
                return ret;
            } 
            catch(Exception ex)
            {
                Console.WriteLine("CreateInstance failed:"+ex);
                throw ex;
            }
        }

        public static bool IsOrHasGroupField(SessionVarsParsed vars, out Type groupType)
        {
            //## don't judge based on presence of groupByExpr - 
            //## in case of GroupBy(City).Select(new{g.Key,g.Count}) the user never sees the GroupBy
            //## so it behaves like a regular select
            groupType = null;
            //if(vars.groupByExpr==null)
            //{
            //    return false; 
            //}

            bool isGroup1 = typeof(T).Name=="IGrouping`2";
            if(isGroup1)
            {
                groupType = typeof(T);
                return true;
            }

            PropertyInfo[] props = typeof(T).GetProperties();
            foreach(PropertyInfo prop in props)
            {
                //bool isGroup = GroupHelper.IsGrouping(typeof(T));
                bool isGroup = prop.PropertyType.Name=="IGrouping`2";
                if(isGroup)
                {
                    groupType = prop.PropertyType;
                    return true;
                }
            }

            //##handle e.g. {g.Key,g.Count} - which is basically {int,int}
            //groupType = vars.selectExpr.Parameters[0].Type; //{g => new {Key = g.Key, Count = g.Count()}}
            //return true;
            return false;
        }
    }
}

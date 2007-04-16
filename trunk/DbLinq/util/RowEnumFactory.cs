////////////////////////////////////////////////////////////////////
//Initial author: Jiri George Moudry, 2006.
//License: LGPL. (Visit http://www.gnu.org)
//Commercial code may call into this library, if it's in a different module (DLL)
////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using DBLinq.Linq;

namespace DBLinq.util
{
    static class RowEnumFactory<T>
    {
        /// <summary>
        /// factory method to create either RowEnum class, or derived RowEnumGroupBy class
        /// </summary>
        public static RowEnumerator<T> Create(SessionVars vars, Dictionary<T,T> rowCache)
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

        public static bool IsOrHasGroupField(SessionVars vars, out Type groupType)
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

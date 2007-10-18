using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DBLinq.util
{
    public class Util
    {
        /// <summary>
        /// break a bigList into pages of size N.
        /// </summary>
        public static IEnumerable<List<T>> Paginate<T>(List<T> bigList, int pageSize)
        {
            List<T> smallList = new List<T>();
            foreach(T t in bigList)
            {
                smallList.Add(t);
                if (smallList.Count >= pageSize)
                {
                    yield return smallList;
                    smallList.Clear();
                }
            }
            if(smallList.Count>0)
                yield return smallList;
        }
    }
}

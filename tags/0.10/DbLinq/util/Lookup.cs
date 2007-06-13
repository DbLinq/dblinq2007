////////////////////////////////////////////////////////////////////
//Initial author: Jiri George Moudry, 2006.
//License: LGPL. (Visit http://www.gnu.org)
//Commercial code may call into this library, if it's in a different module (DLL)
////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
#if LINQ_PREVIEW_2006
//Visual Studio 2005 with Linq Preview May 2006 - can run on Win2000
using System.Query;
using System.Expressions;
#else
//Visual Studio Orcas - requires WinXP
using System.Linq;
using System.Linq.Expressions;
#endif


namespace DBLinq.util
{
    /// <summary>
    /// Lookup: 2nd implementation of IGrouping, as needed in group-by queries.
    /// The original System.Query.Lookup is inaccessible.
    /// </summary>
    public class Lookup<K,T> : IGrouping<K,T>
    {
        K _key;
        public List<T> _elements = new List<T>();

        public Lookup(K k, T val){ _key=k; _elements.Add(val); }

        public K Key { get { return _key; } }
        IEnumerator<T> IEnumerable<T>.GetEnumerator(){ return _elements.GetEnumerator(); }
        public System.Collections.IEnumerator GetEnumerator(){ return _elements.GetEnumerator(); }

        public static IEnumerable<Lookup<K,T>> EnumGroups(DataReader2 rdr, Func<DataReader2,K> keyReadFunc, Func<DataReader2,T> valueReadFunc)
        {
            //assumption: there is no Read() loop around this code

            K prevK = default(K); //keyReadFunc(rdr);
            Lookup<K,T> lookup = null;
            while(rdr.Read())
            {
                if(lookup==null)
                {
                    prevK = keyReadFunc(rdr);
                    T firstVal = valueReadFunc(rdr);
                    lookup=new Lookup<K,T>(prevK, firstVal);
                    continue;
                }

                K currK = keyReadFunc(rdr);
                T currVal = valueReadFunc(rdr);
                if(currK.Equals(prevK)){
                    lookup._elements.Add(currVal);
                } else {
                    yield return lookup;
                    lookup = new Lookup<K,T>(currK,currVal);
                }
            }

            if(lookup!=null)
                yield return lookup;
        }

        public override string ToString()
        {
            return "Lookup Key="+this._key+" Elems="+_elements.Count;
        }
    }
}

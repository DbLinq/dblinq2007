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
using System.Linq;
using System.Linq.Expressions;


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

#if DEAD_CODE
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
#endif

        public override string ToString()
        {
            return "Lookup Key="+this._key+" Elems="+_elements.Count;
        }
    }
}

#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne
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

using System.Collections.Generic;
#if MONO_STRICT
using System.Data.Linq.Sugar;
#else
using DbLinq.Data.Linq.Sugar;
#endif
using System.Reflection;

#if MONO_STRICT
namespace System.Data.Linq.Sugar
#else
namespace DbLinq.Data.Linq.Sugar
#endif
{
    public interface IQueryRunner
    {
        /// <summary>
        /// Enumerates all records return by SQL request
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="selectQuery"></param>
        /// <returns></returns>
        IEnumerable<T> GetEnumerator<T>(SelectQuery selectQuery);

        S Execute<S>(SelectQuery selectQuery);

        /// <summary>
        /// Runs an InsertQuery on a provided object
        /// </summary>
        /// <param name="target"></param>
        /// <param name="insertQuery"></param>
        void Insert(object target, UpsertQuery insertQuery);

        /// <summary>
        /// Performans an update
        /// </summary>
        /// <param name="target">Entity to be flushed</param>
        /// <param name="updateQuery">SQL update query</param>
        /// <param name="modifiedMembers">List of modified members, or null to update all members</param>
        void Update(object target, UpsertQuery updateQuery,IList<MemberInfo> modifiedMembers);

        /// <summary>
        /// Performs a delete
        /// </summary>
        /// <param name="target">Entity to be deleted</param>
        /// <param name="deleteQuery">SQL delete query</param>
        void Delete(object target, DeleteQuery deleteQuery);
    }
}
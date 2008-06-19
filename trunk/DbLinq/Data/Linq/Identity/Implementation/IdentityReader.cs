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

using System;
using System.Reflection;
using System.Collections.Generic;
using DbLinq.Data.Linq.Identity;
using DbLinq.Util;

namespace DbLinq.Data.Linq.Identity.Implementation
{
    public class IdentityReader : IIdentityReader
    {
        private Type type;
        private IList<MemberInfo> keyMembers = new List<MemberInfo>();

        public IdentityKey GetIdentityKey(object entity)
        {
            // no PK? --> null as identity (==we can not collect it)
            if (keyMembers.Count == 0)
                return null;
            var keys = new List<object>();
            foreach (var keyMember in keyMembers)
            {
                var key = keyMember.GetMemberValue(entity);
                keys.Add(key);
            }
            return new IdentityKey(type, keys);
        }

        public IdentityReader(Type t)
        {
            type = t;
            foreach (var memberInfo in t.GetMembers())
            {
                var columnAttribute = AttribHelper.GetColumnAttribute(memberInfo);
                if (columnAttribute != null)
                {
                    if (columnAttribute.IsPrimaryKey)
                        keyMembers.Add(memberInfo);
                }
            }
        }
    }
}
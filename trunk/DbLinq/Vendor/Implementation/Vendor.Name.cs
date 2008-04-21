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

using System.Linq;
using DbLinq.Util;

namespace DbLinq.Vendor.Implementation
{
    partial class Vendor
    {
        public virtual string GetSqlCaseSafeName(string name)
        {
            string[] nameParts = name.Split('.');
            for (int index = 0; index < nameParts.Length; index++)
            {
                nameParts[index] = GetSqlCaseSafeNamePart(nameParts[index]);
            }
            return string.Join(".", nameParts);
        }

        protected virtual string GetSqlCaseSafeNamePart(string namePart)
        {
            if (IsNameCaseSafe(namePart))
                return namePart;
            return MakeNameCaseSafe(namePart);
        }

        protected virtual bool IsNameCaseSafe(string namePart)
        {
            foreach (char c in namePart)
            {
                if (char.IsLower(c))
                    return false;
            }
            return true;
        }

        protected virtual string MakeNameCaseSafe(string namePart)
        {
            return namePart.Enquote('\"');
        }

        /// <summary>
        /// Determines if a given field is dangerous (related to a SQL keyword or containing problematic characters)
        /// </summary>
        protected virtual bool IsFieldNameSafe(string name)
        {
            string nameL = name.ToLower();
            switch (nameL)
            {
            case "user":
            case "bit":
            case "int":
            case "smallint":
            case "tinyint":
            case "mediumint":

            case "float":
            case "double":
            case "real":
            case "decimal":
            case "numeric":

            case "blob":
            case "text":
            case "char":
            case "varchar":

            case "date":
            case "time":
            case "datetime":
            case "timestamp":
            case "year":

                return false;
            default:
                return !name.Contains(' ');
            }
        }

    }
}

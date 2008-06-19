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

namespace DbLinq.Util
{
    public static class StringExtensions
    {
        public static string Enquote(this string text, char startQuote, char endQuote)
        {
            return QuotesHelper.Enquote(text, startQuote, endQuote);
        }

        public static string Enquote(this string text, char quote)
        {
            return QuotesHelper.Enquote(text, quote);
        }

        /// <summary>
        /// Returns true is the provided string is a valid .NET symbol
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool IsIdentifier(this string name)
        {
            for (int index = 0; index < name.Length; index++)
            {
                var category = char.GetUnicodeCategory(name, index);
                // this is not nice, but I found no other way to identity a valid identifier
                switch (category)
                {
                case System.Globalization.UnicodeCategory.DecimalDigitNumber:
                case System.Globalization.UnicodeCategory.LetterNumber:
                case System.Globalization.UnicodeCategory.LowercaseLetter:
                case System.Globalization.UnicodeCategory.UppercaseLetter:
                    break;
                default:
                    return false;
                }
            }
            return true;
        }
    }
}

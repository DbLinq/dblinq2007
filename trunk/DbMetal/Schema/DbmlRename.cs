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
using System.Xml.Serialization;

namespace DbMetal.Schema
{
    /// <summary>
    /// This class main purpose is to allow renamings.
    /// It is based on DBML format (but simpler).
    /// </summary>
    [XmlRoot("Database")]
    public class DbmlRename
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("Class")]
        public string Class { get; set; }

        [XmlElement("Table")]
        public Table[] Tables { get; set; }

        public class Table
        {
            [XmlAttribute("Name")]
            public string Name { get; set; }

            [XmlAttribute("Member")]
            public string Member { get; set; }

            [XmlElement("Type")]
            public Type Type { get; set; }
        }

        public class Type
        {
            [XmlAttribute("Name")]
            public string Name { get; set; }

            [XmlElement("Column")]
            public Column[] Columns { get; set; }
        }

        public class Column
        {
            [XmlAttribute("Name")]
            public string Name { get; set; }

            [XmlAttribute("Member")]
            public string Member { get; set; }

            [XmlAttribute("Storage")]
            public string Storage { get; set; }

            [XmlAttribute("Type")]
            public string Type { get; set; }
        }
    }
}

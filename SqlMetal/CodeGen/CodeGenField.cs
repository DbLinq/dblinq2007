#region MIT license
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
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using DbLinq.Linq;
using SqlMetal.Util;

namespace SqlMetal.CodeGen
{
    /// <summary>
    /// generates a public property representing a SQL table column.
    /// Also generates the backing field.
    /// </summary>
    public class CodeGenField
    {
        DlinqSchema.Column _column;
        string _propertyName;
        string _attrib2;
        string _constraintWarn;
        string _tableClassName;
        string _columnType;


        public CodeGenField(string tableClassName, DlinqSchema.Column column, List<DlinqSchema.Association> constraintsOnField, Parameters mmConfig)
        {
            _column = column;
            _tableClassName = tableClassName;

            //glue together "(Id=true, AutoGen=true,DbType="float")"
            List<string> attribParts = new List<string>();

            if (column.Storage != null && column.Storage!="null")
                attribParts.Add("Storage = \"" + column.Storage + "\"");

            attribParts.Add("Name = \"" + column.Name + "\"");

            attribParts.Add("DbType = \"" + column.DbType + "\"");

            if (column.IsPrimaryKey)
            {
                attribParts.Add("IsPrimaryKey = true");
            }

            if (column.IsDbGenerated)
            {
                attribParts.Add("IsDbGenerated = true");
            }

            if (!column.IsPrimaryKey)
            {
                attribParts.Add("CanBeNull = " + column.CanBeNull.ToString().ToLower());
            }

            if (column.Expression != null)
            {
                attribParts.Add("Expression = \"" + column.Expression + "\"");
            }

            if (column.IsDiscriminator)
            {
                attribParts.Add("IsDiscriminator = true");
            }

            _attrib2 = string.Join(", ", attribParts.ToArray());

            _constraintWarn = "";

            //_nameU = Util.FieldName(column.Name);
            //_columnType = CSharp.FormatType(column.Type, column.CanBeNull);
            _propertyName = column.Member;
            _columnType = column.Type;
        }

        public string generateField(Parameters mmConfig)
        {
            string template = @"
protected $type $storage;";
            template = template.Replace("$type", _columnType);
            template = template.Replace("$storage", _column.Storage);
            template = template.Replace("$attribOpt", _attrib2);
            template = template.Replace("$constraintWarn", _constraintWarn);
            if (_column.IsDbGenerated)
            {
                //mark this field - it must be modified on insertion
                template = "[DbLinq.Linq.Mapping.AutoGenId] " + template;
            }
            return template;
        }

        public string generateProperty(Parameters mmConfig)
        {
            string template = @"
[Column($attribOpt)]
[DebuggerNonUserCode]
public $type $propertyName
{$constraintWarn
    get { return $storage; }
    set { $storage = value; IsModified = true; }
}
";
            template = template.Replace("$type", _columnType);
            template = template.Replace("$propertyName", _propertyName);
            template = template.Replace("$storage", _column.Storage);
            template = template.Replace("$attribOpt", _attrib2);
            template = template.Replace("$constraintWarn", _constraintWarn);
            return template;
        }


    }
}

////////////////////////////////////////////////////////////////////
//Initial author: Jiri George Moudry, 2006.
//License: LGPL. (Visit http://www.gnu.org)
////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Text;
using SqlMetal.schema;
using SqlMetal.util;

namespace SqlMetal.codeGen
{
    public class CodeGenField
    {
        DlinqSchema.Column _column;
        //string _nameU; //camelcase field name, eg. 'Cat'
        string _propertyName;
        string _attrib2;
        string _constraintWarn;
        string _tableClassName;
        string _columnType;
        //bool _attribAutoGen;


        public CodeGenField(string tableClassName, DlinqSchema.Column column, List<DlinqSchema.Association> constraintsOnField )
        {
            _column = column;
            _tableClassName = tableClassName;

            //glue together "(Id=true, AutoGen=true,DbType="float")"
            List<string> attribParts = new List<string>();
            attribParts.Add("Name = \""+column.Name+"\"");
            
            attribParts.Add("DbType = \""+column.DbType+"\"");

            if(column.IsPrimaryKey)
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
            _attrib2 = string.Join(", ", attribParts.ToArray());

            _constraintWarn = "";

            //_nameU = Util.FieldName(column.Name);
            //_columnType = CSharp.FormatType(column.Type, column.CanBeNull);
            _propertyName = column.Member;
            _columnType = column.Type;
        }

        public string generateField()
        {
            string template = @"
protected $type _$name;";
            template = template.Replace("$type", _columnType);
            template = template.Replace("$name", _column.Name);
            template = template.Replace("$attribOpt", _attrib2);
            template = template.Replace("$constraintWarn", _constraintWarn);
            if(_column.IsDbGenerated){
                //mark this field - it must be modified on insertion
                template = "[DBLinq.Linq.Mapping.AutoGenId] "+template;
            }
            return template;
        }

        public string generateProperty()
        {
            string template = @"
[Column($attribOpt)]
[DebuggerNonUserCode]
public $type $propertyName
{$constraintWarn
    get { return _$name; }
    set { _$name = value; IsModified = true; }
}
";
            if (_propertyName == _tableClassName)
            {
                //_nameU += "_1"; //prevent error CS0542: 'XXX': member names cannot be the same as their enclosing type
                _propertyName = "Content"; //same as Linq To Sql
            }

            template = template.Replace("$type", _columnType);
            template = template.Replace("$propertyName", _propertyName);
            template = template.Replace("$name", this._column.Name);
            template = template.Replace("$attribOpt", _attrib2);
            template = template.Replace("$constraintWarn", _constraintWarn);
            return template;
        }


    }
}

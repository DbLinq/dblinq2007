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
        string _nameU; //camelcase field name, eg. 'Cat'
        string _attrib2;
        string _constraintWarn;
        //bool _attribAutoGen;


        public CodeGenField(DlinqSchema.Column column, List<DlinqSchema.Association> constraintsOnField )
        {
            _column = column;

            //glue together "(Id=true, AutoGen=true,DbType="float")"
            List<string> attribParts = new List<string>();
            attribParts.Add("Name=\""+column.Name+"\"");
            
            //if(column.extra=="auto_increment")
            if(column.IsAutogen)
            {
                //attribParts.Add("Id=true, AutoGen=true");
                attribParts.Add("AutoGen=true");
                //_attribAutoGen = true;
            }
            attribParts.Add("DBType=\""+column.DBType+"\"");

            bool isPrimaryKeyCol = column.IsIdentity;
            //bool hasForeignKey   = false;
            if(isPrimaryKeyCol){
                attribParts.Add("Id=true");
            }
            _attrib2 = string.Join(", ", attribParts.ToArray());

            //print warning on constraints
            //_constraintWarn = hasForeignKey
            //    ? "\n#warning TODO L96: handle foreign constraint "+constraintsOnField[0].constraint_name
            //    : "";
            _constraintWarn = "";

            //_nameU = mmConfig.forceUcaseTableName 
            //    ? column.Name.Capitalize() //Char.ToUpper(column.Name[0])+column.Name.Substring(1)
            //    : column.Name;
            _nameU = Util.FieldName(column.Name);
        }

        public string generateField()
        {
            string template = @"
protected $type _$name;";
            template = template.Replace("$type", _column.Type);
            template = template.Replace("$name", _column.Name);
            template = template.Replace("$attribOpt", _attrib2);
            template = template.Replace("$constraintWarn", _constraintWarn);
            if(_column.IsAutogen){
                //mark this field - it must be modified on insertion
                template = "[DBLinq.Linq.AutoGenId] "+template;
            }
            return template;
        }

        public string generateProperty()
        {
            string template = @"
[Column($attribOpt)]
[DebuggerNonUserCode]
public $type $nameU
{$constraintWarn
    get { return _$name; }
    set { _$name=value; _isModified_=true; }
}
";
            template = template.Replace("$type", this._column.Type);
            template = template.Replace("$nameU", _nameU);
            template = template.Replace("$name", this._column.Name);
            template = template.Replace("$attribOpt", _attrib2);
            template = template.Replace("$constraintWarn", _constraintWarn);
            return template;
        }


    }
}

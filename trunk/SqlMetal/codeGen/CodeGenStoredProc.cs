////////////////////////////////////////////////////////////////////
//Initial author: Jiri George Moudry, 2007.
//License: LGPL. (Visit http://www.gnu.org)
////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Text;
using SqlMetal.schema;
using SqlMetal.util;

namespace SqlMetal.codeGen
{
    public class CodeGenStoredProc
    {
        const string SP_BODY_TEMPLATE = @"
[Function(Name=""$procNameSql"")]
public $retType $procNameCsharp($paramsIn)
{
    IExecuteResult result = base.ExecuteMethodCall(this, ((MethodInfo)(MethodInfo.GetCurrentMethod())), $sqlArgs);
    return $resultValue;
}
";

        public string FormatProc(DlinqSchema.Function storedProc)
        {
            string text = SP_BODY_TEMPLATE;
            text = text.Replace("$procNameCsharp", storedProc.Name);
            text = text.Replace("$procNameSql", storedProc.Name);
            return text;
        }
    }
}
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
public $retType $procNameCsharp($paramString)
{
    IExecuteResult result = base.ExecuteMethodCall(this, ((MethodInfo)(MethodInfo.GetCurrentMethod())), $sqlArgs);
    return $resultValue;
}
";

        public static string FormatProc(DlinqSchema.Function storedProc)
        {
            string text = SP_BODY_TEMPLATE.Replace("\n", "\t\n");
            text = text.Replace("$procNameCsharp", storedProc.Name);
            text = text.Replace("$procNameSql", storedProc.Name);

            List<string> paramStringsList = new List<string>();
            foreach (DlinqSchema.Parameter param in storedProc.Parameters)
            {
                string paramStr = FormatProcParam(param);
                paramStringsList.Add(paramStr);
            }
            string paramString = string.Join("\t\t,", paramStringsList.ToArray());

            string retType = "void";
            string resultValue = "1";
            if (storedProc.Return.Count > 0)
            {
                retType = storedProc.Return[0].Type;
                resultValue = "result.ReturnValue as $retType"; //for functions...
            }
            text = text.Replace("$resultValue", resultValue);
            text = text.Replace("$paramString", paramString);
            text = text.Replace("$retType", retType);
            return text;
        }

        const string PARAM_TEMPLATE = @"[Parameter(Name=""$dbName"", DbType=""$dbType"")] $type $name";

        private static string FormatProcParam(DlinqSchema.Parameter param)
        {
            string text = PARAM_TEMPLATE;
            text = text.Replace("$dbName", param.Name);
            text = text.Replace("$name", param.Name);
            text = text.Replace("$dbType", param.DbType);
            text = text.Replace("$type", param.Type);
            return text;
        }

    }
}
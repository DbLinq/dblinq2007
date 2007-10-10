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
[Function(Name=""$procNameSql"" ProcedureOrFunction=""$procType"")]
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
            text = text.Replace("$procType", storedProc.ProcedureOrFunction);

            List<string> paramStringsList = new List<string>();
            List<string> sqlArgList = new List<string>();
            foreach (DlinqSchema.Parameter param in storedProc.Parameters)
            {
                string paramStr = FormatProcParam(param);
                paramStringsList.Add(paramStr);
                sqlArgList.Add(FormatInnerArg(param));
            }
            string paramString = string.Join("\t\t,", paramStringsList.ToArray());
            string sqlArgs = string.Join(", ", sqlArgList.ToArray());

            string retType = "void";
            string resultValue = "1";
            if (storedProc.Return.Count > 0)
            {
                retType = storedProc.Return[0].Type;
                resultValue = "result.ReturnValue as $retType"; //for functions...
            }

            bool isDataShapeUnknown = storedProc.ElementType==null 
                && storedProc.BodyContainsSelectStatement;
            if (isDataShapeUnknown)
            {
                //if we don't know the shape of results, and the proc body contains some selects,
                //we have no choice but to return an untyped DataSet
                retType = "System.Data.DataSet";
            }
            text = text.Replace("$resultValue", resultValue);
            text = text.Replace("$paramString", paramString);
            text = text.Replace("$retType", retType);
            text = text.Replace("$sqlArgs", sqlArgs);
            return text;
        }

        const string ARG_TEMPLATE = @"$inOut $name";
        const string PARAM_TEMPLATE = @"[Parameter(Name=""$dbName"", DbType=""$dbType"")] $inOut $type $name";

        private static string FormatInnerArg(DlinqSchema.Parameter param)
        {
            string text = ARG_TEMPLATE;
            text = text.Replace("$name", param.Name);
            text = text.Replace("$inOut ", formatInOut(param.InOut));
            return text;
        }

        private static string FormatProcParam(DlinqSchema.Parameter param)
        {
            string text = PARAM_TEMPLATE;
            text = text.Replace("$dbName", param.Name);
            text = text.Replace("$name", param.Name);
            text = text.Replace("$dbType", param.DbType);
            text = text.Replace("$type", param.Type);
            text = text.Replace(" $inOut", formatInOut(param.InOut));
            return text;
        }

        static string formatInOut(System.Data.ParameterDirection inOut)
        {
            switch (inOut)
            {
                //case System.Data.ParameterDirection.Input: return "";
                case System.Data.ParameterDirection.Output: return "out ";
                case System.Data.ParameterDirection.InputOutput: return "ref ";
                default: return "";
            }


        }

    }
}
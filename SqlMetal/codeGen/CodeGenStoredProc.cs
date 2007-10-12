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
        const string NL = "\r\n";
        const string NLNL = "\r\n\r\n";
        const string NLT = "\r\n\t";

        const string SP_BODY_TEMPLATE = @"
[FunctionEx(Name=""$procNameSql"", ProcedureOrFunction=""$procType"")]
public $retType $procNameCsharp($paramString)
{
    IExecuteResult result = base.ExecuteMethodCall(this, ((MethodInfo)(MethodInfo.GetCurrentMethod()))$sqlInArgs);
    $assignOutParams
    return $resultValue;
}
";

        public static string FormatProc(DlinqSchema.Function storedProc)
        {
            string text = SP_BODY_TEMPLATE.Replace(NL, "\t" + NL);
            text = text.Replace("$procNameCsharp", storedProc.Name);
            text = text.Replace("$procNameSql", storedProc.Name);
            text = text.Replace("$procType", storedProc.ProcedureOrFunction);

            List<string> paramStringsList = new List<string>();
            List<string> sqlInArgList = new List<string>();
            List<string> outParamLineList = new List<string>();
            int paramIndex = -1;
            foreach (DlinqSchema.Parameter param in storedProc.Parameters)
            {
                paramIndex++;
                string paramStr = FormatProcParam(param);
                paramStringsList.Add(paramStr);

                if (param.InOut == System.Data.ParameterDirection.Input || param.InOut == System.Data.ParameterDirection.InputOutput)
                {
                    sqlInArgList.Add(FormatInnerArg(param));
                }

                if (param.InOut == System.Data.ParameterDirection.Output || param.InOut == System.Data.ParameterDirection.InputOutput)
                {
                    string outParamLine = "\t" + param.Name + " = ("+param.Type+") result.GetParameterValue(" + paramIndex + ");";
                    outParamLineList.Add(outParamLine);
                }
            }

            string paramString = string.Join(NL + "\t\t,", paramStringsList.ToArray());
            string sqlInArgs = string.Join(", ", sqlInArgList.ToArray());
            string outParamLines = string.Join(NL, outParamLineList.ToArray());

            string retType = "void";
            string resultValue = "";
            if (storedProc.Return.Count > 0)
            {
                retType = storedProc.Return[0].Type;
                resultValue = "result.ReturnValue as $retType"; //for functions...
            }

            bool isDataShapeUnknown = storedProc.ElementType == null
                && storedProc.BodyContainsSelectStatement;
            if (isDataShapeUnknown)
            {
                //if we don't know the shape of results, and the proc body contains some selects,
                //we have no choice but to return an untyped DataSet.
                //
                //TODO: either parse proc body like microsoft, 
                //or create a little GUI tool which would call the proc with test values, to determine result shape.
                retType = "System.Data.DataSet";
                resultValue = "result.ReturnValue as System.Data.DataSet";
            }

            text = text.Replace("$resultValue", resultValue);
            text = text.Replace("$paramString", paramString);
            text = text.Replace("$retType", retType);
            if (sqlInArgs.Length > 0) 
            { 
                sqlInArgs = ", " + sqlInArgs; //pre-pend comma, if there are some args
            }
            text = text.Replace("$sqlInArgs", sqlInArgs);
            text = text.Replace("$assignOutParams", outParamLines);
            text = text.Replace("    \r\n", ""); //if there were no out params assigned, remove the blank line
            text = text.Replace("    ", "\t"); //4 spaces -> tab
            text = text.Replace(NL, NLT); //move one tab to the right
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
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

using System;
using System.Collections.Generic;
using System.Text;
using DbLinq.Linq;
using SqlMetal.Util;

namespace SqlMetal.CSharpGenerator
{
    public class StoredProcedureGenerator
    {
        const string NL = "\r\n";
        const string NLNL = "\r\n\r\n";
        const string NLT = "\r\n\t";

        const string SP_BODY_TEMPLATE = @"
[FunctionEx(Name = ""$procNameSql"", ProcedureOrFunction = ""$procType"")]
public $retType $procNameCsharp($paramString)
{
    System.Data.Linq.IExecuteResult result = base.ExecuteMethodCall(this, ((MethodInfo)(MethodInfo.GetCurrentMethod()))$sqlInArgs);
    $assignOutParams
    return $resultValue;
}
";

        public string GetProcedureCall(DlinqSchema.Function storedProcedure)
        {
            if (storedProcedure == null || storedProcedure.Name == null)
            {
                Console.WriteLine("CodeGenStoredProc: Error L33 Invalid storedProcedure object");
                return "//error L33 Invalid storedProcedure object";
            }

            string text = SP_BODY_TEMPLATE.Replace(NL, "\t" + NL);
            text = text.Replace("$procNameCsharp", storedProcedure.Method ?? storedProcedure.Name);
            text = text.Replace("$procNameSql", storedProcedure.Name);
            text = text.Replace("$procType", storedProcedure.ProcedureOrFunction);

            List<string> paramStringsList = new List<string>();
            List<string> sqlInArgList = new List<string>();
            List<string> outParamLineList = new List<string>();
            int paramIndex = -1;
            foreach (DlinqSchema.Parameter param in storedProcedure.Parameters)
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
            if (storedProcedure.Return.Count > 0)
            {
                retType = storedProcedure.Return[0].Type;
                resultValue = "($retType)result.ReturnValue"; //for functions, e.g. (int)result.RetValue
            }

            bool isDataShapeUnknown = storedProcedure.ElementType == null
                                      && storedProcedure.BodyContainsSelectStatement
                                      && storedProcedure.ProcedureOrFunction == "PROCEDURE";
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
            text = text.Replace("\t\r\n    \t\r\n", "\t\r\n"); //if there were no out params assigned, remove the blank line
            text = text.Replace("    ", "\t"); //4 spaces -> tab
            text = text.Replace(NL, NLT); //move one tab to the right
            return text;
        }

        const string ARG_TEMPLATE = @"$inOut $name";
        const string PARAM_TEMPLATE = @"[Parameter(Name = ""$dbName"", DbType = ""$dbType"")] $inOut $type $name";

        private static string FormatInnerArg(DlinqSchema.Parameter param)
        {
            string text = ARG_TEMPLATE;
            text = text.Replace("$name", param.Name);
            text = text.Replace("$inOut ", FormatInOut(param.InOut));
            return text;
        }

        private static string FormatProcParam(DlinqSchema.Parameter param)
        {
            string text = PARAM_TEMPLATE;
            text = text.Replace("$dbName", param.Name);
            text = text.Replace("$name", param.Name);
            text = text.Replace("$dbType", param.DbType);
            text = text.Replace("$type", param.Type);
            text = text.Replace(" $inOut", FormatInOut(param.InOut));
            return text;
        }

        private static string FormatInOut(System.Data.ParameterDirection inOut)
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
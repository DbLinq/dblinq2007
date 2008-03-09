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
using System.Data;
using DbLinq.Linq;

namespace SqlMetal.Generator.Implementation
{
    public partial class CodeGenerator
    {
        protected virtual void WriteDataContextProcedures(CodeWriter writer, DlinqSchema.Database schema, GenerationContext context)
        {
            foreach (var procedure in schema.Functions)
            {
                WriteDataContextProcedure(writer, procedure, context);
            }
        }

        private void WriteDataContextProcedure(CodeWriter writer, DlinqSchema.Function procedure, GenerationContext context)
        {
            if (procedure == null || procedure.Name == null)
            {
                Console.WriteLine("CodeGenStoredProc: Error L33 Invalid storedProcedure object");
                writer.WriteComment("error L33 Invalid storedProcedure object");
                return;
            }

            var functionAttribute = new AttributeDefinition("Function");
            functionAttribute["Name"] = procedure.Name;
            functionAttribute["IsComposable"] = procedure.IsComposable;

            using (writer.WriteAttribute(functionAttribute))
            using (writer.WriteMethod(Specifications.Public, GetProcedureName(procedure),
                GetProcedureType(procedure), GetProcedureParameters(procedure)))
            {
                string result = WriteProcedureBodyMethodCall(writer, procedure, context);
                WriteProcedureBodyOutParameters(writer, procedure, result, context);
                WriteProcedureBodyReturnValue(writer, procedure, result, context);
            }
            writer.WriteLine();
        }

        protected virtual void WriteProcedureBodyReturnValue(CodeWriter writer, DlinqSchema.Function procedure, string result, GenerationContext context)
        {
            Type returnType = GetProcedureType(procedure);
            if (returnType != null)
                writer.WriteLine(writer.GetReturnStatement(writer.GetCast(writer.GetMember(result, "ReturnValue"), returnType, true)));
        }

        protected virtual void WriteProcedureBodyOutParameters(CodeWriter writer, DlinqSchema.Function procedure, string result, GenerationContext context)
        {
            int parameterIndex = 0;
            foreach (var parameter in procedure.Parameters)
            {
                if (parameter.DirectionOut)
                    WriteProcedureBodyOutParameter(writer, parameter, result, parameterIndex, context);

                parameterIndex++;
            }
        }

        protected virtual void WriteProcedureBodyOutParameter(CodeWriter writer, DlinqSchema.Parameter parameter, string result, int parameterIndex, GenerationContext context)
        {
            string p = writer.GetMethodCall(writer.GetMember(result, "GetParameterValue"), parameterIndex.ToString());
            string cp = writer.GetCast(p, GetType(parameter.Type), true);
            writer.WriteLine(writer.GetStatement(writer.GetAssignment(parameter.Name, cp)));
        }

        protected abstract string WriteProcedureBodyMethodCall(CodeWriter writer, DlinqSchema.Function procedure, GenerationContext context);

        protected virtual string GetProcedureName(DlinqSchema.Function procedure)
        {
            return procedure.Method ?? procedure.Name;
        }

        protected virtual Type GetProcedureType(DlinqSchema.Function procedure)
        {
            Type type = null;
            if (procedure.Return != null)
            {
                type = GetType(procedure.Return.Type);
            }

            bool isDataShapeUnknown = procedure.ElementType == null
                                      && procedure.BodyContainsSelectStatement
                                      && !procedure.IsComposable;
            if (isDataShapeUnknown)
            {
                //if we don't know the shape of results, and the proc body contains some selects,
                //we have no choice but to return an untyped DataSet.
                //
                //TODO: either parse proc body like microsoft, 
                //or create a little GUI tool which would call the proc with test values, to determine result shape.
                type = typeof(DataSet);
            }
            return type;
        }

        protected virtual ParameterDefinition[] GetProcedureParameters(DlinqSchema.Function procedure)
        {
            var parameters = new List<ParameterDefinition>();
            foreach (var parameter in procedure.Parameters)
                parameters.Add(GetProcedureParameter(parameter));
            return parameters.ToArray();
        }

        protected virtual ParameterDefinition GetProcedureParameter(DlinqSchema.Parameter parameter)
        {
            var parameterDefinition = new ParameterDefinition();
            parameterDefinition.Name = parameter.Name;
            parameterDefinition.Type = GetType(parameter.Type);
            switch (parameter.Direction)
            {
            case DlinqSchema.ParameterDirection.In:
                parameterDefinition.Specifications |= Specifications.In;
                break;
            case DlinqSchema.ParameterDirection.Out:
                parameterDefinition.Specifications |= Specifications.Out;
                break;
            case DlinqSchema.ParameterDirection.InOut:
                parameterDefinition.Specifications |= Specifications.Ref;
                break;
            default:
                throw new ArgumentOutOfRangeException();
            }
            parameterDefinition.Attribute = new AttributeDefinition("Parameter");
            parameterDefinition.Attribute["Name"] = parameter.Name;
            parameterDefinition.Attribute["DbType"] = parameter.DbType;
            return parameterDefinition;
        }
    }
}

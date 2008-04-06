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

namespace SqlMetal.Generator.EntityInterface.Implementation
{
    public class IModifiedImplementation : InterfaceImplementation
    {
        public override string InterfaceName
        {
            get { return "IModified"; }
        }

        private const string ModifiedName = "IsModified"; // mandatory value, since the IModified interface requires this member

        public override void WriteHeader(CodeWriter writer, DbLinq.Schema.Dbml.Table table, GenerationContext context)
        {
            writer.WriteCommentLine("IModified backing field");
            writer.WritePropertyWithBackingField(SpecificationDefinition.Public, ModifiedName, writer.GetLiteralType(typeof(bool)));
            writer.WriteLine();
        }

        public override void WritePropertyAfterSet(CodeWriter writer, DbLinq.Schema.Dbml.Column property, GenerationContext context)
        {
            writer.WriteLine(writer.GetStatement(writer.GetAssignmentExpression(ModifiedName, writer.GetLiteralValue(true))));
        }
    }
}
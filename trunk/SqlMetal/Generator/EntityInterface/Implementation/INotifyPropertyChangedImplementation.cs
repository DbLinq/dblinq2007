﻿#region MIT license
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

using System.ComponentModel;
using DbLinq.Linq;

namespace SqlMetal.Generator.EntityInterface.Implementation
{
    public class INotifyPropertyChangedImplementation : InterfaceImplementation
    {
        public override string InterfaceName
        {
            get { return typeof(INotifyPropertyChanged).Name; }
        }

        private const string sendPropertyChangedMethod = "OnPropertyChanged";

        public override void WriteHeader(CodeWriter writer, DlinqSchema.Table table, GenerationContext context)
        {
            using (writer.WriteRegion(string.Format("{0} handling", typeof(INotifyPropertyChanged).Name)))
            {
                const string eventName = "PropertyChanged"; // do not change, part of INotifyPropertyChanged
                const string propertyNameName = "propertyName";

                // event
                writer.WriteEvent(SpecificationDefinition.Public, eventName, typeof(PropertyChangedEventHandler).Name);
                writer.WriteLine();
                // method
                using (writer.WriteMethod(SpecificationDefinition.Protected | SpecificationDefinition.Virtual,
                             sendPropertyChangedMethod, null, new ParameterDefinition { Name = propertyNameName, Type = typeof(string) }))
                {
                    using (writer.WriteIf(writer.GetDifferentExpression(eventName, writer.GetNullExpression())))
                    {
                        writer.WriteLine(writer.GetStatement(writer.GetMethodCallExpression(eventName,
                            writer.GetThisExpression(),
                            writer.GetNewExpression(writer.GetMethodCallExpression(typeof(PropertyChangedEventArgs).Name,
                                                                                   propertyNameName)))));
                    }
                }
            }
        }

        public override void WritePropertyAfterSet(CodeWriter writer, DlinqSchema.Column property, GenerationContext context)
        {
            writer.WriteLine(writer.GetStatement(writer.GetMethodCallExpression(sendPropertyChangedMethod,
                writer.GetLiteralValue(property.Name))));
        }
    }
}

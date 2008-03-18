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
//        Andrey Shchekin
////////////////////////////////////////////////////////////////////
#endregion

using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace DbLinq.Schema
{
    public static class DbmlSerializer
    {
        private class ValidationException : XmlSchemaValidationException
        {
            public ValidationException(IList<string> errors)
            {
                Data["Errors"] = errors;
            }
        }

        private static XmlReader OpenXml(Stream xmlStream, Stream xsdStream, IList<string> validationErrors)
        {
            validationErrors.Clear();

            var xmlReaderSettings = new XmlReaderSettings();
            xmlReaderSettings.Schemas.Add(null, XmlReader.Create(xsdStream));
            xmlReaderSettings.ValidationType = ValidationType.Schema;
            xmlReaderSettings.ValidationEventHandler += delegate(object sender, ValidationEventArgs e)
                                                        {
                                                            validationErrors.Add(e.Message);
                                                        };
            var xmlValidator = XmlReader.Create(xmlStream, xmlReaderSettings);
            return xmlValidator;
        }

        private static Stream OpenXsd()
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(DbmlSerializer), "DbmlSchema.xsd");
        }

        private static void CheckValidation(IList<string> validationErrors)
        {
            if (validationErrors.Count > 0)
            {
                throw new ValidationException(validationErrors);
            }
        }

        public static Dbml.Database Read(Stream xmlStream, IList<string> validationErrors)
        {
            using (Stream xsdStream = OpenXsd())
            using (XmlReader xmlReader = OpenXml(xmlStream, xsdStream, validationErrors))
            {
                var xmlSerializer = new XmlSerializer(typeof(Dbml.Database));
                var dbml = (Dbml.Database)xmlSerializer.Deserialize(xmlReader);
                return dbml;
            }
        }

        public static Dbml.Database Read(Stream xmlStream)
        {
            var validationErrors = new List<string>();
            var dbml = Read(xmlStream, validationErrors);
            CheckValidation(validationErrors);
            return dbml;
        }

        public static void Write(Stream xmlStream, Dbml.Database dbml)
        {
            var xmlSerializer = new XmlSerializer(dbml.GetType());
            xmlSerializer.Serialize(xmlStream, dbml);
        }
    }
}

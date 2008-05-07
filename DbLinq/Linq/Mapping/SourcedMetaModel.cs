#region MIT license
// 
// Copyright (c) 2007-2008 Jiri Moudry
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion

using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Reflection;

namespace DbLinq.Linq.Mapping
{
    /// <summary>
    /// This class wraps a real (and MappingSource independant) meta model
    /// It uses the underlying MetaModel for all members, except for MappingSource
    /// Why did MS people conceived it this way?
    /// </summary>
    internal class SourcedMetaModel : MetaModel
    {
        public SourcedMetaModel(MetaModel source, MappingSource mappingSource)
        {
            metaModel = source;
            this.mappingSource = mappingSource;
        }

        protected MetaModel metaModel;

        private MappingSource mappingSource;
        public override MappingSource MappingSource
        {
            get { return mappingSource; }
        }

        public override Type ContextType
        {
            get { return metaModel.ContextType; }
        }

        public override string DatabaseName
        {
            get { return metaModel.DatabaseName; }
        }

        public override MetaFunction GetFunction(MethodInfo method)
        {
            return metaModel.GetFunction(method);
        }

        public override IEnumerable<MetaFunction> GetFunctions()
        {
            return metaModel.GetFunctions();
        }

        public override MetaType GetMetaType(Type type)
        {
            return metaModel.GetMetaType(type);
        }

        public override MetaTable GetTable(Type rowType)
        {
            return metaModel.GetTable(rowType);
        }

        public override IEnumerable<MetaTable> GetTables()
        {
            return metaModel.GetTables();
        }

        public override Type ProviderType
        {
            get { return metaModel.ProviderType; }
        }
    }
}

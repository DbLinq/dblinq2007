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
using System.Collections.ObjectModel;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Reflection;
using DbLinq.Util;
using System.Collections.Generic;

namespace DbLinq.Linq.Mapping
{
    internal class AttributedMetaType : MetaType
    {
        public AttributedMetaType(Type classType)
        {
            type = classType;

            // associations and members
            var associationsList = new List<MetaAssociation>();
            var dataMembersList = new List<MetaDataMember>();
            foreach (var memberInfo in classType.GetMembers())
            {
                var association = memberInfo.GetAttribute<AssociationAttribute>();
                if (association != null)
                    associationsList.Add(new AttributedMetaAssociation(memberInfo, association, new AttributedAssociationMetaDataMember(memberInfo, association, this)));
                var column = memberInfo.GetAttribute<ColumnAttribute>();
                if (column != null)
                    dataMembersList.Add(new AttributedColumnMetaDataMember(memberInfo, column, this));
            }
            associations = new ReadOnlyCollection<MetaAssociation>(associationsList);
            dataMembers = new ReadOnlyCollection<MetaDataMember>(dataMembersList);
        }

        internal void SetMetaTable(MetaTable metaTable)
        {
            table = metaTable;
        }

        private ReadOnlyCollection<MetaAssociation> associations;
        public override ReadOnlyCollection<MetaAssociation> Associations
        {
            get { return associations; }
        }

        public override bool CanInstantiate
        {
            // TODO: shall we expect something else?
            get { return true; }
        }

        private ReadOnlyCollection<MetaDataMember> dataMembers;
        public override ReadOnlyCollection<MetaDataMember> DataMembers
        {
            get { return dataMembers; }
        }

        public override MetaDataMember DBGeneratedIdentityMember
        {
            get { throw new NotImplementedException(); }
        }

        public override ReadOnlyCollection<MetaType> DerivedTypes
        {
            get { throw new NotImplementedException(); }
        }

        public override MetaDataMember Discriminator
        {
            get { throw new NotImplementedException(); }
        }

        public override MetaDataMember GetDataMember(MemberInfo member)
        {
            // TODO: optimize?
            return (from dataMember in DataMembers where dataMember.Member == member select dataMember).SingleOrDefault();
        }

        public override MetaType GetInheritanceType(Type type)
        {
            throw new NotImplementedException();
        }

        public override MetaType GetTypeForInheritanceCode(object code)
        {
            throw new NotImplementedException();
        }

        public override bool HasAnyLoadMethod
        {
            get { throw new NotImplementedException(); }
        }

        public override bool HasAnyValidateMethod
        {
            get { throw new NotImplementedException(); }
        }

        public override bool HasInheritance
        {
            get { throw new NotImplementedException(); }
        }

        public override bool HasInheritanceCode
        {
            get { throw new NotImplementedException(); }
        }

        public override bool HasUpdateCheck
        {
            get { throw new NotImplementedException(); }
        }

        public override ReadOnlyCollection<MetaDataMember> IdentityMembers
        {
            get { throw new NotImplementedException(); }
        }

        public override MetaType InheritanceBase
        {
            get { throw new NotImplementedException(); }
        }

        public override object InheritanceCode
        {
            get { throw new NotImplementedException(); }
        }

        public override MetaType InheritanceDefault
        {
            get { throw new NotImplementedException(); }
        }

        public override MetaType InheritanceRoot
        {
            get { throw new NotImplementedException(); }
        }

        public override ReadOnlyCollection<MetaType> InheritanceTypes
        {
            get { throw new NotImplementedException(); }
        }

        public override bool IsEntity
        {
            get { return true; }
        }

        public override bool IsInheritanceDefault
        {
            get { throw new NotImplementedException(); }
        }

        public override MetaModel Model
        {
            get { return Table.Model; }
        }

        public override string Name
        {
            get { return type.Name; }
        }

        public override MethodInfo OnLoadedMethod
        {
            get { throw new NotImplementedException(); }
        }

        public override MethodInfo OnValidateMethod
        {
            get { throw new NotImplementedException(); }
        }

        public override ReadOnlyCollection<MetaDataMember> PersistentDataMembers
        {
            get { throw new NotImplementedException(); }
        }

        private MetaTable table;
        public override MetaTable Table
        {
            get { return table; }
        }

        private Type type;
        public override Type Type
        {
            get { return type; }
        }

        public override MetaDataMember VersionMember
        {
            get { throw new NotImplementedException(); }
        }
    }
}

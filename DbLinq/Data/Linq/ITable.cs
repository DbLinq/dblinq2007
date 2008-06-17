using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if MONO_STRICT
namespace System.Data.Linq
{
    public interface ITable
    {
        DataContext Context { get; }
        bool IsReadOnly { get; }
        void Attach(object entity);
        void Attach(object entity, bool asModified);
        void Attach(object entity, object original);
        void AttachAll(IEnumerable entities);
        void AttachAll(IEnumerable entities, bool asModified);
        void DeleteAllOnSubmit(IEnumerable entities);
        void DeleteOnSubmit(object entity);
        ModifiedMemberInfo[] GetModifiedMembers(object entity);
        object GetOriginalEntityState(object entity);
        void InsertAllOnSubmit(IEnumerable entities);
        void InsertOnSubmit(object entity);
    }
}
#endif

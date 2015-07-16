#if MONO_STRICT
using System.Data.Linq;
#else
using DbLinq.Data.Linq;
#endif

namespace DbLinq.Data.Linq.Implementation
{
    /// <summary>
    /// Interface of QueryProvider
    /// </summary>
    public interface IQueryProvider<T>
    {
        DataContext Context { get; }
    }
}

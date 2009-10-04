using System.Linq;
using EPiServer.Core;
using PageTypeBuilder;

namespace LinqToEpiServer.PageTypeBuilder
{
    public interface IPageTypeBuilderRepository
    {
        IQueryable<T> FindDescendantsOf<T>(PageReference reference) where T : TypedPageData;
    }
}
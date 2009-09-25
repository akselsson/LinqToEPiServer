using System.Linq;
using EPiServer.Core;

namespace LinqToEPiServer
{
    public interface IPageDataRepository
    {
        IQueryable<PageData> FindDescendantsOf(PageReference page);
    }
}
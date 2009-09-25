using System.Linq;
using EPiServer.Core;
using LinqToEPiServer.Implementation;

namespace LinqToEPiServer
{
    public class PageDataRepository : IPageDataRepository
    {
        private readonly IQueryExecutor _executor;

        public PageDataRepository(IQueryExecutor executor)
        {
            _executor = executor;
        }

        public IQueryable<PageData> FindDescendantsOf(PageReference page)
        {
            return new PageDataQuery(new FindPagesWithCriteriaQueryProvider(page, _executor));
        }
    }
}
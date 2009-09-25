using EPiServer;
using EPiServer.Core;

namespace LinqToEPiServer
{
    public interface IQueryExecutor
    {
        PageDataCollection FindPagesWithCriteria(PageReference start, params PropertyCriteria[] criteria);
    }
}
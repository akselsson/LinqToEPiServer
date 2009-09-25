using EPiServer;
using EPiServer.Core;

namespace LinqToEPiServer.Tests.Fakes
{
    public class StubQueryExecutor : IQueryExecutor
    {
        private PropertyCriteria[] _last;

        public PropertyCriteria[] Last
        {
            get { return _last; }
        }

        public PageDataCollection FindPagesWithCriteria(PageReference start, params PropertyCriteria[] criteria)
        {
            _last = criteria;
            return new PageDataCollection();
        }
    }
}
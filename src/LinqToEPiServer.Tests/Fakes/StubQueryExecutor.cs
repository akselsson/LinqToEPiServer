using System;
using EPiServer;
using EPiServer.Core;

namespace LinqToEPiServer.Tests.Fakes
{
    public class StubQueryExecutor : IQueryExecutor
    {
        private PropertyCriteria[] _last;
        private PageDataCollection _nextReturn;

        public PropertyCriteria[] Last
        {
            get { return _last; }
        }

        public void SetNextReturn(PageDataCollection value)
        {
            _nextReturn = value;
        }

        public PageDataCollection FindPagesWithCriteria(PageReference start, params PropertyCriteria[] criteria)
        {
            _last = criteria;
            return _nextReturn ?? new PageDataCollection();
        }
    }
}
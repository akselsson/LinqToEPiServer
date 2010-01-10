using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer;
using EPiServer.Core;

namespace LinqToEPiServer.Tests.IntegrationTests
{
    public class RecursiveGetChildrenRepository : IPageDataRepository
    {
        private readonly DataFactory _pageSource = DataFactory.Instance;

        public IQueryable<PageData> FindDescendantsOf(PageReference page)
        {
            return GetChildrenOf(page).AsQueryable();
        }

        private IEnumerable<PageData> GetChildrenOf(PageReference reference)
        {
            foreach (PageReference descendant in _pageSource.GetDescendents(reference))
            {
                PageData data = _pageSource.GetPage(descendant);
                if (data == null)
                    throw new InvalidOperationException("Could not get page " + descendant);
                yield return data;
            }
        }
    }
}
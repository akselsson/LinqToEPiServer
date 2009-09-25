using EPiServer.Configuration;

namespace LinqToEPiServer.Tests.Helpers
{
    class TestSiteElementCollection : SiteElementCollection
    {
        public void Add(SiteElement e)
        {
            BaseAdd(e);
        }
    }
}
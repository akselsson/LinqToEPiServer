using EPiServer.Configuration;

namespace LinqToEPiServer.Tests.Helpers
{
    class TestEPiServerSection : EPiServerSection
    {
        private readonly TestSiteElementCollection _siteElementCollection;

        public TestEPiServerSection()
        {
            _siteElementCollection = new TestSiteElementCollection();
            base["sites"] = _siteElementCollection;
        }

        public void AddSite(SiteElement e)
        {
            _siteElementCollection.Add(e);
        }
    }
}
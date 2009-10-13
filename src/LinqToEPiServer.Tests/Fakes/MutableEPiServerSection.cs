using EPiServer.Configuration;

namespace LinqToEPiServer.Tests.Fakes
{
    class MutableEPiServerSection : EPiServerSection
    {
        private readonly MutableSiteElementCollection _siteElementCollection;

        public MutableEPiServerSection()
        {
            _siteElementCollection = new MutableSiteElementCollection();
            base["sites"] = _siteElementCollection;
        }

        public void AddSite(SiteElement e)
        {
            _siteElementCollection.Add(e);
        }
    }
}